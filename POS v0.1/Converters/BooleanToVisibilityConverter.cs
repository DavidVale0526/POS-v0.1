using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace POS_v0_1.Converters
{
    public class VisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            if (value is bool b) boolValue = b;
            else if (value is int i) boolValue = i > 0;

            if (parameter?.ToString() == "Inverse") boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
