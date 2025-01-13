using System.Globalization;

namespace com.kizwiz.sipnsign.Converters
{
    public class ButtonEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] is not bool isProcessing || values[1] is not bool isGameActive)
                return true;

            return !isProcessing && isGameActive;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
