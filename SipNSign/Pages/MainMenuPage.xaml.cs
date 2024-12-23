using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Services;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        public required IServiceProvider _serviceProvider;
        public required IVideoService _videoService;
        public required ILoggingService _logger;
        public required IProgressService _progressService;
        private readonly IThemeService _themeService;
        private bool _isNavigating = false;


        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenuPage"/> class.
        /// This constructor sets up the required services via dependency injection and initializes the page.
        /// </summary>
        /// <param name="serviceProvider">The service provider used for dependency injection.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="serviceProvider"/> is null.</exception>
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
                _themeService = _serviceProvider.GetRequiredService<IThemeService>();

                // Ensure _logger is initialized
                if (_logger == null)
                {
                    throw new InvalidOperationException("Logging service failed to initialize.");
                }

                _logger?.Debug("MainMenuPage initialized successfully with all services");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainMenuPage initialization error: {ex}");
                // Displaying an error message to the user if initialization fails
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
            if (_isNavigating) return;
            var logger = _serviceProvider?.GetService<ILoggingService>();

            try
            {
                _isNavigating = true;
                logger?.Debug("Starting Guess Mode initialization");

                var videoService = _videoService ?? _serviceProvider?.GetRequiredService<IVideoService>();
                var loggingService = _logger ?? _serviceProvider?.GetRequiredService<ILoggingService>();
                var progressService = _progressService ?? _serviceProvider?.GetRequiredService<IProgressService>();

                logger?.Debug("Calling InitializeVideos");
                await videoService.InitializeVideos();
                logger?.Debug("InitializeVideos completed");

                logger?.Debug("Creating GamePage");
                var gamePage = new GamePage(_serviceProvider, videoService, loggingService, progressService);
                gamePage.ViewModel.CurrentMode = GameMode.Guess;

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
            finally
            {
                _isNavigating = false;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Refresh theme from saved preferences
            var savedTheme = Preferences.Get(Constants.THEME_KEY, CustomAppTheme.Blue.ToString());
            if (Enum.TryParse<CustomAppTheme>(savedTheme, out var theme))
            {
                _themeService.SetTheme(theme);
            }
        }

        private async void OnPerformGameClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            var logger = _serviceProvider?.GetService<ILoggingService>();

            try
            {
                _isNavigating = true;
                var videoService = _videoService ?? _serviceProvider.GetRequiredService<IVideoService>();
                var loggingService = _logger ?? _serviceProvider.GetRequiredService<ILoggingService>();
                var progressService = _progressService ?? _serviceProvider.GetRequiredService<IProgressService>();

                var gamePage = new GamePage(_serviceProvider, videoService, loggingService, progressService);
                gamePage.ViewModel.CurrentMode = GameMode.Perform;
                await Navigation.PushAsync(gamePage);
            }
            catch (Exception ex)
            {
                logger?.Error($"Error starting Perform Mode: {ex.Message}");
                await DisplayAlert("Error", "Unable to start Perform Mode.", "OK");
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async void OnViewScoresClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            var logger = _serviceProvider?.GetService<ILoggingService>();

            try
            {
                _isNavigating = true;
                Debug.WriteLine("OnViewScoresClicked started");

                var progressService = _progressService ?? _serviceProvider.GetRequiredService<IProgressService>();
                var progressPage = new ProgressPage(progressService);
                await Navigation.PushAsync(progressPage);
            }
            catch (Exception ex)
            {
                logger?.Error($"Error viewing scores: {ex.Message}");
                await DisplayAlert("Error", "Unable to load progress data", "OK");
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            var logger = _serviceProvider?.GetService<ILoggingService>();

            try
            {
                _isNavigating = true;
                var themeService = _serviceProvider.GetRequiredService<IThemeService>();
                var settingsPage = new SettingsPage(themeService);
                await Navigation.PushAsync(settingsPage);
            }
            catch (Exception ex)
            {
                logger?.Error($"Settings error: {ex.Message}");
                await DisplayAlert("Error", "Unable to open settings", "OK");
            }
            finally
            {
                _isNavigating = false;
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