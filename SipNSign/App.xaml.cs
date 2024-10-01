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
            // Clear the existing merged dictionaries to remove the current theme
            Resources.MergedDictionaries.Clear();

            // Add the global styles again since they were cleared
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Resources/Styles/Colors.xaml", UriKind.Relative) });
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Resources/Styles/Styles.xaml", UriKind.Relative) });

            // Load the appropriate theme ResourceDictionary
            if (theme == AppTheme.Light)
            {
                // Load LightTheme.xaml
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Resources/Themes/LightTheme.xaml", UriKind.Relative) });
            }
            else
            {
                // Load DarkTheme.xaml
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Resources/Themes/DarkTheme.xaml", UriKind.Relative) });
            }
        }
    }
}
