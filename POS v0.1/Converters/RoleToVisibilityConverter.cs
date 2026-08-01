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
            string userRole = value as string;
            string requiredRole = parameter as string;

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(requiredRole))
                return Visibility.Collapsed;

            // Si el requerimiento es "Administrador", solo el Admin lo ve.
            // Si el requerimiento es "Cualquiera", todos lo ven.
            if (requiredRole == "Administrador")
            {
                return userRole == "Administrador" ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
