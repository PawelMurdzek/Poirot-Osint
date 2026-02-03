using SherlockOsint.Shared.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Real YouTube discovery provider. Uses YouTube Data API or public channel pages.
/// </summary>
public class YouTubeDiscovery
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YouTubeDiscovery> _logger;

    public YouTubeDiscovery(IHttpClientFactory httpClientFactory, ILogger<YouTubeDiscovery> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<VerifiedProfile>> SearchAsync(string nickname, CancellationToken ct = default)
    {
        var results = new List<VerifiedProfile>();
        if (string.IsNullOrWhiteSpace(nickname)) return results;

        try
        {
            // Note: YouTube handles are prefixed with @
            var url = $"https://www.youtube.com/@{nickname}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var html = await response.Content.ReadAsStringAsync(ct);

            // STRICTOR VALIDATION for YouTube
            // Check for channel ID and actual presence of the handle
            if (!html.Contains("\"channelId\"", StringComparison.OrdinalIgnoreCase)) return results;
            if (!html.Contains($"@{nickname}", StringComparison.OrdinalIgnoreCase)) return results;

            var profile = new VerifiedProfile
            {
                Url = url,
                Platform = "YouTube",
                Username = nickname,
                ConfidenceScore = 0.40m, // Base confidence for YouTube (can be high overlap)
                Evidence = new List<string> { $"YouTube handle @{nickname} verified on channel page" }
            };

            // Extract display name
            var nameMatch = Regex.Match(html, @"\""name\""\s*:\s*\""([^\""]+)\""");
            if (nameMatch.Success)
            {
                profile.DisplayName = nameMatch.Groups[1].Value;
                profile.Evidence.Add($"Display name: {profile.DisplayName}");
            }

            // Extract Bio/Description
            var bioMatch = Regex.Match(html, @"\""description\""\s*:\s*\""([^\""]+)\""");
            if (bioMatch.Success)
            {
                profile.Bio = Regex.Unescape(bioMatch.Groups[1].Value);
            }

            results.Add(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching YouTube for {Nickname}", nickname);
        }

        return results;
    }
}
