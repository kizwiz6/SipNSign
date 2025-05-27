using System.Globalization;

namespace com.kizwiz.sipnsign.Converters
{
    /// <summary>
    /// Converts boolean values to opacity for disabled state
    /// </summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : 0.5;
            }
            return 0.5;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
