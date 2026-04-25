using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// HackerRank user lookup via the public REST endpoint.
/// Endpoint: https://www.hackerrank.com/rest/hackers/{username}
/// No auth required. Returns 404 for nonexistent users.
/// </summary>
public class HackerRankLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HackerRankLookup> _logger;

    public HackerRankLookup(IHttpClientFactory httpClientFactory, ILogger<HackerRankLookup> logger)
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
            var url = $"https://www.hackerrank.com/rest/hackers/{Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            // HackerRank wraps the user in a `model` property
            JsonElement model;
            if (doc.RootElement.TryGetProperty("model", out var m) && m.ValueKind == JsonValueKind.Object)
                model = m;
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                model = doc.RootElement;
            else
                return results;

            var profileUrl = $"https://www.hackerrank.com/{username}";
            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 }
            };

            if (model.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
            {
                var nameText = name.GetString();
                if (!string.IsNullOrWhiteSpace(nameText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = nameText, Depth = 2 });
            }

            if (model.TryGetProperty("short_bio", out var bio) && bio.ValueKind == JsonValueKind.String)
            {
                var bioText = bio.GetString();
                if (!string.IsNullOrWhiteSpace(bioText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = bioText, Depth = 2 });
            }

            if (model.TryGetProperty("country", out var country) && country.ValueKind == JsonValueKind.String)
            {
                var c = country.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Country", Value = c, Depth = 2 });
            }

            if (model.TryGetProperty("school", out var school) && school.ValueKind == JsonValueKind.String)
            {
                var s = school.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "School", Value = s, Depth = 2 });
            }

            if (model.TryGetProperty("company", out var company) && company.ValueKind == JsonValueKind.String)
            {
                var co = company.GetString();
                if (!string.IsNullOrWhiteSpace(co))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "🏢 Company", Value = co, Depth = 2 });
            }

            if (model.TryGetProperty("level", out var level) && level.ValueKind == JsonValueKind.Number)
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Level", Value = level.GetInt32().ToString(), Depth = 2 });

            if (model.TryGetProperty("avatar", out var avatar) && avatar.ValueKind == JsonValueKind.String)
            {
                var a = avatar.GetString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    var fixedUrl = a.StartsWith("//") ? $"https:{a}" : a;
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = fixedUrl, Depth = 2 });
                }
            }

            if (model.TryGetProperty("created_at", out var created) && created.ValueKind == JsonValueKind.String)
            {
                var c = created.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account created", Value = c, Depth = 2 });
            }

            if (model.TryGetProperty("linkedin_url", out var li) && li.ValueKind == JsonValueKind.String)
            {
                var l = li.GetString();
                if (!string.IsNullOrWhiteSpace(l))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "💼 LinkedIn", Value = l, Depth = 2 });
            }

            if (model.TryGetProperty("github_url", out var gh) && gh.ValueKind == JsonValueKind.String)
            {
                var g = gh.GetString();
                if (!string.IsNullOrWhiteSpace(g))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "🐙 GitHub", Value = g, Depth = 2 });
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "HackerRank User",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found HackerRank user: {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("HackerRank lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
