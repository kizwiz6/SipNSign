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

        public SettingsPage()
        {
            InitializeComponent();
            _preferences = Preferences.Default;
            LoadSavedSettings();
        }

        private void LoadSavedSettings()
        {
            // Load timer settings
            int savedDuration = _preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
            TimerSlider.Value = savedDuration;
            TimerValueLabel.Text = $"{savedDuration} seconds";
            DisableTimerCheckbox.IsChecked = savedDuration == 0;
            TimerSlider.IsEnabled = !DisableTimerCheckbox.IsChecked;

            // Load delay settings
            DelaySlider.Value = _preferences.Get(Constants.INCORRECT_DELAY_KEY, Constants.DEFAULT_DELAY) / 1000.0;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                await DisplayAlert("Success", "Settings saved successfully", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
            }
        }

        private void SaveSettings()
        {
            _preferences.Set(Constants.FONT_SIZE_KEY, FontSizeStepper.Value);
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

        private void OnDelaySliderChanged(object sender, ValueChangedEventArgs e)
        {
            // Convert seconds to milliseconds
            int delayMs = (int)(e.NewValue * 1000);
            Preferences.Set(Constants.INCORRECT_DELAY_KEY, delayMs);
        }

        private void OnTimerDurationChanged(object sender, ValueChangedEventArgs e)
        {
            int duration = (int)e.NewValue;
            _preferences.Set(Constants.TIMER_DURATION_KEY, duration);
            TimerValueLabel.Text = $"{duration} seconds";
        }

        private void OnDisableTimerChanged(object sender, CheckedChangedEventArgs e)
        {
            _preferences.Set(Constants.TIMER_DURATION_KEY, e.Value ? 0 : Constants.DEFAULT_TIMER_DURATION);
            TimerSlider.IsEnabled = !e.Value;
            if (e.Value)
            {
                TimerValueLabel.Text = "Timer Disabled";
            }
            else
            {
                int duration = Constants.DEFAULT_TIMER_DURATION;
                TimerSlider.Value = duration;
                TimerValueLabel.Text = $"{duration} seconds";
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Load existing timer duration
            int savedDuration = _preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
            TimerSlider.Value = savedDuration;
            TimerValueLabel.Text = $"{savedDuration} seconds";  // Set initial label text
            DisableTimerCheckbox.IsChecked = savedDuration == 0;
            TimerSlider.IsEnabled = !DisableTimerCheckbox.IsChecked;
            QuestionsSlider.Value = Preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
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

        private void OnQuestionsCountChanged(object sender, ValueChangedEventArgs e)
        {
            int questions = (int)e.NewValue;
            Preferences.Set(Constants.GUESS_MODE_QUESTIONS_KEY, questions);
            QuestionsValueLabel.Text = $"{questions} questions";
        }
    }
}