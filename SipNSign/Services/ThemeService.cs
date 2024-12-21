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
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Dark, new ThemeColors {
                    Background1 = Color.FromArgb("#121212"),
                    Background2 = Color.FromArgb("#1E1E1E"),
                    Primary = Color.FromArgb("#BB86FC"),
                    Secondary = Color.FromArgb("#03DAC6"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Light, new ThemeColors {
                    Background1 = Colors.White,
                    Background2 = Color.FromArgb("#F5F5F5"),
                    Primary = Color.FromArgb("#1976D2"),
                    Secondary = Color.FromArgb("#2196F3"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Sunset, new ThemeColors {
                    Background1 = Color.FromArgb("#FF512F"),
                    Background2 = Color.FromArgb("#DD2476"),
                    Primary = Color.FromArgb("#FFA07A"),
                    Secondary = Color.FromArgb("#FFD700"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Forest, new ThemeColors {
                    Background1 = Color.FromArgb("#2D5A27"),
                    Background2 = Color.FromArgb("#1B4332"),
                    Primary = Color.FromArgb("#95D5B2"),
                    Secondary = Color.FromArgb("#74C69D"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Ocean, new ThemeColors {
                    Background1 = Color.FromArgb("#1A5F7A"),
                    Background2 = Color.FromArgb("#086E7D"),
                    Primary = Color.FromArgb("#00FFE1"),
                    Secondary = Color.FromArgb("#98DFD6"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Neon, new ThemeColors {
                    Background1 = Color.FromArgb("#0C0032"),
                    Background2 = Color.FromArgb("#190061"),
                    Primary = Color.FromArgb("#FF00FF"),
                    Secondary = Color.FromArgb("#00FF9F"),
                    LightText = Color.FromArgb("#00FF9F"),
                    DarkText = Color.FromArgb("#1E1E1E")
                }
            },
            {
                CustomAppTheme.Monochrome, new ThemeColors {
                    Background1 = Color.FromArgb("#2C3E50"),
                    Background2 = Color.FromArgb("#34495E"),
                    Primary = Color.FromArgb("#ECF0F1"),
                    Secondary = Color.FromArgb("#BDC3C7"),
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1E1E1E")
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

            // Retrieve theme color definitions
            var colors = ThemeDefinitions[theme];

            // Safely check and update application resources
            if (Application.Current?.Resources != null)
            {
                var resources = Application.Current.Resources;

                // Update resources with the new theme colors
                resources["AppBackground1"] = colors.Background1;
                resources["AppBackground2"] = colors.Background2;
                resources["Primary"] = colors.Primary;
                resources["Secondary"] = colors.Secondary;
                resources["TextColor"] = theme == CustomAppTheme.Light ? colors.DarkText : colors.LightText;

                // Update mode-specific colors to match theme
                Application.Current.Resources["GuessMode"] = colors.Primary;
                Application.Current.Resources["PerformMode"] = colors.Secondary;

                // Mode-specific colors that adapt to theme
                if (theme == CustomAppTheme.Light)
                {
                    resources["GuessMode"] = Color.FromArgb("#0056b3");  // Darker blue
                    resources["PerformMode"] = Color.FromArgb("#218838");  // Darker green
                    resources["Progress"] = Color.FromArgb("#d39e00");   // Darker yellow
                    resources["Settings"] = Color.FromArgb("#c34113");   // Darker orange
                }
                else
                {
                    resources["GuessMode"] = Color.FromArgb("#007BFF");  // Brighter blue
                    resources["PerformMode"] = Color.FromArgb("#28a745");  // Brighter green
                    resources["Progress"] = Color.FromArgb("#FFC107");   // Brighter yellow
                    resources["Settings"] = Color.FromArgb("#FF5722");   // Brighter orange
                }
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
    }
}
