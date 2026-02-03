using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SherlockOsint.Mobile.Services;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Mobile.ViewModels;

/// <summary>
/// ViewModel for the Results page with tree-like display
/// </summary>
[QueryProperty(nameof(SearchRequest), "SearchRequest")]
public partial class ResultsViewModel : ObservableObject
{
    private readonly ISignalRService _signalRService;

    [ObservableProperty]
    private SearchRequest? _searchRequest;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _statusMessage = "Initializing...";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfile))]
    private DigitalProfile? _profile;

    /// <summary>
    /// Whether a DigitalProfile has been received
    /// </summary>
    public bool HasProfile => Profile != null;

    /// <summary>
    /// Target candidates with probability scoring
    /// </summary>
    public ObservableCollection<TargetCandidate> Candidates { get; } = new();

    /// <summary>
    /// Whether candidates have been received
    /// </summary>
    public bool HasCandidates => Candidates.Count > 0;

    /// <summary>
    /// Flat list of nodes for display in CollectionView
    /// Nodes are added with proper depth for tree indentation
    /// </summary>
    public ObservableCollection<OsintNode> Nodes { get; } = new();

    /// <summary>
    /// Hierarchical tree structure of nodes
    /// </summary>
    private readonly Dictionary<string, OsintNode> _nodeMap = new();

    public ResultsViewModel(ISignalRService signalRService)
    {
        _signalRService = signalRService;
        
        _signalRService.NodeReceived += OnNodeReceived;
        _signalRService.SearchStarted += OnSearchStarted;
        _signalRService.SearchCompleted += OnSearchCompleted;
        _signalRService.SearchCancelled += OnSearchCancelled;
        _signalRService.SearchError += OnSearchError;
        _signalRService.ProfileReceived += OnProfileReceived;
        _signalRService.CandidatesReceived += OnCandidatesReceived;
        _signalRService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    partial void OnSearchRequestChanged(SearchRequest? value)
    {
        if (value != null)
        {
            _ = StartSearchAsync();
        }
    }

    private async Task StartSearchAsync()
    {
        try
        {
            Nodes.Clear();
            _nodeMap.Clear();
            Candidates.Clear();
            Profile = null;
            IsSearching = true;
            StatusMessage = "Connecting to server...";

            await _signalRService.StartSearchAsync(SearchRequest!);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection failed: {ex.Message}";
            IsSearching = false;
        }
    }

    private void OnNodeReceived(object? sender, OsintNode node)
    {
        // Add node to flat list for CollectionView
        Nodes.Add(node);
        _nodeMap[node.Id] = node;
        
        StatusMessage = $"Found: {node.Label} - {node.Value}";
    }

    private void OnSearchStarted(object? sender, string message)
    {
        StatusMessage = message;
        IsSearching = true;
    }

    private void OnSearchCompleted(object? sender, string message)
    {
        StatusMessage = $"✓ {message} ({Nodes.Count} results found)";
        IsSearching = false;
    }

    private void OnSearchCancelled(object? sender, string message)
    {
        StatusMessage = $"[!] {message}";
        IsSearching = false;
    }

    private void OnSearchError(object? sender, string message)
    {
        StatusMessage = $"✗ {message}";
        IsSearching = false;
    }

    private void OnProfileReceived(object? sender, DigitalProfile profile)
    {
        Profile = profile;
    }

    private void OnCandidatesReceived(object? sender, List<TargetCandidate> candidates)
    {
        Candidates.Clear();
        foreach (var candidate in candidates)
        {
            Candidates.Add(candidate);
        }
        OnPropertyChanged(nameof(HasCandidates));
        StatusMessage = $"✓ Found {candidates.Count} potential matches";
    }

    private void OnConnectionStateChanged(object? sender, bool isConnected)
    {
        IsConnected = isConnected;
        if (!isConnected && IsSearching)
        {
            StatusMessage = "Connection lost...";
        }
    }

    [RelayCommand]
    private async Task CancelSearchAsync()
    {
        await _signalRService.CancelSearchAsync();
        IsSearching = false;
        StatusMessage = "Search cancelled";
    }

    [RelayCommand]
    private async Task RetrySearchAsync()
    {
        if (SearchRequest != null)
        {
            await StartSearchAsync();
        }
    }

    [RelayCommand]
    private void ToggleNodeExpanded(OsintNode node)
    {
        node.IsExpanded = !node.IsExpanded;
    }

    [RelayCommand]
    private async Task OpenUrlAsync(object parameter)
    {
        string? url = null;
        if (parameter is OsintNode node) url = node.Value;
        else if (parameter is SourceEvidence evidence) url = evidence.Url;
        else if (parameter is string s) url = s;

        if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not open URL: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task OpenSourceUrlAsync(SourceEvidence evidence)
    {
        await OpenUrlAsync(evidence);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _signalRService.CancelSearchAsync();
        await Shell.Current.GoToAsync("..");
    }
}
