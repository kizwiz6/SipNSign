using com.kizwiz.sipnsign.Models;
using System.Globalization;

namespace com.kizwiz.sipnsign.Converters
{
    /// <summary>
    /// Converts a Player object and boolean parameter into a PlayerAnswerParameter
    /// for use with the RecordPlayerAnswer command
    /// </summary>
    public class PlayerAnswerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Player player && parameter is bool isCorrect)
            {
                return new PlayerAnswerParameter { Player = player, IsCorrect = isCorrect };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
