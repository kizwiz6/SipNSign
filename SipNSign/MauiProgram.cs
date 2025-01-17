using com.kizwiz.sipnsign.Pages;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.ViewModels;
using CommunityToolkit.Maui;
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

            // Register pages and viewmodels
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<MainMenuPage>();
            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<GameViewModel>();
            builder.Services.AddTransient<HowToPlayPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<StorePage>();
            builder.Services.AddTransient<StoreViewModel>();
            builder.Services.AddTransient<AchievementDetailsPage>();
            builder.Services.AddTransient<AchievementDetailsViewModel>();

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