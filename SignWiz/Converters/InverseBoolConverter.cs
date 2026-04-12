using System.Globalization;

namespace com.kizwiz.signwiz.Converters
{
    /// <summary>
    /// Converts boolean values to their inverse for UI binding
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : null;
        }
    }
}
