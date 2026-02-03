using SherlockOsint.Shared.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Domain WHOIS lookup - get registrant info for custom email domains
/// Uses free RDAP protocol (replacing WHOIS)
/// </summary>
public class DomainWhoisLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DomainWhoisLookup> _logger;

    // Common free email providers to skip
    private static readonly HashSet<string> FreeProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "live.com",
        "aol.com", "icloud.com", "mail.com", "protonmail.com", "proton.me",
        "yandex.com", "zoho.com", "gmx.com", "tutanota.com",
        "wp.pl", "o2.pl", "onet.pl", "interia.pl", // Polish
        "seznam.cz", "email.cz", // Czech
    };

    public DomainWhoisLookup(IHttpClientFactory httpClientFactory, ILogger<DomainWhoisLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    /// <summary>
    /// Get WHOIS/RDAP info for email domain (skips free providers)
    /// </summary>
    public async Task<List<OsintNode>> LookupEmailDomainAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return results;

        var domain = email.Split('@').Last().ToLower();

        // Skip free email providers
        if (FreeProviders.Contains(domain))
        {
            _logger.LogDebug("Skipping WHOIS for free provider: {Domain}", domain);
            return results;
        }

        return await LookupDomainAsync(domain, ct);
    }

    /// <summary>
    /// Get RDAP/WHOIS info for a domain
    /// </summary>
    public async Task<List<OsintNode>> LookupDomainAsync(string domain, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(domain))
            return results;

        try
        {
            // Use RDAP (modern replacement for WHOIS) - no API key needed
            var rdapUrl = $"https://rdap.org/domain/{domain}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, rdapUrl);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");
            request.Headers.Add("Accept", "application/rdap+json");

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("RDAP lookup failed for {Domain}: {Status}", domain, response.StatusCode);
                return results;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonDocument.Parse(json);

            var children = new List<OsintNode>();

            // Domain name
            if (data.RootElement.TryGetProperty("ldhName", out var ldhName))
            {
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Domain",
                    Value = ldhName.GetString() ?? domain,
                    Depth = 2
                });
            }

            // Status
            if (data.RootElement.TryGetProperty("status", out var status))
            {
                var statuses = status.EnumerateArray().Select(s => s.GetString()).Where(s => s != null);
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Status",
                    Value = string.Join(", ", statuses),
                    Depth = 2
                });
            }

            // Registration events (created, updated, expires)
            if (data.RootElement.TryGetProperty("events", out var events))
            {
                foreach (var evt in events.EnumerateArray())
                {
                    var action = evt.TryGetProperty("eventAction", out var a) ? a.GetString() : "";
                    var date = evt.TryGetProperty("eventDate", out var d) ? d.GetString() : "";

                    if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(date))
                    {
                        var dateOnly = date.Length >= 10 ? date.Substring(0, 10) : date;
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = action switch
                            {
                                "registration" => "Registered",
                                "last changed" => "Last Updated",
                                "expiration" => "Expires",
                                _ => action
                            },
                            Value = dateOnly,
                            Depth = 2
                        });
                    }
                }
            }

            // Entities (registrant, admin, tech contacts)
            if (data.RootElement.TryGetProperty("entities", out var entities))
            {
                foreach (var entity in entities.EnumerateArray())
                {
                    var roles = entity.TryGetProperty("roles", out var r) 
                        ? r.EnumerateArray().Select(x => x.GetString()).ToList() 
                        : new List<string?>();

                    if (entity.TryGetProperty("vcardArray", out var vcard))
                    {
                        var vcardData = ParseVCard(vcard);
                        
                        foreach (var role in roles.Where(x => x != null))
                        {
                            if (vcardData.TryGetValue("fn", out var fullName) && !string.IsNullOrEmpty(fullName))
                            {
                                children.Add(new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = $"{role} Name",
                                    Value = fullName,
                                    Depth = 2
                                });
                            }

                            if (vcardData.TryGetValue("org", out var org) && !string.IsNullOrEmpty(org))
                            {
                                children.Add(new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = $"{role} Organization",
                                    Value = org,
                                    Depth = 2
                                });
                            }

                            if (vcardData.TryGetValue("email", out var contactEmail) && !string.IsNullOrEmpty(contactEmail))
                            {
                                children.Add(new OsintNode
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Label = $"{role} Email",
                                    Value = contactEmail,
                                    Depth = 2
                                });
                            }
                        }
                    }
                }
            }

            // Nameservers
            if (data.RootElement.TryGetProperty("nameservers", out var nameservers))
            {
                var ns = nameservers.EnumerateArray()
                    .Select(n => n.TryGetProperty("ldhName", out var name) ? name.GetString() : null)
                    .Where(n => n != null)
                    .Take(2);
                
                if (ns.Any())
                {
                    children.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Nameservers",
                        Value = string.Join(", ", ns),
                        Depth = 2
                    });
                }
            }

            if (children.Count > 0)
            {
                children.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Discovery",
                    Value = "Custom domain - may indicate company/personal site",
                    Depth = 2
                });

                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = $"[WHOIS] Domain: {domain}",
                    Value = $"https://who.is/whois/{domain}",
                    Depth = 1,
                    Children = children
                });

                _logger.LogInformation("Found WHOIS data for {Domain}", domain);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("WHOIS lookup failed: {Error}", ex.Message);
        }

        return results;
    }

    private Dictionary<string, string> ParseVCard(JsonElement vcard)
    {
        var result = new Dictionary<string, string>();

        try
        {
            // vCard is array: ["vcard", [...properties...]]
            if (vcard.GetArrayLength() >= 2)
            {
                var props = vcard[1];
                foreach (var prop in props.EnumerateArray())
                {
                    if (prop.GetArrayLength() >= 4)
                    {
                        var propName = prop[0].GetString()?.ToLower() ?? "";
                        var propValue = prop[3].ValueKind == JsonValueKind.String 
                            ? prop[3].GetString() 
                            : prop[3].ToString();

                        if (!string.IsNullOrEmpty(propValue) && !result.ContainsKey(propName))
                        {
                            result[propName] = propValue;
                        }
                    }
                }
            }
        }
        catch { }

        return result;
    }
}
