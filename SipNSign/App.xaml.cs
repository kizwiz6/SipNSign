using com.kizwiz.sipnsign.Pages;

namespace com.kizwiz.sipnsign
{
    /// <summary>
    /// Represents the main application for the SipNSign project.
    /// Initializes the application and sets the main page.
    /// </summary>
    public partial class App : Microsoft.Maui.Controls.Application // Fully qualify the Application class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Set the initial theme based on the system's current preference
            SetAppTheme(Application.Current.RequestedTheme);

            // Subscribe to the RequestedThemeChanged event to update the theme dynamically
            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                SetAppTheme(e.RequestedTheme);
            };

            // Set the main page of the application
            MainPage = new NavigationPage(new MainMenuPage());
        }

        /// <summary>
        /// Sets the application theme based on the specified AppTheme.
        /// </summary>
        /// <param name="theme">The theme to set (Light or Dark).</param>
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
                // Add Light Theme resources
                Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Themes.LightThemeResourceDictionary());
            }
            else if (theme == AppTheme.Dark)
            {
                // Add Dark Theme resources
                Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Themes.DarkThemeResourceDictionary());
            }
            else
            {
                // If neither theme is detected, default to Light Theme
                Resources.MergedDictionaries.Add(new com.kizwiz.sipnsign.Resources.Themes.LightThemeResourceDictionary());
            }
        }
    }
}
