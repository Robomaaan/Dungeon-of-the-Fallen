using System.Globalization;
using System.Windows.Data;

namespace Projektarbeit_Dungeon_of_the_Fallen.Converters
{
    public class VictoryTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVictory)
                return isVictory ? "🏆 SIEG! 🏆" : "💀 NIEDERLAGE! 💀";
            return "SPIEL VORBEI";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
