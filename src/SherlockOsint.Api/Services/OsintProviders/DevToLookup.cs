using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// DEV.to user lookup. Public REST API, no auth required.
/// Endpoint: https://dev.to/api/users/by_username?url={username}
/// </summary>
public class DevToLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DevToLookup> _logger;

    public DevToLookup(IHttpClientFactory httpClientFactory, ILogger<DevToLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(username)) return results;

        try
        {
            var url = $"https://dev.to/api/users/by_username?url={Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);
            if (data.RootElement.ValueKind != JsonValueKind.Object) return results;

            var profileUrl = $"https://dev.to/{username}";
            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 }
            };

            if (data.RootElement.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                var nameText = name.GetString();
                if (!string.IsNullOrWhiteSpace(nameText))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = nameText, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("summary", out var summary) && summary.ValueKind == JsonValueKind.String)
            {
                var summaryText = summary.GetString();
                if (!string.IsNullOrWhiteSpace(summaryText))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = summaryText, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("location", out var location) && location.ValueKind == JsonValueKind.String)
            {
                var loc = location.GetString();
                if (!string.IsNullOrWhiteSpace(loc))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Location", Value = loc, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("twitter_username", out var twitter) && twitter.ValueKind == JsonValueKind.String)
            {
                var t = twitter.GetString();
                if (!string.IsNullOrWhiteSpace(t))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "🐦 Twitter", Value = $"https://twitter.com/{t}", Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("github_username", out var github) && github.ValueKind == JsonValueKind.String)
            {
                var g = github.GetString();
                if (!string.IsNullOrWhiteSpace(g))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "🐙 GitHub", Value = $"https://github.com/{g}", Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("website_url", out var website) && website.ValueKind == JsonValueKind.String)
            {
                var w = website.GetString();
                if (!string.IsNullOrWhiteSpace(w))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "🌐 Website", Value = w, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("profile_image", out var avatar) && avatar.ValueKind == JsonValueKind.String)
            {
                var a = avatar.GetString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = a, Depth = 2 });
                }
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "DEV.to User",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found DEV.to user: {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("DEV.to lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
