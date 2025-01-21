using System.Globalization;

namespace com.kizwiz.sipnsign.Converters
{
    public class BoolToAchievementStatusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool isUnlocked && isUnlocked ? "🏆" : string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
