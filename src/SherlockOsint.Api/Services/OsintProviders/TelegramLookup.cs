using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Telegram public channel/group/user lookup via the Bot API getChat method.
/// Skips silently if Osint:TelegramBotToken is missing.
/// API: https://api.telegram.org/bot{token}/getChat?chat_id=@{username}
/// </summary>
public class TelegramLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramLookup> _logger;
    private readonly string? _botToken;

    public TelegramLookup(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TelegramLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _botToken = configuration["Osint:TelegramBotToken"];
    }

    public async Task<List<OsintNode>> SearchAsync(string username, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        if (string.IsNullOrWhiteSpace(username)) return results;
        if (string.IsNullOrEmpty(_botToken))
        {
            _logger.LogInformation("Telegram lookup skipped — Osint:TelegramBotToken not configured");
            return results;
        }

        var handle = username.TrimStart('@');

        try
        {
            var url = $"https://api.telegram.org/bot{_botToken}/getChat?chat_id=@{Uri.EscapeDataString(handle)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Poirot-OSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Telegram getChat returned {Status} for {User}", response.StatusCode, handle);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True) return results;
            if (!doc.RootElement.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Object) return results;

            var chatType = result.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : "unknown";
            var profileUrl = $"https://t.me/{handle}";

            var children = new List<OsintNode>
            {
                new() { Id = Guid.NewGuid().ToString(), Label = "Username", Value = handle, Depth = 2 },
                new() { Id = Guid.NewGuid().ToString(), Label = "Type", Value = chatType ?? "unknown", Depth = 2 }
            };

            if (result.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                var titleText = title.GetString();
                if (!string.IsNullOrWhiteSpace(titleText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Title", Value = titleText, Depth = 2 });
            }

            if (result.TryGetProperty("first_name", out var fn) && fn.ValueKind == JsonValueKind.String)
            {
                var fnText = fn.GetString();
                if (!string.IsNullOrWhiteSpace(fnText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "First Name", Value = fnText, Depth = 2 });
            }

            if (result.TryGetProperty("last_name", out var ln) && ln.ValueKind == JsonValueKind.String)
            {
                var lnText = ln.GetString();
                if (!string.IsNullOrWhiteSpace(lnText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Last Name", Value = lnText, Depth = 2 });
            }

            if (result.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
            {
                var descText = desc.GetString();
                if (!string.IsNullOrWhiteSpace(descText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Description", Value = descText, Depth = 2 });
            }

            if (result.TryGetProperty("bio", out var bio) && bio.ValueKind == JsonValueKind.String)
            {
                var bioText = bio.GetString();
                if (!string.IsNullOrWhiteSpace(bioText))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "📝 Bio", Value = bioText, Depth = 2 });
            }

            if (result.TryGetProperty("invite_link", out var invite) && invite.ValueKind == JsonValueKind.String)
            {
                var i = invite.GetString();
                if (!string.IsNullOrWhiteSpace(i))
                    children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Invite Link", Value = i, Depth = 2 });
            }

            results.Add(new OsintNode
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"Telegram {chatType}",
                Value = profileUrl,
                Depth = 1,
                Children = children
            });

            _logger.LogInformation("Found Telegram {Type}: {Handle}", chatType, handle);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Telegram lookup failed for {Handle}: {Error}", handle, ex.Message);
        }

        return results;
    }
}
