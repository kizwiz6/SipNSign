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
        private Color _primaryTextColor;
        private Color _secondaryTextColor;

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
            UpdateTextColor(savedTheme);
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

        private void UpdateTextColor(string theme)
        {
            // Update text colors based on the theme
            if (theme == "Dark")
            {
                _primaryTextColor = (Color)Application.Current.Resources["PrimaryTextColor"];
                _secondaryTextColor = (Color)Application.Current.Resources["SecondaryTextColor"];
            }
            else
            {
                _primaryTextColor = (Color)Application.Current.Resources["Black"]; // Assuming Black for light theme
                _secondaryTextColor = (Color)Application.Current.Resources["SecondaryTextColor"]; // Light gray for secondary text
            }

            // Update the text colors immediately
            SettingsTitleLabel.TextColor = _primaryTextColor; // Assuming you have named the title label in XAML
            ThemeSelectionLabel.TextColor = _primaryTextColor; // Assuming you have named the toggle label in XAML
            InfoLabel.TextColor = _secondaryTextColor; // Assuming you have named the info label in XAML
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
                Application.Current.UserAppTheme = AppTheme.Dark; // Apply theme for the entire app
                (Application.Current as App)?.SetAppTheme(AppTheme.Dark); // Set the app theme
                UpdateBackgroundColor("Dark"); // Update the background color for SettingsPage
                UpdateTextColor("Dark"); // Update the text colors immediately
                Debug.WriteLine("Theme changed to Dark");
            }
            else
            {
                Preferences.Set("AppTheme", "Light");
                Application.Current.UserAppTheme = AppTheme.Light; // Apply theme for the entire app
                (Application.Current as App)?.SetAppTheme(AppTheme.Light); // Set the app theme
                UpdateBackgroundColor("Light"); // Update the background color for SettingsPage
                UpdateTextColor("Light"); // Update the text colors immediately
                Debug.WriteLine("Theme changed to Light");
            }
        }
    }
}
