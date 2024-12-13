using com.kizwiz.sipnsign.Enums;
using System.Diagnostics;
using com.kizwiz.sipnsign.Services;  // Add this
using Microsoft.Extensions.DependencyInjection; // Add this

namespace com.kizwiz.sipnsign.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainMenuPage(IServiceProvider serviceProvider)
        {
            try
            {
                _serviceProvider = serviceProvider;
                System.Diagnostics.Debug.WriteLine("MainMenuPage: Initializing...");
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("MainMenuPage: Initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainMenuPage initialization error: {ex}");
            }
        }

        private async void OnGuessGameClicked(object sender, EventArgs e)
        {
            var logger = _serviceProvider?.GetService<ILoggingService>();
            try
            {
                logger?.Error("Starting Guess Mode initialization");
                if (_serviceProvider == null)
                {
                    logger?.Error("ServiceProvider is null!");
                    throw new InvalidOperationException("ServiceProvider is null");
                }

                logger?.Error("Getting VideoService");
                var videoService = _serviceProvider.GetRequiredService<IVideoService>();

                logger?.Error("Getting LoggingService");
                var progressService = _serviceProvider.GetRequiredService<IProgressService>();

                logger?.Error("Initializing Videos");
                await videoService.InitializeVideos();

                logger?.Error("Creating GamePage");
                var gamePage = new GamePage(videoService, logger, progressService);

                logger?.Error("Setting game mode");
                if (gamePage.ViewModel == null)
                {
                    logger?.Error("ViewModel is null!");
                    throw new InvalidOperationException("ViewModel not initialized");
                }

                gamePage.ViewModel.CurrentMode = GameMode.Guess;
                logger?.Error("Game mode set successfully");

                logger?.Error("Starting navigation");
                await Navigation.PushAsync(gamePage);
            }
            catch (Exception ex)
            {
                logger?.Error($"Critical error: {ex.Message}");
                logger?.Error($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    logger?.Error($"Inner exception: {ex.InnerException.Message}");
                }
                await DisplayAlert("Error", "Unable to start game. Please try again.", "OK");
            }
        }

        private async void OnPerformGameClicked(object sender, EventArgs e)
        {
            var videoService = _serviceProvider.GetRequiredService<IVideoService>();
            var logger = _serviceProvider.GetRequiredService<ILoggingService>();
            var progressService = _serviceProvider.GetRequiredService<IProgressService>();
            var gamePage = new GamePage(videoService, logger, progressService);
            gamePage.ViewModel.CurrentMode = GameMode.Perform;
            await Navigation.PushAsync(gamePage);
        }

        private async void OnViewScoresClicked(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("OnViewScoresClicked started");

                if (_serviceProvider == null)
                {
                    Debug.WriteLine("ServiceProvider is null!");
                    throw new InvalidOperationException("ServiceProvider is null");
                }

                Debug.WriteLine("Getting ProgressService");
                var progressService = _serviceProvider.GetRequiredService<IProgressService>();

                // Test progress service before creating page
                Debug.WriteLine("Testing GetUserProgressAsync");
                try
                {
                    var test = await progressService.GetUserProgressAsync();
                    Debug.WriteLine($"Progress test result - null?: {test == null}");
                }
                catch (Exception progressEx)
                {
                    Debug.WriteLine($"Progress service error: {progressEx.Message}");
                    Debug.WriteLine($"Inner exception: {progressEx.InnerException?.Message}");
                    throw;
                }

                Debug.WriteLine("Creating ProgressPage");
                var ProgressPage = new ProgressPage(progressService);

                Debug.WriteLine("Navigating to ProgressPage");
                await Navigation.PushAsync(ProgressPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnViewScoresClicked error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                await DisplayAlert("Error", $"Unable to load progress: {ex.InnerException?.Message ?? ex.Message}", "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            var logger = _serviceProvider?.GetService<ILoggingService>();
            try
            {
                logger?.Error("Creating settings page - start");
                if (_serviceProvider == null)
                {
                    logger?.Error("_serviceProvider is null!");
                    throw new InvalidOperationException("ServiceProvider not available");
                }

                // Try to get required services for settings
                var videoService = _serviceProvider.GetService<IVideoService>();
                logger?.Error("Got video service");

                var logService = _serviceProvider.GetService<ILoggingService>();
                logger?.Error("Got logging service");

                var progressService = _serviceProvider.GetService<IProgressService>();
                logger?.Error("Got progress service");

                var settingsPage = new SettingsPage();
                logger?.Error("Settings page created");

                if (Navigation == null)
                {
                    logger?.Error("Navigation is null!");
                    throw new InvalidOperationException("Navigation not available");
                }

                await Navigation.PushAsync(settingsPage);
                logger?.Error("Settings navigation completed");
            }
            catch (Exception ex)
            {
                logger?.Error($"Settings error: {ex.Message}");
                logger?.Error($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", "Unable to open settings", "OK");
            }
        }

        private async void OnViewLogsClicked(object sender, EventArgs e)
        {
            try
            {
                var logFile = Path.Combine(FileSystem.AppDataDirectory, "app.log");
                if (File.Exists(logFile))
                {
                    var logs = await File.ReadAllTextAsync(logFile);
                    await DisplayAlert("Application Logs", logs, "OK");
                }
                else
                {
                    await DisplayAlert("Logs", "No logs found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not read logs: {ex.Message}", "OK");
            }
        }
    }
}