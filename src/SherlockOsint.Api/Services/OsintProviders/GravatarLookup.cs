using SherlockOsint.Shared.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Looks up Gravatar profile data from an email address.
/// No API key required - uses MD5 hash of email.
/// </summary>
public class GravatarLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GravatarLookup> _logger;

    public GravatarLookup(IHttpClientFactory httpClientFactory, ILogger<GravatarLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> LookupAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return results;

        try
        {
            var hash = GetMd5Hash(email.Trim().ToLowerInvariant());
            var url = $"https://www.gravatar.com/{hash}.json";
            
            var response = await _httpClient.GetAsync(url, ct);
            
            if (!response.IsSuccessStatusCode)
                return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);
            
            if (data.RootElement.TryGetProperty("entry", out var entries) && entries.GetArrayLength() > 0)
            {
                var entry = entries[0];
                
                // Display name
                if (entry.TryGetProperty("displayName", out var displayName))
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Gravatar Profile",
                        Value = displayName.GetString() ?? "Unknown",
                        Depth = 1
                    });
                }
                
                // Profile URL
                if (entry.TryGetProperty("profileUrl", out var profileUrl))
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Profile URL",
                        Value = profileUrl.GetString() ?? "",
                        Depth = 1
                    });
                }
                
                // Photo
                if (entry.TryGetProperty("thumbnailUrl", out var photoUrl))
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Photo",
                        Value = photoUrl.GetString() ?? "",
                        Depth = 1
                    });
                }
                
                // Linked accounts
                if (entry.TryGetProperty("accounts", out var accounts))
                {
                    foreach (var account in accounts.EnumerateArray())
                    {
                        var domain = account.TryGetProperty("domain", out var d) ? d.GetString() : "";
                        var url2 = account.TryGetProperty("url", out var u) ? u.GetString() : "";
                        
                        if (!string.IsNullOrEmpty(url2))
                        {
                            results.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = $"Linked: {domain}",
                                Value = url2,
                                Depth = 1
                            });
                        }
                    }
                }
                
                _logger.LogInformation("Found Gravatar profile for {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Gravatar lookup failed for {Email}: {Error}", email, ex.Message);
        }

        return results;
    }

    private static string GetMd5Hash(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
