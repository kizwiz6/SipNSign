using com.kizwiz.sipnsign.Enums;
using System.Diagnostics;
using com.kizwiz.sipnsign.Services;  // Add this
using Microsoft.Extensions.DependencyInjection; // Add this

namespace com.kizwiz.sipnsign.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainMenuPage(IServiceProvider serviceProvider)  // Add service provider parameter
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private async void OnGuessGameClicked(object sender, EventArgs e)
        {
            var videoService = _serviceProvider.GetRequiredService<IVideoService>();
            var gamePage = new GamePage(videoService);
            gamePage.ViewModel.CurrentMode = GameMode.Guess;
            await Navigation.PushAsync(gamePage);
        }

        private async void OnPerformGameClicked(object sender, EventArgs e)
        {
            var videoService = _serviceProvider.GetRequiredService<IVideoService>();
            var gamePage = new GamePage(videoService);
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