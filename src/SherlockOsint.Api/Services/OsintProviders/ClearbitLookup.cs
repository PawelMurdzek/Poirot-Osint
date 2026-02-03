using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Clearbit API - Email to person/company enrichment
/// Free tier available with limited requests
/// Sign up: https://dashboard.clearbit.com/signup
/// </summary>
public class ClearbitLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClearbitLookup> _logger;
    private readonly IConfiguration _configuration;

    public ClearbitLookup(
        IHttpClientFactory httpClientFactory,
        ILogger<ClearbitLookup> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Enrich email with person and company data
    /// </summary>
    public async Task<List<OsintNode>> EnrichEmailAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(email))
            return results;

        var apiKey = _configuration["Osint:ClearbitApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogDebug("Clearbit API key not configured");
            return results;
        }

        try
        {
            // Combined enrichment endpoint
            var url = $"https://person.clearbit.com/v2/combined/find?email={Uri.EscapeDataString(email)}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Clearbit: No data found for {Email}", email);
                return results;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Clearbit returned {Status}", response.StatusCode);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            var children = new List<OsintNode>();

            // Person data
            if (data.RootElement.TryGetProperty("person", out var person) && 
                person.ValueKind != JsonValueKind.Null)
            {
                // Full name
                if (person.TryGetProperty("name", out var nameObj))
                {
                    var fullName = nameObj.TryGetProperty("fullName", out var fn) ? fn.GetString() : "";
                    if (!string.IsNullOrEmpty(fullName))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Full Name",
                            Value = fullName,
                            Depth = 2
                        });
                    }
                }

                // Location
                if (person.TryGetProperty("location", out var location))
                {
                    var loc = location.GetString();
                    if (!string.IsNullOrEmpty(loc))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Location",
                            Value = loc,
                            Depth = 2
                        });
                    }
                }

                // Bio
                if (person.TryGetProperty("bio", out var bio))
                {
                    var bioText = bio.GetString();
                    if (!string.IsNullOrEmpty(bioText))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Bio",
                            Value = bioText,
                            Depth = 2
                        });
                    }
                }

                // Avatar
                if (person.TryGetProperty("avatar", out var avatar))
                {
                    var avatarUrl = avatar.GetString();
                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Photo",
                            Value = avatarUrl,
                            Depth = 2
                        });
                    }
                }

                // Employment
                if (person.TryGetProperty("employment", out var employment))
                {
                    var company = employment.TryGetProperty("name", out var c) ? c.GetString() : "";
                    var title = employment.TryGetProperty("title", out var t) ? t.GetString() : "";

                    if (!string.IsNullOrEmpty(company))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Company",
                            Value = company,
                            Depth = 2
                        });
                    }

                    if (!string.IsNullOrEmpty(title))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Title",
                            Value = title,
                            Depth = 2
                        });
                    }
                }

                // Social profiles - THE GOLD
                // GitHub
                if (person.TryGetProperty("github", out var github))
                {
                    var handle = github.TryGetProperty("handle", out var h) ? h.GetString() : "";
                    if (!string.IsNullOrEmpty(handle))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "GitHub",
                            Value = $"https://github.com/{handle}",
                            Depth = 2
                        });
                    }
                }

                // Twitter
                if (person.TryGetProperty("twitter", out var twitter))
                {
                    var handle = twitter.TryGetProperty("handle", out var h) ? h.GetString() : "";
                    if (!string.IsNullOrEmpty(handle))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Twitter",
                            Value = $"https://twitter.com/{handle}",
                            Depth = 2
                        });
                    }
                }

                // LinkedIn
                if (person.TryGetProperty("linkedin", out var linkedin))
                {
                    var handle = linkedin.TryGetProperty("handle", out var h) ? h.GetString() : "";
                    if (!string.IsNullOrEmpty(handle))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "LinkedIn",
                            Value = $"https://linkedin.com/in/{handle}",
                            Depth = 2
                        });
                    }
                }

                // Facebook
                if (person.TryGetProperty("facebook", out var facebook))
                {
                    var handle = facebook.TryGetProperty("handle", out var h) ? h.GetString() : "";
                    if (!string.IsNullOrEmpty(handle))
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = "Facebook",
                            Value = $"https://facebook.com/{handle}",
                            Depth = 2
                        });
                    }
                }
            }

            // Company data
            if (data.RootElement.TryGetProperty("company", out var company2) && 
                company2.ValueKind != JsonValueKind.Null)
            {
                var companyName = company2.TryGetProperty("name", out var cn) ? cn.GetString() : "";
                var domain = company2.TryGetProperty("domain", out var d) ? d.GetString() : "";
                var industry = company2.TryGetProperty("category", out var cat) 
                    ? (cat.TryGetProperty("industry", out var ind) ? ind.GetString() : "") 
                    : "";

                if (!string.IsNullOrEmpty(companyName))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Company (domain)",
                        Value = $"{companyName} ({domain})",
                        Depth = 2
                    });
                }

                if (!string.IsNullOrEmpty(industry))
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Industry",
                        Value = industry,
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
                    Value = "Clearbit Person Enrichment",
                    Depth = 2
                });

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[CLEARBIT] Person Profile",
                    Value = email,
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Clearbit found profile for {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Clearbit lookup failed: {Error}", ex.Message);
        }

        return results;
    }
}
