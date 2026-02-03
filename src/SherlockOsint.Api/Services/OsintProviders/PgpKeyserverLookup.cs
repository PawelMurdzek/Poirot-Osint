using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// PGP Keyserver lookup - people publish their emails with encryption keys
/// Uses keys.openpgp.org API (no key required)
/// </summary>
public class PgpKeyserverLookup
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PgpKeyserverLookup> _logger;

    public PgpKeyserverLookup(IHttpClientFactory httpClientFactory, ILogger<PgpKeyserverLookup> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    /// <summary>
    /// Search for PGP keys associated with an email
    /// </summary>
    public async Task<List<OsintNode>> SearchByEmailAsync(string email, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();

        if (string.IsNullOrWhiteSpace(email))
            return results;

        try
        {
            // keys.openpgp.org API
            var url = $"https://keys.openpgp.org/vks/v1/by-email/{Uri.EscapeDataString(email)}";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "SherlockOSINT/1.0");

            var response = await _httpClient.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
            {
                var keyData = await response.Content.ReadAsStringAsync(ct);
                
                // Parse the ASCII armored key to extract info
                var keyInfo = ParsePgpKey(keyData);

                if (keyInfo.Count > 0)
                {
                    var children = new List<OsintNode>
                    {
                        new() { Id = Guid.NewGuid().ToString(), Label = "Discovery", Value = "Email has published PGP key", Depth = 2 },
                        new() { Id = Guid.NewGuid().ToString(), Label = "Confidence", Value = "HIGH - Email ownership verified", Depth = 2 }
                    };

                    foreach (var (key, value) in keyInfo)
                    {
                        children.Add(new OsintNode
                        {
                            Id = Guid.NewGuid().ToString(),
                            Label = key,
                            Value = value,
                            Depth = 2
                        });
                    }

                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "[PGP] OpenPGP Key Found",
                        Value = $"https://keys.openpgp.org/search?q={Uri.EscapeDataString(email)}",
                        Depth = 1,
                        Children = children
                    });

                    _logger.LogInformation("Found PGP key for {Email}", email);
                }
            }
            else
            {
                _logger.LogDebug("No PGP key found for {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("PGP keyserver lookup failed: {Error}", ex.Message);
        }

        return results;
    }

    private Dictionary<string, string> ParsePgpKey(string armored)
    {
        var info = new Dictionary<string, string>();

        // Extract User ID from key (usually contains name and email)
        var lines = armored.Split('\n');
        foreach (var line in lines)
        {
            // Look for comment lines that often contain user info
            if (line.StartsWith("Comment:"))
            {
                info["Comment"] = line.Substring(8).Trim();
            }
        }

        // Note: Full PGP parsing would require a library like BouncyCastle
        // For now, just indicate key exists
        info["Key Type"] = "PGP Public Key";
        info["Source"] = "keys.openpgp.org";

        return info;
    }
}
