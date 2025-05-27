using System.Globalization;

namespace com.kizwiz.sipnsign.Converters
{
    /// <summary>
    /// Converts boolean values to colors for visual indicators
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolValue ? (boolValue ? Colors.Green : Colors.Gray) : null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToTextColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool boolValue ? (boolValue ? Colors.White : Colors.DarkGray) : null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
