namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para parámetros de configuración clave-valor (ej. PIN de admin).
/// </summary>
public interface IConfiguracionRepository
{
    Task<string?> ObtenerValorAsync(string clave);
    Task GuardarValorAsync(string clave, string valor);
}
