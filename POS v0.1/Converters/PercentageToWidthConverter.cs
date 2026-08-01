using System;
using System.Globalization;
using System.Windows.Data;

namespace POS_v0_1.Converters
{
    public class PercentageToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return 0.0;

            try
            {
                // Safe extraction of decimal values
                decimal value = GetDecimal(values[0]);
                decimal max = GetDecimal(values[1]);

                double totalWidth = 100.0;
                if (parameter != null) double.TryParse(parameter.ToString(), out totalWidth);

                if (max <= 0) return 0.0;

                double percentage = (double)(value / max);
                double result = percentage * totalWidth;

                // Safety checks for layout
                if (double.IsNaN(result) || double.IsInfinity(result)) return 0.0;

                return Math.Max(0, Math.Min(totalWidth, result));
            }
            catch
            {
                return 0.0;
            }
        }

        private decimal GetDecimal(object value)
        {
            if (value == null ||
                value == System.Windows.DependencyProperty.UnsetValue ||
                value.GetType().FullName == "MS.Internal.NamedObject")
                return 0m;

            if (value is decimal d) return d;
            if (value is double db) return (decimal)db;
            if (value is int i) return (decimal)i;
            if (value is float f) return (decimal)f;

            try
            {
                if (value is IConvertible)
                    return System.Convert.ToDecimal(value);
            }
            catch
            {
                // Fallback
            }
            return 0m;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
