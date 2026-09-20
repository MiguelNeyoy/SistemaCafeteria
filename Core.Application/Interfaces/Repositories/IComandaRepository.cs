using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para Comandas (Cocina).
/// </summary>
public interface IComandaRepository
{
    Task<Comanda?> ObtenerPorIdAsync(int id);
    Task<List<Comanda>> ObtenerPorVentaIdAsync(int ventaId);
    Task<List<Comanda>> ObtenerPendientesAsync();
    Task AgregarAsync(Comanda comanda);
    void Actualizar(Comanda comanda);
}
