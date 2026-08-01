using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace POS_v0_1.Converters
{
    public class DifferenceToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == System.Windows.DependencyProperty.UnsetValue)
                return Brushes.Black;

            if (value is decimal diff)
            {
                if (diff == 0) return Brushes.Black;
                if (diff > 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#198754")); // Green (Sobrante)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc3545")); // Red (Faltante)
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
