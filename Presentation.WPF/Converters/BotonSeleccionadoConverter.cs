using System;
using System.Windows.Data;
using System.Globalization;

namespace Presentation.WPF.Converters
{
    public class BotonSeleccionadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == parameter)
            {
                return true;

            }else 
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}