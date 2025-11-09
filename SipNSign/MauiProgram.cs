using com.kizwiz.sipnsign.Pages;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Diagnostics;
using System.Text.Json;

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
        try
        {
            Debug.WriteLine("Starting app initialization");

            var builder = MauiApp.CreateBuilder();
            Debug.WriteLine("Builder created");

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

            Debug.WriteLine("Basic MAUI configuration completed");

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

            try
            {
                // Register core services
                builder.Services.AddSingleton<IVideoService, VideoService>();
                builder.Services.AddSingleton<ILoggingService, LoggingService>();
                builder.Services.AddSingleton<IProgressService, ProgressService>();
                builder.Services.AddSingleton<SignRepository>();
                builder.Services.AddSingleton<IThemeService, ThemeService>();
                builder.Services.AddSingleton<IShareService, ShareService>();
                builder.Services.AddSingleton<IIAPService, IAPService>();

                Debug.WriteLine("Core services registered");

                builder.Services.AddSingleton<App>();
                builder.Services.AddSingleton<AppShell>();
                Debug.WriteLine("App and Shell registered");

                // Register pages and viewmodels
                builder.Services.AddTransient<MainMenuPage>();
                builder.Services.AddTransient<GamePage>();
                builder.Services.AddTransient<GameViewModel>();
                builder.Services.AddTransient<ProfilePage>();
                builder.Services.AddTransient<ProfileViewModel>();
                builder.Services.AddTransient<SettingsPage>();
                builder.Services.AddTransient<StorePage>();
                builder.Services.AddTransient<StoreViewModel>();
                builder.Services.AddTransient<AchievementDetailsPage>();
                builder.Services.AddTransient<AchievementDetailsViewModel>();
                Debug.WriteLine("Pages registered");

                var app = builder.Build();
                Debug.WriteLine("App built successfully");
                return app;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Service registration error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Critical error in CreateMauiApp: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}