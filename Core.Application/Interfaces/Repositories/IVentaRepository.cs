using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para Ventas / Cuentas.
/// </summary>
public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(int id);
    Task<Venta?> ObtenerPorIdConDetallesAsync(int id);
    Task<List<Venta>> ObtenerPendientesAsync();
    Task<List<Venta>> ObtenerPorFechaAsync(DateTime fecha);
    Task<List<Venta>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta);
    Task AgregarAsync(Venta venta);
    void Actualizar(Venta venta);
    Task EliminarVentasAnterioresAAsync(DateTime fecha);
}
