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
            try
            {
                Debug.WriteLine("OnGuessGameClicked started");

                var videoService = _serviceProvider.GetRequiredService<IVideoService>();
                Debug.WriteLine("VideoService retrieved");

                var logger = _serviceProvider.GetRequiredService<ILoggingService>();
                Debug.WriteLine("LoggingService retrieved");

                var progressService = _serviceProvider.GetRequiredService<IProgressService>();
                Debug.WriteLine("ProgressService retrieved");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var gamePage = new GamePage(videoService, logger, progressService);
                    Debug.WriteLine("GamePage created");

                    gamePage.ViewModel.CurrentMode = GameMode.Guess;
                    Debug.WriteLine("Mode set to Guess");

                    await Navigation.PushAsync(gamePage);
                    Debug.WriteLine("Navigation completed");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnGuessGameClicked: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
            // Create ScoreboardPage with the required service
            var progressService = _serviceProvider.GetRequiredService<IProgressService>();
            var scoreboardPage = new ScoreboardPage(progressService);
            await Navigation.PushAsync(scoreboardPage);
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new SettingsPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to SettingsPage: {ex.Message}");
            }
        }
    }
}