using com.kizwiz.sipnsign.Enums;
using System.Diagnostics;
using com.kizwiz.sipnsign.Services;
using Microsoft.Extensions.DependencyInjection;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IVideoService _videoService;
        private readonly ILoggingService _logger;
        private readonly IProgressService _progressService;


        public MainMenuPage(IServiceProvider serviceProvider)
        {
            try
            {
                InitializeComponent();

                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

                // Initialize services directly in constructor
                _videoService = _serviceProvider.GetRequiredService<IVideoService>();
                _logger = _serviceProvider.GetRequiredService<ILoggingService>();
                _progressService = _serviceProvider.GetRequiredService<IProgressService>();

                _logger?.Debug("MainMenuPage initialized successfully with all services");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainMenuPage initialization error: {ex}");
                DisplayAlert("Error", "Failed to initialize application services", "OK").Wait();
            }
        }

        /// <summary>
        /// Handles the Guess Game button click event. 
        /// Initialises services and navigates to the Guess Game page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void OnGuessGameClicked(object sender, EventArgs e)
        {
            var logger = _serviceProvider?.GetService<ILoggingService>();

            try
            {
                logger?.Debug("Starting Guess Mode initialization");
                logger?.Debug($"_serviceProvider is null: {_serviceProvider == null}");

                var videoService = _videoService ?? _serviceProvider?.GetService<IVideoService>();
                if (videoService == null)
                {
                    logger?.Error("videoService is null");
                    await DisplayAlert("Error", "Unable to initialize video service.", "OK");
                    return;
                }

                var loggingService = _logger ?? _serviceProvider?.GetService<ILoggingService>();
                if (loggingService == null)
                {
                    logger?.Error("loggingService is null");
                    await DisplayAlert("Error", "Unable to initialize logging service.", "OK");
                    return;
                }

                var progressService = _progressService ?? _serviceProvider?.GetService<IProgressService>();
                if (progressService == null)
                {
                    logger?.Error("progressService is null");
                    await DisplayAlert("Error", "Unable to initialize progress service.", "OK");
                    return;
                }

                logger?.Debug("Calling InitializeVideos");
                await videoService.InitializeVideos();
                logger?.Debug("InitializeVideos completed");

                logger?.Debug("Creating GamePage");
                var gamePage = new GamePage(videoService, loggingService, progressService);
                logger?.Debug("GamePage created");

                logger?.Debug("Setting CurrentMode");
                gamePage.ViewModel.CurrentMode = GameMode.Guess;
                logger?.Debug("CurrentMode set");

                if (Navigation == null)
                {
                    logger?.Error("Navigation is null");
                    await DisplayAlert("Error", "Navigation is not initialized.", "OK");
                    return;
                }
                await Navigation.PushAsync(gamePage);
                logger?.Debug("Navigation completed");
            }
            catch (Exception ex)
            {
                logger?.Error($"Critical error: {ex.Message}");
                logger?.Error($"Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", "Unable to start game. Please restart the application.", "OK");
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