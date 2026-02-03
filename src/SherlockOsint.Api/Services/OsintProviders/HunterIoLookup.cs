using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Hunter.io API - Email verification and company information
/// Free tier: 25 requests/month (no credit card required)
/// Sign up: https://hunter.io/users/sign_up
/// </summary>
public class HunterIoLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HunterIoLookup> _logger;
    private readonly IConfiguration _configuration;

    public HunterIoLookup(
        IHttpClientFactory httpClientFactory, 
        ILogger<HunterIoLookup> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Verify email and get associated data
    /// </summary>
    public async Task<List<OsintNode>> VerifyEmailAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(email))
            return results;

        var apiKey = _configuration["Osint:HunterApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogDebug("Hunter.io API key not configured");
            return results;
        }

        try
        {
            // Email verifier endpoint
            var url = $"https://api.hunter.io/v2/email-verifier?email={Uri.EscapeDataString(email)}&api_key={apiKey}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Hunter.io rate limit (429) exceeded. No more credits for this month.");
                return results;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Hunter.io returned {Status}", response.StatusCode);
                return results;
            }

            // AUTO-DOMAIN DISCOVERY: If this is a corporate email, try to find the company!
            if (email.Contains("@") && !IsGenericEmail(email))
            {
                var domain = email.Split('@').Last();
                var domainNodes = await SearchDomainAsync(domain, ct);
                results.AddRange(domainNodes);
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            if (!data.RootElement.TryGetProperty("data", out var resultData))
                return results;

            var children = new List<OsintNode>();

            // Status (valid, invalid, accept_all, etc.)
            if (resultData.TryGetProperty("status", out var status))
            {
                var statusVal = status.GetString() ?? "";
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Email Status",
                    Value = statusVal.ToUpper(),
                    Depth = 2
                });
            }

            // Result (deliverable, undeliverable, risky)
            if (resultData.TryGetProperty("result", out var result))
            {
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Deliverability",
                    Value = result.GetString() ?? "",
                    Depth = 2
                });
            }

            // Score (0-100)
            if (resultData.TryGetProperty("score", out var score))
            {
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Quality Score",
                    Value = $"{score.GetInt32()}%",
                    Depth = 2
                });
            }

            // First name (if detected)
            if (resultData.TryGetProperty("first_name", out var firstName))
            {
                var name = firstName.GetString();
                if (!string.IsNullOrEmpty(name))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "First Name",
                        Value = name,
                        Depth = 2
                    });
                }
            }

            // Last name
            if (resultData.TryGetProperty("last_name", out var lastName))
            {
                var name = lastName.GetString();
                if (!string.IsNullOrEmpty(name))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Last Name",
                        Value = name,
                        Depth = 2
                    });
                }
            }

            // Company/Organization
            if (resultData.TryGetProperty("company", out var company))
            {
                var comp = company.GetString();
                if (!string.IsNullOrEmpty(comp))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Company",
                        Value = comp,
                        Depth = 2
                    });
                }
            }

            // Position/Title
            if (resultData.TryGetProperty("position", out var position))
            {
                var pos = position.GetString();
                if (!string.IsNullOrEmpty(pos))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Position",
                        Value = pos,
                        Depth = 2
                    });
                }
            }

            if (children.Count > 0)
            {
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Source",
                    Value = "Hunter.io Email Intelligence",
                    Depth = 2
                });

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[HUNTER] Email Intelligence",
                    Value = email,
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Hunter.io found data for {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Hunter.io lookup failed: {Error}", ex.Message);
        }

        return results;
    }

    /// <summary>
    /// Find emails at a domain (useful if email is @company.com)
    /// </summary>
    public async Task<List<OsintNode>> SearchDomainAsync(string domain, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(domain))
            return results;

        var apiKey = _configuration["Osint:HunterApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return results;

        try
        {
            var url = $"https://api.hunter.io/v2/domain-search?domain={Uri.EscapeDataString(domain)}&api_key={apiKey}&limit=5";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return results;

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            if (!data.RootElement.TryGetProperty("data", out var resultData))
                return results;

            var children = new List<OsintNode>();

            // Organization name
            if (resultData.TryGetProperty("organization", out var org))
            {
                var orgName = org.GetString();
                if (!string.IsNullOrEmpty(orgName))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Organization",
                        Value = orgName,
                        Depth = 2
                    });
                }
            }

            // Found emails
            if (resultData.TryGetProperty("emails", out var emails))
            {
                var emailCount = 0;
                foreach (var emailEntry in emails.EnumerateArray().Take(5))
                {
                    var emailAddr = emailEntry.TryGetProperty("value", out var v) ? v.GetString() : "";
                    var firstName = emailEntry.TryGetProperty("first_name", out var fn) ? fn.GetString() : "";
                    var lastName = emailEntry.TryGetProperty("last_name", out var ln) ? ln.GetString() : "";
                    var position = emailEntry.TryGetProperty("position", out var pos) ? pos.GetString() : "";

                    if (!string.IsNullOrEmpty(emailAddr))
                    {
                        var label = !string.IsNullOrEmpty(firstName) ? $"{firstName} {lastName}".Trim() : "Employee";
                        if (!string.IsNullOrEmpty(position))
                            label += $" ({position})";

                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = label,
                            Value = emailAddr,
                            Depth = 2
                        });
                        emailCount++;
                    }
                }

                if (emailCount > 0)
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = $"[HUNTER] Domain Emails ({domain})",
                        Value = $"Found {emailCount} employees",
                        Depth = 1,
                        Children = children
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Hunter.io domain search failed: {Error}", ex.Message);
        }

        return results;
    }

    private bool IsGenericEmail(string email)
    {
        var generic = new[] { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "icloud.com", "protonmail.com", "mail.com", "yandex.ru" };
        var domain = email.Split('@').Last().ToLower();
        return generic.Contains(domain);
    }
}
