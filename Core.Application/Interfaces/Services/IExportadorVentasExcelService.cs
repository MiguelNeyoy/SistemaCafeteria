using Core.Application.Dtos.Ventas;

namespace Core.Application.Interfaces.Services;

/// <summary>
/// Genera archivos Excel (.xlsx) a partir de listas de ventas filtradas.
/// </summary>
public interface IExportadorVentasExcelService
{
    byte[] GenerarExcelVentas(
        IEnumerable<VentaResumenDto> ventas,
        string tituloPeriodo,
        string? resumenFiltros = null);
}
