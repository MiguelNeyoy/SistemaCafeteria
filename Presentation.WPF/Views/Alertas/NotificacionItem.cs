using System;
using System.Windows.Media;

namespace Presentation.WPF.Views.Alertas;

public class NotificacionItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public TipoAlerta Tipo { get; set; } = TipoAlerta.Exito;
    public int DuracionSegundos { get; set; } = 3;
    public DateTime FechaCreacion { get; } = DateTime.Now;

    public Brush ColorAcento => Tipo switch
    {
        TipoAlerta.Exito => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")),
        TipoAlerta.Advertencia => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")),
        TipoAlerta.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828")),
        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"))
    };

    public Brush ColorFondoIcono => Tipo switch
    {
        TipoAlerta.Exito => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")),
        TipoAlerta.Advertencia => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")),
        TipoAlerta.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")),
        _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"))
    };

    public string SimboloIcono => Tipo switch
    {
        TipoAlerta.Exito => "✓",
        TipoAlerta.Advertencia => "!",
        TipoAlerta.Error => "✕",
        _ => "ℹ"
    };
}
