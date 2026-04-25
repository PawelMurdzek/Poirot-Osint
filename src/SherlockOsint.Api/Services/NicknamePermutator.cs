using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Generates plausible username variations for OSINT pivots.
///
/// Three entry points:
///   - GeneratePermutations(nickname, fullName?) — full set of variants given a known handle.
///   - FullNameToHandleCandidates(fullName) — when only the real name is known, generate
///     the standard handle conventions (firstlast, flast, first.last, last.first, …).
///   - EmailToHandleCandidates(email, fullName?) — extract local part + smart splits.
///
/// Bidirectional pattern recognition:
///   - "pmurdzek" + "Paweł Murdzek"      → also try "pawelmurdzek", "pawel.murdzek", …
///   - "pawelmurdzek" + "Paweł Murdzek" → also try "pmurdzek", "p.murdzek", "pawelm", …
///   - "murdzek" + "Paweł Murdzek"       → also try "pawelmurdzek", "pmurdzek", …
///
/// Diacritics are stripped (Polish "Paweł" → "pawel") because real-world handle
/// registrations on Latin-only platforms always strip them.
/// </summary>
public class NicknamePermutator
{
    private static readonly char[] Separators = { '.', '_', '-' };

    /// <summary>
    /// Build the full permutation set for a (nickname, fullName) pair.
    /// Either argument may be null/empty.
    /// </summary>
    public List<string> GeneratePermutations(string? nickname, string? fullName = null)
    {
        var bag = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var nickClean = StripDiacritics(nickname?.Trim() ?? "").ToLowerInvariant();
        var (first, last) = SplitName(fullName);

        if (!string.IsNullOrEmpty(nickClean)) bag.Add(nickClean);
        if (!string.IsNullOrEmpty(nickname))
        {
            // Keep raw form too — some platforms preserve case
            bag.Add(nickname.Trim());
        }

        // Full-name driven variants (independent of nickname)
        foreach (var v in FullNameToHandleCandidates(fullName)) bag.Add(v);

        // Bidirectional bridges between the supplied nickname and the fullName.
        if (!string.IsNullOrEmpty(nickClean) && !string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(last))
        {
            // Pattern: nickname is first-initial + last (e.g. "pmurdzek")
            if (nickClean == $"{first[0]}{last}")
                AddNameForms(bag, first, last);

            // Pattern: nickname is first + last-initial (e.g. "pawelm")
            if (nickClean == $"{first}{last[0]}")
                AddNameForms(bag, first, last);

            // Pattern: nickname is full first+last (e.g. "pawelmurdzek")
            if (nickClean == $"{first}{last}")
            {
                AddNameForms(bag, first, last);
                AddInitialForms(bag, first, last);
            }

            // Pattern: nickname is last only (e.g. "murdzek")
            if (nickClean == last)
            {
                AddNameForms(bag, first, last);
                AddInitialForms(bag, first, last);
            }

            // Pattern: nickname is first only (e.g. "pawel")
            if (nickClean == first)
            {
                AddNameForms(bag, first, last);
            }
        }

        // Tail-end legacy behaviour: numeric stripping, separator injection, vowel
        // removal, common suffixes — keep these for any free-form nickname.
        if (!string.IsNullOrEmpty(nickClean))
        {
            var numbersMatch = Regex.Match(nickClean, @"^(.*?)(\d+)$");
            if (numbersMatch.Success) bag.Add(numbersMatch.Groups[1].Value);

            if (nickClean.Length >= 3)
            {
                bag.Add($"{nickClean[0]}_{nickClean[1..]}");
                bag.Add($"{nickClean[0]}.{nickClean[1..]}");
                bag.Add($"{nickClean[..^1]}_{nickClean[^1]}");
                bag.Add($"{nickClean[..^1]}.{nickClean[^1]}");

                if (nickClean.Length >= 5)
                {
                    for (int i = 2; i <= nickClean.Length - 2; i++)
                    {
                        bag.Add($"{nickClean[..i]}_{nickClean[i..]}");
                        bag.Add($"{nickClean[..i]}.{nickClean[i..]}");
                        bag.Add($"{nickClean[..i]}-{nickClean[i..]}");
                    }
                }
            }

            var noVowels = Regex.Replace(nickClean, "[aeiou]", "");
            if (noVowels.Length >= 2 && noVowels != nickClean)
            {
                bag.Add(noVowels);
                if (noVowels.Length >= 3) bag.Add(noVowels + "1");
            }

            if (nickClean.Length is > 2 and < 12)
            {
                bag.Add(nickClean + "1");
                bag.Add(nickClean + "0");
                bag.Add(nickClean + "_");
            }
        }

        return bag
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .OrderBy(s => s.Length)
            .Take(80)
            .ToList();
    }

    /// <summary>
    /// Standard handle conventions derived purely from a real name.
    /// e.g. "Paweł Murdzek" → ["paweł", "pawel", "murdzek", "pawelmurdzek", "pmurdzek",
    /// "pawel.murdzek", "pawel_murdzek", "pawel-murdzek", "pawelm", "murdzekp", "murdzek.pawel", …].
    /// </summary>
    public List<string> FullNameToHandleCandidates(string? fullName)
    {
        var bag = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var (first, last) = SplitName(fullName);
        if (string.IsNullOrEmpty(first)) return bag.ToList();

        bag.Add(first);
        if (!string.IsNullOrEmpty(last))
        {
            bag.Add(last);
            AddNameForms(bag, first, last);
            AddInitialForms(bag, first, last);
        }

        // Multi-part names (Mary Jane Smith) — also try first + last_part
        var parts = StripDiacritics(fullName!.ToLowerInvariant())
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 2)
        {
            var f = parts[0];
            var l = parts[^1];
            if (!string.IsNullOrEmpty(f) && !string.IsNullOrEmpty(l))
            {
                AddNameForms(bag, f, l);
                AddInitialForms(bag, f, l);
            }
        }

        return bag.OrderBy(s => s.Length).ToList();
    }

    /// <summary>
    /// Pull plausible handles out of an email address. Local part is the headline
    /// candidate; if it splits on a separator we also try each fragment and
    /// recombinations. If a fullName is also supplied, we cross-check pattern matches
    /// (e.g. "pmurdzek@..." + "Paweł Murdzek" recognises the initial+last pattern).
    /// </summary>
    public List<string> EmailToHandleCandidates(string? email, string? fullName = null)
    {
        var bag = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(email)) return bag.ToList();

        var at = email.IndexOf('@');
        if (at <= 0) return bag.ToList();

        var local = StripDiacritics(email[..at].ToLowerInvariant());
        // Drop "+tag" gmail-style aliases
        var plus = local.IndexOf('+');
        if (plus > 0) local = local[..plus];

        bag.Add(local);

        // Split on common separators and add fragments + recombinations
        var fragments = local.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        if (fragments.Length >= 2)
        {
            foreach (var f in fragments) if (f.Length >= 2) bag.Add(f);
            bag.Add(string.Concat(fragments));
            // Reverse-order recombination (last.first)
            var reversed = fragments.Reverse().ToArray();
            bag.Add(string.Join('.', reversed));
            bag.Add(string.Concat(reversed));
        }

        // Bridge with fullName — feed the local part as if it were a nickname
        if (!string.IsNullOrEmpty(fullName))
        {
            foreach (var v in GeneratePermutations(local, fullName)) bag.Add(v);
        }

        return bag.OrderBy(s => s.Length).Take(40).ToList();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static (string first, string last) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return ("", "");
        var clean = StripDiacritics(fullName.ToLowerInvariant());
        var parts = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ("", "");
        if (parts.Length == 1) return (parts[0], "");
        return (parts[0], parts[^1]); // first + LAST token (handles middle names)
    }

    private static void AddNameForms(HashSet<string> bag, string first, string last)
    {
        bag.Add($"{first}{last}");
        bag.Add($"{first}.{last}");
        bag.Add($"{first}_{last}");
        bag.Add($"{first}-{last}");
        bag.Add($"{last}{first}");
        bag.Add($"{last}.{first}");
        bag.Add($"{last}_{first}");
    }

    private static void AddInitialForms(HashSet<string> bag, string first, string last)
    {
        var fi = first[0];
        var li = last[0];
        bag.Add($"{fi}{last}");
        bag.Add($"{fi}.{last}");
        bag.Add($"{fi}_{last}");
        bag.Add($"{fi}-{last}");
        bag.Add($"{first}{li}");
        bag.Add($"{first}.{li}");
        bag.Add($"{first}_{li}");
        bag.Add($"{fi}{li}");
        bag.Add($"{last}{fi}");
        bag.Add($"{last}.{fi}");
    }

    private static string StripDiacritics(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        // Polish "ł" doesn't decompose — handle explicitly
        return sb.ToString().Normalize(NormalizationForm.FormC)
            .Replace("ł", "l", StringComparison.OrdinalIgnoreCase)
            .Replace("Ł", "L");
    }
}
