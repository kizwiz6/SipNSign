using com.kizwiz.sipnsign.Enums;
using com.kizwiz.sipnsign.Pages;
using System.Diagnostics;

namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Manages application-wide theme settings and colors
    /// </summary>
    public class ThemeService : IThemeService
    {
        private const string THEME_KEY = "app_theme";
        public event EventHandler ThemeChanged;

        /// <summary>
        /// Theme color definitions with complementary button/text colors
        /// </summary>
        private readonly Dictionary<CustomAppTheme, ThemeColors> ThemeDefinitions = new()
        {
            {
                CustomAppTheme.Blue, new ThemeColors {
                    Background1 = Color.FromArgb("#1a237e"),
                    Background2 = Color.FromArgb("#0d47a1"),
                    Primary = Color.FromArgb("#FFA726"),
                    Secondary = Color.FromArgb("#FFD95B"),
                    MenuButton1 = Color.FromArgb("#007BFF"),  // Blue
                    MenuButton2 = Color.FromArgb("#28a745"),  // Green
                    MenuButton3 = Color.FromArgb("#FFC107"),  // Yellow
                    MenuButton4 = Color.FromArgb("#FF5722"),  // Orange
                    AnswerButton = Color.FromArgb("#4FC3F7"),  // Light blue
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#1a237e"),
                    ShellForeground = Colors.White,
                    BackgroundImage = "blue_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Dark, new ThemeColors {
                    Background1 = Color.FromArgb("#121212"),
                    Background2 = Color.FromArgb("#1E1E1E"),
                    Primary = Color.FromArgb("#BB86FC"),
                    Secondary = Color.FromArgb("#03DAC6"),
                    MenuButton1 = Color.FromArgb("#BB86FC"),  // Purple
                    MenuButton2 = Color.FromArgb("#03DAC6"),  // Teal
                    MenuButton3 = Color.FromArgb("#CF6679"),  // Pink
                    MenuButton4 = Color.FromArgb("#FF7597"),  // Light Pink
                    AnswerButton = Color.FromArgb("#B39DDB"),  // Light purple
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#121212"),
                    ShellForeground = Color.FromArgb("#BB86FC"),
                    BackgroundImage = "dark_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Light, new ThemeColors {
                    Background1 = Colors.White,
                    Background2 = Color.FromArgb("#F5F5F5"),
                    Primary = Color.FromArgb("#1976D2"),
                    Secondary = Color.FromArgb("#2196F3"),
                    MenuButton1 = Color.FromArgb("#0056b3"),    // Darker blue for contrast
                    MenuButton2 = Color.FromArgb("#004d40"),    // Darker teal
                    MenuButton3 = Color.FromArgb("#e65100"),    // Darker orange
                    MenuButton4 = Color.FromArgb("#b71c1c"),    // Darker red
                    AnswerButton = Color.FromArgb("#0056b3"),  // Dark blue
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Colors.White,
                    ShellForeground = Color.FromArgb("#1976D2"),
                    BackgroundImage = "grey_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Sunset, new ThemeColors {
                    Background1 = Color.FromArgb("#FF512F"),
                    Background2 = Color.FromArgb("#DD2476"),
                    Primary = Color.FromArgb("#FFA07A"),
                    Secondary = Color.FromArgb("#FFD700"),
                    MenuButton1 = Color.FromArgb("#FF6B6B"),  // Coral
                    MenuButton2 = Color.FromArgb("#FFB900"),  // Gold
                    MenuButton3 = Color.FromArgb("#FF8C42"),  // Orange
                    MenuButton4 = Color.FromArgb("#FF5757"),  // Red-Orange
                    AnswerButton = Color.FromArgb("#FFB74D"),  // Light peach
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#FF512F"),
                    ShellForeground = Colors.White,
                    BackgroundImage = "sunset_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Forest, new ThemeColors {
                    Background1 = Color.FromArgb("#2D5A27"),
                    Background2 = Color.FromArgb("#1B4332"),
                    Primary = Color.FromArgb("#95D5B2"),
                    Secondary = Color.FromArgb("#74C69D"),
                    MenuButton1 = Color.FromArgb("#2D6A4F"),  // Dark Green
                    MenuButton2 = Color.FromArgb("#40916C"),  // Medium Green
                    MenuButton3 = Color.FromArgb("#52B788"),  // Light Green
                    MenuButton4 = Color.FromArgb("#74C69D"),  // Pale Green
                    AnswerButton = Color.FromArgb("#81C784"),  // Light mint green
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#2D5A27"),  // Dark Forest Green
                    ShellForeground = Color.FromArgb("#95D5B2"),   // Light Forest Green
                    BackgroundImage = "forest_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Ocean, new ThemeColors {
                    Background1 = Color.FromArgb("#1A5F7A"),
                    Background2 = Color.FromArgb("#086E7D"),
                    Primary = Color.FromArgb("#00FFE1"),
                    Secondary = Color.FromArgb("#98DFD6"),
                    MenuButton1 = Color.FromArgb("#00B4D8"),    // Brighter blue
                    MenuButton2 = Color.FromArgb("#48CAE4"),    // Light blue
                    MenuButton3 = Color.FromArgb("#90E0EF"),    // Pale blue
                    MenuButton4 = Color.FromArgb("#ADE8F4"),    // Very light blue
                    AnswerButton = Color.FromArgb("#4DD0E1"),  // Light cyan
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#1A5F7A"),
                    ShellForeground = Color.FromArgb("#00FFE1"),
                    BackgroundImage = "ocean_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Neon, new ThemeColors {
                    Background1 = Color.FromArgb("#0C0032"),
                    Background2 = Color.FromArgb("#190061"),
                    Primary = Color.FromArgb("#FF00FF"),
                    Secondary = Color.FromArgb("#00FF9F"),
                    MenuButton1 = Color.FromArgb("#FF00FF"),  // Magenta
                    MenuButton2 = Color.FromArgb("#00FF9F"),  // Cyan
                    MenuButton3 = Color.FromArgb("#FFFF00"),  // Yellow
                    MenuButton4 = Color.FromArgb("#FF3366"),  // Pink
                    AnswerButton = Color.FromArgb("#00E5FF"),  // Bright cyan
                    LightText = Color.FromArgb("#00FF9F"),
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#0C0032"),  // Deep Purple
                    ShellForeground = Color.FromArgb("#FF00FF"),   // Neon Pink
                    BackgroundImage = "neon_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Monochrome, new ThemeColors {
                    Background1 = Color.FromArgb("#2C3E50"),
                    Background2 = Color.FromArgb("#34495E"),
                    Primary = Color.FromArgb("#ECF0F1"),
                    Secondary = Color.FromArgb("#BDC3C7"),
                    MenuButton1 = Color.FromArgb("#95A5A6"),  // Light Gray
                    MenuButton2 = Color.FromArgb("#7F8C8D"),  // Medium Gray
                    MenuButton3 = Color.FromArgb("#34495E"),  // Dark Gray
                    MenuButton4 = Color.FromArgb("#2C3E50"),  // Darker Gray
                    AnswerButton = Color.FromArgb("#BDBDBD"),  // Light gray
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#2C3E50"),  // Dark Gray
                    ShellForeground = Color.FromArgb("#ECF0F1"),   // Light Gray
                    BackgroundImage = "monochrome_theme_bg.png"
                }
            }
        };

        /// <summary>
        /// Gets current theme colors based on saved preferences
        /// </summary>
        public ThemeColors CurrentTheme => ThemeDefinitions[GetCurrentTheme()];

        /// <summary>
        /// Changes the application theme and updates all resources, including the background image.
        /// </summary>
        /// <param name="theme">The theme to set for the application.</param>
        public void SetTheme(CustomAppTheme theme)
        {
            // Save the selected theme to preferences
            Preferences.Set(THEME_KEY, theme.ToString());

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var resources = Application.Current?.Resources;
                if (resources == null)
                {
                    Debug.WriteLine("Application resources are null. Theme update aborted.");
                    return;
                }

                if (!ThemeDefinitions.TryGetValue(theme, out var themeColors))
                {
                    Debug.WriteLine($"Theme '{theme}' not found in ThemeDefinitions.");
                    return;
                }

                // Update theme-related resources
                resources["AppBackground1"] = themeColors.Background1;
                resources["AppBackground2"] = themeColors.Background2;
                resources["Primary"] = themeColors.Primary;
                resources["Secondary"] = themeColors.Secondary;
                resources["GuessMode"] = themeColors.MenuButton1;
                resources["PerformMode"] = themeColors.MenuButton2;
                resources["Progress"] = themeColors.MenuButton3;
                resources["Settings"] = themeColors.MenuButton4;
                resources["AnswerButton"] = themeColors.AnswerButton;

                // Set the background image resource
                resources["CurrentThemeBackground"] = $"Themes/{themeColors.BackgroundImage}";
                Debug.WriteLine($"Theme '{theme}' applied with AnswerButton color: {themeColors.AnswerButton} and background image: {themeColors.BackgroundImage}");

                // Update Shell background
                if (Shell.Current != null)
                {
                    Shell.Current.BackgroundColor = themeColors.ShellBackground;
                }

                // Refresh the UI for Shell and MainPage
                if (Application.Current?.MainPage is Shell shell)
                {
                    shell.ForceLayout();

                    // Refresh MainMenuPage if present
                    var mainPage = shell.Navigation?.NavigationStack
                        .FirstOrDefault(page => page is MainMenuPage) as MainMenuPage;

                    mainPage?.Dispatcher.Dispatch(mainPage.ForceRefresh);
                }

                // Update all pages in the navigation stack
                var navigation = Application.Current?.MainPage?.Navigation;
                if (navigation != null)
                {
                    foreach (var page in navigation.NavigationStack.OfType<MainMenuPage>())
                    {
                        page.Dispatcher.Dispatch(page.ForceRefresh);
                    }
                }

                // Notify listeners that the theme has changed
                ThemeChanged?.Invoke(this, EventArgs.Empty);
            });
        }


        /// <summary>
        /// Gets the current theme from preferences or returns default
        /// </summary>
        public CustomAppTheme GetCurrentTheme()  // Make this a normal method, not explicit implementation
        {
            var themeName = Preferences.Get(THEME_KEY, CustomAppTheme.Blue.ToString());
            return Enum.Parse<CustomAppTheme>(themeName);
        }
    }

    /// <summary>
    /// Represents a complete set of colors for a theme.
    /// </summary>
    public class ThemeColors
    {
        /// <summary>
        /// Gets or sets the primary background color.
        /// </summary>
        public Color Background1 { get; set; } = Colors.DarkBlue;

        /// <summary>
        /// Gets or sets the secondary background color.
        /// </summary>
        public Color Background2 { get; set; } = Colors.BlueViolet;

        /// <summary>
        /// Gets or sets the primary color.
        /// </summary>
        public Color Primary { get; set; } = Colors.Blue;

        /// <summary>
        /// Gets or sets the secondary color.
        /// </summary>
        public Color Secondary { get; set; } = Colors.Gray;

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Color Text { get; set; } = Colors.Black;

        public Color LightText { get; set; } = Colors.White;

        public Color DarkText { get; set; } = Color.FromArgb("#1E1E1E");
        public Color MenuButton1 { get; set; }  // Guess Mode button
        public Color MenuButton2 { get; set; }  // Perform Mode button
        public Color MenuButton3 { get; set; }  // Progress button
        public Color MenuButton4 { get; set; }  // Settings button
        public Color AnswerButton { get; set; } // Answer choice buttons
        public Color ShellBackground { get; set; }
        public Color ShellForeground { get; set; }  // For text/icons
        public string BackgroundImage { get; set; }
    }
}
