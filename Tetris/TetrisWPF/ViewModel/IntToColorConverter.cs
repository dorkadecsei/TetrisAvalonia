using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TetrisWPF.ViewModel
{
    public class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int colorCode)
            {
                return colorCode switch
                {
                    0 => Brushes.WhiteSmoke,
                    1 => Brushes.DarkGreen,
                    2 => Brushes.Green,
                    3 => Brushes.DarkSeaGreen,
                    4 => Brushes.ForestGreen,
                    5 => Brushes.LimeGreen,
                    6 => Brushes.LightGreen,
                    7 => Brushes.SpringGreen,
                    _ => Brushes.LawnGreen
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}