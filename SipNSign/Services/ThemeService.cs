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
        public event EventHandler? ThemeChanged;

        private readonly Dictionary<CustomAppTheme, Dictionary<string, object>> _themeCache
       = new Dictionary<CustomAppTheme, Dictionary<string, object>>();

        /// <summary>
        /// Theme color definitions with complementary button/text colors
        /// </summary>
        private readonly Dictionary<CustomAppTheme, ThemeColors> ThemeDefinitions = new()
        {
            {
                CustomAppTheme.Blue, new ThemeColors {
                    Background1 = Color.FromArgb("#1a237e"),  // Deep navy
                    Background2 = Color.FromArgb("#0d47a1"),  // Royal blue
                    Primary = Color.FromArgb("#FFA726"),      // Orange
                    Secondary = Color.FromArgb("#FFD95B"),    // Light orange
                    CardBackground = Color.FromArgb("#1565C0"), // Lighter blue for cards
                    CardText = Colors.White,                  // White text for contrast with blue
                    MenuButton1 = Color.FromArgb("#007BFF"),
                    MenuButton2 = Color.FromArgb("#28a745"),
                    MenuButton3 = Color.FromArgb("#FFC107"),
                    MenuButton4 = Color.FromArgb("#FF5722"),
                    AnswerButton = Color.FromArgb("#4FC3F7"),
                    ShellBackground = Color.FromArgb("#1a237e"),
                    ShellForeground = Colors.White,
                    BackgroundImage = "blue_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Dark, new ThemeColors {
                    Background1 = Color.FromArgb("#121212"),  // Almost black
                    Background2 = Color.FromArgb("#1E1E1E"),  // Dark gray
                    Primary = Color.FromArgb("#BB86FC"),      // Purple
                    Secondary = Color.FromArgb("#03DAC6"),    // Teal
                    CardBackground = Color.FromArgb("#2D2D2D"), // Slightly lighter dark for cards
                    CardText = Colors.White,                  // White text for dark theme
                    MenuButton1 = Color.FromArgb("#7B1FA2"),
                    MenuButton2 = Color.FromArgb("#00695C"),
                    MenuButton3 = Color.FromArgb("#7B1FA2"),
                    MenuButton4 = Color.FromArgb("#FF7597"),
                    AnswerButton = Color.FromArgb("#C2185B"),
                    ShellBackground = Color.FromArgb("#121212"),
                    ShellForeground = Color.FromArgb("#BB86FC"),
                    BackgroundImage = "dark_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Light, new ThemeColors {
                Background1 = Color.FromArgb("#DEBACE"),
                Background2 = Color.FromArgb("#F8F9FA"),
                Primary = Color.FromArgb("#9ED5C5"),      // Soft mint
                Secondary = Color.FromArgb("#BFACE2"),    // Soft lavender
                CardBackground = Color.FromArgb("#E3F2FD"),
                CardText = Color.FromArgb("#2C3E50"),     // Dark blue-gray for contrast
                MenuButton1 = Color.FromArgb("#8DDFCB"),  // Mint for Guess Mode
                MenuButton2 = Color.FromArgb("#DEBACE"),  // Soft pink for Perform Mode
                MenuButton3 = Color.FromArgb("#BFA2DB"),  // Light purple for Profile
                MenuButton4 = Color.FromArgb("#95B8D1"),  // Sky blue for Settings
                AnswerButton = Color.FromArgb("#8DDFCB"), // Mint
                ShellBackground = Colors.White,
                ShellForeground = Color.FromArgb("#9ED5C5"),
                BackgroundImage = "pastel_garden_bg.png"
                }
            },
            {
                CustomAppTheme.Sunset, new ThemeColors {
                    Background1 = Color.FromArgb("#FF512F"),  // Orange-red
                    Background2 = Color.FromArgb("#DD2476"),  // Pink
                    Primary = Color.FromArgb("#FFA07A"),      // Light salmon
                    Secondary = Color.FromArgb("#FFD700"),    // Gold
                    CardBackground = Color.FromArgb("#FF7F50"), // Coral for cards
                    CardText = Colors.White,                  // White text
                    MenuButton1 = Color.FromArgb("#FF6B6B"),
                    MenuButton2 = Color.FromArgb("#FFB900"),
                    MenuButton3 = Color.FromArgb("#FF8C42"),
                    MenuButton4 = Color.FromArgb("#FF5757"),
                    AnswerButton = Color.FromArgb("#FFB74D"),
                    ShellBackground = Color.FromArgb("#FF512F"),
                    ShellForeground = Colors.White,
                    BackgroundImage = "sunset_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Forest, new ThemeColors {
                    Background1 = Color.FromArgb("#2D5A27"),  // Dark green
                    Background2 = Color.FromArgb("#1B4332"),  // Darker green
                    Primary = Color.FromArgb("#95D5B2"),      // Light green
                    Secondary = Color.FromArgb("#74C69D"),    // Medium green
                    CardBackground = Color.FromArgb("#2D6A4F"), // Forest green for cards
                    CardText = Colors.White,                  // White text
                    MenuButton1 = Color.FromArgb("#2D6A4F"),
                    MenuButton2 = Color.FromArgb("#40916C"),
                    MenuButton3 = Color.FromArgb("#52B788"),
                    MenuButton4 = Color.FromArgb("#74C69D"),
                    AnswerButton = Color.FromArgb("#81C784"),
                    ShellBackground = Color.FromArgb("#2D5A27"),
                    ShellForeground = Color.FromArgb("#95D5B2"),
                    BackgroundImage = "forest_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Space, new ThemeColors {
                    Background1 = Color.FromArgb("#0B0B1F"),  // Very dark blue
                    Background2 = Color.FromArgb("#1B1B3A"),  // Dark blue-purple
                    Primary = Color.FromArgb("#4CC9F0"),      // Bright cyan
                    Secondary = Color.FromArgb("#7209B7"),    // Deep purple
                    CardBackground = Color.FromArgb("#20205A"), // Deep space blue
                    CardText = Colors.White,
                    MenuButton1 = Color.FromArgb("#4CC9F0"),  // Bright cyan for Guess Mode
                    MenuButton2 = Color.FromArgb("#F72585"),  // Hot pink for Perform Mode
                    MenuButton3 = Color.FromArgb("#7209B7"),  // Deep purple for Profile
                    MenuButton4 = Color.FromArgb("#3A0CA3"),  // Royal blue for Settings
                    AnswerButton = Color.FromArgb("#4361EE"), // Electric blue
                    ShellBackground = Color.FromArgb("#0B0B1F"),
                    ShellForeground = Color.FromArgb("#4CC9F0"),
                    BackgroundImage = "space_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Ocean, new ThemeColors {
                    Background1 = Color.FromArgb("#1A5F7A"),  // Deep blue
                    Background2 = Color.FromArgb("#086E7D"),  // Teal
                    Primary = Color.FromArgb("#00FFE1"),      // Bright cyan
                    Secondary = Color.FromArgb("#98DFD6"),    // Light cyan
                    CardBackground = Color.FromArgb("#147B89"), // Ocean blue for cards
                    CardText = Colors.White,                  // White text
                    MenuButton1 = Color.FromArgb("#00B4D8"),
                    MenuButton2 = Color.FromArgb("#48CAE4"),
                    MenuButton3 = Color.FromArgb("#90E0EF"),
                    MenuButton4 = Color.FromArgb("#ADE8F4"),
                    AnswerButton = Color.FromArgb("#4DD0E1"),
                    ShellBackground = Color.FromArgb("#1A5F7A"),
                    ShellForeground = Color.FromArgb("#00FFE1"),
                    BackgroundImage = "ocean_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Neon, new ThemeColors {
                    Background1 = Color.FromArgb("#0C0032"),  // Very dark blue
                    Background2 = Color.FromArgb("#190061"),  // Dark purple
                    Primary = Color.FromArgb("#FF00FF"),      // Magenta
                    Secondary = Color.FromArgb("#00FF9F"),    // Bright green
                    CardBackground = Color.FromArgb("#240090"), // Deep purple for cards
                    CardText = Colors.White,                  // White text
                    MenuButton1 = Color.FromArgb("#FF00FF"),
                    MenuButton2 = Color.FromArgb("#00FF9F"),
                    MenuButton3 = Color.FromArgb("#9945FF"),
                    MenuButton4 = Color.FromArgb("#FF3366"),
                    AnswerButton = Color.FromArgb("#00E5FF"),
                    ShellBackground = Color.FromArgb("#0C0032"),
                    ShellForeground = Color.FromArgb("#FF00FF"),
                    BackgroundImage = "neon_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Monochrome, new ThemeColors {
                    Background1 = Color.FromArgb("#2C3E50"),  // Dark gray-blue
                    Background2 = Color.FromArgb("#34495E"),  // Slightly lighter gray-blue
                    Primary = Color.FromArgb("#2C3E50"),      // Very light gray
                    Secondary = Color.FromArgb("#34495E"),    // Light gray
                    CardBackground = Color.FromArgb("#465C70"), // Mid gray for cards
                    CardText = Colors.White,                  // White text
                  MenuButton1 = Color.FromArgb("#2C3E50"),
                    MenuButton2 = Color.FromArgb("#34495E"),
                    MenuButton3 = Color.FromArgb("#2C3E50"),
                    MenuButton4 = Color.FromArgb("#34495E"),
                    AnswerButton = Color.FromArgb("#2C3E50"),
                    ShellBackground = Color.FromArgb("#2C3E50"),
                    ShellForeground = Color.FromArgb("#ECF0F1"),
                    BackgroundImage = "monochrome_theme_bg.png"
                }
            }
        };

        private void CacheThemeResources(CustomAppTheme theme, ThemeColors colors)
        {
            if (!_themeCache.ContainsKey(theme))
            {
                _themeCache[theme] = new Dictionary<string, object>
                {
                    ["AppBackground1"] = colors.Background1,
                    ["AppBackground2"] = colors.Background2,
                    ["Primary"] = colors.Primary,
                    ["Secondary"] = colors.Secondary,
                    ["GuessMode"] = colors.MenuButton1,
                    ["PerformMode"] = colors.MenuButton2,
                    ["Profile"] = colors.MenuButton3,
                    ["Settings"] = colors.MenuButton4,
                    ["AnswerButton"] = colors.AnswerButton,
                    ["CardBackground"] = colors.CardBackground,
                    ["CardText"] = colors.CardText,
                    ["CurrentThemeBackground"] = $"Themes/{colors.BackgroundImage}"
                };
            }
        }

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
            Preferences.Set(THEME_KEY, theme.ToString());

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var resources = Application.Current?.Resources;
                if (resources == null) return;

                if (!ThemeDefinitions.TryGetValue(theme, out var themeColors))
                {
                    Debug.WriteLine($"Theme '{theme}' not found in ThemeDefinitions.");
                    return;
                }

                // Cache theme resources if not already cached
                CacheThemeResources(theme, themeColors);

                // Apply cached resources
                foreach (var resource in _themeCache[theme])
                {
                    resources[resource.Key] = resource.Value;
                }

                // Update Shell background
                if (Shell.Current != null)
                {
                    Shell.Current.BackgroundColor = themeColors.ShellBackground;
                }

                // Update UI
                if (Application.Current?.MainPage is Shell shell)
                {
                    shell.ForceLayout();
                    var mainPage = shell.Navigation?.NavigationStack
                        .FirstOrDefault(page => page is MainMenuPage) as MainMenuPage;
                    mainPage?.Dispatcher.Dispatch(mainPage.ForceRefresh);
                }

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

        /// <summary>
        /// Gets or sets the card background color.
        /// </summary>
        public Color CardBackground { get; set; } = Colors.White;

        /// <summary>
        /// Gets or sets the card text color.
        /// </summary>
        public Color CardText { get; set; } = Colors.Black;

        public required Color MenuButton1 { get; set; }  // Guess Mode button
        public required Color MenuButton2 { get; set; }  // Perform Mode button
        public required Color MenuButton3 { get; set; }  // Progress button
        public required Color MenuButton4 { get; set; }  // Settings button
        public required Color AnswerButton { get; set; } // Answer choice buttons
        public required Color ShellBackground { get; set; }
        public required Color ShellForeground { get; set; }  // For text/icons
        public required string BackgroundImage { get; set; }
    }
}
