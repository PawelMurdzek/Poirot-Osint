using SherlockOsint.Shared.Models;
using SherlockOsint.Api.Services;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// ORCID public search — given-names + family-name → ORCID iD + employment chain.
/// Free public API, no key required, JSON via Accept: application/json.
/// Single highest-leverage academic identity signal: an ORCID match collapses
/// dozens of fuzzy permutator candidates into one canonical researcher record.
/// </summary>
public class OrcidLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrcidLookup> _logger;

    private const string ExpandedSearchUrl = "https://pub.orcid.org/v3.0/expanded-search/";

    public OrcidLookup(IHttpClientFactory httpClientFactory, ILogger<OrcidLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string? fullName, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(fullName)) return results;

        var (first, last) = SplitName(fullName);
        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last)) return results;

        // ORCID's Solr-backed expanded-search stores `family-names` in the form the
        // researcher registered — for non-Anglo names that's almost always ASCII
        // (diacritic variants live in `other-name` which the filter doesn't search).
        // Try the input form first, then fall back to ASCII-stripped if zero hits,
        // so a query "Gradoń" still finds an ORCID record stored as "Gradon".
        var firstAscii = TextNormalization.StripDiacritics(first);
        var lastAscii = TextNormalization.StripDiacritics(last);

        await TryQueryAsync(first, last, results, fullName!, ct);
        if (results.Count == 0 && (!string.Equals(first, firstAscii) || !string.Equals(last, lastAscii)))
        {
            await TryQueryAsync(firstAscii, lastAscii, results, fullName!, ct);
        }

        return results;
    }

    private async Task TryQueryAsync(string first, string last, List<OsintNode> results, string originalName, CancellationToken ct)
    {
        try
        {
            var query = $"given-names:{first} AND family-name:{last}";
            var url = $"{ExpandedSearchUrl}?q={Uri.EscapeDataString(query)}&rows=5";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("ORCID returned {Status} for {Name}", response.StatusCode, originalName);
                return;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);

            if (!data.RootElement.TryGetProperty("expanded-result", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return;

            foreach (var entry in arr.EnumerateArray().Take(5))
            {
                var orcidId = entry.TryGetProperty("orcid-id", out var oid) ? oid.GetString() : null;
                if (string.IsNullOrEmpty(orcidId)) continue;

                var givenNames = entry.TryGetProperty("given-names", out var gn) ? gn.GetString() : null;
                var familyNames = entry.TryGetProperty("family-names", out var fn) ? fn.GetString() : null;
                var creditName = entry.TryGetProperty("credit-name", out var cn) ? cn.GetString() : null;
                var displayName = !string.IsNullOrEmpty(creditName)
                    ? creditName
                    : $"{givenNames} {familyNames}".Trim();

                var profileUrl = $"https://orcid.org/{orcidId}";
                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = orcidId!, Depth = 2 },
                };

                if (!string.IsNullOrEmpty(displayName))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Name", Value = displayName, Depth = 2 });

                if (entry.TryGetProperty("institution-name", out var inst) && inst.ValueKind == JsonValueKind.Array)
                {
                    var institutions = inst.EnumerateArray()
                        .Select(i => i.GetString())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .ToList();
                    if (institutions.Count > 0)
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Affiliation",
                            Value = string.Join("; ", institutions!),
                            Depth = 2
                        });
                    }
                }

                if (entry.TryGetProperty("email", out var emails) && emails.ValueKind == JsonValueKind.Array)
                {
                    foreach (var e in emails.EnumerateArray())
                    {
                        var emailVal = e.GetString();
                        if (!string.IsNullOrWhiteSpace(emailVal))
                        {
                            children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Email",
                                Value = emailVal,
                                Depth = 2
                            });
                        }
                    }
                }

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[OR] ORCID",
                    Value = profileUrl,
                    Depth = 1,
                    Children = children
                });
            }

            _logger.LogInformation("ORCID found {Count} records for {Name} (query: given={First}, family={Last})",
                results.Count, originalName, first, last);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("ORCID lookup failed for {Name}: {Error}", originalName, ex.Message);
        }
    }

    private static (string first, string last) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return ("", "");
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return (parts[0], "");
        return (parts[0], parts[^1]);
    }
}
