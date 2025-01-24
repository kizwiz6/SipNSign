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
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Starting MainMenuPage initialization");
#endif
                InitializeComponent();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "MainMenuPage InitializeComponent completed");
#endif
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

                // Initialize services with logging
                _videoService = _serviceProvider.GetRequiredService<IVideoService>();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Video service initialized");
#endif
                _logger = _serviceProvider.GetRequiredService<ILoggingService>();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Logger service initialized");
#endif
                _progressService = _serviceProvider.GetRequiredService<IProgressService>();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Progress service initialized");
#endif
                _themeService = _serviceProvider.GetRequiredService<IThemeService>();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "Theme service initialized");
#endif
                ForceRefresh();
#if ANDROID
                Android.Util.Log.Debug("SipNSignApp", "MainMenuPage force refresh completed");
#endif
            }
            catch (Exception ex)
            {
#if ANDROID
                Android.Util.Log.Error("SipNSignApp", $"Critical error in MainMenuPage: {ex}");
#endif
                throw;
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
            _themeService.ThemeChanged += OnThemeChanged;
            // Force initial refresh
            ForceRefresh();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _themeService.ThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var mainLayout = this.FindByName<VerticalStackLayout>("MainLayout");
                    if (mainLayout == null) return;

                    var buttons = new[] { "GuessMode", "PerformMode", "Profile", "Settings", "Store" };
                    foreach (var styleId in buttons)
                    {
                        // Safely get resource value
                        if (Application.Current != null &&
                            Application.Current.Resources.TryGetValue(styleId, out object resourceValue) &&
                            resourceValue is Color themeColor)
                        {
                            var button = mainLayout.Children
                                .OfType<Button>()
                                .FirstOrDefault(b => b.StyleId == styleId);

                            if (button != null)
                            {
                                button.BackgroundColor = themeColor;
                                Debug.WriteLine($"Updated {styleId} button color to {themeColor}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating theme: {ex.Message}");
                }
            });
        }

        public void ForceRefresh()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (Application.Current?.Resources == null) return;

                    var mainLayout = this.FindByName<VerticalStackLayout>("MainLayout");
                    if (mainLayout != null)
                    {
                        var buttons = mainLayout.Children.OfType<Button>().ToList();

                        foreach (var button in buttons)
                        {
                            if (!string.IsNullOrEmpty(button.StyleId))
                            {
                                Debug.WriteLine($"Refreshing button: {button.StyleId}");
                                if (Application.Current.Resources.TryGetValue(button.StyleId, out var colorValue) &&
                                    colorValue is Color color)
                                {
                                    // Force color update by briefly setting to null then new color
                                    button.BackgroundColor = null;
                                    button.BackgroundColor = color;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in ForceRefresh: {ex.Message}");
                    Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            });
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
                var profilePage = new ProfilePage(progressService);
                await Navigation.PushAsync(profilePage);
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
                var settingsPage = new SettingsPage(themeService, _serviceProvider); // Update this line
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

        private async void OnStoreClicked(object sender, EventArgs e)
        {
            if (_isNavigating) return;
            var logger = _serviceProvider?.GetService<ILoggingService>();

            try
            {
                _isNavigating = true;
                var storePage = new StorePage(_serviceProvider);
                await Navigation.PushAsync(storePage);
            }
            catch (Exception ex)
            {
                logger?.Error($"Store error: {ex.Message}");
                await DisplayAlert("Error", "Unable to open store", "OK");
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