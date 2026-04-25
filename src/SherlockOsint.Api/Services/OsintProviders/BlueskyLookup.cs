using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Bluesky (AT Protocol) profile lookup. No auth required for public reads.
/// Endpoint: https://public.api.bsky.app/xrpc/app.bsky.actor.getProfile?actor={handle}
/// </summary>
public class BlueskyLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlueskyLookup> _logger;

    public BlueskyLookup(IHttpClientFactory httpClientFactory, ILogger<BlueskyLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> SearchAsync(string handle, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(handle)) return results;

        // Bluesky handles look like "alice.bsky.social". Try the bare handle first;
        // if it has no dot, also try appending ".bsky.social" as a sensible default.
        var candidates = new List<string> { handle };
        if (!handle.Contains('.'))
        {
            candidates.Add($"{handle}.bsky.social");
        }

        foreach (var actor in candidates)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                var url = $"https://public.api.bsky.app/xrpc/app.bsky.actor.getProfile?actor={Uri.EscapeDataString(actor)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");
                request.Headers.Add("Accept", "application/json");

                var response = await _httpClient.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode) continue;

                var json = await response.Content.ReadAsStringAsync(ct);
                using var data = JsonDocument.Parse(json);
                if (data.RootElement.ValueKind != JsonValueKind.Object) continue;

                var resolvedHandle = data.RootElement.TryGetProperty("handle", out var h) && h.ValueKind == JsonValueKind.String
                    ? h.GetString() ?? actor
                    : actor;

                var profileUrl = $"https://bsky.app/profile/{resolvedHandle}";
                var children = new List<OsintNode>
                {
                    new() { Id = Guid.NewGuid().ToString(), Label = "Handle", Value = resolvedHandle, Depth = 2 }
                };

                if (data.RootElement.TryGetProperty("displayName", out var dn) && dn.ValueKind == JsonValueKind.String)
                {
                    var dnText = dn.GetString();
                    if (!string.IsNullOrWhiteSpace(dnText))
                    {
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Display Name", Value = dnText, Depth = 2 });
                    }
                }

                if (data.RootElement.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
                {
                    var descText = desc.GetString();
                    if (!string.IsNullOrWhiteSpace(descText))
                    {
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = descText, Depth = 2 });
                    }
                }

                if (data.RootElement.TryGetProperty("followersCount", out var fc) && fc.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Followers", Value = fc.GetInt32().ToString(), Depth = 2 });
                }

                if (data.RootElement.TryGetProperty("postsCount", out var pc) && pc.ValueKind == JsonValueKind.Number)
                {
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Posts", Value = pc.GetInt32().ToString(), Depth = 2 });
                }

                if (data.RootElement.TryGetProperty("avatar", out var avatar) && avatar.ValueKind == JsonValueKind.String)
                {
                    var a = avatar.GetString();
                    if (!string.IsNullOrWhiteSpace(a))
                    {
                        children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📷 Profile Photo", Value = a, Depth = 2 });
                    }
                }

                if (data.RootElement.TryGetProperty("createdAt", out var createdAt) && createdAt.ValueKind == JsonValueKind.String)
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
                    Label = "Bluesky User",
                    Value = profileUrl,
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Found Bluesky user: {Handle}", resolvedHandle);
                break; // first hit wins; don't double-report
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Bluesky lookup failed for {Actor}: {Error}", actor, ex.Message);
            }
        }

        return results;
    }
}
