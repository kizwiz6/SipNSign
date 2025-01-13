using com.kizwiz.sipnsign.Enums;

namespace com.kizwiz.sipnsign.Services
{
    /// <summary>
    /// Defines the contract for theme management
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the current theme colors
        /// </summary>
        ThemeColors CurrentTheme { get; }

        /// <summary>
        /// Sets the application theme
        /// </summary>
        /// <param name="theme">The theme to apply</param>
        void SetTheme(CustomAppTheme theme);

        /// <summary>
        /// Gets the current theme setting
        /// </summary>
        /// <returns>The current AppTheme</returns>
        CustomAppTheme GetCurrentTheme();

        event EventHandler ThemeChanged;
    }
}
