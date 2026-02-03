using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Have I Been Pwned (HIBP) breach check service
/// Checks if an email appears in known data breaches (metadata only - no passwords)
/// </summary>
public class HibpBreachCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HibpBreachCheck> _logger;
    private readonly string? _apiKey;

    private const string HibpBaseUrl = "https://haveibeenpwned.com/api/v3";
    
    public HibpBreachCheck(IHttpClientFactory httpClientFactory, ILogger<HibpBreachCheck> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["Osint:HibpApiKey"];
    }

    /// <summary>
    /// Check if email appears in known data breaches
    /// Returns breach metadata (names, dates) - NOT actual leaked data
    /// </summary>
    public async Task<List<OsintNode>> CheckBreachesAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(email))
            return results;

        try
        {
            var client = _httpClientFactory.CreateClient("OsintClient");
            client.Timeout = TimeSpan.FromSeconds(10);

            client.DefaultRequestHeaders.Add("User-Agent", "SherlockOsint-Security-Tool");
            if (!string.IsNullOrEmpty(_apiKey))
            {
                client.DefaultRequestHeaders.Add("hibp-api-key", _apiKey);
            }

            var encodedEmail = Uri.EscapeDataString(email);
            var url = $"{HibpBaseUrl}/breachedaccount/{encodedEmail}?truncateResponse=false";

            var response = await client.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var breaches = JsonSerializer.Deserialize<List<HibpBreach>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (breaches?.Count > 0)
                {
                    var breachNode = new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = $"[WARNING] Data Breaches ({breaches.Count})",
                        Value = $"Email found in {breaches.Count} known data breaches",
                        Depth = 1,
                        Children = new List<OsintNode>()
                    };

                    foreach (var breach in breaches.Take(10))
                    {
                        var breachDetails = new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = $"Breach: {breach.Name}",
                            Value = $"{breach.Domain} - {breach.BreachDate:yyyy-MM-dd}",
                            Depth = 2,
                            Children = new List<OsintNode>
                            {
                                new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = "Date",
                                    Value = breach.BreachDate.ToString("yyyy-MM-dd"),
                                    Depth = 3
                                },
                                new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = "Records",
                                    Value = FormatNumber(breach.PwnCount),
                                    Depth = 3
                                },
                                new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = "Data Types",
                                    Value = string.Join(", ", breach.DataClasses.Take(5)),
                                    Depth = 3
                                }
                            }
                        };

                        breachNode.Children.Add(breachDetails);
                    }

                    results.Add(breachNode);
                    _logger.LogInformation("Found {BreachCount} breaches for {Email}", breaches.Count, email);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "No Known Breaches",
                    Value = $"Email not found in known data breaches",
                    Depth = 1
                });
                _logger.LogDebug("No breaches found for {Email}", email);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("HIBP rate limit exceeded");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("HIBP API key required or invalid");
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("HIBP check timed out for {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("HIBP check failed: {Error}", ex.Message);
        }

        return results;
    }

    private static string FormatNumber(long number)
    {
        if (number >= 1_000_000_000)
            return $"{number / 1_000_000_000.0:F1}B";
        if (number >= 1_000_000)
            return $"{number / 1_000_000.0:F1}M";
        if (number >= 1_000)
            return $"{number / 1_000.0:F1}K";
        return number.ToString();
    }

    private class HibpBreach
    {
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Domain { get; set; } = "";
        public DateTime BreachDate { get; set; }
        public DateTime AddedDate { get; set; }
        public long PwnCount { get; set; }
        public List<string> DataClasses { get; set; } = new();
        public bool IsVerified { get; set; }
        public bool IsSensitive { get; set; }
    }
}
