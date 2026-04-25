using System.Text;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Tui.Render;

/// <summary>
/// Builds a copy-paste-ready prompt that the user can drop into a fresh `claude`
/// (Claude Code CLI) session — meant for max-effort manual analysis on Opus,
/// complementing the API's lighter automatic profiler.
///
/// The TUI prints this in a panel after a search completes and tells the user:
///   "Skopiuj poniższe i wklej do `claude` w nowym terminalu."
/// </summary>
public static class PromptBuilder
{
    public static string BuildPersonalityPrompt(
        SearchRequest request,
        DigitalProfile? profile,
        List<TargetCandidate> candidates,
        List<PersonalityProfile> autoProfiles,
        string knowledgeBasePath = "OSINT/")
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Pogłębiona analiza osobowości — Poirot OSINT");
        sb.AppendLine();
        sb.AppendLine("Działaj jako ekspert OSINT. Mam zebrane poniżej dane z wyszukiwania Poirot.");
        sb.AppendLine($"Skorzystaj z bazy wiedzy w `{knowledgeBasePath}` (Read tool) jako kontekstu o platformach,");
        sb.AppendLine("regionalnych ekosystemach i wzorcach wykrywania sock-puppetów.");
        sb.AppendLine();
        sb.AppendLine("Dla TOP-3 kandydatów wygeneruj:");
        sb.AppendLine("1. **Streszczenie osobowości** — 4-6 zdań");
        sb.AppendLine("2. **5-7 sygnałów behawioralnych** — każdy zacytowany ze ścieżką pliku z bazy wiedzy");
        sb.AppendLine("3. **Sock-puppet red flags** — konkretne, oparte na: wieku konta, kadencji postowania, brakach cross-platform consistency. Albo \"brak\".");
        sb.AppendLine("4. **Kontekst regionalny / językowy** — wnioski z mieszanki platform");
        sb.AppendLine("5. **Confidence (0-100) + uzasadnienie**");
        sb.AppendLine("6. **Co byś dopytał OSINT-em** — 3 hipotezy + jakie selektory by je sprawdziły");
        sb.AppendLine();
        sb.AppendLine("## Search query");
        if (!string.IsNullOrEmpty(request.FullName)) sb.AppendLine($"- Pełne imię: {request.FullName}");
        if (!string.IsNullOrEmpty(request.Email)) sb.AppendLine($"- Email: {request.Email}");
        if (!string.IsNullOrEmpty(request.Phone)) sb.AppendLine($"- Telefon: {request.Phone}");
        if (!string.IsNullOrEmpty(request.Nickname)) sb.AppendLine($"- Nickname: {request.Nickname}");

        if (profile != null)
        {
            sb.AppendLine();
            sb.AppendLine("## Aggregated DigitalProfile");
            if (!string.IsNullOrEmpty(profile.Name)) sb.AppendLine($"- Discovered name: {profile.Name}");
            if (profile.Platforms.Count > 0)
                sb.AppendLine($"- Platforms ({profile.Platforms.Count}): {string.Join(", ", profile.Platforms.Select(p => p.Name))}");
            sb.AppendLine($"- Confidence (heuristic): {profile.ConfidenceScore}%");
        }

        if (candidates.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"## Kandydaci ({candidates.Count})");
            foreach (var c in candidates.Take(5))
            {
                sb.AppendLine();
                sb.AppendLine($"### Candidate `{c.PrimaryUsername}` — probability {c.ProbabilityScore}/100");
                if (!string.IsNullOrEmpty(c.Name) && c.Name != c.PrimaryUsername)
                    sb.AppendLine($"- Name: {c.Name}");
                if (c.KnownAliases.Count > 0)
                    sb.AppendLine($"- Aliases: {string.Join(", ", c.KnownAliases)}");
                if (!string.IsNullOrEmpty(c.ProbableLocation))
                    sb.AppendLine($"- Location: {c.ProbableLocation}");
                if (!string.IsNullOrEmpty(c.ProfessionalRole))
                    sb.AppendLine($"- Role (inferred): {c.ProfessionalRole}");
                if (c.VerifiedEmails.Count > 0)
                    sb.AppendLine($"- Emails: {string.Join(", ", c.VerifiedEmails.Select(e => e.Email))}");
                if (!string.IsNullOrEmpty(c.ConsistencyAnalysis))
                    sb.AppendLine($"- Consistency: {c.ConsistencyAnalysis}");
                if (!string.IsNullOrEmpty(c.UncertaintyNotes))
                    sb.AppendLine($"- Uncertainty: {c.UncertaintyNotes}");

                sb.AppendLine($"- Platforms ({c.Sources.Count}):");
                foreach (var s in c.Sources)
                {
                    sb.Append($"  - {s.Platform} ({s.Username})");
                    if (!string.IsNullOrEmpty(s.Url)) sb.Append($" — {s.Url}");
                    if (!string.IsNullOrEmpty(s.Bio))
                    {
                        var bio = s.Bio.Replace('\n', ' ');
                        if (bio.Length > 200) bio = bio[..200] + "…";
                        sb.Append($" — bio: \"{bio}\"");
                    }
                    sb.AppendLine();
                }

                if (c.InferredAttributes.Count > 0)
                    sb.AppendLine($"- Atrybuty: {string.Join(", ", c.InferredAttributes)}");
            }
        }

        if (autoProfiles.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Wstępna analiza (Sonnet, automatyczna)");
            sb.AppendLine("Poniższe profile wygenerował lżejszy model w API. Potraktuj jako baseline — chcę głębiej.");
            foreach (var p in autoProfiles)
            {
                sb.AppendLine();
                sb.AppendLine($"### {p.CandidateUsername}");
                sb.AppendLine(p.Summary);
                if (p.BehavioralIndicators.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Behavioral indicators (auto):");
                    foreach (var ind in p.BehavioralIndicators) sb.AppendLine($"- {ind}");
                }
                if (p.SockPuppetRedFlags.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Sock-puppet red flags (auto):");
                    foreach (var f in p.SockPuppetRedFlags) sb.AppendLine($"- {f}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("Zaczynaj.");
        return sb.ToString();
    }
}
