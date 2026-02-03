using SherlockOsint.Shared.Models;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SherlockOsint.Api.Services.OsintProviders;

/// <summary>
/// Detects country probability distribution from multiple evidence sources
/// </summary>
public class CountryDetector
{
    private readonly ILogger<CountryDetector> _logger;

    // Country indicators with patterns
    private static readonly Dictionary<string, CountryInfo> Countries = new(StringComparer.OrdinalIgnoreCase)
    {
        // European countries
        { "PL", new("Poland", "🇵🇱", new[] { "poland", "polska", "warsaw", "kraków", "krakow", "wrocław", "wroclaw", "gdańsk", "gdansk", "poznań", "poznan", "łódź", "lodz", "szczecin", "lublin", "katowice" }, new[] { "+48" }, new[] { ".pl" }) },
        { "CZ", new("Czech Republic", "🇨🇿", new[] { "czech", "czechia", "česká", "ceska", "prague", "praha", "brno", "ostrava", "plzeň", "plzen" }, new[] { "+420" }, new[] { ".cz" }) },
        { "DE", new("Germany", "🇩🇪", new[] { "germany", "deutschland", "berlin", "munich", "münchen", "hamburg", "frankfurt", "cologne", "köln" }, new[] { "+49" }, new[] { ".de" }) },
        { "UK", new("United Kingdom", "🇬🇧", new[] { "uk", "united kingdom", "britain", "england", "london", "manchester", "birmingham", "scotland", "wales" }, new[] { "+44" }, new[] { ".uk", ".co.uk" }) },
        { "US", new("United States", "🇺🇸", new[] { "usa", "united states", "america", "california", "new york", "texas", "florida", "washington", "seattle", "san francisco" }, new[] { "+1" }, new[] { ".us", ".gov", ".mil" }) },
        { "FR", new("France", "🇫🇷", new[] { "france", "paris", "lyon", "marseille", "toulouse", "nice", "nantes" }, new[] { "+33" }, new[] { ".fr" }) },
        { "ES", new("Spain", "🇪🇸", new[] { "spain", "españa", "espana", "madrid", "barcelona", "valencia", "seville" }, new[] { "+34" }, new[] { ".es" }) },
        { "IT", new("Italy", "🇮🇹", new[] { "italy", "italia", "rome", "milan", "milano", "napoli", "turin", "torino" }, new[] { "+39" }, new[] { ".it" }) },
        { "NL", new("Netherlands", "🇳🇱", new[] { "netherlands", "holland", "amsterdam", "rotterdam", "utrecht", "den haag" }, new[] { "+31" }, new[] { ".nl" }) },
        { "BE", new("Belgium", "🇧🇪", new[] { "belgium", "belgique", "brussels", "bruxelles", "antwerp", "ghent" }, new[] { "+32" }, new[] { ".be" }) },
        { "AT", new("Austria", "🇦🇹", new[] { "austria", "österreich", "vienna", "wien", "salzburg", "innsbruck" }, new[] { "+43" }, new[] { ".at" }) },
        { "CH", new("Switzerland", "🇨🇭", new[] { "switzerland", "schweiz", "suisse", "zurich", "zürich", "geneva", "genève", "bern" }, new[] { "+41" }, new[] { ".ch" }) },
        { "SE", new("Sweden", "🇸🇪", new[] { "sweden", "sverige", "stockholm", "gothenburg", "malmö" }, new[] { "+46" }, new[] { ".se" }) },
        { "NO", new("Norway", "🇳🇴", new[] { "norway", "norge", "oslo", "bergen", "trondheim" }, new[] { "+47" }, new[] { ".no" }) },
        { "DK", new("Denmark", "🇩🇰", new[] { "denmark", "danmark", "copenhagen", "københavn" }, new[] { "+45" }, new[] { ".dk" }) },
        { "FI", new("Finland", "🇫🇮", new[] { "finland", "suomi", "helsinki", "espoo", "tampere" }, new[] { "+358" }, new[] { ".fi" }) },
        { "RU", new("Russia", "🇷🇺", new[] { "russia", "россия", "moscow", "москва", "saint petersburg", "санкт-петербург" }, new[] { "+7" }, new[] { ".ru", ".su" }) },
        { "UA", new("Ukraine", "🇺🇦", new[] { "ukraine", "україна", "kyiv", "київ", "kharkiv", "odesa", "lviv" }, new[] { "+380" }, new[] { ".ua" }) },
        { "SK", new("Slovakia", "🇸🇰", new[] { "slovakia", "slovensko", "bratislava", "košice", "kosice" }, new[] { "+421" }, new[] { ".sk" }) },
        { "HU", new("Hungary", "🇭🇺", new[] { "hungary", "magyarország", "budapest", "debrecen", "szeged" }, new[] { "+36" }, new[] { ".hu" }) },
        { "RO", new("Romania", "🇷🇴", new[] { "romania", "românia", "bucharest", "bucurești", "cluj", "timișoara" }, new[] { "+40" }, new[] { ".ro" }) },
        
        // Other regions
        { "CA", new("Canada", "🇨🇦", new[] { "canada", "toronto", "vancouver", "montreal", "calgary", "ottawa" }, new[] { "+1" }, new[] { ".ca" }) },
        { "AU", new("Australia", "🇦🇺", new[] { "australia", "sydney", "melbourne", "brisbane", "perth", "adelaide" }, new[] { "+61" }, new[] { ".au" }) },
        { "JP", new("Japan", "🇯🇵", new[] { "japan", "日本", "tokyo", "東京", "osaka", "大阪", "kyoto" }, new[] { "+81" }, new[] { ".jp" }) },
        { "KR", new("South Korea", "🇰🇷", new[] { "korea", "한국", "seoul", "서울", "busan" }, new[] { "+82" }, new[] { ".kr" }) },
        { "CN", new("China", "🇨🇳", new[] { "china", "中国", "beijing", "shanghai", "shenzhen", "guangzhou" }, new[] { "+86" }, new[] { ".cn" }) },
        { "IN", new("India", "🇮🇳", new[] { "india", "भारत", "mumbai", "delhi", "bangalore", "hyderabad", "chennai" }, new[] { "+91" }, new[] { ".in" }) },
        { "BR", new("Brazil", "🇧🇷", new[] { "brazil", "brasil", "são paulo", "rio de janeiro", "brasília" }, new[] { "+55" }, new[] { ".br" }) },
    };

    // Language to country mapping
    private static readonly Dictionary<string, string[]> LanguageCountries = new()
    {
        { "polish", new[] { "PL" } },
        { "czech", new[] { "CZ" } },
        { "german", new[] { "DE", "AT", "CH" } },
        { "english", new[] { "US", "UK", "CA", "AU" } },
        { "french", new[] { "FR", "BE", "CH", "CA" } },
        { "spanish", new[] { "ES" } },
        { "italian", new[] { "IT" } },
        { "dutch", new[] { "NL", "BE" } },
        { "russian", new[] { "RU" } },
        { "ukrainian", new[] { "UA" } },
        { "portuguese", new[] { "BR" } },
        { "japanese", new[] { "JP" } },
        { "korean", new[] { "KR" } },
        { "chinese", new[] { "CN" } },
    };

    public CountryDetector(ILogger<CountryDetector> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyze evidence from multiple sources and return country probability distribution
    /// </summary>
    public List<CountryProbability> AnalyzeCountryProbability(
        string? phoneNumber,
        List<OsintNode> results,
        string? inputName = null)
    {
        var evidence = new Dictionary<string, CountryEvidence>();

        // 1. Phone number analysis (highest confidence)
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            AnalyzePhoneNumber(phoneNumber, evidence);
        }

        // 2. Analyze profile locations from OSINT results
        AnalyzeProfileLocations(results, evidence);

        // 3. Analyze language patterns in bios
        AnalyzeLanguagePatterns(results, evidence);

        // 4. Analyze platform-specific country indicators
        AnalyzePlatformIndicators(results, evidence);

        // 5. Calculate probabilities
        return CalculateProbabilities(evidence);
    }

    private void AnalyzePhoneNumber(string phone, Dictionary<string, CountryEvidence> evidence)
    {
        var cleanPhone = Regex.Replace(phone, @"[^\d+]", "");
        
        foreach (var (code, info) in Countries)
        {
            foreach (var prefix in info.PhonePrefixes)
            {
                if (cleanPhone.StartsWith(prefix) || cleanPhone.StartsWith(prefix.Replace("+", "00")))
                {
                    AddEvidence(evidence, code, 60, $"Phone number prefix {prefix}");
                    _logger.LogDebug("Phone {Phone} simplifies to {Country} via prefix {Prefix}", phone, info.Name, prefix);
                    return;
                }
            }
        }
    }

    private void AnalyzeProfileLocations(List<OsintNode> results, Dictionary<string, CountryEvidence> evidence)
    {
        foreach (var node in results)
        {
            // Check node value for location mentions
            CheckTextForCountry(node.Value, evidence, 15, "Profile URL");

            // Check children for location data
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    var label = child.Label?.ToLower() ?? "";
                    if (label.Contains("location") || label.Contains("country") || label.Contains("city"))
                    {
                        CheckTextForCountry(child.Value, evidence, 25, $"Profile location on {ExtractPlatform(node.Label)}");
                    }
                    else if (label.Contains("bio"))
                    {
                        CheckTextForCountry(child.Value, evidence, 10, $"Bio on {ExtractPlatform(node.Label)}");
                    }
                }
            }
        }
    }

    private void AnalyzeLanguagePatterns(List<OsintNode> results, Dictionary<string, CountryEvidence> evidence)
    {
        foreach (var node in results)
        {
            if (node.Children == null) continue;

            foreach (var child in node.Children)
            {
                if (child.Label?.ToLower().Contains("bio") != true) continue;
                
                var text = child.Value ?? "";
                if (string.IsNullOrWhiteSpace(text)) continue;

                // Detect language from text
                var detectedLanguage = DetectLanguage(text);
                if (detectedLanguage != null && LanguageCountries.TryGetValue(detectedLanguage, out var countries))
                {
                    var weight = 10 / countries.Length; // Split weight among possible countries
                    foreach (var country in countries)
                    {
                        AddEvidence(evidence, country, weight, $"Language ({detectedLanguage}) in bio");
                    }
                }
            }
        }
    }

    private void AnalyzePlatformIndicators(List<OsintNode> results, Dictionary<string, CountryEvidence> evidence)
    {
        foreach (var node in results)
        {
            var url = node.Value?.ToLower() ?? "";
            
            // TLD Check
            foreach (var (code, info) in Countries)
            {
                foreach (var tld in info.Tlds)
                {
                    if (url.EndsWith(tld) || url.Contains($"{tld}/"))
                    {
                        AddEvidence(evidence, code, 15, $"Country TLD {tld}");
                    }
                }
            }

            // Polish platforms
            if (url.Contains("wykop.pl") || url.Contains("elektroda.pl") || url.Contains("dobreprogramy.pl"))
            {
                AddEvidence(evidence, "PL", 30, "Uses Polish community platform");
            }
            // Czech platforms
            else if (url.Contains(".cz") && !url.Contains("soundcloud.cz"))
            {
                AddEvidence(evidence, "CZ", 15, "Uses .cz domain");
            }
            // Russian platforms
            else if (url.Contains("vk.com") || url.Contains("ok.ru"))
            {
                AddEvidence(evidence, "RU", 15, "Uses Russian social platform");
            }
        }
    }

    private void CheckTextForCountry(string? text, Dictionary<string, CountryEvidence> evidence, int weight, string source)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var lowerText = text.ToLower();

        foreach (var (code, info) in Countries)
        {
            foreach (var indicator in info.Indicators)
            {
                if (lowerText.Contains(indicator))
                {
                    AddEvidence(evidence, code, weight, $"{source}: '{indicator}'");
                    return; // Only count first match per text
                }
            }
        }
    }

    private string? DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var lowerText = text.ToLower();

        // Polish characters
        if (Regex.IsMatch(text, @"[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]"))
            return "polish";
        
        // Czech characters
        if (Regex.IsMatch(text, @"[ěščřžýáíéůúďťňĚŠČŘŽÝÁÍÉŮÚĎŤŇ]"))
            return "czech";
        
        // Cyrillic (Russian/Ukrainian)
        if (Regex.IsMatch(text, @"[\u0400-\u04FF]"))
        {
            // Ukrainian has specific letters
            if (Regex.IsMatch(text, @"[іїєґІЇЄҐ]"))
                return "ukrainian";
            return "russian";
        }

        // Japanese
        if (Regex.IsMatch(text, @"[\u3040-\u309F\u30A0-\u30FF]"))
            return "japanese";

        // Korean
        if (Regex.IsMatch(text, @"[\uAC00-\uD7AF]"))
            return "korean";

        // Chinese
        if (Regex.IsMatch(text, @"[\u4E00-\u9FFF]"))
            return "chinese";

        return null;
    }

    private void AddEvidence(Dictionary<string, CountryEvidence> evidence, string countryCode, int weight, string source)
    {
        if (!evidence.TryGetValue(countryCode, out var existing))
        {
            existing = new CountryEvidence { CountryCode = countryCode };
            evidence[countryCode] = existing;
        }

        existing.TotalWeight += weight;
        existing.Sources.Add(source);
    }

    private List<CountryProbability> CalculateProbabilities(Dictionary<string, CountryEvidence> evidence)
    {
        if (evidence.Count == 0)
        {
            return new List<CountryProbability>
            {
                new CountryProbability
                {
                    Country = "Unknown",
                    CountryCode = "XX",
                    Probability = 100,
                    Evidence = "No location data found"
                }
            };
        }

        var totalWeight = evidence.Values.Sum(e => e.TotalWeight);
        if (totalWeight == 0) totalWeight = 1;

        var results = new List<CountryProbability>();
        
        foreach (var (code, data) in evidence.OrderByDescending(e => e.Value.TotalWeight))
        {
            var info = Countries.GetValueOrDefault(code);
            var probability = (int)Math.Round((double)data.TotalWeight / totalWeight * 100);
            
            if (probability < 5) continue; // Skip very low probabilities

            results.Add(new CountryProbability
            {
                Country = info?.Name ?? code,
                CountryCode = code,
                Flag = info?.Flag ?? "[?]",
                Probability = probability,
                Evidence = string.Join("; ", data.Sources.Distinct().Take(5))
            });
        }

        // Ensure we have at least one result
        if (results.Count == 0)
        {
            var top = evidence.OrderByDescending(e => e.Value.TotalWeight).First();
            var info = Countries.GetValueOrDefault(top.Key);
            results.Add(new CountryProbability
            {
                Country = info?.Name ?? top.Key,
                CountryCode = top.Key,
                Flag = info?.Flag ?? "[?]",
                Probability = 100,
                Evidence = string.Join("; ", top.Value.Sources.Distinct().Take(5))
            });
        }

        // Add "Other" if probabilities don't sum to 100
        var sum = results.Sum(r => r.Probability);
        if (sum < 100 && sum > 0)
        {
            results.Add(new CountryProbability
            {
                Country = "Other",
                CountryCode = "XX",
                Flag = "[?]",
                Probability = 100 - sum,
                Evidence = "Remaining probability"
            });
        }

        return results;
    }

    private string ExtractPlatform(string? label)
    {
        if (string.IsNullOrEmpty(label)) return "Unknown";
        // Remove emoji prefix
        return Regex.Replace(label, @"^[\p{So}\p{Cs}]+\s*", "").Trim();
    }

    private record CountryInfo(string Name, string Flag, string[] Indicators, string[] PhonePrefixes, string[] Tlds);
    
    private class CountryEvidence
    {
        public string CountryCode { get; set; } = "";
        public int TotalWeight { get; set; }
        public List<string> Sources { get; set; } = new();
    }
}
