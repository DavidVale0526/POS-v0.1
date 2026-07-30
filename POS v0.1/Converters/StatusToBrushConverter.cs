using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace POS_v0_1.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOpen && isOpen)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#198754")); // Green
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c757d")); // Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
