using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Bilibili user search. No auth for basic public search; anti-bot is moderate.
/// Endpoint: https://api.bilibili.com/x/web-interface/search/type?search_type=bili_user&keyword={user}
/// </summary>
public class BilibiliLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BilibiliLookup> _logger;

    public BilibiliLookup(IHttpClientFactory httpClientFactory, ILogger<BilibiliLookup> logger)
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
            var url = $"https://api.bilibili.com/x/web-interface/search/type?search_type=bili_user&keyword={Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Referer", "https://search.bilibili.com/");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Bilibili search returned {Status} for {User}", response.StatusCode, username);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object) return results;
            if (!data.TryGetProperty("result", out var resultArr) || resultArr.ValueKind != JsonValueKind.Array) return results;

            // Take top 3 matches; Bilibili search is fuzzy
            foreach (var user in resultArr.EnumerateArray().Take(3))
            {
                var mid = user.TryGetProperty("mid", out var midProp) && midProp.ValueKind == JsonValueKind.Number
                    ? midProp.GetInt64()
                    : 0;
                if (mid == 0) continue;

                var profileUrl = $"https://space.bilibili.com/{mid}";
                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "UID", Value = mid.ToString(), Depth = 2 }
                };

                if (user.TryGetProperty("uname", out var uname) && uname.ValueKind == JsonValueKind.String)
                {
                    var u = uname.GetString();
                    if (!string.IsNullOrWhiteSpace(u))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = u, Depth = 2 });
                }

                if (user.TryGetProperty("usign", out var sign) && sign.ValueKind == JsonValueKind.String)
                {
                    var s = sign.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Sign", Value = s, Depth = 2 });
                }

                if (user.TryGetProperty("level", out var level) && level.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Level", Value = level.GetInt32().ToString(), Depth = 2 });
                }

                if (user.TryGetProperty("gender", out var gender) && gender.ValueKind == JsonValueKind.Number)
                {
                    var g = gender.GetInt32() switch { 1 => "Male", 2 => "Female", _ => "Unknown" };
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Gender", Value = g, Depth = 2 });
                }

                if (user.TryGetProperty("fans", out var fans) && fans.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Fans", Value = fans.GetInt64().ToString(), Depth = 2 });
                }

                if (user.TryGetProperty("videos", out var videos) && videos.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Videos", Value = videos.GetInt32().ToString(), Depth = 2 });
                }

                if (user.TryGetProperty("upic", out var upic) && upic.ValueKind == JsonValueKind.String)
                {
                    var p = upic.GetString();
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        var fixedUrl = p.StartsWith("//") ? $"https:{p}" : p;
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = fixedUrl, Depth = 2 });
                    }
                }

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Bilibili User",
                    Value = profileUrl,
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Found Bilibili user: {Mid}", mid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Bilibili lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
