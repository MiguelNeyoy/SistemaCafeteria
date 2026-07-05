using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Presentation.WPF.Converters;

public class EstadoPedidoToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Pendiente" => new SolidColorBrush(Colors.Orange),
            "EnProceso" => new SolidColorBrush(Colors.DodgerBlue),
            "Completado" => new SolidColorBrush(Colors.Green),
            "Cancelado" => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Gray),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
