using com.kizwiz.sipnsign.Pages;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics; // Ensure this is included

namespace com.kizwiz.sipnsign
{
    /// <summary>
    /// The main application class that initializes the app, manages themes,
    /// and sets the main page for the application.
    /// </summary>
    public partial class App : Microsoft.Maui.Controls.Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// This constructor retrieves the user's saved theme preference
        /// and sets the initial application theme accordingly.
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Retrieve the saved theme preference, defaulting to Light if not set
            var savedTheme = Preferences.Get("UserTheme", "light");
            AppTheme initialTheme = savedTheme == "dark" ? AppTheme.Dark : AppTheme.Light;

            // Set the initial theme based on the user's saved preference
            SetAppTheme(initialTheme);

            // Set the main page of the application
            MainPage = new NavigationPage(new MainMenuPage());
        }

        /// <summary>
        /// Sets the application's theme based on the specified <see cref="AppTheme"/>.
        /// This method clears existing merged resource dictionaries and applies
        /// the relevant theme dictionaries, updating the app's appearance.
        /// </summary>
        /// <param name="theme">The theme to apply, either Light or Dark.</param>
        public void SetAppTheme(AppTheme theme)
        {
            // Clear existing merged dictionaries to avoid conflicts
            Resources.MergedDictionaries.Clear();

            // Add global resource dictionaries (Colors and Styles) again
            Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Styles.ColoursResourceDictionary());
            Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Styles.StylesResourceDictionary());

            // Load the appropriate theme ResourceDictionary based on the current theme
            if (theme == AppTheme.Light)
            {
                Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Themes.LightThemeResourceDictionary());
                UpdateBackgroundColor("Light"); // Ensure background color is updated for Light theme
            }
            else if (theme == AppTheme.Dark)
            {
                Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Themes.DarkThemeResourceDictionary());
                UpdateBackgroundColor("Dark"); // Ensure background color is updated for Dark theme
            }

            // Save the user's preference for future launches
            Preferences.Set("UserTheme", theme == AppTheme.Dark ? "dark" : "light");
        }

        private void UpdateBackgroundColor(string theme)
        {
            // Check if the MainPage is set
            if (Application.Current.MainPage != null)
            {
                // Set the background color based on the theme
                if (theme == "Light")
                {
                    Application.Current.MainPage.BackgroundColor = Color.FromHex("#FFFFFF");
                }
                else if (theme == "Dark")
                {
                    Application.Current.MainPage.BackgroundColor = Color.FromHex("#121212");
                }
            }
            else
            {
                Debug.WriteLine("MainPage is not set. Cannot update background color.");
            }
        }
    }
}
