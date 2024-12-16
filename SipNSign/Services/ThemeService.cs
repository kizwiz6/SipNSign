using com.kizwiz.sipnsign.Enums;
using System;
using System.Collections.Generic;
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
                    Primary = Color.FromArgb("#FFA726"),     // Orange complement
                    Secondary = Color.FromArgb("#FFD95B"),   // Light orange
                    Text = Colors.White
                }
            },
            {
                CustomAppTheme.Dark, new ThemeColors {
                    Background1 = Color.FromArgb("#121212"),
                    Background2 = Color.FromArgb("#1E1E1E"),
                    Primary = Color.FromArgb("#BB86FC"),     // Purple accent
                    Secondary = Color.FromArgb("#03DAC6"),   // Teal accent
                    Text = Colors.White
                }
            },
            {
                CustomAppTheme.Light, new ThemeColors {
                    Background1 = Colors.White,
                    Background2 = Color.FromArgb("#F5F5F5"),
                    Primary = Color.FromArgb("#1976D2"),     // Material Blue
                    Secondary = Color.FromArgb("#2196F3"),   // Lighter blue
                    Text = Colors.Black
                }
            }
        };

        /// <summary>
        /// Gets current theme colors based on saved preferences
        /// </summary>
        public ThemeColors CurrentTheme => ThemeDefinitions[GetCurrentTheme()];

        /// <summary>
        /// Changes the application theme and updates all resources
        /// </summary>
        public void SetTheme(CustomAppTheme theme)
        {
            Preferences.Set(THEME_KEY, theme.ToString());
            var colors = ThemeDefinitions[theme];

            var mergedDict = Application.Current.Resources.MergedDictionaries;
            if (mergedDict != null)
            {
                Application.Current.Resources["AppBackground1"] = colors.Background1;
                Application.Current.Resources["AppBackground2"] = colors.Background2;
                Application.Current.Resources["Primary"] = colors.Primary;
                Application.Current.Resources["Secondary"] = colors.Secondary;
                Application.Current.Resources["TextColor"] = colors.Text;

                // Update mode-specific colors to match theme
                Application.Current.Resources["GuessMode"] = colors.Primary;
                Application.Current.Resources["PerformMode"] = colors.Secondary;
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
    /// Represents a complete set of colors for a theme
    /// </summary>
    public class ThemeColors
    {
        public Color Background1 { get; set; }
        public Color Background2 { get; set; }
        public Color Primary { get; set; }
        public Color Secondary { get; set; }
        public Color Text { get; set; }
    }
}
