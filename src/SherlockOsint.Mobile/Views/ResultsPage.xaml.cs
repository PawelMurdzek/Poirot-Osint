using SherlockOsint.Mobile.ViewModels;

namespace SherlockOsint.Mobile.Views;

public partial class ResultsPage : ContentPage
{
    public ResultsPage()
    {
        InitializeComponent();
    }

    public ResultsPage(ResultsViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // If BindingContext wasn't set via DI, get it from Shell's DependencyService
        if (BindingContext == null)
        {
            BindingContext = Application.Current?.MainPage?.Handler?.MauiContext?.Services.GetService<ResultsViewModel>();
        }
    }
}
