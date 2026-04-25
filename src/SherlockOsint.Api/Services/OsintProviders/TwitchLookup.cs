using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Twitch (Helix) user lookup. Two-step auth: client_credentials grant -> app access token,
/// cached for ~60 days, then GET /helix/users?login={user}.
/// Skips silently if Osint:TwitchClientId or Osint:TwitchClientSecret is missing.
/// </summary>
public class TwitchLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TwitchLookup> _logger;
    private readonly string? _clientId;
    private readonly string? _clientSecret;

    private string? _appAccessToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public TwitchLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TwitchLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _clientId = configuration["Osint:TwitchClientId"];
        _clientSecret = configuration["Osint:TwitchClientSecret"];
    }

    public async Task<List<OsintNode>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(username)) return results;
        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
        {
            _logger.LogInformation("Twitch lookup skipped — Osint:TwitchClientId / TwitchClientSecret not configured");
            return results;
        }

        var token = await GetAppAccessTokenAsync(ct);
        if (string.IsNullOrEmpty(token)) return results;

        try
        {
            var url = $"https://api.twitch.tv/helix/users?login={Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Client-Id", _clientId);
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Twitch helix returned {Status} for {User}", response.StatusCode, username);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var dataArr) || dataArr.ValueKind != JsonValueKind.Array) return results;

            foreach (var user in dataArr.EnumerateArray())
            {
                var login = user.TryGetProperty("login", out var l) ? l.GetString() : null;
                if (string.IsNullOrEmpty(login)) continue;

                var profileUrl = $"https://twitch.tv/{login}";
                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = login, Depth = 2 }
                };

                if (user.TryGetProperty("display_name", out var dn) && dn.ValueKind == JsonValueKind.String)
                {
                    var dnText = dn.GetString();
                    if (!string.IsNullOrWhiteSpace(dnText))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = dnText, Depth = 2 });
                }

                if (user.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
                {
                    var descText = desc.GetString();
                    if (!string.IsNullOrWhiteSpace(descText))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = descText, Depth = 2 });
                }

                if (user.TryGetProperty("broadcaster_type", out var bt) && bt.ValueKind == JsonValueKind.String)
                {
                    var btText = bt.GetString();
                    if (!string.IsNullOrWhiteSpace(btText))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Broadcaster Type", Value = btText, Depth = 2 });
                }

                if (user.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String)
                {
                    var typeText = type.GetString();
                    if (!string.IsNullOrWhiteSpace(typeText))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account Type", Value = typeText, Depth = 2 });
                }

                if (user.TryGetProperty("view_count", out var vc) && vc.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "View Count", Value = vc.GetInt64().ToString(), Depth = 2 });
                }

                if (user.TryGetProperty("created_at", out var createdAt) && createdAt.ValueKind == JsonValueKind.String)
                {
                    var c = createdAt.GetString();
                    if (!string.IsNullOrWhiteSpace(c))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account created", Value = c, Depth = 2 });
                }

                if (user.TryGetProperty("profile_image_url", out var img) && img.ValueKind == JsonValueKind.String)
                {
                    var i = img.GetString();
                    if (!string.IsNullOrWhiteSpace(i))
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = i, Depth = 2 });
                }

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Twitch User",
                    Value = profileUrl,
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Found Twitch user: {Login}", login);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Twitch lookup failed for {Username}: {Error}", username, ex.Message);
        }

        return results;
    }

    private async Task<string?> GetAppAccessTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_appAccessToken) && DateTimeOffset.UtcNow < _tokenExpiry - TimeSpan.FromMinutes(5))
            return _appAccessToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_appAccessToken) && DateTimeOffset.UtcNow < _tokenExpiry - TimeSpan.FromMinutes(5))
                return _appAccessToken;

            var url = "https://id.twitch.tv/oauth2/token";
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _clientId!),
                new KeyValuePair<string, string>("client_secret", _clientSecret!),
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
            req.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var resp = await _httpClient.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twitch token endpoint returned {Status}", resp.StatusCode);
                return null;
            }

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("access_token", out var t) && t.ValueKind == JsonValueKind.String)
            {
                _appAccessToken = t.GetString();
                var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var e) && e.ValueKind == JsonValueKind.Number
                    ? e.GetInt64()
                    : 3600;
                _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
                return _appAccessToken;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to acquire Twitch app access token");
        }
        finally
        {
            _tokenLock.Release();
        }

        return null;
    }
}
