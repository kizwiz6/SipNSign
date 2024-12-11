using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using com.kizwiz.sipnsign;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.Pages;
using com.kizwiz.sipnsign.ViewModels;
using System.Text.Json;
using System.Diagnostics;

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

        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;
        });

        builder.Services.AddSingleton(sp =>
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        });

        builder.Services.AddSingleton<IServiceProvider>(sp =>
        {
            try
            {
                return sp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ServiceProvider: {ex}");
                throw;
            }
        });

        try
        {
            // Register core services
            builder.Services.AddSingleton<IVideoService, VideoService>();
            builder.Services.AddSingleton<ILoggingService, LoggingService>();
            builder.Services.AddSingleton<IProgressService>(serviceProvider =>
            {
                try
                {
                    return new ProgressService();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to create ProgressService: {ex.Message}");
                    throw;
                }
            });
            builder.Services.AddSingleton<SignRepository>();

            // Register pages and viewmodels
            builder.Services.AddSingleton<App>();
            builder.Services.AddTransient<MainMenuPage>();
            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<GameViewModel>();
            builder.Services.AddTransient<HowToPlayPage>();
            builder.Services.AddTransient<ScoreboardPage>();
            builder.Services.AddTransient<ScoreboardViewModel>();
            builder.Services.AddTransient<SettingsPage>();

            return builder.Build();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CreateMauiApp: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}