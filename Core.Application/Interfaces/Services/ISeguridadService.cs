namespace Core.Application.Interfaces.Services;

public interface ISeguridadService
{
    Task<bool> ValidarPinAsync(string pin);
    Task CambiarPinAsync(string pinActual, string pinNuevo);
    Task ResetearPinConMaestroAsync(string pinMaestro);
}
