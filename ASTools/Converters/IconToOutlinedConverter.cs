using ASTools.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ASTools.Converters
{
    public class IconToOutlinedMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is Geometry baseIcon && values[1] is string iconKey)
            {
                string outlinedKey = $"{iconKey}Outlined";

                var outlinedGeometry = Application.Current.FindResource(outlinedKey) as Geometry;
                return outlinedGeometry ?? baseIcon;
            }
            return values[0] ?? DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
