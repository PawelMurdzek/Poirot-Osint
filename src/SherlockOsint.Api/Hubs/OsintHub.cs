using Microsoft.AspNetCore.SignalR;
using SherlockOsint.Api.Services;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Api.Hubs;

/// <summary>
/// SignalR hub for streaming OSINT search results to connected clients
/// </summary>
public class OsintHub : Hub
{
    private readonly ISearchOrchestrator _searchOrchestrator;
    private readonly ILogger<OsintHub> _logger;

    public OsintHub(ISearchOrchestrator searchOrchestrator, ILogger<OsintHub> logger)
    {
        _searchOrchestrator = searchOrchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        _searchOrchestrator.CancelSearch(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Initiates an OSINT search for the connected client
    /// </summary>
    /// <param name="request">Search parameters</param>
    public async Task StartSearch(SearchRequest request)
    {
        if (!request.HasSearchCriteria)
        {
            await Clients.Caller.SendAsync("SearchError", "Please provide at least one search criterion.");
            return;
        }

        request.ConnectionId = Context.ConnectionId;
        
        _logger.LogInformation("Starting search for {ConnectionId}: Name={Name}, Email={Email}, Phone={Phone}, Nickname={Nickname}",
            Context.ConnectionId, request.FullName, request.Email, request.Phone, request.Nickname);

        await Clients.Caller.SendAsync("SearchStarted", "Search initiated. Results will stream as they are found.");
        
        // Queue the search request
        _searchOrchestrator.QueueSearch(request, Context.ConnectionId);
    }

    /// <summary>
    /// Cancels an ongoing search for the connected client
    /// </summary>
    public Task CancelSearch()
    {
        _logger.LogInformation("Search cancelled by client: {ConnectionId}", Context.ConnectionId);
        _searchOrchestrator.CancelSearch(Context.ConnectionId);
        return Task.CompletedTask;
    }
}
