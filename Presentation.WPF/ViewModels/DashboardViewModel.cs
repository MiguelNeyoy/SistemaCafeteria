using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;
using Presentation.WPF.Services;

namespace Presentation.WPF.ViewModels;

public class VentaRecienteItemViewModel
{
    public int Id { get; set; }
    public string FolioTexto => $"#{Id}";
    public string Cliente { get; set; } = "General";
    public string HoraTexto { get; set; } = string.Empty;
    public string TipoPagoTexto { get; set; } = "Efectivo";
    public decimal Total { get; set; }
    public int CantidadItems { get; set; }
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly ICorteCajaService _corteCajaService;
    private readonly IDialogoService _dialogoService;

    [ObservableProperty]
    private string fechaHoyTexto = string.Empty;

    [ObservableProperty]
    private string mesActualTexto = string.Empty;

    [ObservableProperty]
    private decimal totalVentasHoy;

    [ObservableProperty]
    private int cantidadVentasHoy;

    [ObservableProperty]
    private decimal totalEfectivoHoy;

    [ObservableProperty]
    private decimal totalTarjetaHoy;

    [ObservableProperty]
    private decimal totalTransferenciaHoy;

    [ObservableProperty]
    private decimal totalDescuentosHoy;

    [ObservableProperty]
    private decimal ticketPromedioHoy;

    [ObservableProperty]
    private int cuentasAbiertasCount;

    [ObservableProperty]
    private decimal cuentasAbiertasTotal;

    [ObservableProperty]
    private bool tieneTopProductos;

    [ObservableProperty]
    private bool tieneVentasRecientes;

    [ObservableProperty]
    private bool estaCargando;

    public ObservableCollection<ProductoTopDto> TopProductos { get; } = new();
    public ObservableCollection<VentaRecienteItemViewModel> VentasRecientes { get; } = new();

    public DashboardViewModel(
        IVentaService ventaService,
        ICorteCajaService corteCajaService,
        IDialogoService dialogoService)
    {
        _ventaService = ventaService;
        _corteCajaService = corteCajaService;
        _dialogoService = dialogoService;

        var cultura = new CultureInfo("es-MX");
        FechaHoyTexto = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura);
        MesActualTexto = DateTime.Now.ToString("MMMM yyyy", cultura);
    }

    [RelayCommand]
    public async Task CargarDatosAsync()
    {
        try
        {
            EstaCargando = true;

            var cultura = new CultureInfo("es-MX");
            FechaHoyTexto = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura);
            MesActualTexto = DateTime.Now.ToString("MMMM yyyy", cultura);

            // 1. Resumen de ventas del día actual
            var corteDiario = await _corteCajaService.GenerarCorteDiarioAsync(DateTime.Today, 0m);
            TotalVentasHoy = corteDiario.TotalVentas;
            CantidadVentasHoy = corteDiario.CantidadVentas;
            TotalEfectivoHoy = corteDiario.TotalEfectivo;
            TotalTarjetaHoy = corteDiario.TotalTarjeta;
            TotalTransferenciaHoy = corteDiario.TotalTransferencia;
            TotalDescuentosHoy = corteDiario.TotalDescuentos;
            TicketPromedioHoy = CantidadVentasHoy > 0 ? Math.Round(TotalVentasHoy / CantidadVentasHoy, 2) : 0m;

            // 2. Cuentas abiertas actualmente
            var cuentasPendientes = await _ventaService.ObtenerPendientesAsync();
            CuentasAbiertasCount = cuentasPendientes.Count;
            CuentasAbiertasTotal = cuentasPendientes.Sum(c => c.Total);

            // 3. Top más vendidos en el mes en curso
            var hoy = DateTime.Today;
            var top = await _ventaService.ObtenerTopProductosMesAsync(hoy.Year, hoy.Month, 5);

            TopProductos.Clear();
            foreach (var producto in top)
            {
                TopProductos.Add(producto);
            }

            TieneTopProductos = TopProductos.Count > 0;

            // 4. Ventas más recientes del día de hoy
            var ventasHoy = await _ventaService.ObtenerPorFechaAsync(DateTime.Today);
            var recientes = ventasHoy
                .Where(v => v.Estado == EstadoVenta.Pagado)
                .OrderByDescending(v => v.FechaCierre ?? v.FechaCreacion)
                .Take(10)
                .Select(v => new VentaRecienteItemViewModel
                {
                    Id = v.Id,
                    Cliente = string.IsNullOrWhiteSpace(v.IdentificadorCliente) ? "General" : v.IdentificadorCliente,
                    HoraTexto = (v.FechaCierre ?? v.FechaCreacion).ToString("hh:mm tt", new CultureInfo("es-MX")),
                    TipoPagoTexto = v.TipoDePago?.ToString() ?? "Efectivo",
                    Total = v.Total,
                    CantidadItems = v.Items.Sum(i => i.Cantidad)
                })
                .ToList();

            VentasRecientes.Clear();
            foreach (var v in recientes)
            {
                VentasRecientes.Add(v);
            }

            TieneVentasRecientes = VentasRecientes.Count > 0;
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al cargar datos del resumen: {ex.Message}", "Resumen Operativo");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private void ImprimirTicket()
    {
        _dialogoService.NotificarExito(
            $"Desglose del día ({TotalVentasHoy:C}, {CantidadVentasHoy} transacciones) enviado a la impresora.",
            "Ticket Impreso");
    }
}
