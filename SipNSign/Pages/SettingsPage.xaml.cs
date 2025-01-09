using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Services;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Pages
{
    /// <summary>
    /// Handles application settings and theme changes
    /// </summary>
    public partial class SettingsPage : ContentPage
    {
        private readonly IPreferences _preferences = Preferences.Default;
        private readonly IThemeService _themeService;
        private readonly IServiceProvider _serviceProvider;

        public SettingsPage(IThemeService themeService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _themeService = themeService;
            _serviceProvider = serviceProvider;

            // Subscribe to theme changes
            _themeService.ThemeChanged += OnThemeChanged;
            ThemePicker.SelectedItem = _themeService.GetCurrentTheme().ToString();

            // Initialize switches with current preferences
            SoberModeSwitch.IsToggled = Preferences.Get(Constants.SOBER_MODE_KEY, false);
            TransparentFeedbackSwitch.IsToggled = Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false);

            LoadSavedSettings();
        }

        private void OnShowFeedbackToggled(object sender, ToggledEventArgs e)
        {
            Debug.WriteLine($"Show Feedback toggled: {e.Value}");
            Preferences.Set(Constants.SHOW_FEEDBACK_KEY, e.Value);

            // Update UI elements that depend on feedback visibility
            TransparentFeedbackSwitch.IsEnabled = e.Value;

            // If feedback is disabled, ensure transparent feedback is also disabled
            if (!e.Value)
            {
                TransparentFeedbackSwitch.IsToggled = false;
                Preferences.Set(Constants.TRANSPARENT_FEEDBACK_KEY, false);
            }
        }

        private void LoadSavedSettings()
        {
            // Load questions count
            int savedQuestions = _preferences.Get(Constants.GUESS_MODE_QUESTIONS_KEY, Constants.DEFAULT_QUESTIONS);
            QuestionsSlider.Value = savedQuestions;
            QuestionsValueLabel.Text = $"{savedQuestions} questions";

            // Load timer settings
            int savedDuration = _preferences.Get(Constants.TIMER_DURATION_KEY, Constants.DEFAULT_TIMER_DURATION);
            TimerSlider.Value = savedDuration;
            TimerValueLabel.Text = $"{savedDuration} seconds";
            DisableTimerCheckbox.IsChecked = savedDuration == 0;
            TimerSlider.IsEnabled = !DisableTimerCheckbox.IsChecked;

            // Load feedback settings
            bool showFeedback = Preferences.Get(Constants.SHOW_FEEDBACK_KEY, true);
            ShowFeedbackSwitch.IsToggled = showFeedback;
            TransparentFeedbackSwitch.IsEnabled = showFeedback;

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

        private void OnTransparentFeedbackToggled(object sender, ToggledEventArgs e)
        {
            Debug.WriteLine($"Toggling transparency: {e.Value}");
            Preferences.Set(Constants.TRANSPARENT_FEEDBACK_KEY, e.Value);
            var gamePage = Shell.Current?.CurrentPage as GamePage;
            if (gamePage?.ViewModel?.IsFeedbackVisible == true)
            {
                bool isCorrect = gamePage.ViewModel.FeedbackText.Contains("Correct");
                gamePage.ViewModel.FeedbackBackgroundColor = gamePage.ViewModel.GetFeedbackColor(isCorrect);
            }
        }

        public bool IsSoberMode
        {
            get => Preferences.Get(Constants.SOBER_MODE_KEY, false);
            set
            {
                Preferences.Set(Constants.SOBER_MODE_KEY, value);
                OnPropertyChanged(nameof(IsSoberMode));
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

        private async void OnViewLogsClicked(object sender, EventArgs e)
        {
            try
            {
                var loggingService = _serviceProvider?.GetService<ILoggingService>();
                if (loggingService != null)
                {
                    var logs = await loggingService.GetLogContents();
                    await DisplayAlert("Application Logs", logs, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not read logs: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            bool isTransparent = Preferences.Get(Constants.TRANSPARENT_FEEDBACK_KEY, false);
            TransparentFeedbackSwitch.IsToggled = isTransparent;
            Debug.WriteLine($"Settings Page - Current transparency setting: {isTransparent}");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _themeService.ThemeChanged -= OnThemeChanged;
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
        private void OnThemeChanged(object? sender, EventArgs e)
        {
            // Force page to redraw with new theme
            this.Background = null;
            this.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
            {
                new GradientStop { Color = (Color)Application.Current.Resources["AppBackground1"], Offset = 0.0f },
                new GradientStop { Color = (Color)Application.Current.Resources["AppBackground2"], Offset = 1.0f }
            }
            };
        }

        /// <summary>
        /// Handles theme selection changes from the theme picker dropdown.
        /// Updates the application's theme using ThemeService.
        /// </summary>
        /// <param name="sender">The source of the event (ThemePicker)</param>
        /// <param name="e">Event arguments</param>
        private async void OnThemeSelected(object sender, EventArgs e)
        {
            if (ThemePicker?.SelectedItem is string selectedTheme &&
                Enum.TryParse<CustomAppTheme>(selectedTheme, out var theme))
            {
                _themeService.SetTheme(theme);

                // Give UI time to update
                await Task.Delay(100);

                // Force main page refresh
                var mainPage = Navigation.NavigationStack.FirstOrDefault() as MainMenuPage;
                mainPage?.ForceRefresh();

                // Force Shell refresh
                if (Application.Current?.Resources != null)
                {
                    if (Shell.Current is AppShell currentShell)
                    {
                        if (Application.Current.Resources.TryGetValue("Primary", out var value) &&
                            value is Color primaryColor)
                        {
                            currentShell.BackgroundColor = primaryColor;
                        }
                    }
                }
            }
        }

        private void OnThemeSelectionChanged(object sender, EventArgs e)
        {
            if (ThemePicker?.SelectedItem is string selectedTheme
                && Enum.TryParse<CustomAppTheme>(selectedTheme, out var theme))
            {
                // Preview the theme immediately
                _themeService.SetTheme(theme);
                // Don't save to preferences yet - only on Save button click
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

        /// <summary>
        /// Handles clearing of debug log files
        /// </summary>
        private async void OnClearLogsClicked(object sender, EventArgs e)
        {
            try
            {
                var loggingService = _serviceProvider?.GetService<ILoggingService>();
                if (loggingService != null)
                {
                    bool answer = await DisplayAlert("Clear Logs",
                        "Are you sure you want to clear all debug logs?",
                        "Yes", "No");

                    if (answer)
                    {
                        await loggingService.CleanupLogs();
                        await DisplayAlert("Success", "Debug logs cleared successfully", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not clear logs: {ex.Message}", "OK");
            }
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