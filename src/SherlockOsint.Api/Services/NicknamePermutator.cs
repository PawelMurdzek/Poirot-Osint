using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Generates logical permutations of a nickname to find similar identities.
/// Example: "peszew" -> ["pe_szew", "pe.szew", "pe-szew", "peszew", "p.eszew", "ptrg"]
/// </summary>
public class NicknamePermutator
{
    public List<string> GeneratePermutations(string nickname, string? fullName = null)
    {
        if (string.IsNullOrWhiteSpace(nickname) && string.IsNullOrWhiteSpace(fullName))
            return new List<string>();

        var permutations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        if (!string.IsNullOrEmpty(nickname))
            permutations.Add(nickname);

        // 1. Full Name Heuristics (e.g. "Piotr Szewczyk" -> "piotrszewczyk", "pszewczyk")
        if (!string.IsNullOrEmpty(fullName))
        {
            var clean = new string(fullName.Where(char.IsLetterOrDigit).ToArray()).ToLower();
            if (clean.Length > 3) permutations.Add(clean);

            var parts = fullName.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                permutations.Add(parts[0] + parts[1]);
                permutations.Add(parts[0][0] + parts[1]);
                permutations.Add(parts[0] + "." + parts[1]);
                permutations.Add(parts[0] + "_" + parts[1]);
            }
        }

        if (string.IsNullOrEmpty(nickname))
            return permutations.ToList();

        // 2. Suffix/Prefix variations
        var numbersMatch = Regex.Match(nickname, @"^(.*?)(\d+)$");
        if (numbersMatch.Success)
            permutations.Add(numbersMatch.Groups[1].Value);

        // 3. Separator Injection (Smart Splits)
        if (nickname.Length >= 3)
        {
            // Always try injecting at common points (near start/end)
            permutations.Add($"{nickname[0]}_{nickname[1..]}");
            permutations.Add($"{nickname[0]}.{nickname[1..]}");
            permutations.Add($"{nickname[..^1]}_{nickname[^1]}");
            permutations.Add($"{nickname[..^1]}.{nickname[^1]}");

            if (nickname.Length >= 5)
            {
                for (int i = 2; i <= nickname.Length - 2; i++)
                {
                    var part1 = nickname[..i];
                    var part2 = nickname[i..];
                    permutations.Add($"{part1}_{part2}");
                    permutations.Add($"{part1}.{part2}");
                    permutations.Add($"{part1}-{part2}");
                }
            }
        }

        // 4. Vowel Removal (Common for handles)
        var noVowels = Regex.Replace(nickname, "[aeiouAEIOU]", "");
        if (noVowels.Length >= 2 && noVowels != nickname)
        {
            permutations.Add(noVowels);
            if (noVowels.Length >= 3)
            {
                permutations.Add(noVowels + "1");
            }
        }

        // 5. Common Suffixes
        if (nickname.Length > 2 && nickname.Length < 12)
        {
            permutations.Add(nickname + "1");
            permutations.Add(nickname + "0");
            permutations.Add(nickname + "_");
        }

        return permutations.OrderBy(p => p.Length).Take(50).ToList();
    }
}
