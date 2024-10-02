using System;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace com.kizwiz.sipnsign.Pages
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private bool isDarkTheme = false; // Track current theme

        private async void OnStartGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GamePage());
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
