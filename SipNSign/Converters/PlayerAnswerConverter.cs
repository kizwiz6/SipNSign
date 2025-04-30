using com.kizwiz.sipnsign.Models;
using System.Diagnostics;
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
            Debug.WriteLine($"PlayerAnswerConverter.Convert called with value={value?.GetType().Name ?? "null"}, parameter={parameter ?? "null"}");

            if (value is Player player)
            {
                bool isCorrect = false;

                // Handle string parameter from XAML
                if (parameter is string strParam)
                {
                    if (bool.TryParse(strParam, out bool boolResult))
                    {
                        isCorrect = boolResult;
                    }
                    else if (strParam.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrect = true;
                    }
                }
                else if (parameter is bool boolParam)
                {
                    isCorrect = boolParam;
                }

                var playerAnswer = new PlayerAnswerParameter { Player = player, IsCorrect = isCorrect };
                Debug.WriteLine($"Created PlayerAnswerParameter: Player={player.Name}, IsCorrect={isCorrect}");
                return playerAnswer;
            }

            Debug.WriteLine($"Value is not a Player. Actual type: {value?.GetType().Name ?? "null"}");
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}