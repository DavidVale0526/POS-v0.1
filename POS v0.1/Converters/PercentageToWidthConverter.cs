using System;
using System.Globalization;
using System.Windows.Data;

namespace POS_v0_1.Converters
{
    public class PercentageToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0.0;

            try
            {
                decimal value = System.Convert.ToDecimal(values[0]);
                decimal max = System.Convert.ToDecimal(values[1]);
                double totalWidth = 100.0; // Base width in pixels or percentage reference

                if (parameter != null) double.TryParse(parameter.ToString(), out totalWidth);

                if (max <= 0) return 0.0;

                double percentage = (double)(value / max);
                return percentage * totalWidth;
            }
            catch
            {
                return 0.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
