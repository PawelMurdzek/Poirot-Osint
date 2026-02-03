using SherlockOsint.Shared.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Real Reddit discovery provider. Uses Reddit public .json endpoints.
/// </summary>
public class RedditDiscovery
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RedditDiscovery> _logger;

    public RedditDiscovery(IHttpClientFactory httpClientFactory, ILogger<RedditDiscovery> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<VerifiedProfile>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<VerifiedProfile>();
        if (string.IsNullOrWhiteSpace(username)) return results;

        try
        {
            // Reddit allows appending .json to user profiles
            var url = $"https://www.reddit.com/user/{username}/about.json";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0 (Real Identity Verification)");
            
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);

            if (data.RootElement.TryGetProperty("data", out var dataProp))
            {
                var redditName = dataProp.GetProperty("name").GetString();
                if (string.Equals(redditName, username, StringComparison.OrdinalIgnoreCase))
                {
                    var profile = new VerifiedProfile
                    {
                        Url = $"https://www.reddit.com/user/{username}",
                        Platform = "Reddit",
                        Username = username,
                        ConfidenceScore = 0.45m, // Reddit usernames are often unique but can be recycled
                        Evidence = new List<string> { $"Reddit profile confirmed via official API endpoint" }
                    };

                    if (dataProp.TryGetProperty("total_karma", out var karma))
                    {
                        profile.Evidence.Add($"Total Karma: {karma.GetInt32()}");
                    }

                    if (dataProp.TryGetProperty("created_utc", out var created))
                    {
                        var createdDate = DateTimeOffset.FromUnixTimeSeconds((long)created.GetDouble()).UtcDateTime;
                        profile.Evidence.Add($"Account created: {createdDate:yyyy-MM-dd}");
                    }

                    results.Add(profile);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Reddit for {Username}", username);
        }

        return results;
    }
}
