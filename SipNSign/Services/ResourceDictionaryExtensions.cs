using com.kizwiz.sipnsign.Enums;

namespace com.kizwiz.sipnsign.Services
{
    public static class ResourceDictionaryExtensions
    {
        public static void SetThemeResources(this ResourceDictionary resources, CustomAppTheme theme, ThemeColors colors)
        {
            resources["AppBackground1"] = colors.Background1;
            resources["AppBackground2"] = colors.Background2;
            resources["Primary"] = colors.Primary;
            resources["Secondary"] = colors.Secondary;
            resources["GuessMode"] = colors.MenuButton1;
            resources["PerformMode"] = colors.MenuButton2;
            resources["Progress"] = colors.MenuButton3;
            resources["Settings"] = colors.MenuButton4;
            resources["ShellBackgroundColor"] = colors.ShellBackground;
            resources["ShellForegroundColor"] = colors.ShellForeground;
            resources["ShellTitleColor"] = colors.ShellForeground;
            resources["ShellDisabledColor"] = colors.ShellForeground.WithAlpha(0.5f);
            resources["ShellUnselectedColor"] = colors.ShellForeground.WithAlpha(0.7f);
            resources["TextColor"] = theme == CustomAppTheme.Light ? colors.DarkText : colors.LightText;
        }
    }
}
