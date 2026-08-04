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

            decimal diff = 0;
            try { diff = System.Convert.ToDecimal(value); } catch { return Brushes.Black; }

            if (diff == 0) return Brushes.Black;
            if (diff > 0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#198754")); // Green (Entrada/Sobrante)
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc3545")); // Red (Salida/Faltante)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
