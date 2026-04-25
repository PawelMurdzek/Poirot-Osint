using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// HackerNews user lookup via the public Firebase REST API.
/// No auth required. Endpoint: https://hacker-news.firebaseio.com/v0/user/{id}.json
/// </summary>
public class HackerNewsLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerNewsLookup> _logger;

    public HackerNewsLookup(IHttpClientFactory httpClientFactory, ILogger<HackerNewsLookup> logger)
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
            var url = $"https://hacker-news.firebaseio.com/v0/user/{Uri.EscapeDataString(username)}.json";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json) || json == "null") return results;

            using var data = JsonDocument.Parse(json);
            if (data.RootElement.ValueKind != JsonValueKind.Object) return results;

            var profileUrl = $"https://news.ycombinator.com/user?id={username}";
            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 }
            };

            if (data.RootElement.TryGetProperty("karma", out var karma) && karma.ValueKind == JsonValueKind.Number)
            {
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Karma", Value = karma.GetInt32().ToString(), Depth = 2 });
            }

            if (data.RootElement.TryGetProperty("created", out var created) && created.ValueKind == JsonValueKind.Number)
            {
                var createdDate = DateTimeOffset.FromUnixTimeSeconds(created.GetInt64()).UtcDateTime;
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account created", Value = createdDate.ToString("yyyy-MM-dd"), Depth = 2 });
            }

            if (data.RootElement.TryGetProperty("about", out var about) && about.ValueKind == JsonValueKind.String)
            {
                var aboutText = about.GetString();
                if (!string.IsNullOrWhiteSpace(aboutText))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 About", Value = aboutText, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("submitted", out var submitted) && submitted.ValueKind == JsonValueKind.Array)
            {
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Submitted items", Value = submitted.GetArrayLength().ToString(), Depth = 2 });
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "HackerNews User",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found HackerNews user: {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("HackerNews lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
