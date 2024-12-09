using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using com.kizwiz.sipnsign;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.Pages;
using com.kizwiz.sipnsign.ViewModels;

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
                fonts.AddFont("Bangers-Regular.ttf", "Bangers");
            });

        // Register services
        builder.Services.AddSingleton<IServiceProvider>(provider => provider);
        builder.Services.AddSingleton<IVideoService, VideoService>();
        builder.Services.AddSingleton<ILoggingService, LoggingService>();
        builder.Services.AddSingleton<IProgressService, ProgressService>();
        builder.Services.AddSingleton<SignRepository>();

        // Register pages
        builder.Services.AddSingleton<App>();
        builder.Services.AddTransient<MainMenuPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<HowToPlayPage>();
        builder.Services.AddTransient<ScoreboardPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
