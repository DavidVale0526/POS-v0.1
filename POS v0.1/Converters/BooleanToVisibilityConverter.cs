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
            if (value == null || value == System.Windows.DependencyProperty.UnsetValue || value.ToString() == "{DisconnectedItem}")
                return parameter?.ToString() == "Inverse" ? Visibility.Visible : Visibility.Collapsed;

            bool boolValue = false;
            if (value is bool b) boolValue = b;
            else if (value is int i) boolValue = i > 0;
            else if (value is System.Collections.IEnumerable enumerable)
            {
                try
                {
                    var enumerator = enumerable.GetEnumerator();
                    boolValue = enumerator.MoveNext();
                }
                catch { boolValue = false; }
            }

            if (parameter?.ToString() == "Inverse") boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
