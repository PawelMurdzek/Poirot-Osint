using SherlockOsint.Shared.Models;

namespace SherlockOsint.Mobile.Services;

/// <summary>
/// Interface for SignalR communication with the backend
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Event raised when a new OSINT node is received from the server
    /// </summary>
    event EventHandler<OsintNode>? NodeReceived;

    /// <summary>
    /// Event raised when the search is started by the server
    /// </summary>
    event EventHandler<string>? SearchStarted;

    /// <summary>
    /// Event raised when the search is completed
    /// </summary>
    event EventHandler<string>? SearchCompleted;

    /// <summary>
    /// Event raised when the search is cancelled
    /// </summary>
    event EventHandler<string>? SearchCancelled;

    /// <summary>
    /// Event raised when a search error occurs
    /// </summary>
    event EventHandler<string>? SearchError;

    /// <summary>
    /// Event raised when the aggregated DigitalProfile is received
    /// </summary>
    event EventHandler<DigitalProfile>? ProfileReceived;

    /// <summary>
    /// Event raised when target candidates are received
    /// </summary>
    event EventHandler<List<TargetCandidate>>? CandidatesReceived;

    /// <summary>
    /// Event raised when connection state changes
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// Gets whether the client is connected to the server
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects to the SignalR hub
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// Disconnects from the SignalR hub
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Starts an OSINT search
    /// </summary>
    Task StartSearchAsync(SearchRequest request);

    /// <summary>
    /// Cancels the current search
    /// </summary>
    Task CancelSearchAsync();
}
