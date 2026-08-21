using Core.Application.Dtos.Ventas;

namespace Core.Application.Interfaces.Services;

public interface IVentaService
{
    // Crear y administrar cuenta abierta
    Task<VentaResumenDto> CrearCuentaAsync(string? identificadorCliente = null);
    Task<VentaResumenDto> AgregarItemAsync(int ventaId, AgregarItemVentaDto dto);
    Task<VentaResumenDto> RemoverItemAsync(int ventaId, int ventaItemId);
    Task<VentaResumenDto> AplicarDescuentoAsync(int ventaId, decimal montoDescuento);

    // Cobro y cierre
    Task<VentaResumenDto> CobrarAsync(CobrarVentaDto dto);

    // Post-cobro y cancelación
    Task<VentaResumenDto> DevolverAsync(int ventaId);
    Task CancelarAsync(int ventaId);

    // Consultas
    Task<VentaResumenDto?> ObtenerPorIdAsync(int id);
    Task<List<VentaResumenDto>> ObtenerPendientesAsync();
    Task<List<VentaResumenDto>> ObtenerPorFechaAsync(DateTime fecha);
}
