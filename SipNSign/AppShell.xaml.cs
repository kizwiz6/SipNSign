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

    public void RefreshShell()
    {
        if (Application.Current?.Resources != null)
        {
            // Update shell appearance
            this.BackgroundColor = (Color)Application.Current.Resources["ShellBackgroundColor"];

            // Hide and show to force redraw
            this.IsVisible = false;
            this.IsVisible = true;
        }
    }

    private void RegisterRoutes()
    {
        try
        {
            Routing.RegisterRoute("gamepage", typeof(GamePage));
            Routing.RegisterRoute("profile", typeof(ProfilePage));
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

    public void UpdateTheme()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var resources = Application.Current?.Resources;
            if (resources != null)
            {
                this.BackgroundColor = (Color)resources["ShellBackgroundColor"];

                // Force redraw
                var temp = this.IsVisible;
                this.IsVisible = !temp;
                this.IsVisible = temp;
            }
        });
    }

    private async Task GoToAsync(string route, string? title = null)
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