using com.kizwiz.sipnsign.Enums;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private async void OnGuessGameClicked(object sender, EventArgs e)
        {
            var gamePage = new GamePage();
            gamePage.ViewModel.CurrentMode = GameMode.Guess;
            await Navigation.PushAsync(gamePage);
        }

        private async void OnPerformGameClicked(object sender, EventArgs e)
        {
            var gamePage = new GamePage();
            gamePage.ViewModel.CurrentMode = GameMode.Perform;
            await Navigation.PushAsync(gamePage);
        }

        private async void OnViewScoresClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScoreboardPage());
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