using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SherlockOsint.Mobile.Services;
using SherlockOsint.Mobile.Views;
using SherlockOsint.Shared.Models;

namespace SherlockOsint.Mobile.ViewModels;

/// <summary>
/// ViewModel for the Search page
/// </summary>
public partial class SearchViewModel : ObservableObject
{
    private readonly ISignalRService _signalRService;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _nickname = string.Empty;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _statusMessage = "Enter search criteria to begin";

    [ObservableProperty]
    private bool _isConnected;

    public SearchViewModel(ISignalRService signalRService)
    {
        _signalRService = signalRService;
        _signalRService.ConnectionStateChanged += OnConnectionStateChanged;
        _isConnected = _signalRService.IsConnected;
    }

    private void OnConnectionStateChanged(object? sender, bool isConnected)
    {
        IsConnected = isConnected;
        StatusMessage = isConnected ? "Connected to server" : "Disconnected from server";
    }

    public bool CanSearch => !string.IsNullOrWhiteSpace(FullName) ||
                             !string.IsNullOrWhiteSpace(Email) ||
                             !string.IsNullOrWhiteSpace(Phone) ||
                             !string.IsNullOrWhiteSpace(Nickname);

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (!CanSearch)
        {
            StatusMessage = "Please enter at least one search criterion";
            return;
        }

        try
        {
            IsSearching = true;
            StatusMessage = "Connecting...";

            var request = new SearchRequest
            {
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                Nickname = Nickname
            };

            // Navigate to results page with the search request
            await Shell.Current.GoToAsync(nameof(ResultsPage), new Dictionary<string, object>
            {
                { "SearchRequest", request }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        FullName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Nickname = string.Empty;
        StatusMessage = "Form cleared";
    }

    partial void OnFullNameChanged(string value) => OnPropertyChanged(nameof(CanSearch));
    partial void OnEmailChanged(string value) => OnPropertyChanged(nameof(CanSearch));
    partial void OnPhoneChanged(string value) => OnPropertyChanged(nameof(CanSearch));
    partial void OnNicknameChanged(string value) => OnPropertyChanged(nameof(CanSearch));
}
