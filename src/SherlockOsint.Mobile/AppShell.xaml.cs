namespace SherlockOsint.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(Views.ResultsPage), typeof(Views.ResultsPage));
    }
}
