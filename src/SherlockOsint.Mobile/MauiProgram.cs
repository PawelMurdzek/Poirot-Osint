using CommunityToolkit.Maui;
using SherlockOsint.Mobile.Services;
using SherlockOsint.Mobile.ViewModels;
using SherlockOsint.Mobile.Views;

namespace SherlockOsint.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        // Register services
        builder.Services.AddSingleton<ISignalRService, SignalRService>();

        // Register ViewModels
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<ResultsViewModel>();

        // Register Views
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ResultsPage>();

        return builder.Build();
    }
}
