using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace POS_v0_1.Converters
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Estandarización: Se eliminan restricciones de rol. Todos los elementos son visibles para usuarios logueados.
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
