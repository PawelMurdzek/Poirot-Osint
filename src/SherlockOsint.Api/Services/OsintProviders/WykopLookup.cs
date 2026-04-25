using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Wykop (PL) profile lookup. Uses the public Wykop v3 API.
/// Endpoint: https://wykop.pl/api/v3/profile/users/{username}
/// Public profile data is returned without auth; some response shapes wrap content in `data`.
/// </summary>
public class WykopLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WykopLookup> _logger;

    public WykopLookup(IHttpClientFactory httpClientFactory, ILogger<WykopLookup> logger)
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
            var url = $"https://wykop.pl/api/v3/profile/users/{Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            // v3 commonly wraps content in { "data": {...} }
            JsonElement profile;
            if (doc.RootElement.TryGetProperty("data", out var dataWrap) && dataWrap.ValueKind == JsonValueKind.Object)
            {
                profile = dataWrap;
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                profile = doc.RootElement;
            }
            else
            {
                return results;
            }

            var profileUrl = $"https://wykop.pl/ludzie/{username}";
            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 }
            };

            if (profile.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.String)
            {
                var s = status.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Status", Value = s, Depth = 2 });
                }
            }

            if (profile.TryGetProperty("gender", out var gender) && gender.ValueKind == JsonValueKind.String)
            {
                var g = gender.GetString();
                if (!string.IsNullOrWhiteSpace(g))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Gender", Value = g, Depth = 2 });
                }
            }

            if (profile.TryGetProperty("city", out var city) && city.ValueKind == JsonValueKind.String)
            {
                var c = city.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "City", Value = c, Depth = 2 });
                }
            }

            if (profile.TryGetProperty("about", out var about) && about.ValueKind == JsonValueKind.String)
            {
                var a = about.GetString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    var clipped = a.Length > 400 ? a[..400] + "…" : a;
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 About", Value = clipped, Depth = 2 });
                }
            }

            if (profile.TryGetProperty("created_at", out var created) && created.ValueKind == JsonValueKind.String)
            {
                var c = created.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account created", Value = c, Depth = 2 });
                }
            }

            if (profile.TryGetProperty("avatar", out var avatar) && avatar.ValueKind == JsonValueKind.String)
            {
                var a = avatar.GetString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = a, Depth = 2 });
                }
            }

            if (profile.TryGetProperty("avatars", out var avatars) && avatars.ValueKind == JsonValueKind.Object)
            {
                if (avatars.TryGetProperty("medium", out var med) && med.ValueKind == JsonValueKind.String)
                {
                    var a = med.GetString();
                    if (!string.IsNullOrWhiteSpace(a))
                    {
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = a, Depth = 2 });
                    }
                }
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Wykop User",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found Wykop user: {Username}", username);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Wykop lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }
}
