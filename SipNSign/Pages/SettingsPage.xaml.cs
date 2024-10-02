using Microsoft.Maui.Storage; // For Preferences
using Microsoft.Maui.ApplicationModel; // For AppTheme
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// The SettingsPage allows users to modify application preferences, including theme selection (light/dark mode).
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// This constructor retrieves the saved theme preference and sets the switch accordingly.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            // Retrieve the saved theme preference from Preferences storage.
            // If no preference is saved, default to "Light".
            string savedTheme = Preferences.Get("AppTheme", "Light");

            // Set the switch based on the saved preference
            ThemeToggleSwitch.IsToggled = (savedTheme == "Dark");

            // Set the background color based on the theme
            UpdateBackgroundColor(savedTheme);
        }

        private void UpdateBackgroundColor(string theme)
        {
            // Set the background color based on the theme
            if (theme == "Dark")
            {
                ThemeStackLayout.BackgroundColor = Color.FromHex("#1E1E1E"); // Dark background
            }
            else
            {
                ThemeStackLayout.BackgroundColor = (Color)Application.Current.Resources["White"]; // Use resource dictionary
            }
        }

        /// <summary>
        /// Handles the event when the user toggles the switch for the theme.
        /// Updates the saved preference and applies the selected theme.
        /// </summary>
        /// <param name="sender">The Switch control that triggered the event.</param>
        /// <param name="e">Event data for the toggle change.</param>
        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            // Check the state of the switch
            if (ThemeToggleSwitch.IsToggled)
            {
                Preferences.Set("AppTheme", "Dark");
                ((App)Application.Current).SetAppTheme(AppTheme.Dark);
                UpdateBackgroundColor("Dark"); // Update background for this page
                Debug.WriteLine("Theme changed to Dark");
            }
            else
            {
                Preferences.Set("AppTheme", "Light");
                ((App)Application.Current).SetAppTheme(AppTheme.Light);
                UpdateBackgroundColor("Light"); // Update background for this page
                Debug.WriteLine("Theme changed to Light");
            }
        }
    }
}
