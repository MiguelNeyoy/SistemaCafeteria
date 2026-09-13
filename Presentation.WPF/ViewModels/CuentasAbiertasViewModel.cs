using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces.Services;
using Presentation.WPF.Services;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

/// <summary>
/// Modelo de presentación para cada ítem de producto dentro de la cuenta abierta.
/// </summary>
public class CuentaItemDetalleModel
{
    public int Id { get; }
    public string ProductoNombre { get; }
    public decimal PrecioUnitario { get; }
    public int Cantidad { get; }
    public string? Notas { get; }
    public decimal Subtotal { get; }
    public List<string> Extras { get; } = new();
    public bool TieneExtras => Extras.Count > 0;
    public bool TieneNotas => !string.IsNullOrWhiteSpace(Notas);

    public CuentaItemDetalleModel(VentaItemDto item)
    {
        Id = item.Id;
        ProductoNombre = item.ProductoNombre;
        PrecioUnitario = item.PrecioUnitario;
        Cantidad = item.Cantidad;
        Notas = item.Notas;
        Subtotal = item.Subtotal;

        if (item.Extras != null && item.Extras.Count > 0)
        {
            Extras = item.Extras
                .Select(e => e.Precio > 0 ? $"+ {e.Nombre} (${e.Precio:F2})" : $"+ {e.Nombre}")
                .ToList();
        }
    }
}

/// <summary>
/// Modelo de presentación para una tarjeta de cuenta abierta en la lista maestra.
/// </summary>
public class CuentaAbiertaItemModel
{
    public VentaResumenDto Venta { get; }
    public int Id => Venta.Id;
    public string Cliente => string.IsNullOrWhiteSpace(Venta.IdentificadorCliente) ? "General" : Venta.IdentificadorCliente;
    public DateTime FechaCreacion => Venta.FechaCreacion;
    public string HoraCreacion => Venta.FechaCreacion.ToString("hh:mm tt");

    public string TiempoTranscurrido
    {
        get
        {
            var diff = DateTime.Now - Venta.FechaCreacion;
            if (diff.TotalMinutes < 1) return "Hace un momento";
            if (diff.TotalMinutes < 60) return $"Hace {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Hace {(int)diff.TotalHours}h {diff.Minutes}m";
            return Venta.FechaCreacion.ToString("dd/MM hh:mm tt");
        }
    }

    public decimal Subtotal => Venta.Subtotal;
    public decimal Descuento => Venta.Descuento;
    public decimal Total => Venta.Total;
    public int TotalProductos => Venta.Items?.Sum(i => i.Cantidad) ?? 0;
    public List<CuentaItemDetalleModel> Items { get; }

    public CuentaAbiertaItemModel(VentaResumenDto venta)
    {
        Venta = venta;
        Items = venta.Items?.Select(i => new CuentaItemDetalleModel(i)).ToList() ?? new();
    }
}

/// <summary>
/// ViewModel principal para la visualización y gestión de cuentas abiertas (Master-Detail).
/// </summary>
public partial class CuentasAbiertasViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly IDialogoService _dialogoService;

    public ObservableCollection<CuentaAbiertaItemModel> Cuentas { get; } = new();

    [ObservableProperty]
    private CuentaAbiertaItemModel? _cuentaSeleccionada;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private int _totalCuentas;

    [ObservableProperty]
    private decimal _montoTotalAcumulado;

    [ObservableProperty]
    private bool _tieneCuentas;

    public CuentasAbiertasViewModel(IVentaService ventaService, IDialogoService dialogoService)
    {
        _ventaService = ventaService;
        _dialogoService = dialogoService;
    }

    [RelayCommand]
    public async Task CargarCuentasAsync()
    {
        EstaCargando = true;
        try
        {
            var pendientes = await _ventaService.ObtenerPendientesAsync();

            var cuentasOrdenadas = pendientes
                .OrderByDescending(v => v.FechaCreacion)
                .Select(v => new CuentaAbiertaItemModel(v))
                .ToList();

            var idSeleccionadoPreviamente = CuentaSeleccionada?.Id;

            Cuentas.Clear();
            foreach (var cuenta in cuentasOrdenadas)
            {
                Cuentas.Add(cuenta);
            }

            TotalCuentas = Cuentas.Count;
            MontoTotalAcumulado = Cuentas.Sum(c => c.Total);
            TieneCuentas = Cuentas.Count > 0;

            if (idSeleccionadoPreviamente.HasValue)
            {
                CuentaSeleccionada = Cuentas.FirstOrDefault(c => c.Id == idSeleccionadoPreviamente.Value)
                                     ?? Cuentas.FirstOrDefault();
            }
            else
            {
                CuentaSeleccionada = Cuentas.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al cargar cuentas abiertas: {ex.Message}", "Error");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CancelarCuentaAsync()
    {
        if (CuentaSeleccionada is null) return;

        var cuenta = CuentaSeleccionada;
        bool confirmado = _dialogoService.Confirmar(
            $"¿Estás seguro de cancelar la cuenta #{cuenta.Id} de \"{cuenta.Cliente}\"?\n\nTotal: ${cuenta.Total:F2}\nEsta acción no se puede deshacer.",
            "Confirmar cancelación");

        if (!confirmado) return;

        EstaCargando = true;
        try
        {
            await _ventaService.CancelarAsync(cuenta.Id);
            _dialogoService.MostrarMensaje($"La cuenta #{cuenta.Id} ha sido cancelada.", "Cuenta Cancelada");
            await CargarCuentasAsync();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al cancelar la cuenta: {ex.Message}", "Error");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    public event Action<int>? CobrarCuentaSolicitado;

    [RelayCommand]
    private void CobrarCuenta()
    {
        if (CuentaSeleccionada is null) return;
        CobrarCuentaSolicitado?.Invoke(CuentaSeleccionada.Id);
    }
}
