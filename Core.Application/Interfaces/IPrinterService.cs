using Core.Application.Dtos.Comandas;
using Core.Application.Dtos.Reportes;
using Core.Application.Dtos.Ventas;

namespace Core.Application.Interfaces;

/// <summary>
/// Puerto de salida para impresión térmica física (ESC/POS).
/// La implementación concreta reside en Infrastructure.Hardware.
/// </summary>
public interface IPrinterService
{
    Task ImprimirTicketAsync(VentaResumenDto venta, string folio);
    Task ImprimirComandaAsync(ComandaResumenDto comanda);
    Task ImprimirCorteCajaAsync(CorteCajaDto corte);
}
