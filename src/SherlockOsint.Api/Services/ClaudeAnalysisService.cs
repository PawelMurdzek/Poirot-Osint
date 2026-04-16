using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Calls Claude API to assess identity candidates discovered by OSINT providers.
/// Replaces hardcoded probability scoring with Claude's reasoning.
/// </summary>
public class ClaudeAnalysisService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ClaudeAnalysisService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeAnalysisService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<ClaudeAnalysisService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Sends all collected candidates to Claude and receives probability assessments.
    /// Returns empty list if API key is missing or request fails.
    /// </summary>
    public async Task<List<CandidateAssessment>> AnalyzeCandidatesAsync(
        SearchRequest request,
        List<TargetCandidate> candidates,
        CancellationToken ct = default)
    {
        var apiKey = _config["Osint:ClaudeApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Claude API key not configured (Osint:ClaudeApiKey) - skipping AI analysis");
            return [];
        }

        if (candidates.Count == 0)
            return [];

        var prompt = BuildPrompt(request, candidates);

        var requestBody = new
        {
            model = "claude-sonnet-4-6",
            max_tokens = 2048,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("Claude");
            var body = JsonSerializer.Serialize(requestBody, JsonOpts);

            using var response = await client.PostAsync(
                "https://api.anthropic.com/v1/messages",
                new StringContent(body, Encoding.UTF8, "application/json"),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Claude API returned {Status}: {Body}", response.StatusCode, err);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var assessments = ParseResponse(json);
            _logger.LogInformation("Claude assessed {Count} candidates", assessments.Count);
            return assessments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Claude API");
            return [];
        }
    }

    private string BuildPrompt(SearchRequest request, List<TargetCandidate> candidates)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an expert OSINT analyst. Analyze the OSINT search results below and for each candidate assess how likely they are the person being searched for.");
        sb.AppendLine();
        sb.AppendLine("## Search Query");

        if (!string.IsNullOrEmpty(request.FullName))
            sb.AppendLine($"- Full name: {request.FullName}");
        if (!string.IsNullOrEmpty(request.Email))
            sb.AppendLine($"- Email: {request.Email}");
        if (!string.IsNullOrEmpty(request.Phone))
            sb.AppendLine($"- Phone: {request.Phone}");
        if (!string.IsNullOrEmpty(request.Nickname))
            sb.AppendLine($"- Nickname/username: {request.Nickname}");

        sb.AppendLine();
        sb.AppendLine("## Candidates Found");

        foreach (var c in candidates)
        {
            sb.AppendLine($"### Candidate ID: {c.Id}");
            sb.AppendLine($"- Primary username: {c.PrimaryUsername}");

            if (!string.IsNullOrEmpty(c.Name) && c.Name != c.PrimaryUsername)
                sb.AppendLine($"- Discovered name: {c.Name}");
            if (c.KnownAliases.Count > 0)
                sb.AppendLine($"- Aliases: {string.Join(", ", c.KnownAliases)}");
            if (!string.IsNullOrEmpty(c.ProbableLocation))
                sb.AppendLine($"- Location: {c.ProbableLocation}");
            if (!string.IsNullOrEmpty(c.MergeReason))
                sb.AppendLine($"- Profiles linked by: {c.MergeReason}");
            if (c.VerifiedEmails.Count > 0)
            {
                var emailList = c.VerifiedEmails.Select(e =>
                    $"{e.Email} (source: {e.Source}, verified: {e.IsVerified})");
                sb.AppendLine($"- Emails: {string.Join("; ", emailList)}");
            }

            sb.AppendLine($"- Platforms found ({c.Sources.Count}):");
            foreach (var s in c.Sources)
            {
                sb.Append($"  - {s.Platform}: username={s.Username}");
                if (!string.IsNullOrEmpty(s.DisplayName))
                    sb.Append($", display name={s.DisplayName}");
                if (!string.IsNullOrEmpty(s.Bio))
                {
                    var bio = s.Bio.Replace('\n', ' ');
                    if (bio.Length > 120) bio = bio[..120] + "...";
                    sb.Append($", bio=\"{bio}\"");
                }
                if (s.ExtractedData.TryGetValue("location", out var loc))
                    sb.Append($", location={loc}");
                if (s.ExtractedData.TryGetValue("company", out var company))
                    sb.Append($", company={company}");
                sb.AppendLine();
            }

            if (c.InferredAttributes.Count > 0)
                sb.AppendLine($"- Inferred attributes: {string.Join(", ", c.InferredAttributes)}");
        }

        sb.AppendLine();
        sb.AppendLine("## Your Task");
        sb.AppendLine("For each candidate provide:");
        sb.AppendLine("- `candidateId`: the candidate's ID (copy exactly from above)");
        sb.AppendLine("- `probabilityScore` (0-95): probability this is the searched person. Weight: exact username match > email match > name match > location match > number of platforms.");
        sb.AppendLine("- `consistencyAnalysis`: 1-3 sentences explaining what evidence supports this candidate.");
        sb.AppendLine("- `uncertaintyNotes`: 1-2 sentences on what is uncertain or potentially misleading.");
        sb.AppendLine("- `professionalRole`: inferred professional role or null (e.g. \"Software Developer\", \"Content Creator\", \"Musician\").");
        sb.AppendLine("- `activitySummary`: brief description of digital footprint and online activity patterns, or null.");
        sb.AppendLine("- `confidenceLow`: lower bound of confidence interval (0-100).");
        sb.AppendLine("- `confidenceHigh`: upper bound of confidence interval (0-100).");
        sb.AppendLine();
        sb.AppendLine("Respond ONLY with a valid JSON array, no markdown, no explanation:");
        sb.AppendLine("[{\"candidateId\":\"...\",\"probabilityScore\":72,\"consistencyAnalysis\":\"...\",\"uncertaintyNotes\":\"...\",\"professionalRole\":\"Developer\",\"activitySummary\":\"...\",\"confidenceLow\":60,\"confidenceHigh\":84}]");

        return sb.ToString();
    }

    private List<CandidateAssessment> ParseResponse(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "[]";

            // Extract the JSON array even if Claude wrapped it in extra text
            var start = text.IndexOf('[');
            var end = text.LastIndexOf(']');
            if (start == -1 || end == -1 || end < start)
            {
                _logger.LogWarning("Claude response did not contain a JSON array");
                return [];
            }

            var jsonArray = text[start..(end + 1)];
            return JsonSerializer.Deserialize<List<CandidateAssessment>>(jsonArray, JsonOpts) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Claude response");
            return [];
        }
    }
}

/// <summary>
/// Claude's assessment of a single candidate
/// </summary>
public class CandidateAssessment
{
    [JsonPropertyName("candidateId")]
    public string CandidateId { get; set; } = "";

    [JsonPropertyName("probabilityScore")]
    public int ProbabilityScore { get; set; }

    [JsonPropertyName("consistencyAnalysis")]
    public string ConsistencyAnalysis { get; set; } = "";

    [JsonPropertyName("uncertaintyNotes")]
    public string UncertaintyNotes { get; set; } = "";

    [JsonPropertyName("professionalRole")]
    public string? ProfessionalRole { get; set; }

    [JsonPropertyName("activitySummary")]
    public string? ActivitySummary { get; set; }

    [JsonPropertyName("confidenceLow")]
    public int ConfidenceLow { get; set; }

    [JsonPropertyName("confidenceHigh")]
    public int ConfidenceHigh { get; set; }
}
