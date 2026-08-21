namespace Core.Application.Dtos.Seguridad;

public class CambiarPinDto
{
    public string PinActual { get; set; } = string.Empty;
    public string PinNuevo { get; set; } = string.Empty;
}
