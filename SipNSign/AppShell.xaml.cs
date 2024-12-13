using com.kizwiz.sipnsign.Pages;
using System.Diagnostics;

namespace com.kizwiz.sipnsign;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;

    public AppShell(IServiceProvider serviceProvider)
    {
        try
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeComponent();

            Debug.WriteLine("Registering routes...");
            RegisterRoutes();
            Debug.WriteLine("Routes registered successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing AppShell: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void RegisterRoutes()
    {
        try
        {
            Routing.RegisterRoute("gamepage", typeof(GamePage));
            Routing.RegisterRoute("progress", typeof(ProgressPage));
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("howtoplay", typeof(HowToPlayPage));
            Debug.WriteLine("All routes registered");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error registering routes: {ex.Message}");
            throw;
        }
    }

    private async Task GoToAsync(string route, string title = null)
    {
        try
        {
            if (string.IsNullOrEmpty(route))
            {
                throw new ArgumentException("Route cannot be null or empty");
            }

            Debug.WriteLine($"Attempting to navigate to route: {route}");
            await Shell.Current.GoToAsync(route);

            // Set the navigation title if provided
            if (!string.IsNullOrEmpty(title))
            {
                Shell.Current.CurrentItem.Title = title;
                Debug.WriteLine($"Title set to: {title}");
            }
            else
            {
                Debug.WriteLine("No title provided");
            }

            Debug.WriteLine($"Successfully navigated to: {route}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error to {route}: {ex.Message}");
            await DisplayAlert("Navigation Error", "Unable to navigate to the requested page. Please try again.", "OK");
        }
    }
}