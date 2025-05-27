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
                // If HasAnswered is true, make it semi-transparent (0.5)
                // If HasAnswered is false, make it fully opaque (1.0)
                return boolValue ? 0.5 : 1.0;
            }
            return 1.0; // Default to fully opaque
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
