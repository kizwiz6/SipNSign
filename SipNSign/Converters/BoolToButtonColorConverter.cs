using System.Globalization;

namespace com.kizwiz.sipnsign.Converters
{
    /// <summary>
    /// Converts boolean values to button colors
    /// </summary>
    public class BoolToButtonColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (Application.Current?.Resources != null &&
                    Application.Current.Resources.TryGetValue("Primary", out var primaryColor) &&
                    primaryColor is Color primary)
                {
                    return boolValue ? primary : Colors.Gray;
                }
                return boolValue ? Colors.Blue : Colors.Gray;
            }
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
