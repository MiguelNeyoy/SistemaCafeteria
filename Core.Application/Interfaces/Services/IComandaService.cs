using Core.Application.Dtos.Comandas;

namespace Core.Application.Interfaces.Services;

public interface IComandaService
{
    Task<ComandaResumenDto> EnviarACocinaAsync(int ventaId, List<EnviarComandaItemDto> items);
    Task MarcarEntregadaAsync(int comandaId);
    Task<List<ComandaResumenDto>> ObtenerPendientesAsync();
    Task<List<ComandaResumenDto>> ObtenerPorVentaAsync(int ventaId);
}
