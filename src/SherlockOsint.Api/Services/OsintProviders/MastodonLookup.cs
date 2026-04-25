using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Mastodon (and Mastodon-fork: Truth Social, Gab, Pleroma, Akkoma) account lookup.
/// Per-instance REST API: GET /api/v1/accounts/lookup?acct={user}. No auth for read.
/// Default instances cover en + jp + popular forks. Configurable via Osint:MastodonInstances.
/// </summary>
public class MastodonLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MastodonLookup> _logger;
    private readonly List<string> _instances;

    public MastodonLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<MastodonLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;

        var configured = configuration.GetSection("Osint:MastodonInstances").Get<string[]>();
        _instances = configured is { Length: > 0 }
            ? configured.ToList()
            : new List<string>
            {
                "mastodon.social",
                "fosstodon.org",
                "mstdn.social",
                "mstdn.jp",
                "pawoo.net"
            };
    }

    public async Task<List<OsintNode>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(username)) return results;

        var tasks = _instances.Select(instance => LookupOnInstanceAsync(instance, username, ct)).ToList();
        var perInstanceResults = await Task.WhenAll(tasks);
        foreach (var nodes in perInstanceResults) results.AddRange(nodes);

        return results;
    }

    private async Task<List<OsintNode>> LookupOnInstanceAsync(string instance, string username, CancellationToken ct)
    {
        var results = new List<OsintNode>();

        try
        {
            var url = $"https://{instance}/api/v1/accounts/lookup?acct={Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);
            if (data.RootElement.ValueKind != JsonValueKind.Object) return results;

            var profileUrl = data.RootElement.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String
                ? urlProp.GetString() ?? $"https://{instance}/@{username}"
                : $"https://{instance}/@{username}";

            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Instance", Value = instance, Depth = 2 }
            };

            if (data.RootElement.TryGetProperty("display_name", out var dn) && dn.ValueKind == JsonValueKind.String)
            {
                var dnText = dn.GetString();
                if (!string.IsNullOrWhiteSpace(dnText))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = dnText, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("note", out var note) && note.ValueKind == JsonValueKind.String)
            {
                var noteText = note.GetString();
                if (!string.IsNullOrWhiteSpace(noteText))
                {
                    // The note often contains HTML — clip to a reasonable preview
                    var clipped = noteText.Length > 400 ? noteText[..400] + "…" : noteText;
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = clipped, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("followers_count", out var fc) && fc.ValueKind == JsonValueKind.Number)
            {
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Followers", Value = fc.GetInt32().ToString(), Depth = 2 });
            }

            if (data.RootElement.TryGetProperty("statuses_count", out var sc) && sc.ValueKind == JsonValueKind.Number)
            {
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Posts", Value = sc.GetInt32().ToString(), Depth = 2 });
            }

            if (data.RootElement.TryGetProperty("avatar", out var avatar) && avatar.ValueKind == JsonValueKind.String)
            {
                var a = avatar.GetString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = a, Depth = 2 });
                }
            }

            if (data.RootElement.TryGetProperty("created_at", out var createdAt) && createdAt.ValueKind == JsonValueKind.String)
            {
                var c = createdAt.GetString();
                if (!string.IsNullOrWhiteSpace(c))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account created", Value = c, Depth = 2 });
                }
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"Mastodon User ({instance})",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found Mastodon user: {Username}@{Instance}", username, instance);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Mastodon lookup failed for {Username}@{Instance}: {Error}", username, instance, ex.Message);
        }

        return results;
    }
}
