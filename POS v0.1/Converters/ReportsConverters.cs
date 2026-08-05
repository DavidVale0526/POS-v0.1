using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using POS_v0_1.Models;

namespace POS_v0_1.Converters
{
    public class ChartScaleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return 0.0;
            if (!(values[0] is decimal value)) return 0.0;
            if (!(values[1] is IEnumerable<DailySalesSummary> history)) return 0.0;

            double maxHeight = 150;
            if (values[2] != null) double.TryParse(values[2].ToString(), out maxHeight);

            var maxVal = (history != null && history.Any()) ? history.Max(x => x.Total) : 0;
            if (maxVal == 0) return 0.0;

            double ratio = (double)(value / maxVal);
            return Math.Max(ratio * maxHeight, 2.0); // Al menos 2px si hay algo
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CategoryPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0.0;
            if (!(values[0] is decimal value)) return 0.0;
            if (!(values[1] is decimal total)) return 0.0;

            double maxWidth = 300;
            if (values.Length > 2 && values[2] != null) double.TryParse(values[2].ToString(), out maxWidth);

            if (total == 0) return 0.0;

            double ratio = (double)(value / total);
            return Math.Max(ratio * maxWidth, 5.0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
