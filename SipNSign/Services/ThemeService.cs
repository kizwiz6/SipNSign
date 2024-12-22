using com.kizwiz.sipnsign.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    AnswerButton = Color.FromArgb("#007BFF"), // Blue
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#1a237e"),
                    ShellForeground = Colors.White
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
                    AnswerButton = Color.FromArgb("#BB86FC"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#121212"),
                    ShellForeground = Color.FromArgb("#BB86FC")
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
                    AnswerButton = Color.FromArgb("#0056b3"),   // Darker blue
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Colors.White,
                    ShellForeground = Color.FromArgb("#1976D2")
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
                    AnswerButton = Color.FromArgb("#FF6B6B"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#FF512F"),
                    ShellForeground = Colors.White
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
                    AnswerButton = Color.FromArgb("#2D6A4F"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#2D5A27"),  // Dark Forest Green
                    ShellForeground = Color.FromArgb("#95D5B2")   // Light Forest Green
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
                    AnswerButton = Color.FromArgb("#00B4D8"),   // Brighter blue
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#1A5F7A"),
                    ShellForeground = Color.FromArgb("#00FFE1")
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
                    AnswerButton = Color.FromArgb("#FF00FF"), // Magenta
                    LightText = Color.FromArgb("#00FF9F"),
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#0C0032"),  // Deep Purple
                    ShellForeground = Color.FromArgb("#FF00FF")   // Neon Pink
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
                    AnswerButton = Color.FromArgb("#95A5A6"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E"),
                    ShellBackground = Color.FromArgb("#2C3E50"),  // Dark Gray
                    ShellForeground = Color.FromArgb("#ECF0F1")   // Light Gray
                }
            }
        };

        /// <summary>
        /// Gets current theme colors based on saved preferences
        /// </summary>
        public ThemeColors CurrentTheme => ThemeDefinitions[GetCurrentTheme()];

        /// <summary>
        /// Changes the application theme and updates all resources.
        /// </summary>
        /// <param name="theme">The theme to set for the application.</param>
        public void SetTheme(CustomAppTheme theme)
        {
            // Save the selected theme to preferences
            Preferences.Set(THEME_KEY, theme.ToString());

            if (Application.Current?.Resources != null)
            {
                var resources = Application.Current.Resources;
                var themeColors = ThemeDefinitions[theme];

                // Update resources with the new theme colors
                resources["AppBackground1"] = themeColors.Background1;
                resources["AppBackground2"] = themeColors.Background2;
                resources["Primary"] = themeColors.Primary;
                resources["Secondary"] = themeColors.Secondary;
                resources["TextColor"] = theme == CustomAppTheme.Light ?
                    themeColors.DarkText : themeColors.LightText;

                // Menu buttons
                resources["GuessMode"] = themeColors.MenuButton1;
                resources["PerformMode"] = themeColors.MenuButton2;
                resources["Progress"] = themeColors.MenuButton3;
                resources["Settings"] = themeColors.MenuButton4;

                // Answer button
                resources["AnswerButton"] = themeColors.AnswerButton;

                // Shell colors
                resources["ShellBackgroundColor"] = themeColors.ShellBackground;
                resources["ShellForegroundColor"] = themeColors.ShellForeground;
                resources["ShellTitleColor"] = themeColors.ShellForeground;
                resources["ShellDisabledColor"] = themeColors.ShellForeground.WithAlpha(0.5f);
                resources["ShellUnselectedColor"] = themeColors.ShellForeground.WithAlpha(0.7f);

                // Theme background
                var backgroundKey = $"{theme}ThemeBackground";
                if (resources.TryGetValue(backgroundKey, out var themeBackground))
                {
                    resources["CurrentThemeBackground"] = themeBackground;
                }

                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
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
    }
}
