using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using com.kizwiz.sipnsign; // Ensure this namespace is correct

namespace com.kizwiz.sipnsign;

/// <summary>
/// The main entry point for the SipNSign application.
/// Configures the MAUI application with necessary services and fonts.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application.
    /// </summary>
    /// <returns>A configured instance of the MauiApp.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Use the App class as the entry point
        builder
            .UseMauiApp<App>() // Use the App class
            .UseMauiCommunityToolkit() // Register the CommunityToolkit
            .UseMauiCommunityToolkitMediaElement() // Register the MediaElement from the CommunityToolkit
            .ConfigureFonts(fonts =>
            {
                // Add custom fonts for the application
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // Enable debugging logging in the DEBUG configuration
        builder.Logging.AddDebug();
#endif

        // Register MainPage
        builder.Services.AddSingleton<MainPage>();

        // Build and return the configured MauiApp instance
        return builder.Build();
    }
}