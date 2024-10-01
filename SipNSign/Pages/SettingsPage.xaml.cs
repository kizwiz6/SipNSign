using Microsoft.Maui.Storage; // For Preferences
using Microsoft.Maui.ApplicationModel; // For AppTheme

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// The SettingsPage allows users to modify app preferences, including theme selection (light/dark mode).
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        /// <summary>
        /// Initializes the SettingsPage and sets the current theme in the Picker based on saved preferences.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            // Retrieve the saved theme preference from Microsoft.Maui.Storage.Preferences
            string savedTheme = Preferences.Get("AppTheme", "Light") ?? "Light"; // Default to Light if not set

            // Set the current theme in the picker based on the saved preference
            ThemePicker.SelectedIndex = (savedTheme == "Light") ? 0 : 1;
        }

        /// <summary>
        /// Handles theme changes when the user selects a theme from the Picker and saves the preference.
        /// </summary>
        /// <param name="sender">The Picker control where the selection was made.</param>
        /// <param name="e">Event data for the selection change.</param>
        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (ThemePicker.SelectedIndex == 0)
            {
                // Light mode
                Preferences.Set("AppTheme", "Light"); // Save preference
                ((App)Application.Current).SetAppTheme(AppTheme.Light); // Apply Light mode
            }
            else
            {
                // Dark mode
                Preferences.Set("AppTheme", "Dark"); // Save preference
                ((App)Application.Current).SetAppTheme(AppTheme.Dark); // Apply Dark mode
            }
        }
    }
}
