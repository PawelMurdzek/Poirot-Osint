using CommunityToolkit.Mvvm.ComponentModel;

namespace SherlockOsint.Shared.Models;

/// <summary>
/// Represents a node in the OSINT search result tree.
/// Uses ObservableObject for MVVM data binding support.
/// </summary>
public partial class OsintNode : ObservableObject
{
    /// <summary>
    /// The label/category of this node (e.g., "Social Media", "Email", "Repository")
    /// </summary>
    [ObservableProperty]
    private string _label = string.Empty;

    /// <summary>
    /// The value/content of this node (e.g., "twitter.com/user", "user@example.com")
    /// </summary>
    [ObservableProperty]
    private string _value = string.Empty;

    /// <summary>
    /// Child nodes representing sub-results discovered from this node
    /// </summary>
    [ObservableProperty]
    private List<OsintNode> _children = new();

    /// <summary>
    /// Indicates whether this node is currently being searched/loaded
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Indicates whether this node is expanded in the tree view
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>
    /// Unique identifier for this node
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Parent node ID for tree reconstruction
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// The depth level in the tree (0 = root)
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Icon to display for this node type
    /// </summary>
    public string Icon => Label?.ToLower() switch
    {
        "email" => "📧",
        "social media" => "📱",
        "twitter" => "🐦",
        "linkedin" => "💼",
        "github" => "🐙",
        "repository" => "📁",
        "person" => "👤",
        "phone" => "📞",
        "website" => "🌐",
        "username" => "🏷️",
        _ => "📄"
    };

    /// <summary>
    /// Computed property indicating whether this node has children
    /// </summary>
    public bool HasChildren => Children?.Count > 0;

    /// <summary>
    /// Type of node (e.g., "Profile", "LeakedPassword", "Infrastructure")
    /// </summary>
    [ObservableProperty]
    private string _type = "Info";

    /// <summary>
    /// Additional metadata for this node (e.g., confidence, original data)
    /// </summary>
    [ObservableProperty]
    private Dictionary<string, string> _metadata = new();

    /// <summary>
    /// Left margin for tree indentation (based on depth)
    /// </summary>
    public int IndentMargin => Depth * 24;
}
