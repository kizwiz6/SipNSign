using com.kizwiz.sipnsign.Pages;
using Microsoft.Maui.ApplicationModel; // Ensure this is included

namespace com.kizwiz.sipnsign
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
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
            }
            else if (theme == AppTheme.Dark)
            {
                Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Themes.DarkThemeResourceDictionary());
            }

            // Save the user's preference for future launches
            Preferences.Set("UserTheme", theme == AppTheme.Dark ? "dark" : "light");
        }

    }
}
