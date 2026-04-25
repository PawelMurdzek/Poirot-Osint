using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Persists top-N candidates from each search to a JSON+MD pair under the
/// project-root /sessions/ folder. The folder is the "memory" the user runs
/// the Claude CLI against when no Osint:ClaudeApiKey is configured — we hand
/// the client the exact `claude` command on search-complete instead of
/// emitting arbitrary fallback probability scores.
///
/// Path resolution: walks up from the running binary looking for a marker
/// (.git, SherlockOsint.sln) so it works for both `dotnet run` from the
/// project dir and Docker (where the working dir is /app).
/// </summary>
public class SessionMemoryService
{
    private const int MarkdownDigestSize = 10;
    private static readonly string[] RootMarkers = [".git", "SherlockOsint.sln"];

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IConfiguration _config;
    private readonly ILogger<SessionMemoryService> _logger;
    private readonly Lazy<string> _sessionsRoot;

    public SessionMemoryService(IConfiguration config, ILogger<SessionMemoryService> logger)
    {
        _config = config;
        _logger = logger;
        _sessionsRoot = new Lazy<string>(ResolveSessionsRoot);
    }

    public string SessionsRoot => _sessionsRoot.Value;

    /// <summary>
    /// Persist the search session.
    /// - .json gets ALL candidates when Claude API isn't available (CLI-Claude
    ///   ranks the full set), or just the top digest when it is — we don't
    ///   need to ship the long tail twice if AI already scored.
    /// - .md is always a top-10 digest, human-readable, and it explicitly
    ///   tells Claude that the .json is the authoritative source.
    /// </summary>
    public async Task<SessionMemoryRecord> PersistAsync(
        SearchRequest request,
        IReadOnlyList<TargetCandidate> candidates,
        bool aiScoringAvailable,
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(SessionsRoot);

        var ordered = candidates
            .OrderByDescending(c => c.ProbabilityScore)
            .ToList();

        var jsonCandidates = aiScoringAvailable
            ? ordered.Take(MarkdownDigestSize).ToList()  // AI-ranked highlights
            : ordered;                                    // full set for CLI-Claude

        var markdownDigest = ordered.Take(MarkdownDigestSize).ToList();

        var timestamp = DateTime.UtcNow;
        var slug = BuildSlug(request);
        var baseName = $"{timestamp:yyyyMMdd-HHmmss}-{slug}";

        var jsonPath = Path.Combine(SessionsRoot, baseName + ".json");
        var markdownPath = Path.Combine(SessionsRoot, baseName + ".md");

        var snapshot = new SessionSnapshot
        {
            Timestamp = timestamp,
            Query = new QuerySnapshot
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Nickname = request.Nickname
            },
            AiScoringAvailable = aiScoringAvailable,
            TotalCandidateCount = candidates.Count,
            CandidateCount = jsonCandidates.Count,
            TopCandidates = jsonCandidates
        };

        await File.WriteAllTextAsync(
            jsonPath,
            JsonSerializer.Serialize(snapshot, JsonOpts),
            Encoding.UTF8,
            ct);

        await File.WriteAllTextAsync(
            markdownPath,
            BuildMarkdown(snapshot, markdownDigest, jsonPath),
            Encoding.UTF8,
            ct);

        var record = new SessionMemoryRecord
        {
            FolderPath = SessionsRoot,
            JsonPath = jsonPath,
            MarkdownPath = markdownPath,
            FileBaseName = baseName,
            ClaudeCommand = BuildClaudeCommand(jsonPath, markdownPath),
            ClaudePromptHint = BuildPromptHint(request)
        };

        _logger.LogInformation(
            "Persisted session memory to {Path} (json={JsonCount}/{Total}, md=top {MdCount}, ai={Ai})",
            jsonPath, jsonCandidates.Count, candidates.Count, markdownDigest.Count, aiScoringAvailable);

        return record;
    }

    private string ResolveSessionsRoot()
    {
        var configured = _config["Osint:SessionsPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.IsPathRooted(configured)
                ? configured
                : Path.GetFullPath(configured, AppContext.BaseDirectory);
        }

        // Walk up from the binary looking for a project-root marker
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (RootMarkers.Any(m => Path.Exists(Path.Combine(dir.FullName, m))))
                return Path.Combine(dir.FullName, "sessions");
            dir = dir.Parent;
        }

        // Fallback: alongside the binary
        return Path.Combine(AppContext.BaseDirectory, "sessions");
    }

    private static string BuildSlug(SearchRequest request)
    {
        var basis = request.Nickname
            ?? (request.Email is { Length: > 0 } e ? e.Split('@')[0] : null)
            ?? request.FullName
            ?? request.Phone
            ?? "search";

        var sb = new StringBuilder(basis.Length);
        foreach (var ch in basis.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (ch is ' ' or '-' or '_' or '.') sb.Append('-');
        }
        var slug = sb.ToString().Trim('-');
        if (slug.Length == 0) slug = "search";
        if (slug.Length > 40) slug = slug[..40];
        return slug;
    }

    private static string BuildClaudeCommand(string jsonPath, string markdownPath)
    {
        // The .json is the source of truth (full candidate list with all
        // sources, signals, IsFromUserInput flag); the .md is just a human
        // digest. Tell Claude that explicitly so it doesn't stop at the
        // markdown's top-10 summary.
        var prompt = $"Read \"{jsonPath}\" — that is the AUTHORITATIVE source: full candidate list with sources, IsFromUserInput flags, merge reasons. " +
                     $"\"{markdownPath}\" is only a top-10 digest for humans, refer to it for context but rank from the JSON. " +
                     "These OSINT candidates were not scored because no Anthropic API key was configured. " +
                     "For each candidate in the JSON, judge how likely they are the search target. " +
                     "Weight: exact username match > email match > name match > location match > number of platforms; downrank candidates where IsFromUserInput is false unless multiple platforms corroborate. " +
                     "Output a Markdown table sorted by probability with: rank, primaryUsername, probability%, one-line evidence, one-line uncertainty. " +
                     "After the table add a 'Suggested next steps' section with 3-5 follow-up actions (handles to try next, platforms to scan, lookups to add).";

        var escaped = prompt.Replace("\"", "\\\"");
        return $"claude -p \"{escaped}\"";
    }

    private static string BuildPromptHint(SearchRequest request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.FullName)) parts.Add($"name={request.FullName}");
        if (!string.IsNullOrWhiteSpace(request.Email))    parts.Add($"email={request.Email}");
        if (!string.IsNullOrWhiteSpace(request.Phone))    parts.Add($"phone={request.Phone}");
        if (!string.IsNullOrWhiteSpace(request.Nickname)) parts.Add($"nickname={request.Nickname}");
        return parts.Count > 0 ? string.Join(", ", parts) : "(no query fields supplied)";
    }

    private static string BuildMarkdown(SessionSnapshot snapshot, List<TargetCandidate> digest, string jsonPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# OSINT Session — {snapshot.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine($"> **Authoritative source for ranking**: `{jsonPath}` ({snapshot.CandidateCount} candidates).");
        sb.AppendLine($"> This file is a top-{digest.Count} human digest only. AI scoring available: **{(snapshot.AiScoringAvailable ? "yes" : "no — rank from JSON")}**.");
        sb.AppendLine();
        sb.AppendLine("## Query");
        if (!string.IsNullOrWhiteSpace(snapshot.Query.FullName)) sb.AppendLine($"- **Full name**: {snapshot.Query.FullName}");
        if (!string.IsNullOrWhiteSpace(snapshot.Query.Email))    sb.AppendLine($"- **Email**: {snapshot.Query.Email}");
        if (!string.IsNullOrWhiteSpace(snapshot.Query.Phone))    sb.AppendLine($"- **Phone**: {snapshot.Query.Phone}");
        if (!string.IsNullOrWhiteSpace(snapshot.Query.Nickname)) sb.AppendLine($"- **Nickname**: {snapshot.Query.Nickname}");
        sb.AppendLine();
        sb.AppendLine($"_{snapshot.TotalCandidateCount} total candidates discovered, top {digest.Count} digested below._");
        sb.AppendLine();

        for (int i = 0; i < digest.Count; i++)
        {
            var c = digest[i];
            sb.AppendLine($"## {i + 1}. {c.PrimaryUsername} {(c.IsFromUserInput ? "(user-input)" : "(permutation)")}");
            if (!string.IsNullOrEmpty(c.Name) && c.Name != c.PrimaryUsername) sb.AppendLine($"- **Discovered name**: {c.Name}");
            if (c.KnownAliases.Count > 0)        sb.AppendLine($"- **Aliases**: {string.Join(", ", c.KnownAliases)}");
            if (!string.IsNullOrEmpty(c.ProbableLocation)) sb.AppendLine($"- **Location**: {c.ProbableLocation}");
            if (!string.IsNullOrEmpty(c.MergeReason))      sb.AppendLine($"- **Merge reason**: {c.MergeReason}");
            if (c.VerifiedEmails.Count > 0)
            {
                var emails = string.Join("; ", c.VerifiedEmails.Select(e => $"{e.Email} (source={e.Source}, verified={e.IsVerified})"));
                sb.AppendLine($"- **Emails**: {emails}");
            }
            sb.AppendLine($"- **Platforms ({c.Sources.Count})**:");
            foreach (var s in c.Sources)
            {
                var line = $"  - {s.Platform}: `{s.Username}` — {s.Url}";
                if (!string.IsNullOrEmpty(s.DisplayName)) line += $" — display=\"{s.DisplayName}\"";
                if (!string.IsNullOrEmpty(s.Bio))
                {
                    var bio = s.Bio.Replace('\n', ' ');
                    if (bio.Length > 160) bio = bio[..160] + "...";
                    line += $" — bio=\"{bio}\"";
                }
                sb.AppendLine(line);
            }
            if (c.InferredAttributes.Count > 0)
                sb.AppendLine($"- **Inferred attributes**: {string.Join(", ", c.InferredAttributes)}");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine($"**Task for Claude**: open `{jsonPath}` (JSON = full {snapshot.CandidateCount}-candidate set, source of truth) and rank candidates by how likely they are the search target. ");
        sb.AppendLine("Weight: exact username match > email match > name match > location match > number of platforms. Downrank `IsFromUserInput=false` candidates unless multiple platforms corroborate. ");
        sb.AppendLine("For each, give probability (0-95), one-line evidence, one-line uncertainty.");
        sb.AppendLine();
        sb.AppendLine("Then add a **Suggested next steps** section: 3-5 follow-up actions (handles to try, platforms to scan, lookups to add).");
        return sb.ToString();
    }
}

public sealed class SessionMemoryRecord
{
    public string FolderPath { get; set; } = "";
    public string JsonPath { get; set; } = "";
    public string MarkdownPath { get; set; } = "";
    public string FileBaseName { get; set; } = "";
    public string ClaudeCommand { get; set; } = "";
    public string ClaudePromptHint { get; set; } = "";
}

internal sealed class SessionSnapshot
{
    public DateTime Timestamp { get; set; }
    public QuerySnapshot Query { get; set; } = new();
    public bool AiScoringAvailable { get; set; }
    /// <summary>How many candidates were discovered in total before any trimming.</summary>
    public int TotalCandidateCount { get; set; }
    /// <summary>How many candidates this snapshot actually contains in TopCandidates.</summary>
    public int CandidateCount { get; set; }
    public List<TargetCandidate> TopCandidates { get; set; } = [];
}

internal sealed class QuerySnapshot
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Nickname { get; set; }
}
