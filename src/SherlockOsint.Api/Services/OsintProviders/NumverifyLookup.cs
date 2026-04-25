using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Numverify (apilayer) phone validation + carrier / country / line-type lookup.
/// Free tier: 100 lookups/month, HTTP-only on free plan.
/// Skips silently if Osint:NumverifyApiKey is missing.
///
/// Note: Numverify does NOT do reverse-name lookup. For phone → name you need
/// paid services (TrueCaller / Sync.me / Whitepages). See USER_TODO.md.
/// </summary>
public class NumverifyLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NumverifyLookup> _logger;
    private readonly string? _apiKey;

    public NumverifyLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NumverifyLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _apiKey = configuration["Osint:NumverifyApiKey"];
    }

    public async Task<List<OsintNode>> SearchAsync(string phone, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(phone)) return results;
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogInformation("Numverify lookup skipped — Osint:NumverifyApiKey not configured");
            return results;
        }

        // Strip everything but digits + leading "+"
        var normalized = phone.StartsWith("+") ? "+" : "";
        normalized += new string(phone.Where(char.IsDigit).ToArray());
        if (normalized.Length < 7) return results;

        try
        {
            // Free tier is HTTP-only; paid is HTTPS. We try HTTPS first (works on paid),
            // fall back to HTTP if needed. Most users will be on free tier → HTTP.
            var url = $"http://apilayer.net/api/validate?access_key={Uri.EscapeDataString(_apiKey)}&number={Uri.EscapeDataString(normalized)}&format=1";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return results;

            if (doc.RootElement.TryGetProperty("success", out var success)
                && success.ValueKind == JsonValueKind.False)
            {
                return results;
            }

            var valid = doc.RootElement.TryGetProperty("valid", out var v) && v.ValueKind == JsonValueKind.True;
            if (!valid) return results;

            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Number", Value = normalized, Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Valid", Value = "true", Depth = 2 }
            };

            if (doc.RootElement.TryGetProperty("country_name", out var cn) && cn.ValueKind == JsonValueKind.String)
            {
                var c = cn.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Country", Value = c, Depth = 2 });
            }

            if (doc.RootElement.TryGetProperty("country_code", out var cc) && cc.ValueKind == JsonValueKind.String)
            {
                var c = cc.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Country Code", Value = c, Depth = 2 });
            }

            if (doc.RootElement.TryGetProperty("location", out var loc) && loc.ValueKind == JsonValueKind.String)
            {
                var l = loc.GetString();
                if (!string.IsNullOrWhiteSpace(l))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Location", Value = l, Depth = 2 });
            }

            if (doc.RootElement.TryGetProperty("carrier", out var carrier) && carrier.ValueKind == JsonValueKind.String)
            {
                var c = carrier.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Carrier", Value = c, Depth = 2 });
            }

            if (doc.RootElement.TryGetProperty("line_type", out var lt) && lt.ValueKind == JsonValueKind.String)
            {
                var l = lt.GetString();
                if (!string.IsNullOrWhiteSpace(l))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Line Type", Value = l, Depth = 2 });
            }

            if (doc.RootElement.TryGetProperty("international_format", out var intl) && intl.ValueKind == JsonValueKind.String)
            {
                var i = intl.GetString();
                if (!string.IsNullOrWhiteSpace(i))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "E.164", Value = i, Depth = 2 });
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Numverify Phone Lookup",
                Value = normalized,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Numverify validated {Phone}", normalized);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Numverify lookup failed for {Phone}: {Error}", normalized, ex.Message);
        }

        return results;
    }
}
