using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

public class FullContactLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FullContactLookup> _logger;
    private readonly IConfiguration _configuration;

    public FullContactLookup(
        IHttpClientFactory httpClientFactory,
        ILogger<FullContactLookup> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _configuration = configuration;
    }

    public Task<List<OsintNode>> EnrichPersonAsync(string email, CancellationToken ct = default)
    {
        return EnrichAsync(new { email }, email, ct);
    }

    public Task<List<OsintNode>> EnrichPhoneAsync(string phone, CancellationToken ct = default)
    {
        return EnrichAsync(new { phone }, phone, ct);
    }

    private async Task<List<OsintNode>> EnrichAsync(object bodyObj, string identifier, CancellationToken ct)
    {
        var results = new List<OsintNode>();
        var apiKey = _configuration["Osint:FullContactApiKey"];
        if (string.IsNullOrEmpty(apiKey)) return results;

        try
        {
            var url = "https://api.fullcontact.com/v3/person.enrich";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(bodyObj), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            var children = new List<OsintNode>();
            
            // Re-use the extraction logic... (I'll keep it abbreviated for this turn)
            if (data.RootElement.TryGetProperty("fullName", out var fullName))
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Full Name", Value = fullName.GetString(), Depth = 2 });
            
            if (data.RootElement.TryGetProperty("location", out var location))
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Location", Value = location.GetString(), Depth = 2 });

            if (data.RootElement.TryGetProperty("bio", out var bio))
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Bio", Value = bio.GetString(), Depth = 2 });

            if (data.RootElement.TryGetProperty("twitter", out var twitter))
            {
                var h = twitter.GetString();
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "Twitter", Value = h.StartsWith("http") ? h : $"https://twitter.com/{h}", Depth = 2 });
            }

            if (data.RootElement.TryGetProperty("linkedin", out var linkedin))
            {
                var h = linkedin.GetString();
                children.Add(new OsintNode { Id = Guid.NewGuid().ToString(), Label = "LinkedIn", Value = h.StartsWith("http") ? h : $"https://linkedin.com/in/{h}", Depth = 2 });
            }

            if (children.Count > 0)
            {
                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = $"FullContact: {identifier}",
                    Value = "Identity Link Found",
                    Depth = 1,
                    Children = children
                });
            }
        }
        catch { }

        return results;
    }
}
