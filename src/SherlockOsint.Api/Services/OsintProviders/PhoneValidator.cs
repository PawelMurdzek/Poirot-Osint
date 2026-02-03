using SherlockOsint.Shared.Models;
using System.Text.Json;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Validates phone numbers and extracts carrier/location info.
/// Uses Numverify API (free tier: 100 requests/month)
/// </summary>
public class PhoneValidator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PhoneValidator> _logger;
    
    // Free API key from numverify.com - replace with your own for production
    private const string ApiKey = ""; // Will use fallback if empty

    public PhoneValidator(IHttpClientFactory httpClientFactory, ILogger<PhoneValidator> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OsintClient");
        _logger = logger;
    }

    public async Task<List<OsintNode>> ValidateAsync(string phoneNumber, CancellationToken ct = default)
    {
        var results = new List<OsintNode>();
        
        _logger.LogInformation("Phone validation called with: '{Phone}'", phoneNumber);
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Phone number is empty or whitespace");
            return results;
        }

        // Clean the phone number (remove spaces, dashes, parentheses, etc.)
        var cleanNumber = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        _logger.LogInformation("Cleaned phone number: '{CleanNumber}'", cleanNumber);
        
        // If no + prefix, only convert 00 to +
        if (!cleanNumber.StartsWith('+'))
        {
            // If starts with 00, replace with + (international format)
            if (cleanNumber.StartsWith("00") && cleanNumber.Length > 4)
            {
                cleanNumber = "+" + cleanNumber.Substring(2);
                _logger.LogInformation("Converted 00 prefix to +: '{NormalizedNumber}'", cleanNumber);
            }
            else
            {
                // No country code provided - return error message
                _logger.LogWarning("Phone number missing country code prefix (+XX)");
                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "[WARNING] Invalid Format",
                    Value = "Please enter phone with country code (e.g., +48 for Poland, +34 for Spain)",
                    Depth = 1
                });
                return results;
            }
        }
        
        if (cleanNumber.Length < 8)
        {
            _logger.LogWarning("Phone number too short: {Length} digits", cleanNumber.Length);
            return results;
        }

        try
        {
            // Parse the phone number manually for basic info
            var phoneInfo = ParsePhoneNumber(cleanNumber);
            
            if (phoneInfo != null)
            {
                // Add phone number header with emoji
                results.Add(new OsintNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "📱 Phone Number",
                    Value = phoneNumber.Trim(),
                    Depth = 1
                });
                
                if (!string.IsNullOrEmpty(phoneInfo.Country))
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "Country",
                        Value = phoneInfo.Country,
                        Depth = 1
                    });
                }
                
                if (!string.IsNullOrEmpty(phoneInfo.CountryCode))
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "📞 Country Code",
                        Value = phoneInfo.CountryCode,
                        Depth = 1
                    });
                }
                
                if (!string.IsNullOrEmpty(phoneInfo.Carrier))
                {
                    results.Add(new OsintNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Label = "📡 Carrier",
                        Value = phoneInfo.Carrier,
                        Depth = 1
                    });
                }
                
                _logger.LogInformation("Phone validated: {Phone} from {Country} via {Carrier}", 
                    cleanNumber, phoneInfo.Country, phoneInfo.Carrier);
            }
            else
            {
                _logger.LogWarning("Could not parse phone number country code from: {Phone}", cleanNumber);
            }

            // If API key is set, try external API
            if (!string.IsNullOrEmpty(ApiKey))
            {
                var apiResults = await CallNumverifyApiAsync(cleanNumber, ct);
                results.AddRange(apiResults);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Phone validation failed for: {Phone}", phoneNumber);
        }

        _logger.LogInformation("Phone validation returning {Count} results", results.Count);
        return results;
    }

    private PhoneInfo? ParsePhoneNumber(string number)
    {
        // Country code database - common prefixes
        var countryCodes = new Dictionary<string, (string Country, string Carrier)>
        {
            { "+48", ("Poland 🇵🇱", "Polish Mobile") },
            { "+1", ("United States / Canada 🇺🇸", "") },
            { "+44", ("United Kingdom 🇬🇧", "") },
            { "+49", ("Germany 🇩🇪", "") },
            { "+33", ("France 🇫🇷", "") },
            { "+39", ("Italy 🇮🇹", "") },
            { "+34", ("Spain 🇪🇸", "") },
            { "+31", ("Netherlands 🇳🇱", "") },
            { "+32", ("Belgium 🇧🇪", "") },
            { "+43", ("Austria 🇦🇹", "") },
            { "+41", ("Switzerland 🇨🇭", "") },
            { "+46", ("Sweden 🇸🇪", "") },
            { "+47", ("Norway 🇳🇴", "") },
            { "+45", ("Denmark 🇩🇰", "") },
            { "+358", ("Finland 🇫🇮", "") },
            { "+351", ("Portugal 🇵🇹", "") },
            { "+380", ("Ukraine 🇺🇦", "") },
            { "+7", ("Russia 🇷🇺", "") },
            { "+81", ("Japan 🇯🇵", "") },
            { "+86", ("China 🇨🇳", "") },
            { "+91", ("India 🇮🇳", "") },
            { "+61", ("Australia 🇦🇺", "") },
            { "+55", ("Brazil 🇧🇷", "") },
        };

        // Polish mobile carriers by prefix (3-digit after +48)
        var polishCarriers = new Dictionary<string, string>
        {
            // Orange
            { "451", "Orange" }, { "500", "Orange" }, { "501", "Orange" }, { "502", "Orange" },
            { "503", "Orange" }, { "504", "Orange" }, { "505", "Orange" }, { "506", "Orange" },
            { "507", "Orange" }, { "508", "Orange" }, { "509", "Orange" }, { "510", "Orange" },
            { "511", "Orange" }, { "512", "Orange" }, { "513", "Orange" }, { "514", "Orange" },
            { "515", "Orange" }, { "516", "Orange" }, { "517", "Orange" }, { "518", "Orange" },
            { "519", "Orange" }, { "780", "Orange" }, { "781", "Orange" }, { "782", "Orange" },
            
            // Play
            { "530", "Play" }, { "531", "Play" }, { "532", "Play" }, { "533", "Play" }, 
            { "534", "Play" }, { "535", "Play" }, { "536", "Play" }, { "537", "Play" },
            { "538", "Play" }, { "570", "Play" }, { "571", "Play" }, { "572", "Play" },
            { "573", "Play" }, { "574", "Play" }, { "575", "Play" }, { "576", "Play" },
            { "577", "Play" }, { "578", "Play" }, { "579", "Play" },
            { "720", "Play" }, { "721", "Play" }, { "722", "Play" }, { "723", "Play" },
            { "724", "Play" }, { "725", "Play" }, { "726", "Play" }, { "727", "Play" },
            { "728", "Play" }, { "729", "Play" },
            { "730", "Play" }, { "731", "Play" }, { "732", "Play" }, { "733", "Play" },
            { "734", "Play" }, { "735", "Play" }, { "736", "Play" }, { "737", "Play" },
            { "738", "Play" }, { "739", "Play" },
            { "790", "Play" }, { "791", "Play" }, { "792", "Play" }, { "793", "Play" },
            { "794", "Play" }, { "795", "Play" }, { "796", "Play" }, { "797", "Play" },
            { "798", "Play" }, { "799", "Play" },
            
            // Plus
            { "600", "Plus" }, { "601", "Plus" }, { "602", "Plus" }, { "603", "Plus" },
            { "604", "Plus" }, { "605", "Plus" }, { "606", "Plus" }, { "607", "Plus" },
            { "608", "Plus" }, { "609", "Plus" },
            { "660", "Plus" }, { "661", "Plus" }, { "662", "Plus" }, { "663", "Plus" },
            { "664", "Plus" }, { "665", "Plus" }, { "666", "Plus" }, { "667", "Plus" },
            { "668", "Plus" }, { "669", "Plus" },
            { "690", "Plus" }, { "691", "Plus" }, { "692", "Plus" }, { "693", "Plus" },
            { "694", "Plus" }, { "695", "Plus" }, { "696", "Plus" }, { "697", "Plus" },
            { "698", "Plus" }, { "699", "Plus" },
            { "880", "Plus" }, { "881", "Plus" }, { "882", "Plus" }, { "883", "Plus" },
            { "884", "Plus" }, { "885", "Plus" }, { "886", "Plus" }, { "887", "Plus" },
            { "888", "Plus" }, { "889", "Plus" },
            
            // T-Mobile 
            { "783", "T-Mobile" }, { "784", "T-Mobile" }, { "785", "T-Mobile" },
            { "786", "T-Mobile" }, { "787", "T-Mobile" }, { "788", "T-Mobile" }, { "789", "T-Mobile" },
        };

        foreach (var (code, info) in countryCodes)
        {
            if (number.StartsWith(code))
            {
                var carrier = info.Carrier;
                
                // For Polish numbers, try to detect carrier (3-digit prefix)
                if (code == "+48" && number.Length >= 6)
                {
                    var prefix = number.Substring(3, 3);
                    if (polishCarriers.TryGetValue(prefix, out var polishCarrier))
                    {
                        carrier = polishCarrier;
                    }
                }
                
                return new PhoneInfo
                {
                    Country = info.Country,
                    CountryCode = code,
                    Carrier = carrier
                };
            }
        }

        return null;
    }

    private async Task<List<OsintNode>> CallNumverifyApiAsync(string number, CancellationToken ct)
    {
        var results = new List<OsintNode>();
        
        try
        {
            var url = $"http://apilayer.net/api/validate?access_key={ApiKey}&number={number}";
            var response = await _httpClient.GetAsync(url, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var data = JsonDocument.Parse(json);
                
                if (data.RootElement.TryGetProperty("valid", out var valid) && valid.GetBoolean())
                {
                    if (data.RootElement.TryGetProperty("carrier", out var carrier))
                    {
                        var carrierName = carrier.GetString();
                        if (!string.IsNullOrEmpty(carrierName))
                        {
                            results.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Carrier (API)",
                                Value = carrierName,
                                Depth = 1
                            });
                        }
                    }
                    
                    if (data.RootElement.TryGetProperty("location", out var location))
                    {
                        var locationName = location.GetString();
                        if (!string.IsNullOrEmpty(locationName))
                        {
                            results.Add(new OsintNode
                            {
                                Id = Guid.NewGuid().ToString(),
                                Label = "Location (API)",
                                Value = locationName,
                                Depth = 1
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Numverify API call failed: {Error}", ex.Message);
        }

        return results;
    }

    private class PhoneInfo
    {
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? Carrier { get; set; }
    }
}
