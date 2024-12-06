using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using com.kizwiz.sipnsign;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.Pages;

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
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() 
            .UseMauiCommunityToolkitMediaElement()  
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register all services
        builder.Services.AddSingleton<IVideoService, VideoService>();

        // Register all pages
        builder.Services.AddTransient<MainMenuPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<HowToPlayPage>();
        builder.Services.AddTransient<ScoreboardPage>();
        builder.Services.AddTransient<SettingsPage>();

        // Register repositories
        builder.Services.AddSingleton<SignRepository>();

        return builder.Build();
    }
}
