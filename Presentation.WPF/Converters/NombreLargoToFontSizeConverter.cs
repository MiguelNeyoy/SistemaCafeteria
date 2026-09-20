using System;
using System.Globalization;
using System.Windows.Data;

namespace Presentation.WPF.Converters;

/// <summary>
/// Ajusta dinamicamente el tamano de fuente del nombre del producto segun la cantidad
/// de palabras y la longitud del texto, evitando que nombres extensos (ej. tortas o jugos)
/// se desborden de las tarjetas del catalogo tactil.
/// </summary>
public class NombreLargoToFontSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string texto || string.IsNullOrWhiteSpace(texto))
            return 17.5;

        string limpio = texto.Trim();
        int palabras = limpio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
        int longitud = limpio.Length;

        // Mas de 3 palabras o mas de 25 caracteres (ej. "Torta de pierna especial c/ Jamon y queso")
        if (palabras > 5 || longitud > 25)
            return 14.5;

        // 3 palabras o entre 18 y 25 caracteres
        if (palabras == 3 || longitud > 17)
            return 16.5;

        // Nombres estandar cortos (1 o 2 palabras)
        return 17.5;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
