using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// EmailRep.io - Email reputation and identity check
/// Free tier: 100 requests/day without API key
/// </summary>
public class EmailRepCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EmailRepCheck> _logger;
    private readonly string? _apiKey;

    private const string EmailRepBaseUrl = "https://emailrep.io";

    public EmailRepCheck(IHttpClientFactory httpClientFactory, ILogger<EmailRepCheck> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiKey = configuration["Osint:EmailRepApiKey"];
    }

    /// <summary>
    /// Check email reputation and associated identity information
    /// </summary>
    public async Task<List<OsintNode>> CheckEmailAsync(string email, CancellationToken ct = default)
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
                client.DefaultRequestHeaders.Add("Key", _apiKey);
            }

            var url = $"{EmailRepBaseUrl}/{Uri.EscapeDataString(email)}";
            var response = await client.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var rep = JsonSerializer.Deserialize<EmailRepResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (rep != null)
                {
                    var repNode = new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = $"Email Reputation: [{rep.Reputation.ToUpper()}]",
                        Value = email,
                        Depth = 1,
                        Children = new List<OsintNode>()
                    };

                    if (rep.Details != null)
                    {
                        if (rep.Details.Profiles?.Count > 0)
                        {
                            repNode.Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Found on",
                                Value = string.Join(", ", rep.Details.Profiles),
                                Depth = 2
                            });
                        }

                        var characteristics = new List<string>();
                        if (rep.Details.FreeProvider) characteristics.Add("Free provider");
                        if (rep.Details.Deliverable) characteristics.Add("Deliverable");
                        if (rep.Details.ValidMx) characteristics.Add("Valid MX");
                        if (rep.Details.Disposable) characteristics.Add("[WARNING] Disposable");
                        if (rep.Details.SpamTrap) characteristics.Add("[ALERT] Spam trap");

                        if (characteristics.Count > 0)
                        {
                            repNode.Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Characteristics",
                                Value = string.Join(", ", characteristics),
                                Depth = 2
                            });
                        }

                        if (rep.Details.DomainExists)
                        {
                            repNode.Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Domain Age",
                                Value = rep.Details.DaysSinceDomainCreation > 0 
                                    ? $"{rep.Details.DaysSinceDomainCreation} days old" 
                                    : "Unknown",
                                Depth = 2
                            });
                        }

                        if (rep.Suspicious)
                        {
                            repNode.Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "[WARNING] Suspicious",
                                Value = rep.Details.SuspiciousReason ?? "Flagged as suspicious",
                                Depth = 2
                            });
                        }

                        if (rep.Details.DataBreach)
                        {
                            repNode.Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Breach Exposure",
                                Value = rep.Details.CredentialsLeaked 
                                    ? "Credentials exposed in breach" 
                                    : "Appeared in data breach",
                                Depth = 2
                            });
                        }

                        if (!string.IsNullOrEmpty(rep.Details.LastSeen))
                        {
                            repNode.Children.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Last Seen",
                                Value = rep.Details.LastSeen,
                                Depth = 2
                            });
                        }
                    }

                    results.Add(repNode);
                    _logger.LogInformation("EmailRep check complete for {Email}: {Reputation}", email, rep.Reputation);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("EmailRep rate limit exceeded");
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogDebug("EmailRep check timed out for {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("EmailRep check failed: {Error}", ex.Message);
        }

        return results;
    }

    private class EmailRepResponse
    {
        public string Email { get; set; } = "";
        public string Reputation { get; set; } = "";
        public bool Suspicious { get; set; }
        public int References { get; set; }
        public EmailRepDetails? Details { get; set; }
    }

    private class EmailRepDetails
    {
        public bool Blacklisted { get; set; }
        public bool MaliciousActivity { get; set; }
        public bool CredentialsLeaked { get; set; }
        public bool DataBreach { get; set; }
        public bool FreeProvider { get; set; }
        public bool Deliverable { get; set; }
        public bool ValidMx { get; set; }
        public bool Disposable { get; set; }
        public bool SpamTrap { get; set; }
        public bool DomainExists { get; set; }
        public int DaysSinceDomainCreation { get; set; }
        public string? SuspiciousReason { get; set; }
        public string? LastSeen { get; set; }
        public List<string>? Profiles { get; set; }
    }
}
