using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Wayback Machine lookup - find archived/deleted profiles
/// Uses archive.org CDX API (no key required)
/// </summary>
public class WaybackMachineLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WaybackMachineLookup> _logger;

    public WaybackMachineLookup(IHttpClientFactory httpClientFactory, ILogger<WaybackMachineLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    /// <summary>
    /// Search Wayback Machine for archived profiles
    /// </summary>
    public async Task<List<OsintNode>> SearchProfileArchivesAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(username))
            return results;

        // Platforms to check for archived profiles
        var profileUrls = new[]
        {
            $"https://twitter.com/{username}",
            $"https://github.com/{username}",
            $"https://instagram.com/{username}",
            $"https://facebook.com/{username}",
            $"https://linkedin.com/in/{username}"
        };

        foreach (var profileUrl in profileUrls)
        {
            try
            {
                var archiveResult = await CheckArchiveAsync(profileUrl, ct);
                if (archiveResult != null)
                {
                    results.Add(archiveResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Wayback check failed for {Url}: {Error}", profileUrl, ex.Message);
            }
        }

        return results;
    }

    /// <summary>
    /// Check if a specific URL has archives
    /// </summary>
    public async Task<OsintNode?> CheckArchiveAsync(string url, CancellationToken ct = default)
    {
        try
        {
            // CDX API to check available snapshots
            var cdxUrl = $"https://web.archive.org/cdx/search/cdx?url={Uri.EscapeDataString(url)}&output=json&limit=5";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, cdxUrl);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonSerializer.Deserialize<List<List<string>>>(json);

            if (data == null || data.Count <= 1) // First row is header
                return null;

            // Parse snapshots (skip header row)
            var snapshots = data.Skip(1).ToList();
            if (snapshots.Count == 0)
                return null;

            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Snapshots Found", Value = snapshots.Count.ToString(), Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Discovery", Value = "Profile has been archived (may be deleted now)", Depth = 2 }
            };

            // Get oldest and newest snapshot
            var oldest = snapshots.First();
            var newest = snapshots.Last();

            if (oldest.Count >= 2)
            {
                var timestamp = oldest[1];
                var date = ParseWaybackTimestamp(timestamp);
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "First Archived",
                    Value = date,
                    Depth = 2
                });

                // Add link to oldest snapshot
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "View Archive",
                    Value = $"https://web.archive.org/web/{timestamp}/{url}",
                    Depth = 2
                });
            }

            if (newest.Count >= 2 && newest != oldest)
            {
                var timestamp = newest[1];
                var date = ParseWaybackTimestamp(timestamp);
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Last Archived",
                    Value = date,
                    Depth = 2
                });
            }

            var platform = ExtractPlatform(url);
            
            _logger.LogInformation("Found {Count} Wayback snapshots for {Url}", snapshots.Count, url);

            return new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"[ARCHIVE] {platform} History",
                Value = $"https://web.archive.org/web/*/{url}",
                Depth = 1,
                Children = children
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Wayback Machine lookup failed: {Error}", ex.Message);
            return null;
        }
    }

    private static string ParseWaybackTimestamp(string timestamp)
    {
        // Format: 20230415123456
        if (timestamp.Length >= 8)
        {
            var year = timestamp.Substring(0, 4);
            var month = timestamp.Substring(4, 2);
            var day = timestamp.Substring(6, 2);
            return $"{year}-{month}-{day}";
        }
        return timestamp;
    }

    private static string ExtractPlatform(string url)
    {
        if (url.Contains("twitter.com")) return "Twitter";
        if (url.Contains("github.com")) return "GitHub";
        if (url.Contains("instagram.com")) return "Instagram";
        if (url.Contains("facebook.com")) return "Facebook";
        if (url.Contains("linkedin.com")) return "LinkedIn";
        return "Profile";
    }
}
