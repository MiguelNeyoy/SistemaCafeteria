using Core.Application.Dtos.Reportes;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;

namespace Core.Application.UseCases;

public class CorteCajaService : ICorteCajaService
{
    private readonly IVentaRepository _ventaRepository;

    public CorteCajaService(IVentaRepository ventaRepository)
    {
        _ventaRepository = ventaRepository;
    }

    public async Task<CorteCajaDto> GenerarCorteDiarioAsync(DateTime fecha, decimal fondoInicial)
    {
        var inicio = fecha.Date;
        var fin = inicio.AddDays(1).AddTicks(-1);

        var ventas = await _ventaRepository.ObtenerPorFechaAsync(fecha);

        return CalcularCorte(ventas, inicio, fin, fondoInicial);
    }

    public async Task<CorteCajaDto> GenerarCorteMensualAsync(int anio, int mes)
    {
        var inicio = new DateTime(anio, mes, 1, 0, 0, 0);
        var fin = inicio.AddMonths(1).AddTicks(-1);

        var ventas = await _ventaRepository.ObtenerPorRangoFechasAsync(inicio, fin);

        return CalcularCorte(ventas, inicio, fin, 0m);
    }

    private static CorteCajaDto CalcularCorte(List<Core.Domain.Entities.Venta> ventas, DateTime inicio, DateTime fin, decimal fondoInicial)
    {
        var pagadas = ventas.Where(v => v.Estado == EstadoVenta.Pagado).ToList();
        var devueltas = ventas.Where(v => v.Estado == EstadoVenta.Devuelto).ToList();
        var canceladas = ventas.Where(v => v.Estado == EstadoVenta.Cancelado).ToList();

        var totalEfectivo = pagadas.Where(v => v.TipoDePago == TipoDePago.Efectivo).Sum(v => v.Total);
        var totalTarjeta = pagadas.Where(v => v.TipoDePago == TipoDePago.Tarjeta).Sum(v => v.Total);
        var totalTransferencia = pagadas.Where(v => v.TipoDePago == TipoDePago.Transferencia).Sum(v => v.Total);
        var totalVentas = pagadas.Sum(v => v.Total);
        var totalDescuentos = pagadas.Sum(v => v.Descuento);
        var totalDevoluciones = devueltas.Sum(v => v.Total);

        return new CorteCajaDto
        {
            FechaInicio = inicio,
            FechaFin = fin,
            FondoInicial = fondoInicial,
            TotalEfectivo = totalEfectivo,
            TotalTarjeta = totalTarjeta,
            TotalTransferencia = totalTransferencia,
            TotalVentas = totalVentas,
            TotalDescuentos = totalDescuentos,
            TotalDevoluciones = totalDevoluciones,
            CantidadVentas = pagadas.Count,
            CantidadCanceladas = canceladas.Count,
            CantidadDevueltas = devueltas.Count
        };
    }
}
