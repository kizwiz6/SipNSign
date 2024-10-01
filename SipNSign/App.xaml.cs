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

            // Set initial theme based on system preference
            SetAppTheme(Application.Current.RequestedTheme);

            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                SetAppTheme(e.RequestedTheme);
            };

            MainPage = new NavigationPage(new MainMenuPage());
        }

        public void SetAppTheme(AppTheme theme)
        {
            // Switch to Light or Dark Theme
            if (theme == AppTheme.Light)
            {
                // Set the light theme
                Resources = (ResourceDictionary)Resources.MergedDictionaries.First(x => x is ResourceDictionary dict && dict.ContainsKey("LightTheme"));
            }
            else
            {
                // Set the dark theme
                Resources = (ResourceDictionary)Resources.MergedDictionaries.First(x => x is ResourceDictionary dict && dict.ContainsKey("DarkTheme"));
            }
        }
    }
}
