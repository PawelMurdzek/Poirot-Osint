using System.Globalization;
using System.Text;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Shared text normalisation helpers used by providers that need to retry
/// queries in ASCII form when an upstream API stores names without diacritics.
/// </summary>
public static class TextNormalization
{
    /// <summary>
    /// Strip combining-mark diacritics (NFD decomposition) plus the Polish stroke
    /// letters ł/Ł which aren't combining marks and survive an NFD pass untouched.
    /// "Władysław Gradoń" → "Wladyslaw Gradon".
    /// </summary>
    public static string StripDiacritics(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC)
            .Replace('ł', 'l').Replace('Ł', 'L');
    }

    /// <summary>
    /// True when stripping diacritics would actually change the input — used to
    /// skip the redundant fallback API call when the input is already ASCII.
    /// </summary>
    public static bool HasDiacritics(string? input) =>
        !string.IsNullOrEmpty(input) && !string.Equals(input, StripDiacritics(input), StringComparison.Ordinal);
}
