using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Lemmy (federated Reddit-alternative on ActivityPub) user lookup.
/// Per-instance HTTP API: GET /api/v3/user?username={user}. No auth for read.
/// Default instances: lemmy.world, lemmy.ml, sh.itjust.works.
/// </summary>
public class LemmyLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LemmyLookup> _logger;
    private readonly List<string> _instances;

    public LemmyLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LemmyLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;

        var configured = configuration.GetSection("Osint:LemmyInstances").Get<string[]>();
        _instances = configured is { Length: > 0 }
            ? configured.ToList()
            : new List<string> { "lemmy.world", "lemmy.ml", "sh.itjust.works" };
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
            var url = $"https://{instance}/api/v3/user?username={Uri.EscapeDataString(username)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var data = JsonDocument.Parse(json);
            if (data.RootElement.ValueKind != JsonValueKind.Object) return results;

            if (!data.RootElement.TryGetProperty("person_view", out var personView)) return results;
            if (!personView.TryGetProperty("person", out var person)) return results;

            var profileUrl = person.TryGetProperty("actor_id", out var actorId) && actorId.ValueKind == JsonValueKind.String
                ? actorId.GetString() ?? $"https://{instance}/u/{username}"
                : $"https://{instance}/u/{username}";

            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = username, Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Instance", Value = instance, Depth = 2 }
            };

            if (person.TryGetProperty("display_name", out var dn) && dn.ValueKind == JsonValueKind.String)
            {
                var dnText = dn.GetString();
                if (!string.IsNullOrWhiteSpace(dnText))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = dnText, Depth = 2 });
                }
            }

            if (person.TryGetProperty("bio", out var bio) && bio.ValueKind == JsonValueKind.String)
            {
                var bioText = bio.GetString();
                if (!string.IsNullOrWhiteSpace(bioText))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = bioText, Depth = 2 });
                }
            }

            if (person.TryGetProperty("avatar", out var avatar) && avatar.ValueKind == JsonValueKind.String)
            {
                var a = avatar.GetString();
                if (!string.IsNullOrWhiteSpace(a))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = a, Depth = 2 });
                }
            }

            if (person.TryGetProperty("published", out var published) && published.ValueKind == JsonValueKind.String)
            {
                var p = published.GetString();
                if (!string.IsNullOrWhiteSpace(p))
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Account created", Value = p, Depth = 2 });
                }
            }

            if (personView.TryGetProperty("counts", out var counts) && counts.ValueKind == JsonValueKind.Object)
            {
                if (counts.TryGetProperty("post_count", out var postCount) && postCount.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Posts", Value = postCount.GetInt32().ToString(), Depth = 2 });
                }
                if (counts.TryGetProperty("comment_count", out var commentCount) && commentCount.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Comments", Value = commentCount.GetInt32().ToString(), Depth = 2 });
                }
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"Lemmy User ({instance})",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found Lemmy user: {Username}@{Instance}", username, instance);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Lemmy lookup failed for {Username}@{Instance}: {Error}", username, instance, ex.Message);
        }

        return results;
    }
}
