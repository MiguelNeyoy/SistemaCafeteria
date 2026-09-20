using System;

namespace Presentation.WPF.Views.Alertas;

public class AlertaProps
{
    public string Titulo { get; set; } = "Aviso";
    public string Mensaje { get; set; } = string.Empty;
    public TipoAlerta Tipo { get; set; } = TipoAlerta.Informacion;
    public string TextoConfirmar { get; set; } = "Aceptar";
    public string? TextoCancelar { get; set; } = null;
    public Action? AlConfirmar { get; set; }
    public Action? AlCancelar { get; set; }
}
