using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DemoWpf
{
    public class GreaterThanZeroConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int discount)
            {
                return discount > 0 ? Brushes.LightGreen : Brushes.LightCoral;
            }
            return Brushes.Transparent; // Или любой другой цвет по умолчанию
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
