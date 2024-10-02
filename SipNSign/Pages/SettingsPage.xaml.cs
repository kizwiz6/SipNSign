using Microsoft.Maui.Storage; // For Preferences
using Microsoft.Maui.ApplicationModel; // For AppTheme
using System.Diagnostics;
using CommunityToolkit.Maui.Extensions;
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
        /// This constructor retrieves the saved theme preference and sets the picker accordingly.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();

            // Retrieve the saved theme preference from Preferences storage.
            // If no preference is saved, default to "Light".
            string savedTheme = Preferences.Get("AppTheme", "Light");

            // Set the current theme in the picker based on the saved preference.
            // 0 corresponds to "Light", and 1 corresponds to "Dark".
            ThemePicker.SelectedIndex = (savedTheme == "Light") ? 0 : 1;
        }

        /// <summary>
        /// Handles the event when the user changes the selected theme in the picker.
        /// Updates the saved preference and applies the selected theme.
        /// </summary>
        /// <param name="sender">The Picker control that triggered the event.</param>
        /// <param name="e">Event data for the selection change.</param>
        private void OnThemeChanged(object sender, EventArgs e)
        {
            // Check if the selected index is for Light theme.
            if (ThemePicker.SelectedIndex == 0)
            {
                // Save the selected theme preference as "Light".
                Preferences.Set("AppTheme", "Light");

                // Apply the Light theme by calling the SetAppTheme method.
                ((App)Application.Current).SetAppTheme(AppTheme.Light);

                // Log the change to the debug output.
                Debug.WriteLine("Theme changed to Light");
            }
            else
            {
                // Save the selected theme preference as "Dark".
                Preferences.Set("AppTheme", "Dark");

                // Apply the Dark theme by calling the SetAppTheme method.
                ((App)Application.Current).SetAppTheme(AppTheme.Dark);

                // Log the change to the debug output.
                Debug.WriteLine("Theme changed to Dark");
            }
        }
    }
}
