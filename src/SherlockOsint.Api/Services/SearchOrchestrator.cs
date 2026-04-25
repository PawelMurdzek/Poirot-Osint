using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;
using SherlockOsint.Api.Hubs;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Services;

/// <summary>
/// Interface for the search orchestrator background service
/// </summary>
public interface ISearchOrchestrator
{
    /// <summary>
    /// Queues a new search request for processing
    /// </summary>
    void QueueSearch(SearchRequest request, string connectionId);

    /// <summary>
    /// Cancels an ongoing search for the specified connection
    /// </summary>
    void CancelSearch(string connectionId);
}

/// <summary>
/// Background service that orchestrates OSINT searches and streams results via SignalR
/// </summary>
public class SearchOrchestrator : BackgroundService, ISearchOrchestrator
{
    private readonly Channel<(SearchRequest Request, string ConnectionId)> _searchQueue;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeSearches;
    private readonly IHubContext<OsintHub> _hubContext;
    private readonly IRealSearchService _searchService;
    private readonly ProfileAggregator _profileAggregator;
    private readonly CandidateAggregator _candidateAggregator;
    private readonly PersonalityProfilerService _personalityProfiler;
    private readonly SessionMemoryService _sessionMemory;
    private readonly ClaudeAnalysisService _claudeAnalysis;
    private readonly ILogger<SearchOrchestrator> _logger;

    public SearchOrchestrator(
        IHubContext<OsintHub> hubContext,
        IRealSearchService searchService,
        ProfileAggregator profileAggregator,
        CandidateAggregator candidateAggregator,
        PersonalityProfilerService personalityProfiler,
        SessionMemoryService sessionMemory,
        ClaudeAnalysisService claudeAnalysis,
        ILogger<SearchOrchestrator> logger)
    {
        _hubContext = hubContext;
        _searchService = searchService;
        _profileAggregator = profileAggregator;
        _candidateAggregator = candidateAggregator;
        _personalityProfiler = personalityProfiler;
        _sessionMemory = sessionMemory;
        _claudeAnalysis = claudeAnalysis;
        _logger = logger;
        _searchQueue = Channel.CreateUnbounded<(SearchRequest, string)>();
        _activeSearches = new ConcurrentDictionary<string, CancellationTokenSource>();
    }

    public void QueueSearch(SearchRequest request, string connectionId)
    {
        // Cancel any existing search for this connection
        CancelSearch(connectionId);
        
        _searchQueue.Writer.TryWrite((request, connectionId));
    }

    public void CancelSearch(string connectionId)
    {
        if (_activeSearches.TryRemove(connectionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _logger.LogInformation("Cancelled search for connection: {ConnectionId}", connectionId);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SearchOrchestrator started");

        await foreach (var (request, connectionId) in _searchQueue.Reader.ReadAllAsync(stoppingToken))
        {
            // Process each search in its own task to allow concurrent searches
            _ = ProcessSearchAsync(request, connectionId, stoppingToken);
        }
    }

    private async Task ProcessSearchAsync(SearchRequest request, string connectionId, CancellationToken appStoppingToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
        _activeSearches[connectionId] = cts;

        try
        {
            _logger.LogInformation("Processing search for {ConnectionId}", connectionId);

            var allResults = new List<OsintNode>();
            
            await foreach (var node in _searchService.SearchAsync(request, cts.Token))
            {
                // Collect all nodes for profile building
                allResults.Add(node);
                
                // Stream each node to the client as it's discovered
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNode", node, cts.Token);
                
                _logger.LogDebug("Sent node to {ConnectionId}: {Label} = {Value}", 
                    connectionId, node.Label, node.Value);
            }

            // Build and send the aggregated DigitalProfile
            var profile = _profileAggregator.BuildProfile(request, allResults);
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProfile", profile, cts.Token);
            _logger.LogInformation("Sent DigitalProfile to {ConnectionId} with {PlatformCount} platforms, {Score}% confidence",
                connectionId, profile.Platforms.Count, profile.ConfidenceScore);

            // Build and send target candidates — Claude assesses probability
            var candidates = await _candidateAggregator.BuildCandidatesAsync(request, allResults, cts.Token);
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveCandidates", candidates, cts.Token);
            _logger.LogInformation("Sent {CandidateCount} candidates to {ConnectionId}",
                candidates.Count, connectionId);

            // Persist top-10 candidates to /sessions and surface a Claude CLI
            // command. This is the "memory" mechanism that replaces the old
            // arbitrary-numbers fallback when no API key is configured — and
            // it also runs when Claude *is* configured so the user has a log.
            try
            {
                var record = await _sessionMemory.PersistAsync(request, candidates, _claudeAnalysis.IsConfigured, cts.Token);
                await _hubContext.Clients.Client(connectionId).SendAsync(
                    "ReceiveSessionMemory",
                    new
                    {
                        folderPath = record.FolderPath,
                        jsonPath = record.JsonPath,
                        markdownPath = record.MarkdownPath,
                        claudeCommand = record.ClaudeCommand,
                        promptHint = record.ClaudePromptHint,
                        claudeApiConfigured = _claudeAnalysis.IsConfigured
                    },
                    cts.Token);
                _logger.LogInformation("Session memory written to {Path} for {ConnectionId}",
                    record.JsonPath, connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist session memory for {ConnectionId}", connectionId);
            }

            // Personality profiler — only the TOP-3 by probability, in parallel.
            // Skip when Claude isn't configured: without the API key the profiler
            // returns null and we'd just be wasting cycles on a no-op.
            if (!_claudeAnalysis.IsConfigured)
            {
                _logger.LogInformation("Skipping personality profiler — Claude API key not configured");
                await _hubContext.Clients.Client(connectionId).SendAsync("SearchCompleted", "Search completed.", cts.Token);
                return;
            }

            var top3 = candidates.Take(3).ToList();
            if (top3.Count > 0)
            {
                _logger.LogInformation("Profiling top {Count} candidates for {ConnectionId}", top3.Count, connectionId);
                await Parallel.ForEachAsync(top3, new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cts.Token }, async (candidate, innerCt) =>
                {
                    var profile = await _personalityProfiler.ProfileAsync(request, candidate, innerCt);
                    if (profile != null)
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceivePersonalityProfile", profile, innerCt);
                        _logger.LogInformation("Sent personality profile for {Candidate} to {ConnectionId}",
                            candidate.PrimaryUsername, connectionId);
                    }
                });
            }

            // Notify client that search is complete
            await _hubContext.Clients.Client(connectionId).SendAsync("SearchCompleted", "Search completed.", cts.Token);
            _logger.LogInformation("Search completed for {ConnectionId}", connectionId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Search cancelled for {ConnectionId}", connectionId);
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("SearchCancelled", "Search was cancelled.");
            }
            catch { /* Client may have disconnected */ }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during search for {ConnectionId}", connectionId);
            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("SearchError", $"Search failed: {ex.Message}");
            }
            catch { /* Client may have disconnected */ }
        }
        finally
        {
            _activeSearches.TryRemove(connectionId, out _);
            cts.Dispose();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("SearchOrchestrator stopping");
        
        // Cancel all active searches
        foreach (var kvp in _activeSearches)
        {
            kvp.Value.Cancel();
        }
        _activeSearches.Clear();

        await base.StopAsync(cancellationToken);
    }
}
