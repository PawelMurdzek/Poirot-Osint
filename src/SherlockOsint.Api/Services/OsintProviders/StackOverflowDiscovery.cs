using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Real StackOverflow discovery provider. Uses StackExchange API.
/// </summary>
public class StackOverflowDiscovery
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StackOverflowDiscovery> _logger;

    public StackOverflowDiscovery(IHttpClientFactory httpClientFactory, ILogger<StackOverflowDiscovery> logger)
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
            // StackExchange API - filter by inname (matches nickname)
            // No API key required for low volume requests
            var url = $"https://api.stackexchange.com/2.3/users?order=desc&sort=reputation&inname={Uri.EscapeDataString(nickname)}&site=stackoverflow";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);

            if (data.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var user in items.EnumerateArray().Take(3))
                {
                    var displayName = user.GetProperty("display_name").GetString();
                    
                    // Strong match for nickname
                    if (string.Equals(displayName, nickname, StringComparison.OrdinalIgnoreCase))
                    {
                        var profile = new VerifiedProfile
                        {
                            Url = user.GetProperty("link").GetString() ?? "",
                            Platform = "StackOverflow",
                            Username = nickname,
                            DisplayName = displayName,
                            ConfidenceScore = 0.50m, // Technical profiles are good indicators
                            Evidence = new List<string> { $"StackOverflow profile found matching nickname exactly" }
                        };

                        if (user.TryGetProperty("reputation", out var rep))
                        {
                            profile.Evidence.Add($"Reputation: {rep.GetInt32()}");
                        }

                        if (user.TryGetProperty("location", out var loc))
                        {
                            profile.Location = loc.GetString();
                        }

                        results.Add(profile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching StackOverflow for {Nickname}", nickname);
        }

        return results;
    }
}
