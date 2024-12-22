using Microsoft.Maui.Storage; // For Preferences
using Microsoft.Maui.ApplicationModel; // For AppTheme
using System.Diagnostics;
using Microsoft.Maui.Controls;
using com.kizwiz.sipnsign.Services;
using com.kizwiz.sipnsign.Enums;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Handles application settings and theme changes
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        private readonly IPreferences _preferences = Preferences.Default;
        private readonly IThemeService _themeService;

        public SettingsPage(IThemeService themeService)
        {
            InitializeComponent();
            _themeService = themeService;
            ThemePicker.SelectedItem = _themeService.GetCurrentTheme().ToString();

            // Ensure SoberModeSwitch is initialized with the current preference value
            SoberModeSwitch.IsToggled = Preferences.Get(Constants.SOBER_MODE_KEY, false);
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

        // Event handler for the toggle switch
        private void OnSoberModeToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set(Constants.SOBER_MODE_KEY, e.Value);
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

        private async void SaveSettings()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Settings Saved", "Your preferences have been updated", "OK");
            }
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

            // Update background gradient
            if (Application.Current?.Resources != null)
            {
                var background1 = (Color)Application.Current.Resources["AppBackground1"];
                var background2 = (Color)Application.Current.Resources["AppBackground2"];

                this.Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = new GradientStopCollection
                   {
                       new GradientStop { Color = background1, Offset = 0.0f },
                       new GradientStop { Color = background2, Offset = 1.0f }
                   }
                };
            }

            // Load existing timer duration
            int savedDuration = Preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
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

        /// <summary>
        /// Handles theme selection changes and updates application appearance
        /// </summary>
        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && picker.SelectedItem is string themeName)
            {
                var theme = Enum.Parse<CustomAppTheme>(themeName);  // Use CustomAppTheme here
                _themeService.SetTheme(theme);
            }
        }

        /// <summary>
        /// Handles theme selection changes from the theme picker dropdown.
        /// Updates the application's theme using ThemeService.
        /// </summary>
        /// <param name="sender">The source of the event (ThemePicker)</param>
        /// <param name="e">Event arguments</param>
        private void OnThemeSelected(object sender, EventArgs e)
        {
            try
            {
                if (ThemePicker?.SelectedItem is string selectedTheme)
                {
                    if (Enum.TryParse<CustomAppTheme>(selectedTheme, out var theme))
                    {
                        // Update theme using ThemeService
                        _themeService.SetTheme(theme);

                        // Save selected theme preference
                        Preferences.Set(Constants.THEME_KEY, selectedTheme);
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to parse theme value: {selectedTheme}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying theme: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "Failed to apply theme. Please try again.", "OK");
                });
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

    private async Task ViewLogs()
        {
            try
            {
                var logFile = Path.Combine(FileSystem.AppDataDirectory, "app.log");
                if (File.Exists(logFile))
                {
                    var logs = await File.ReadAllTextAsync(logFile);
                    await DisplayAlert("Application Logs", logs, "OK");
                }
                else
                {
                    await DisplayAlert("Logs", "No logs found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not read logs: {ex.Message}", "OK");
            }
        }
    }
}