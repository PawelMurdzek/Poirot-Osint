using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SherlockOsint.Api.Services.Knowledge;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Deep personality / behavioral profiler for a single TargetCandidate.
///
/// Architecture:
///   1. Build initial system + user prompt with the candidate's evidence.
///   2. Hand Claude three tools:
///        - search_knowledge(query)   — BM25 search over OSINT/*.md
///        - read_full_file(path)      — full text of an indexed markdown file
///        - finalize_profile(json)    — emit the final structured PersonalityProfile
///   3. Loop up to MaxIterations turns, executing each tool call and feeding
///      tool_result blocks back to Claude until finalize_profile is invoked.
///
/// Skips silently (returns null) when Osint:ClaudeApiKey is missing.
/// </summary>
public class PersonalityProfilerService
{
    private const int MaxIterations = 5;
    private const string DefaultModel = "claude-sonnet-4-6";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly KnowledgeBase _knowledge;
    private readonly ILogger<PersonalityProfilerService> _logger;

    public PersonalityProfilerService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        KnowledgeBase knowledge,
        ILogger<PersonalityProfilerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _knowledge = knowledge;
        _logger = logger;
    }

    public async Task<PersonalityProfile?> ProfileAsync(
        SearchRequest request,
        TargetCandidate candidate,
        CancellationToken ct = default)
    {
        var apiKey = _config["Osint:ClaudeApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("PersonalityProfiler skipped — Osint:ClaudeApiKey not configured");
            return null;
        }

        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(request, candidate);

        var messages = new List<object>
        {
            new { role = "user", content = userPrompt }
        };

        PersonalityProfile? finalProfile = null;
        var iterations = 0;
        var citations = new List<KnowledgeCitation>();

        for (int iter = 0; iter < MaxIterations && finalProfile == null; iter++)
        {
            iterations = iter + 1;
            ct.ThrowIfCancellationRequested();

            var response = await CallClaudeAsync(systemPrompt, messages, ct);
            if (response is null) break;

            // Append assistant message verbatim (with all content blocks) so subsequent
            // tool_result messages reference matching tool_use_ids.
            messages.Add(new { role = "assistant", content = response.Value.GetProperty("content") });

            var toolResults = new List<object>();
            var stopReason = response.Value.TryGetProperty("stop_reason", out var sr) ? sr.GetString() : null;

            foreach (var block in response.Value.GetProperty("content").EnumerateArray())
            {
                var blockType = block.TryGetProperty("type", out var t) ? t.GetString() : null;
                if (blockType != "tool_use") continue;

                var toolUseId = block.GetProperty("id").GetString() ?? "";
                var toolName = block.GetProperty("name").GetString() ?? "";
                var input = block.GetProperty("input");

                var toolResult = await ExecuteToolAsync(toolName, input, citations, candidate);
                toolResults.Add(new
                {
                    type = "tool_result",
                    tool_use_id = toolUseId,
                    content = toolResult.Content,
                    is_error = toolResult.IsError
                });

                if (toolName == "finalize_profile" && toolResult.FinalProfile != null)
                {
                    finalProfile = toolResult.FinalProfile;
                    finalProfile.CandidateId = candidate.Id;
                    finalProfile.CandidateUsername = candidate.PrimaryUsername;
                    finalProfile.Citations = citations;
                    finalProfile.IterationsUsed = iterations;
                }
            }

            if (toolResults.Count == 0)
            {
                // Claude stopped without calling a tool — bail out
                _logger.LogWarning("Profiler loop ended for {Candidate} without finalize_profile (stop_reason={Reason})",
                    candidate.PrimaryUsername, stopReason);
                break;
            }

            messages.Add(new { role = "user", content = toolResults });
        }

        if (finalProfile != null)
        {
            _logger.LogInformation("Profiled {Candidate} in {Iter} iterations, {Citations} citations",
                candidate.PrimaryUsername, iterations, citations.Count);
        }

        return finalProfile;
    }

    // ── Prompt building ─────────────────────────────────────────────────────────

    private string BuildSystemPrompt() =>
        """
        You are an OSINT personality analyst. You are given a candidate identity (a person
        suspected to match a search query) and you can query a curated OSINT knowledge base
        about social-media platforms, regional ecosystems, sock-puppet detection, and
        operational tradecraft.

        Your job:
          1. Look at the candidate's platforms / handles / region / activity.
          2. Query the knowledge base for relevant context (use search_knowledge with concise
             keyword queries — platform names, regional terms, behavioural patterns).
          3. Read full files only when a specific section needs deeper context.
          4. When you have enough grounding, call finalize_profile EXACTLY ONCE with a
             concise behavioral profile.

        Hard rules:
          - Ground every claim in something you actually read (or the candidate evidence).
            If you cannot ground it, say so explicitly in the summary.
          - Sock-puppet red flags must be specific: account age, posting cadence, missing
            cross-platform consistency, suspiciously perfect metadata. No generic warnings.
          - Use at most 5 turns. Do NOT call search_knowledge more than 3 times — prefer
            depth over breadth.
          - Output of finalize_profile is the final answer. Do not narrate after it.
        """;

    private string BuildUserPrompt(SearchRequest request, TargetCandidate candidate)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Candidate to profile");
        sb.AppendLine();
        sb.AppendLine("## Search query");
        if (!string.IsNullOrEmpty(request.FullName)) sb.AppendLine($"- Full name: {request.FullName}");
        if (!string.IsNullOrEmpty(request.Email)) sb.AppendLine($"- Email: {request.Email}");
        if (!string.IsNullOrEmpty(request.Phone)) sb.AppendLine($"- Phone: {request.Phone}");
        if (!string.IsNullOrEmpty(request.Nickname)) sb.AppendLine($"- Nickname: {request.Nickname}");
        sb.AppendLine();
        sb.AppendLine("## Candidate evidence");
        sb.AppendLine($"- Primary username: {candidate.PrimaryUsername}");
        if (!string.IsNullOrEmpty(candidate.Name) && candidate.Name != candidate.PrimaryUsername)
            sb.AppendLine($"- Name: {candidate.Name}");
        if (candidate.KnownAliases.Count > 0)
            sb.AppendLine($"- Aliases: {string.Join(", ", candidate.KnownAliases)}");
        if (!string.IsNullOrEmpty(candidate.ProbableLocation))
            sb.AppendLine($"- Location: {candidate.ProbableLocation}");
        if (!string.IsNullOrEmpty(candidate.ProfessionalRole))
            sb.AppendLine($"- Inferred role: {candidate.ProfessionalRole}");
        if (candidate.VerifiedEmails.Count > 0)
            sb.AppendLine($"- Emails: {string.Join(", ", candidate.VerifiedEmails.Select(e => e.Email))}");
        sb.AppendLine($"- Probability score (heuristic): {candidate.ProbabilityScore}/100");

        sb.AppendLine($"- Platforms ({candidate.Sources.Count}):");
        foreach (var s in candidate.Sources)
        {
            sb.Append($"  - {s.Platform} ({s.Username})");
            if (!string.IsNullOrEmpty(s.Bio))
            {
                var bio = s.Bio.Replace('\n', ' ');
                if (bio.Length > 160) bio = bio[..160] + "…";
                sb.Append($" — bio: \"{bio}\"");
            }
            sb.AppendLine();
        }

        if (candidate.InferredAttributes.Count > 0)
            sb.AppendLine($"- Inferred attributes: {string.Join(", ", candidate.InferredAttributes)}");

        sb.AppendLine();
        sb.AppendLine("Begin by deciding what knowledge-base queries (if any) would help, then build the profile.");
        return sb.ToString();
    }

    // ── Claude call ─────────────────────────────────────────────────────────────

    private async Task<JsonElement?> CallClaudeAsync(string systemPrompt, List<object> messages, CancellationToken ct)
    {
        var model = _config["Osint:ClaudeModel"];
        if (string.IsNullOrWhiteSpace(model)) model = DefaultModel;

        var requestBody = new
        {
            model,
            max_tokens = 2048,
            system = systemPrompt,
            tools = ToolDefinitions,
            messages
        };

        try
        {
            var client = _httpClientFactory.CreateClient("Claude");
            var body = JsonSerializer.Serialize(requestBody, JsonOpts);

            using var response = await client.PostAsync(
                "https://api.anthropic.com/v1/messages",
                new StringContent(body, Encoding.UTF8, "application/json"),
                ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Claude profiler call returned {Status}: {Body}", response.StatusCode, json);
                return null;
            }

            return JsonDocument.Parse(json).RootElement.Clone();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profiler Claude call threw");
            return null;
        }
    }

    // ── Tool execution ──────────────────────────────────────────────────────────

    private record ToolExecutionResult(string Content, bool IsError, PersonalityProfile? FinalProfile);

    private Task<ToolExecutionResult> ExecuteToolAsync(string toolName, JsonElement input, List<KnowledgeCitation> citations, TargetCandidate candidate)
    {
        return toolName switch
        {
            "search_knowledge" => Task.FromResult(ExecuteSearchKnowledge(input, citations)),
            "read_full_file" => Task.FromResult(ExecuteReadFullFile(input, citations)),
            "finalize_profile" => Task.FromResult(ExecuteFinalizeProfile(input, candidate)),
            _ => Task.FromResult(new ToolExecutionResult($"Unknown tool: {toolName}", true, null))
        };
    }

    private ToolExecutionResult ExecuteSearchKnowledge(JsonElement input, List<KnowledgeCitation> citations)
    {
        var query = input.TryGetProperty("query", out var q) && q.ValueKind == JsonValueKind.String ? q.GetString() ?? "" : "";
        if (string.IsNullOrWhiteSpace(query))
            return new ToolExecutionResult("query is required", true, null);

        var hits = _knowledge.Search(query, topN: 5);
        if (hits.Count == 0)
            return new ToolExecutionResult($"No matches for '{query}'", false, null);

        var sb = new StringBuilder();
        sb.AppendLine($"Top {hits.Count} matches for '{query}':");
        foreach (var (chunk, score) in hits)
        {
            sb.AppendLine();
            sb.AppendLine($"### [{chunk.FilePath} → {chunk.Anchor}] (score {score:0.00})");
            var excerpt = chunk.Text.Length > 800 ? chunk.Text[..800] + "…" : chunk.Text;
            sb.AppendLine(excerpt);

            citations.Add(new KnowledgeCitation
            {
                FilePath = chunk.FilePath,
                Anchor = chunk.Anchor,
                Excerpt = chunk.Text.Length > 300 ? chunk.Text[..300] + "…" : chunk.Text
            });
        }
        return new ToolExecutionResult(sb.ToString(), false, null);
    }

    private ToolExecutionResult ExecuteReadFullFile(JsonElement input, List<KnowledgeCitation> citations)
    {
        var path = input.TryGetProperty("path", out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() ?? "" : "";
        if (string.IsNullOrWhiteSpace(path))
            return new ToolExecutionResult("path is required", true, null);

        var content = _knowledge.ReadFullFile(path);
        if (content == null)
            return new ToolExecutionResult($"File not in knowledge base: {path}", true, null);

        citations.Add(new KnowledgeCitation
        {
            FilePath = path,
            Anchor = "(full file)",
            Excerpt = content.Length > 300 ? content[..300] + "…" : content
        });

        // Cap returned content at ~10 KB to stay within token budget
        if (content.Length > 10_000) content = content[..10_000] + "\n\n[…truncated…]";
        return new ToolExecutionResult(content, false, null);
    }

    private ToolExecutionResult ExecuteFinalizeProfile(JsonElement input, TargetCandidate candidate)
    {
        try
        {
            var summary = input.TryGetProperty("summary", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() ?? "" : "";
            var regional = input.TryGetProperty("regional_context", out var r) && r.ValueKind == JsonValueKind.String ? r.GetString() ?? "" : "";
            var confidence = input.TryGetProperty("confidence", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : 0;

            var indicators = new List<string>();
            if (input.TryGetProperty("behavioral_indicators", out var bi) && bi.ValueKind == JsonValueKind.Array)
                indicators = bi.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString() ?? "").Where(x => x.Length > 0).ToList();

            var redFlags = new List<string>();
            if (input.TryGetProperty("sock_puppet_red_flags", out var sp) && sp.ValueKind == JsonValueKind.Array)
                redFlags = sp.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString() ?? "").Where(x => x.Length > 0).ToList();

            var profile = new PersonalityProfile
            {
                CandidateId = candidate.Id,
                CandidateUsername = candidate.PrimaryUsername,
                Summary = summary,
                BehavioralIndicators = indicators,
                RegionalContext = regional,
                SockPuppetRedFlags = redFlags,
                Confidence = Math.Clamp(confidence, 0, 100)
            };

            return new ToolExecutionResult("Profile finalized.", false, profile);
        }
        catch (Exception ex)
        {
            return new ToolExecutionResult($"Failed to parse finalize_profile input: {ex.Message}", true, null);
        }
    }

    // ── Tool definitions sent to Claude ─────────────────────────────────────────

    private static readonly object[] ToolDefinitions =
    {
        new
        {
            name = "search_knowledge",
            description = "Searches the OSINT markdown knowledge base for chunks relevant to a query (BM25 ranking). Returns top 5 chunks with file path and anchor. Use concise keyword queries like 'Mastodon federated privacy' or 'VK RUNet sock-puppet'.",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "Concise keyword query, 2-6 words." }
                },
                required = new[] { "query" }
            }
        },
        new
        {
            name = "read_full_file",
            description = "Reads the full text of a markdown file in the knowledge base. Use when a search hit is promising but the excerpt is too short. Path must be repo-relative, e.g. 'OSINT/Regional_RUNet.md'.",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    path = new { type = "string", description = "Repo-relative path of the markdown file, e.g. 'OSINT/Regional_RUNet.md'." }
                },
                required = new[] { "path" }
            }
        },
        new
        {
            name = "finalize_profile",
            description = "Emit the final PersonalityProfile. Call this exactly once when you have enough grounding. After calling this you MUST stop.",
            input_schema = new
            {
                type = "object",
                properties = new
                {
                    summary = new { type = "string", description = "2-3 sentence narrative summary." },
                    behavioral_indicators = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Bullet observations grounded in the knowledge base."
                    },
                    regional_context = new { type = "string", description = "Regional / linguistic context." },
                    sock_puppet_red_flags = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Specific red flags or empty list."
                    },
                    confidence = new { type = "integer", description = "0-100 overall confidence." }
                },
                required = new[] { "summary", "behavioral_indicators", "regional_context", "sock_puppet_red_flags", "confidence" }
            }
        }
    };
}
