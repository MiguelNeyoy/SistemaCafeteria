using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para Extras y Modificadores.
/// </summary>
public interface IExtraRepository
{
    Task<Extra?> ObtenerPorIdAsync(int id);
    Task<List<Extra>> ObtenerTodosAsync();
    Task<List<Extra>> ObtenerActivosAsync();
    Task AgregarAsync(Extra extra);
    void Actualizar(Extra extra);
}
