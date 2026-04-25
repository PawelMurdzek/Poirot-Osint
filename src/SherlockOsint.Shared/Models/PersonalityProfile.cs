using CommunityToolkit.Mvvm.ComponentModel;

namespace SherlockOsint.Shared.Models;

/// <summary>
/// Deep personality / behavioral profile for a top candidate.
/// Produced by PersonalityProfilerService using Claude + RAG over the OSINT/ knowledge base.
/// </summary>
public partial class PersonalityProfile : ObservableObject
{
    /// <summary>
    /// Candidate this profile belongs to (TargetCandidate.Id).
    /// </summary>
    [ObservableProperty]
    private string _candidateId = string.Empty;

    /// <summary>
    /// Primary username of the candidate (denormalised for quick UI display).
    /// </summary>
    [ObservableProperty]
    private string _candidateUsername = string.Empty;

    /// <summary>
    /// 2-3 sentence narrative summary of the person.
    /// </summary>
    [ObservableProperty]
    private string _summary = string.Empty;

    /// <summary>
    /// Bullet-style behavioral observations grounded in the OSINT knowledge base
    /// (e.g. "Active on Mastodon + Lemmy → privacy-conscious, tech-savvy").
    /// </summary>
    [ObservableProperty]
    private List<string> _behavioralIndicators = new();

    /// <summary>
    /// Regional / linguistic context inferred from platform footprint.
    /// </summary>
    [ObservableProperty]
    private string _regionalContext = string.Empty;

    /// <summary>
    /// Signals that suggest the persona may be a sock-puppet (fresh account, perfect metadata,
    /// no cross-platform consistency, etc.). Empty list = no red flags.
    /// </summary>
    [ObservableProperty]
    private List<string> _sockPuppetRedFlags = new();

    /// <summary>
    /// Knowledge-base excerpts the profile is grounded in — for transparency.
    /// </summary>
    [ObservableProperty]
    private List<KnowledgeCitation> _citations = new();

    /// <summary>
    /// Overall confidence in this profile (0-100).
    /// </summary>
    [ObservableProperty]
    private int _confidence;

    /// <summary>
    /// Number of agent iterations consumed (debug / cost diagnostic).
    /// </summary>
    public int IterationsUsed { get; set; }
}

public class KnowledgeCitation
{
    /// <summary>
    /// Repo-relative path, e.g. "OSINT/Regional_RUNet.md".
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// H2/H3 heading anchor the chunk lives under.
    /// </summary>
    public string Anchor { get; set; } = string.Empty;

    /// <summary>
    /// Short excerpt (≤300 chars) shown in the UI.
    /// </summary>
    public string Excerpt { get; set; } = string.Empty;
}
