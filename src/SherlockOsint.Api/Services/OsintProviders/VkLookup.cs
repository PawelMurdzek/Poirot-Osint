using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// VK (vk.com) user lookup via the public users.get endpoint.
/// Skips silently if Osint:VkAccessToken is missing.
/// API: https://api.vk.com/method/users.get?user_ids={screen_name}&fields=...&access_token={tok}&v=5.131
/// </summary>
public class VkLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VkLookup> _logger;
    private readonly string? _accessToken;

    public VkLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<VkLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _accessToken = configuration["Osint:VkAccessToken"];
    }

    public async Task<List<OsintNode>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(username)) return results;
        if (string.IsNullOrEmpty(_accessToken))
        {
            _logger.LogInformation("VK lookup skipped — Osint:VkAccessToken not configured");
            return results;
        }

        try
        {
            var fields = "screen_name,sex,bdate,city,country,photo_max_orig,about,site,status,connections,occupation,verified";
            var url = $"https://api.vk.com/method/users.get?user_ids={Uri.EscapeDataString(username)}&fields={fields}&access_token={Uri.EscapeDataString(_accessToken)}&v=5.131";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                var msg = err.TryGetProperty("error_msg", out var m) ? m.GetString() : "unknown";
                _logger.LogDebug("VK API returned error for {User}: {Msg}", username, msg);
                return results;
            }

            if (!doc.RootElement.TryGetProperty("response", out var responseArr) || responseArr.ValueKind != JsonValueKind.Array) return results;

            foreach (var user in responseArr.EnumerateArray())
            {
                var id = user.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt64() : 0;
                if (id == 0) continue;

                var screenName = user.TryGetProperty("screen_name", out var sn) && sn.ValueKind == JsonValueKind.String ? sn.GetString() : null;
                var profileUrl = !string.IsNullOrEmpty(screenName) ? $"https://vk.com/{screenName}" : $"https://vk.com/id{id}";

                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "VK ID", Value = id.ToString(), Depth = 2 }
                };

                if (!string.IsNullOrEmpty(screenName))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Screen Name", Value = screenName, Depth = 2 });

                var firstName = user.TryGetProperty("first_name", out var fn) && fn.ValueKind == JsonValueKind.String ? fn.GetString() : "";
                var lastName = user.TryGetProperty("last_name", out var ln) && ln.ValueKind == JsonValueKind.String ? ln.GetString() : "";
                var fullName = $"{firstName} {lastName}".Trim();
                if (!string.IsNullOrWhiteSpace(fullName))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Full Name", Value = fullName, Depth = 2 });

                if (user.TryGetProperty("city", out var city) && city.ValueKind == JsonValueKind.Object)
                {
                    if (city.TryGetProperty("title", out var ct1) && ct1.ValueKind == JsonValueKind.String)
                    {
                        var c = ct1.GetString();
                        if (!string.IsNullOrWhiteSpace(c))
                            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "City", Value = c, Depth = 2 });
                    }
                }

                if (user.TryGetProperty("country", out var country) && country.ValueKind == JsonValueKind.Object)
                {
                    if (country.TryGetProperty("title", out var ct1) && ct1.ValueKind == JsonValueKind.String)
                    {
                        var c = ct1.GetString();
                        if (!string.IsNullOrWhiteSpace(c))
                            children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Country", Value = c, Depth = 2 });
                    }
                }

                if (user.TryGetProperty("bdate", out var bdate) && bdate.ValueKind == JsonValueKind.String)
                {
                    var b = bdate.GetString();
                    if (!string.IsNullOrWhiteSpace(b))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Birth Date", Value = b, Depth = 2 });
                }

                if (user.TryGetProperty("about", out var about) && about.ValueKind == JsonValueKind.String)
                {
                    var a = about.GetString();
                    if (!string.IsNullOrWhiteSpace(a))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 About", Value = a, Depth = 2 });
                }

                if (user.TryGetProperty("site", out var site) && site.ValueKind == JsonValueKind.String)
                {
                    var s = site.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "🌐 Website", Value = s, Depth = 2 });
                }

                if (user.TryGetProperty("photo_max_orig", out var photo) && photo.ValueKind == JsonValueKind.String)
                {
                    var p = photo.GetString();
                    if (!string.IsNullOrWhiteSpace(p))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = p, Depth = 2 });
                }

                if (user.TryGetProperty("verified", out var verified) && verified.ValueKind == JsonValueKind.Number && verified.GetInt32() == 1)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Verified", Value = "true", Depth = 2 });
                }

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "VK User",
                    Value = profileUrl,
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Found VK user: {ScreenName} (id={Id})", screenName ?? username, id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("VK lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
