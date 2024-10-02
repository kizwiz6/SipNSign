using System;
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
            await Navigation.PushAsync(new SettingsPage());
        }

        /// <summary>
        /// Event handler for the "Toggle Theme" button click.
        /// Changes the application theme between light and dark.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">The event data containing information about the click event.</param>
        private void OnToggleThemeClicked(object sender, EventArgs e)
        {
            // Toggle the theme
            isDarkTheme = !isDarkTheme;

            // Apply the new theme
            Application.Current.UserAppTheme = isDarkTheme ? AppTheme.Dark : AppTheme.Light;

            // Optional: Log the current theme
            Console.WriteLine($"Theme changed to: {(isDarkTheme ? "Dark" : "Light")}");
        }
    }
}
