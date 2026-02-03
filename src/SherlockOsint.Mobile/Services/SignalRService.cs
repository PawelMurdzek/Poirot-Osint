using Microsoft.AspNetCore.SignalR.Client;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Mobile.Services;

/// <summary>
/// SignalR client service for real-time communication with the backend
/// </summary>
public class SignalRService : ISignalRService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    // Configurable API base URL - change this for production/cloud deployment
    // Set this before app starts, e.g., in MauiProgram.cs or from settings
    // LOCAL NETWORK: Use your PC's WiFi IP address
    public static string ApiBaseUrl { get; set; } = "http://192.168.1.129:57063";

    public event EventHandler<OsintNode>? NodeReceived;
    public event EventHandler<string>? SearchStarted;
    public event EventHandler<string>? SearchCompleted;
    public event EventHandler<string>? SearchCancelled;
    public event EventHandler<string>? SearchError;
    public event EventHandler<DigitalProfile>? ProfileReceived;
    public event EventHandler<List<TargetCandidate>>? CandidatesReceived;
    public event EventHandler<bool>? ConnectionStateChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService()
    {
        // Use configured URL or fallback to localhost for development
        if (!string.IsNullOrEmpty(ApiBaseUrl))
        {
            _hubUrl = $"{ApiBaseUrl.TrimEnd('/')}/osinthub";
        }
        else
        {
            // Default: For Android emulator, use 10.0.2.2 to access host's localhost
            _hubUrl = DeviceInfo.Platform == DevicePlatform.Android 
                ? "http://10.0.2.2:57063/osinthub" 
                : "http://localhost:57063/osinthub";
        }
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection != null)
        {
            await DisconnectAsync();
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers
        _hubConnection.On<OsintNode>("ReceiveNode", node =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NodeReceived?.Invoke(this, node);
            });
        });

        _hubConnection.On<string>("SearchStarted", message =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchStarted?.Invoke(this, message);
            });
        });

        _hubConnection.On<string>("SearchCompleted", message =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchCompleted?.Invoke(this, message);
            });
        });

        _hubConnection.On<string>("SearchCancelled", message =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchCancelled?.Invoke(this, message);
            });
        });

        _hubConnection.On<string>("SearchError", message =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SearchError?.Invoke(this, message);
            });
        });

        _hubConnection.On<DigitalProfile>("ReceiveProfile", profile =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProfileReceived?.Invoke(this, profile);
            });
        });

        _hubConnection.On<List<TargetCandidate>>("ReceiveCandidates", candidates =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CandidatesReceived?.Invoke(this, candidates);
            });
        });

        _hubConnection.Closed += async (error) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConnectionStateChanged?.Invoke(this, false);
            });
            await Task.CompletedTask;
        };

        _hubConnection.Reconnected += async (connectionId) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ConnectionStateChanged?.Invoke(this, true);
            });
            await Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            ConnectionStateChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            SearchError?.Invoke(this, $"Connection failed: {ex.Message}");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            ConnectionStateChanged?.Invoke(this, false);
        }
    }

    public async Task StartSearchAsync(SearchRequest request)
    {
        if (_hubConnection == null || !IsConnected)
        {
            await ConnectAsync();
        }

        await _hubConnection!.InvokeAsync("StartSearch", request);
    }

    public async Task CancelSearchAsync()
    {
        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("CancelSearch");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
