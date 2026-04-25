using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// 4chan archive search via FoolFuuka-based archives (desuarchive.org, archived.moe).
/// 4chan itself is anonymous; meaningful identity hits require a tripcode (e.g. "!abc123" / "!!verified").
/// We accept either a bare tripcode-style handle (containing "!" or "!!") or scan for any post matching
/// the username when used as a name. Most plain usernames will yield zero hits — that is expected.
///
/// Endpoint: https://{archive}/_/api/chan/search/?tripcode={tc} (FoolFuuka).
/// </summary>
public class FourChanArchiveLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FourChanArchiveLookup> _logger;
    private readonly List<string> _archives;

    public FourChanArchiveLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<FourChanArchiveLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;

        var configured = configuration.GetSection("Osint:FourChanArchives").Get<string[]>();
        _archives = configured is { Length: > 0 }
            ? configured.ToList()
            : new List<string> { "desuarchive.org", "archived.moe" };
    }

    public async Task<List<OsintNode>> SearchAsync(string handle, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(handle)) return results;

        // Only meaningful for tripcoded handles. A plain anonymous handle would match too many false positives.
        var looksLikeTripcode = handle.Contains('!');
        if (!looksLikeTripcode)
        {
            _logger.LogDebug("Skipping 4chan archive lookup for {Handle} — no tripcode marker", handle);
            return results;
        }

        var tasks = _archives.Select(archive => SearchOnArchiveAsync(archive, handle, ct)).ToList();
        var perArchive = await Task.WhenAll(tasks);
        foreach (var nodes in perArchive) results.AddRange(nodes);

        return results;
    }

    private async Task<List<OsintNode>> SearchOnArchiveAsync(string archive, string handle, CancellationToken ct)
    {
        var results = new List<OsintNode>();

        try
        {
            var url = $"https://{archive}/_/api/chan/search/?tripcode={Uri.EscapeDataString(handle)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            int hitCount = 0;
            if (doc.RootElement.TryGetProperty("0", out var zero) && zero.TryGetProperty("posts", out var posts) && posts.ValueKind == JsonValueKind.Array)
            {
                hitCount = posts.GetArrayLength();
            }
            else if (doc.RootElement.TryGetProperty("meta", out var meta) && meta.TryGetProperty("total_found", out var total) && total.ValueKind == JsonValueKind.Number)
            {
                hitCount = total.GetInt32();
            }

            if (hitCount == 0) return results;

            var profileUrl = $"https://{archive}/_/search/tripcode/{Uri.EscapeDataString(handle)}";
            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Tripcode", Value = handle, Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Archive", Value = archive, Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Hit count", Value = hitCount.ToString(), Depth = 2 }
            };

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"4chan Archive ({archive})",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found 4chan archive hits for {Handle} on {Archive}: {Count}", handle, archive, hitCount);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("4chan archive lookup failed for {Handle}@{Archive}: {Error}", handle, archive, ex.Message);
        }

        return results;
    }
}
