using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// OpenAlex author search — fullName → author profile + ORCID + last-known institution + works/cites counts.
/// Free public API, no key required, ≤10 req/s in the polite pool (we add `mailto=` to land there).
/// Functional substitute for Google Scholar (which has no official API and requires paid SerpAPI).
/// </summary>
public class OpenAlexLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAlexLookup> _logger;

    private const string SearchUrl = "https://api.openalex.org/authors";
    private const string PoliteMailto = "poirot-osint@example.org";

    public OpenAlexLookup(IHttpClientFactory httpClientFactory, ILogger<OpenAlexLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string? fullName, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(fullName)) return results;

        await TryQueryAsync(fullName.Trim(), results, ct);

        // OpenAlex `?search=` is supposed to be unicode-clean, but for non-Anglo
        // names (Polish ł/ń/ż etc.) we still see records under the ASCII form
        // because that's how authors register. Retry diacritic-stripped if zero.
        var ascii = TextNormalization.StripDiacritics(fullName.Trim());
        if (results.Count == 0 && !string.Equals(ascii, fullName.Trim(), StringComparison.Ordinal))
        {
            await TryQueryAsync(ascii, results, ct);
        }

        return results;
    }

    private async Task TryQueryAsync(string fullName, List<OsintNode> results, CancellationToken ct)
    {
        try
        {
            var url = $"{SearchUrl}?search={Uri.EscapeDataString(fullName)}&per-page=5&mailto={PoliteMailto}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("OpenAlex returned {Status} for {Name}", response.StatusCode, fullName);
                return;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);

            if (!data.RootElement.TryGetProperty("results", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return;

            foreach (var author in arr.EnumerateArray().Take(5))
            {
                var authorId = author.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                if (string.IsNullOrEmpty(authorId)) continue;

                var displayName = author.TryGetProperty("display_name", out var dn) ? dn.GetString() : null;
                var orcid = author.TryGetProperty("orcid", out var or) ? or.GetString() : null;
                var worksCount = author.TryGetProperty("works_count", out var wc) ? wc.GetInt32() : 0;
                var citedBy = author.TryGetProperty("cited_by_count", out var cb) ? cb.GetInt32() : 0;

                // Skip authors with zero works — almost certainly stub records, not real researchers
                if (worksCount == 0) continue;

                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = ExtractOpenAlexShortId(authorId!), Depth = 2 },
                };

                if (!string.IsNullOrEmpty(displayName))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Name", Value = displayName, Depth = 2 });

                if (!string.IsNullOrEmpty(orcid))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "ORCID", Value = orcid, Depth = 2 });

                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Works",
                    Value = $"{worksCount} works, cited {citedBy} times",
                    Depth = 2
                });

                if (author.TryGetProperty("last_known_institutions", out var insts) && insts.ValueKind == JsonValueKind.Array)
                {
                    var institutions = new List<string>();
                    foreach (var inst in insts.EnumerateArray())
                    {
                        var name = inst.TryGetProperty("display_name", out var n) ? n.GetString() : null;
                        var country = inst.TryGetProperty("country_code", out var c) ? c.GetString() : null;
                        if (!string.IsNullOrWhiteSpace(name))
                            institutions.Add(string.IsNullOrEmpty(country) ? name! : $"{name} ({country})");
                    }
                    if (institutions.Count > 0)
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Affiliation",
                            Value = string.Join("; ", institutions),
                            Depth = 2
                        });
                        // Country code on the first institution gets reused as Location signal
                        // for downstream candidate-level location merging in CandidateAggregator.
                        var firstCountry = insts.EnumerateArray().FirstOrDefault();
                        if (firstCountry.ValueKind == JsonValueKind.Object &&
                            firstCountry.TryGetProperty("display_name", out var fcName))
                        {
                            children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Location",
                                Value = fcName.GetString() ?? "",
                                Depth = 2
                            });
                        }
                    }
                }

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[OA] OpenAlex",
                    Value = authorId!,
                    Depth = 1,
                    Children = children
                });
            }

            _logger.LogInformation("OpenAlex found {Count} authors for {Name}", results.Count, fullName);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("OpenAlex lookup failed for {Name}: {Error}", fullName, ex.Message);
        }
    }

    private static string ExtractOpenAlexShortId(string fullId)
    {
        // "https://openalex.org/A5012345678" → "A5012345678"
        var lastSlash = fullId.LastIndexOf('/');
        return lastSlash >= 0 && lastSlash < fullId.Length - 1 ? fullId[(lastSlash + 1)..] : fullId;
    }
}
