using com.kizwiz.sipnsign.Pages;

namespace com.kizwiz.sipnsign;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;

    public AppShell(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();

        Routing.RegisterRoute("gamepage", typeof(GamePage));
        Routing.RegisterRoute("scoreboard", typeof(ScoreboardPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }

    // Update your navigation method to use the service provider
    private async Task GoToAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation Error", ex.Message, "OK");
        }
    }
}
