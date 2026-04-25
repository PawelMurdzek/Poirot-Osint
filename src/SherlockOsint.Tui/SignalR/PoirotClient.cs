using Microsoft.AspNetCore.SignalR.Client;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Tui.SignalR;

/// <summary>
/// Thin SignalR wrapper that mirrors the events used by the mobile client
/// (ReceiveNode / ReceiveProfile / ReceiveCandidates / ReceivePersonalityProfile +
///  SearchStarted / SearchCompleted / SearchError).
/// </summary>
public class PoirotClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public event Action<OsintNode>? OnNode;
    public event Action<DigitalProfile>? OnProfile;
    public event Action<List<TargetCandidate>>? OnCandidates;
    public event Action<PersonalityProfile>? OnPersonalityProfile;
    public event Action<string>? OnStarted;
    public event Action<string>? OnCompleted;
    public event Action<string>? OnError;

    public PoirotClient(string apiBaseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{apiBaseUrl.TrimEnd('/')}/osinthub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<OsintNode>("ReceiveNode", n => OnNode?.Invoke(n));
        _connection.On<DigitalProfile>("ReceiveProfile", p => OnProfile?.Invoke(p));
        _connection.On<List<TargetCandidate>>("ReceiveCandidates", c => OnCandidates?.Invoke(c));
        _connection.On<PersonalityProfile>("ReceivePersonalityProfile", p => OnPersonalityProfile?.Invoke(p));
        _connection.On<string>("SearchStarted", m => OnStarted?.Invoke(m));
        _connection.On<string>("SearchCompleted", m => OnCompleted?.Invoke(m));
        _connection.On<string>("SearchError", m => OnError?.Invoke(m));
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _connection.StartAsync(ct);
    }

    public async Task StartSearchAsync(SearchRequest request, CancellationToken ct = default)
    {
        await _connection.InvokeAsync("StartSearch", request, ct);
    }

    public async Task CancelSearchAsync(CancellationToken ct = default)
    {
        await _connection.InvokeAsync("CancelSearch", ct);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
