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
        private Color _primaryTextColor; // Primary color for text
        private Color _secondaryTextColor; // Secondary color for text

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPage"/> class.
        /// This constructor retrieves the saved theme preference and sets the switch accordingly.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            // Retrieve the saved theme preference from Preferences storage.
            string savedTheme = Preferences.Get("AppTheme", "Light");

            // Set the switch based on the saved preference
            if (ThemeToggleSwitch != null) // Check if the ThemeToggleSwitch is initialized
            {
                ThemeToggleSwitch.IsToggled = (savedTheme == "Dark");
            }

            // Set the background color based on the theme
            UpdateBackgroundColor(savedTheme);
            UpdateTextColor(savedTheme);
        }

        /// <summary>
        /// Updates the background color of the settings page based on the selected theme.
        /// </summary>
        /// <param name="theme">The theme (Light or Dark).</param>
        private void UpdateBackgroundColor(string theme)
        {
            if (theme == "Dark")
            {
                ThemeStackLayout.BackgroundColor = Color.FromArgb("#1E1E1E"); // Dark background
            }
            else
            {
                ThemeStackLayout.BackgroundColor = (Color)Application.Current.Resources["White"]; // Light background
            }
        }

        /// <summary>
        /// Updates the text colors based on the selected theme.
        /// </summary>
        /// <param name="theme">The theme (Light or Dark).</param>
        private void UpdateTextColor(string theme)
        {
            // Update text colors based on the theme
            if (theme == "Dark")
            {
                _primaryTextColor = Application.Current.Resources["PrimaryTextColor"] is Color color ? color : Colors.Transparent; // Fallback to transparent
                _secondaryTextColor = Application.Current.Resources["SecondaryTextColor"] is Color secColor ? secColor : Colors.Gray; // Fallback to gray
            }
            else
            {
                _primaryTextColor = Application.Current.Resources["Black"] is Color blackColor ? blackColor : Colors.Black; // Fallback to black
                _secondaryTextColor = Application.Current.Resources["SecondaryTextColor"] is Color secColor ? secColor : Colors.LightGray; // Fallback to light gray
            }

            // Update the text colors immediately
            if (SettingsTitleLabel != null) // Check if the label is initialized
            {
                SettingsTitleLabel.TextColor = _primaryTextColor; // Set title label color
            }
            if (ThemeSelectionLabel != null) // Check if the label is initialized
            {
                ThemeSelectionLabel.TextColor = _primaryTextColor; // Set selection label color
            }
            if (InfoLabel != null) // Check if the label is initialized
            {
                InfoLabel.TextColor = _secondaryTextColor; // Set info label color
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
            if (ThemeToggleSwitch != null) // Check if the ThemeToggleSwitch is initialized
            {
                string theme = ThemeToggleSwitch.IsToggled ? "Dark" : "Light";
                Preferences.Set("AppTheme", theme);
                Application.Current.UserAppTheme = (AppTheme)Enum.Parse(typeof(AppTheme), theme);
                UpdateBackgroundColor(theme);
                UpdateTextColor(theme);
                Debug.WriteLine($"Theme changed to {theme}"); // Log theme change
            }
        }
    }
}
