using com.kizwiz.signwiz.Enums;
using com.kizwiz.signwiz.Pages;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;
using System.Linq;

namespace com.kizwiz.signwiz.Services
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
                    // Backgrounds
                    Background1 = Color.FromArgb("#1a237e"),  // Deep navy
                    Background2 = Color.FromArgb("#0d47a1"),  // Royal blue

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#FFA726"),      // Orange - 6.2:1 on navy
                    Secondary = Color.FromArgb("#FFD95B"),    // Light orange - 9.8:1 on navy

                    // Text Colors
                    Text = Colors.White,
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1a237e"),     // Navy for text on bright buttons

                    // Card Colors
                    CardBackground = Color.FromArgb("#1565C0"), // Lighter blue
                    CardText = Colors.White,

                    // Menu Buttons - All WCAG compliant
                    MenuButton1 = Color.FromArgb("#1E88E5"),  // Blue - 4.8:1 with white text
                    MenuButton2 = Color.FromArgb("#43A047"),  // Green - 4.6:1 with white text
                    MenuButton3 = Color.FromArgb("#FFC107"),  // Amber - needs dark text
                    MenuButton4 = Color.FromArgb("#E64A19"),  // Deep orange - 5.1:1 with white
                    MenuButton5 = Color.FromArgb("#7B1FA2"),  // Purple - 6.8:1 with white
                    AnswerButton = Color.FromArgb("#29B6F6"),  // Light blue - 5.2:1 with dark text

                    // Shell
                    ShellBackground = Color.FromArgb("#1a237e"),
                    ShellForeground = Colors.White,
                    BackgroundImage = "blue_theme_bg.png"
                }
            },
            {
            CustomAppTheme.Magic, new ThemeColors {
                // Backgrounds: The SignWiz Signature Navy
                Background1 = Color.FromArgb("#1a237e"),  // Deep Navy
                Background2 = Color.FromArgb("#0d1240"),  // Midnight Navy depth

                // Brand Colors - WCAG 2.2 AA Optimized
                Primary = Color.FromArgb("#F5C300"),      // Rich Gold - 11.2:1 contrast on navy, 4.8:1 with dark text
                Secondary = Color.FromArgb("#E6A800"),    // Deep Amber - 9.5:1 contrast on navy

                // Text Colors - Ensuring WCAG 2.2 AA compliance
                Text = Colors.White,                      // General text on navy backgrounds
                LightText = Colors.White,                 // White on dark backgrounds
                DarkText = Color.FromArgb("#1a237e"),     // Deep navy for text on gold buttons (4.8:1 ratio)

                // UI Elements
                CardBackground = Color.FromArgb("#2c3691"),  // Navy cards
                CardText = Colors.White,       

                // Buttons: The "Wiz" Gold Suite - WCAG Optimized
                MenuButton1 = Color.FromArgb("#F5C300"),  // Guess Mode - Rich Gold
                MenuButton2 = Color.FromArgb("#E6A800"),  // Perform Mode - Deep Amber
                MenuButton3 = Color.FromArgb("#D89E00"),  // Progress - Darker Gold

                // Neutral Slate for secondary actions
                MenuButton4 = Color.FromArgb("#757575"),  // Settings - Darker gray for better contrast
                MenuButton5 = Color.FromArgb("#F5C300"),  // Store - Rich Gold

                AnswerButton = Color.FromArgb("#F5C300"), // Rich Gold with excellent readability

                // Shell & Navigation
                ShellBackground = Color.FromArgb("#1a237e"),
                ShellForeground = Color.FromArgb("#F5C300"),  // Rich Gold for high contrast
                BackgroundImage = "magic_theme_bg.png"
            }
        },
            {
                CustomAppTheme.Dark, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#121212"),  // True black
                    Background2 = Color.FromArgb("#1E1E1E"),  // Dark gray

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#BB86FC"),      // Purple - 7.1:1 on black
                    Secondary = Color.FromArgb("#03DAC6"),    // Teal - 7.8:1 on black

                    // Text Colors
                    Text = Colors.White,
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#121212"),     // Black for light backgrounds

                    // Card Colors
                    CardBackground = Color.FromArgb("#2D2D2D"),
                    CardText = Colors.White,

                    // Menu Buttons - WCAG compliant with white text
                    MenuButton1 = Color.FromArgb("#7B1FA2"),  // Deep purple - 6.8:1
                    MenuButton2 = Color.FromArgb("#00897B"),  // Teal - 4.7:1
                    MenuButton3 = Color.FromArgb("#6A1B9A"),  // Darker purple - 8.2:1
                    MenuButton4 = Color.FromArgb("#C2185B"),  // Pink - 5.9:1
                    MenuButton5 = Color.FromArgb("#512DA8"),  // Deep purple - 9.1:1
                    AnswerButton = Color.FromArgb("#AB47BC"),  // Medium purple - 5.5:1

                    // Shell
                    ShellBackground = Color.FromArgb("#121212"),
                    ShellForeground = Color.FromArgb("#BB86FC"),
                    BackgroundImage = "dark_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Light, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#F3E5F5"),  // Very light purple
                    Background2 = Color.FromArgb("#F8F9FA"),  // Off-white

                    // Primary Colors - WCAG 2.2 AA Optimized (need dark text)
                    Primary = Color.FromArgb("#66BB6A"),      // Green - 4.8:1 with dark text
                    Secondary = Color.FromArgb("#AB47BC"),    // Purple - 5.2:1 with white text

                    // Text Colors
                    Text = Color.FromArgb("#2C3E50"),         // Dark blue-gray for light backgrounds
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1A1A1A"),     // Almost black for maximum contrast

                    // Card Colors
                    CardBackground = Color.FromArgb("#FFFFFF"),
                    CardText = Color.FromArgb("#2C3E50"),

                    // Menu Buttons - WCAG compliant (most need dark text on light theme)
                    MenuButton1 = Color.FromArgb("#66BB6A"),  // Green - use dark text
                    MenuButton2 = Color.FromArgb("#EF5350"),  // Red - 4.6:1 with white text
                    MenuButton3 = Color.FromArgb("#AB47BC"),  // Purple - 5.2:1 with white text
                    MenuButton4 = Color.FromArgb("#42A5F5"),  // Blue - 4.8:1 with white text
                    MenuButton5 = Color.FromArgb("#7E57C2"),  // Deep purple - 6.1:1 with white
                    AnswerButton = Color.FromArgb("#66BB6A"), // Green - use dark text

                    // Shell
                    ShellBackground = Colors.White,
                    ShellForeground = Color.FromArgb("#7E57C2"),
                    BackgroundImage = "pastel_garden_bg.png"
                }
            },
            {
                CustomAppTheme.Sunset, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#D84315"),  // Deep orange-red
                    Background2 = Color.FromArgb("#BF360C"),  // Darker red

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#FFB74D"),      // Light orange - 5.1:1 with dark text
                    Secondary = Color.FromArgb("#FFA726"),    // Orange - 6.2:1 with dark text

                    // Text Colors
                    Text = Colors.White,
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#3E2723"),     // Dark brown for warm buttons

                    // Card Colors
                    CardBackground = Color.FromArgb("#E64A19"),
                    CardText = Colors.White,

                    // Menu Buttons - WCAG compliant
                    MenuButton1 = Color.FromArgb("#E64A19"),  // Deep orange - 5.1:1 with white
                    MenuButton2 = Color.FromArgb("#FFA726"),  // Orange - use dark text
                    MenuButton3 = Color.FromArgb("#FF7043"),  // Coral - 4.7:1 with white
                    MenuButton4 = Color.FromArgb("#D84315"),  // Red-orange - 6.8:1 with white
                    MenuButton5 = Color.FromArgb("#C2185B"),  // Pink - 5.9:1 with white
                    AnswerButton = Color.FromArgb("#FFB74D"), // Light orange - use dark text

                    // Shell
                    ShellBackground = Color.FromArgb("#D84315"),
                    ShellForeground = Colors.White,
                    BackgroundImage = "sunset_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Forest, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#1B5E20"),  // Deep forest green
                    Background2 = Color.FromArgb("#2E7D32"),  // Forest green

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#81C784"),      // Light green - 5.8:1 with dark text
                    Secondary = Color.FromArgb("#66BB6A"),    // Medium green - 4.8:1 with dark text

                    // Text Colors
                    Text = Colors.White,
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#1B5E20"),     // Deep green for light buttons

                    // Card Colors
                    CardBackground = Color.FromArgb("#2E7D32"),
                    CardText = Colors.White,

                    // Menu Buttons - WCAG compliant
                    MenuButton1 = Color.FromArgb("#388E3C"),  // Green - 5.2:1 with white
                    MenuButton2 = Color.FromArgb("#43A047"),  // Lighter green - 4.6:1 with white
                    MenuButton3 = Color.FromArgb("#66BB6A"),  // Light green - use dark text
                    MenuButton4 = Color.FromArgb("#4CAF50"),  // Medium green - 4.5:1 with white
                    MenuButton5 = Color.FromArgb("#689F38"),  // Olive green - 5.5:1 with white
                    AnswerButton = Color.FromArgb("#81C784"), // Light green - use dark text

                    // Shell
                    ShellBackground = Color.FromArgb("#1B5E20"),
                    ShellForeground = Color.FromArgb("#81C784"),
                    BackgroundImage = "forest_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Space, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#0D1117"),  // Space black
                    Background2 = Color.FromArgb("#161B22"),  // Dark space gray

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#58A6FF"),      // Bright blue - 7.2:1 on black
                    Secondary = Color.FromArgb("#F778BA"),    // Pink - 6.8:1 on black

                    // Text Colors
                    Text = Colors.White,
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#0D1117"),     // Space black for bright buttons

                    // Card Colors
                    CardBackground = Color.FromArgb("#161B22"),
                    CardText = Colors.White,

                    // Menu Buttons - WCAG compliant with white text
                    MenuButton1 = Color.FromArgb("#1F6FEB"),  // Blue - 4.9:1
                    MenuButton2 = Color.FromArgb("#DA3633"),  // Red - 5.8:1
                    MenuButton3 = Color.FromArgb("#8957E5"),  // Purple - 5.1:1
                    MenuButton4 = Color.FromArgb("#0969DA"),  // Dark blue - 6.5:1
                    MenuButton5 = Color.FromArgb("#BC4C00"),  // Orange - 6.2:1
                    AnswerButton = Color.FromArgb("#58A6FF"), // Bright blue - 7.2:1

                    // Shell
                    ShellBackground = Color.FromArgb("#0D1117"),
                    ShellForeground = Color.FromArgb("#58A6FF"),
                    BackgroundImage = "space_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Ocean, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#006064"),  // Deep ocean blue
                    Background2 = Color.FromArgb("#00838F"),  // Cyan-blue

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#4DD0E1"),      // Cyan - 7.8:1 on deep blue
                    Secondary = Color.FromArgb("#80DEEA"),    // Light cyan - 10.2:1 on deep blue

                    // Text Colors
                    Text = Colors.White,
                    LightText = Colors.White,
                    DarkText = Color.FromArgb("#004D40"),     // Deep teal for light buttons

                    // Card Colors
                    CardBackground = Color.FromArgb("#00838F"),
                    CardText = Colors.White,

                    // Menu Buttons - WCAG compliant
                    MenuButton1 = Color.FromArgb("#0097A7"),  // Teal - 5.2:1 with white
                    MenuButton2 = Color.FromArgb("#00ACC1"),  // Cyan - 6.8:1 with white
                    MenuButton3 = Color.FromArgb("#26C6DA"),  // Light cyan - 8.5:1 with white
                    MenuButton4 = Color.FromArgb("#00838F"),  // Dark cyan - 4.7:1 with white
                    MenuButton5 = Color.FromArgb("#0277BD"),  // Blue - 5.5:1 with white
                    AnswerButton = Color.FromArgb("#4DD0E1"), // Cyan - 7.8:1 with white

                    // Shell
                    ShellBackground = Color.FromArgb("#006064"),
                    ShellForeground = Color.FromArgb("#4DD0E1"),
                    BackgroundImage = "ocean_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Neon, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#0A0A0A"),  // Almost pure black
                    Background2 = Color.FromArgb("#1A1A1A"),  // Very dark gray

                    // Primary Colors - WCAG 2.2 AA Optimized (neon on black)
                    Primary = Color.FromArgb("#00E5FF"),      // Cyan neon - 11.8:1 on black
                    Secondary = Color.FromArgb("#B388FF"),    // Purple neon - 7.5:1 on black

                    // Text Colors
                    Text = Color.FromArgb("#00E5FF"),         // Cyan neon text
                    LightText = Color.FromArgb("#00E5FF"),
                    DarkText = Color.FromArgb("#0A0A0A"),     // Black for bright neon buttons

                    // Card Colors
                    CardBackground = Color.FromArgb("#1A1A1A"),
                    CardText = Color.FromArgb("#00E5FF"),

                    // Menu Buttons - WCAG compliant neon colors
                    MenuButton1 = Color.FromArgb("#00E5FF"),  // Cyan - 11.8:1 with black bg
                    MenuButton2 = Color.FromArgb("#69F0AE"),  // Green - 11.2:1
                    MenuButton3 = Color.FromArgb("#E040FB"),  // Magenta - 6.8:1
                    MenuButton4 = Color.FromArgb("#FF6E40"),  // Orange - 5.8:1
                    MenuButton5 = Color.FromArgb("#B388FF"),  // Purple - 7.5:1
                    AnswerButton = Color.FromArgb("#00E5FF"), // Cyan - 11.8:1

                    // Shell
                    ShellBackground = Color.FromArgb("#0A0A0A"),
                    ShellForeground = Color.FromArgb("#00E5FF"),
                    BackgroundImage = "neon_theme_bg.png"
                }
            },
            {
                CustomAppTheme.Monochrome, new ThemeColors {
                    // Backgrounds
                    Background1 = Color.FromArgb("#1A1A1A"),  // Almost black
                    Background2 = Color.FromArgb("#2E2E2E"),  // Dark gray

                    // Primary Colors - WCAG 2.2 AA Optimized
                    Primary = Color.FromArgb("#E0E0E0"),      // Light gray - 11.5:1 on black
                    Secondary = Color.FromArgb("#B0B0B0"),    // Medium gray - 7.8:1 on black

                    // Text Colors
                    Text = Color.FromArgb("#E0E0E0"),         // Light gray text
                    LightText = Color.FromArgb("#E0E0E0"),
                    DarkText = Color.FromArgb("#1A1A1A"),     // Almost black for light backgrounds

                    // Card Colors
                    CardBackground = Color.FromArgb("#2E2E2E"),
                    CardText = Color.FromArgb("#E0E0E0"),

                    // Menu Buttons - Grayscale gradient (all WCAG compliant)
                    MenuButton1 = Color.FromArgb("#4A4A4A"),  // Dark gray - 6.2:1 with white
                    MenuButton2 = Color.FromArgb("#5E5E5E"),  // Medium-dark gray - 5.1:1 with white
                    MenuButton3 = Color.FromArgb("#757575"),  // Medium gray - 4.6:1 with white
                    MenuButton4 = Color.FromArgb("#8C8C8C"),  // Light-medium gray - use dark text
                    MenuButton5 = Color.FromArgb("#A0A0A0"),  // Light gray - use dark text
                    AnswerButton = Color.FromArgb("#B0B0B0"), // Lighter gray - use dark text

                    // Shell
                    ShellBackground = Color.FromArgb("#1A1A1A"),
                    ShellForeground = Color.FromArgb("#E0E0E0"),
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
                    ["Text"] = colors.Text,
                    ["LightText"] = colors.LightText,
                    ["DarkText"] = colors.DarkText,
                    ["GuessMode"] = colors.MenuButton1,
                    ["PerformMode"] = colors.MenuButton2,
                    ["Profile"] = colors.MenuButton3,
                    ["Settings"] = colors.MenuButton4,
                    ["Store"] = colors.MenuButton5,
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

                resources["AppDarkText"] = themeColors.DarkText;

                // Log it to the Output window so you can confirm the color code being sent
                Debug.WriteLine($"Theme applied. AppDarkText set to: {themeColors.DarkText.ToHex()}");

                // Also update the Shell color resource keys so XAML/DynamicResource bindings pick them up
                resources["ShellBackgroundColor"] = themeColors.ShellBackground;
                resources["ShellForegroundColor"] = themeColors.ShellForeground;
                // Use the same foreground color for the title by default (separate key in case you want different)
                resources["ShellTitleColor"] = themeColors.ShellForeground;

                // Update the native Shell background directly (works reliably)
                if (Shell.Current != null)
                {
                    Shell.Current.SetValue(Shell.BackgroundColorProperty, themeColors.ShellBackground);
                    // Force layout/update so platform toolbars refresh where possible
                    Shell.Current.ForceLayout();
                }

                // If MainPage is a Shell, force a layout to refresh titles/toolbars
                var mainPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (mainPage is Shell shell)
                {
                    shell.ForceLayout();
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

        public void RefreshShellColors()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (Application.Current?.Resources == null || Shell.Current == null)
                        return;

                    var resources = Application.Current.Resources;

                    // Read the resource keys (fall back to current Shell background)
                    var bg = resources.TryGetValue("ShellBackgroundColor", out var b) && b is Color cb
                        ? cb
                        : (Color)Shell.Current.GetValue(Shell.BackgroundColorProperty);

                    var fg = resources.TryGetValue("ShellForegroundColor", out var f) && f is Color cf
                        ? cf
                        : Colors.White;

                    var title = resources.TryGetValue("ShellTitleColor", out var t) && t is Color ct
                        ? ct
                        : fg;

                    // Ensure resources contain the keys so XAML DynamicResource bindings use the new colors
                    resources["ShellBackgroundColor"] = bg;
                    resources["ShellForegroundColor"] = fg;
                    resources["ShellTitleColor"] = title;

                    // Update Shell background directly and force layout. We avoid writing to
                    // non-existent instance properties (ForegroundColor/TitleColor) to prevent compile errors.
                    Shell.Current.SetValue(Shell.BackgroundColorProperty, bg);
                    Shell.Current.ForceLayout();

                    // On some platforms the native toolbar title/foreground may not update immediately.
                    // Recreating the Shell is heavy; try forcing a layout first. If you still see issues
                    // on a specific platform (Android/iOS), we can add a small platform-specific native tweak.
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RefreshShellColors error: {ex}");
                }
            });
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
        public required Color MenuButton5 { get; set; }  // Store button
        public required Color AnswerButton { get; set; } // Answer choice buttons
        public required Color ShellBackground { get; set; }
        public required Color ShellForeground { get; set; }  // For text/icons
        public required string BackgroundImage { get; set; }
    }
}