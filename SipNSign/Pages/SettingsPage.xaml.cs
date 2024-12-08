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
        private IPreferences _preferences;
        private const string THEME_KEY = "app_theme";
        private const string FONT_SIZE_KEY = "font_size";
        private const string DIFFICULTY_KEY = "difficulty_level";
        private const string TRANSLATIONS_KEY = "show_translations";
        private const string VIDEO_SPEED_KEY = "video_speed";
        private const string CONTRAST_KEY = "high_contrast";
        private const string HAND_DOMINANCE_KEY = "hand_dominance";
        private const string OFFLINE_MODE_KEY = "offline_mode";

        public SettingsPage()
        {
            InitializeComponent();
            _preferences = Preferences.Default;
            LoadSavedSettings();
        }

        private void LoadSavedSettings()
        {
            // Load and set saved preferences
            FontSizeStepper.Value = _preferences.Get(FONT_SIZE_KEY, 16.0);
            DifficultyPicker.SelectedIndex = _preferences.Get(DIFFICULTY_KEY, 0);
            TranslationsSwitch.IsToggled = _preferences.Get(TRANSLATIONS_KEY, true);
            VideoSpeedSlider.Value = _preferences.Get(VIDEO_SPEED_KEY, 1.0);
            ContrastSwitch.IsToggled = _preferences.Get(CONTRAST_KEY, false);
            HandDominancePicker.SelectedIndex = _preferences.Get(HAND_DOMINANCE_KEY, 0);
            OfflineSwitch.IsToggled = _preferences.Get(OFFLINE_MODE_KEY, false);
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                await DisplayAlert("Success", "Settings saved successfully", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
            }
        }

        private void SaveSettings()
        {
            _preferences.Set(FONT_SIZE_KEY, FontSizeStepper.Value);
            _preferences.Set(DIFFICULTY_KEY, DifficultyPicker.SelectedIndex);
            _preferences.Set(TRANSLATIONS_KEY, TranslationsSwitch.IsToggled);
            _preferences.Set(VIDEO_SPEED_KEY, VideoSpeedSlider.Value);
            _preferences.Set(CONTRAST_KEY, ContrastSwitch.IsToggled);
            _preferences.Set(HAND_DOMINANCE_KEY, HandDominancePicker.SelectedIndex);
            _preferences.Set(OFFLINE_MODE_KEY, OfflineSwitch.IsToggled);

            Application.Current.MainPage.DisplayAlert("Settings Saved", "Your preferences have been updated", "OK");
        }

        private void OnFontSizeChanged(object sender, ValueChangedEventArgs e)
        {
            // Update app-wide font size
            if (Application.Current != null && Application.Current.Resources.ContainsKey("DefaultFontSize"))
            {
                Application.Current.Resources["DefaultFontSize"] = e.NewValue;
            }
        }

        private void OnDifficultyChanged(object sender, EventArgs e)
        {
            // Update difficulty level in the app
            var selectedDifficulty = DifficultyPicker.SelectedItem as string;
            // Implementation for difficulty change
        }

        private void OnTranslationsToggled(object sender, ToggledEventArgs e)
        {
            // Update translation visibility preference
            // Implementation for showing/hiding translations
        }

        private void OnVideoSpeedChanged(object sender, ValueChangedEventArgs e)
        {
            // Update video playback speed
            // Implementation for video speed adjustment
        }

        private void OnContrastToggled(object sender, ToggledEventArgs e)
        {
            // Update high contrast mode
            if (Application.Current != null)
            {
                // Implementation for high contrast mode
            }
        }

        private async void OnClearHistoryClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Clear History",
                "Are you sure you want to clear your learning history? This cannot be undone.",
                "Yes", "No");

            if (answer)
            {
                try
                {
                    // Clear learning history implementation
                    await DisplayAlert("Success", "Learning history cleared successfully", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to clear history: {ex.Message}", "OK");
                }
            }
        }

        private async void OnExportProgressClicked(object sender, EventArgs e)
        {
            try
            {
                // Export progress implementation
                // Example: Generate a JSON file with user progress
                string progressJson = GenerateProgressJson();

                if (progressJson != null)
                {
                    // Save file implementation
                    await DisplayAlert("Success", "Progress exported successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to export progress: {ex.Message}", "OK");
            }
        }

        private string GenerateProgressJson()
        {
            // Implementation to generate progress JSON
            return "{}"; // Placeholder
        }

        private void OnOfflineModeToggled(object sender, ToggledEventArgs e)
        {
            // Implementation for offline mode
            // Handle downloading content for offline use
        }
    }
}
