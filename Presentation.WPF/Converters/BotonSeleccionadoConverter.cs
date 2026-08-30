using System;
using System.Windows.Data;
using System.Globalization;

namespace Presentation.WPF.Converters
{
    public class BotonSeleccionadoConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == values[1])
            {
                return true;
            }
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}