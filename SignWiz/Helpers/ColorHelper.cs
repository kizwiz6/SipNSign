using Microsoft.Maui.Graphics;

namespace com.kizwiz.signwiz.Helpers
{
    /// <summary>
    /// Helper class for color calculations and accessibility
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// Calculates the relative luminance of a color (WCAG formula)
        /// </summary>
        /// <param name="color">The color to analyze</param>
        /// <returns>Luminance value between 0 (black) and 1 (white)</returns>
        public static double GetLuminance(Color color)
        {
            // Convert to RGB values
            var r = (double)color.Red;
            var g = (double)color.Green;
            var b = (double)color.Blue;

            // Apply gamma correction (WCAG formula)
            r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

            // Calculate luminance
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        /// <summary>
        /// Determines if a color is considered "light" (needs dark text) or "dark" (needs light text)
        /// </summary>
        /// <param name="color">The background color</param>
        /// <returns>True if the color is light, false if dark</returns>
        public static bool IsLightColor(Color color)
        {
            // Threshold: 0.5 means mid-gray
            // Colors brighter than mid-gray are "light" and need dark text
            return GetLuminance(color) > 0.5;
        }

        /// <summary>
        /// Gets the optimal text color (white or dark) for a given background color
        /// Uses WCAG luminance calculations for maximum accessibility
        /// </summary>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="lightTextColor">The light text color to use (default: white)</param>
        /// <param name="darkTextColor">The dark text color to use (default: from resources)</param>
        /// <returns>The optimal text color for accessibility</returns>
        public static Color GetOptimalTextColor(Color backgroundColor, Color? lightTextColor = null, Color? darkTextColor = null)
        {
            Color finalLightColor = lightTextColor ?? Colors.White;
            Color finalDarkColor = darkTextColor ?? Color.FromArgb("#1A1A1A");

            // Try to get DarkText from resources if not provided
            if (darkTextColor == null && Application.Current?.Resources != null)
            {
                if (Application.Current.Resources.TryGetValue("DarkText", out var darkTextValue) &&
                    darkTextValue is Color resourceDarkColor)
                {
                    finalDarkColor = resourceDarkColor;
                }
            }

            return IsLightColor(backgroundColor) ? finalDarkColor : finalLightColor;
        }

        /// <summary>
        /// Calculates the contrast ratio between two colors (WCAG formula)
        /// </summary>
        /// <param name="color1">First color</param>
        /// <param name="color2">Second color</param>
        /// <returns>Contrast ratio (1:1 to 21:1)</returns>
        public static double GetContrastRatio(Color color1, Color color2)
        {
            var lum1 = GetLuminance(color1);
            var lum2 = GetLuminance(color2);

            var lighter = Math.Max(lum1, lum2);
            var darker = Math.Min(lum1, lum2);

            return (lighter + 0.05) / (darker + 0.05);
        }

        /// <summary>
        /// Creates a lighter version of a color (for gradients/highlights)
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="amount">Amount to lighten (0-1)</param>
        /// <returns>Lightened color</returns>
        public static Color Lighten(Color color, float amount = 0.2f)
        {
            return Color.FromRgba(
                Math.Min(1f, color.Red + amount),
                Math.Min(1f, color.Green + amount),
                Math.Min(1f, color.Blue + amount),
                color.Alpha
            );
        }

        /// <summary>
        /// Creates a darker version of a color (for shadows/depth)
        /// </summary>
        /// <param name="color">Base color</param>
        /// <param name="amount">Amount to darken (0-1)</param>
        /// <returns>Darkened color</returns>
        public static Color Darken(Color color, float amount = 0.2f)
        {
            return Color.FromRgba(
                Math.Max(0f, color.Red - amount),
                Math.Max(0f, color.Green - amount),
                Math.Max(0f, color.Blue - amount),
                color.Alpha
            );
        }
    }
}
