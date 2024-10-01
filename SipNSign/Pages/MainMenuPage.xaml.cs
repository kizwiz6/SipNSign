using System;
using Microsoft.Maui.Controls;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Represents the main menu page of the SipNSign application.
    /// Provides navigation to other pages such as the game, scoreboard, and settings.
    /// </summary>
    public partial class MainMenuPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenuPage"/> class.
        /// Sets up the user interface and initializes components.
        /// </summary>
        public MainMenuPage()
        {
            InitializeComponent(); // This should link to the XAML
        }

        /// <summary>
        /// Event handler for the "Start Game" button click.
        /// Navigates to the game page when the button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnStartGameClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GamePage());
        }

        /// <summary>
        /// Event handler for the "View Scores" button click.
        /// Navigates to the scoreboard page when the button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnViewScoresClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ScoreboardPage());
        }

        /// <summary>
        /// Event handler for the "Settings" button click.
        /// Navigates to the settings page when the button is clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
