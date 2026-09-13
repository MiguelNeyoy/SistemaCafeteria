using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;
using Presentation.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

namespace Presentation.WPF.ViewModels;

/// <summary>
/// ViewModel para el modulo de cobro y liquidacion de cuentas (FinalizarCompra).
/// Permite cobrar mediante Efectivo (con desglose de cambio y teclado numerico),
/// Tarjeta o Transferencia bancaria directa.
/// </summary>
public partial class FinalizarCompraViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly IDialogoService _dialogoService;

    public event Action? CobroFinalizado;
    public event Action? RegresarSolicitado;

    [ObservableProperty]
    private int _ventaId;

    [ObservableProperty]
    private string _cliente = "General";

    [ObservableProperty]
    private DateTime _fechaCreacion;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _descuento;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private bool _estaProcesando;

    [ObservableProperty]
    private TipoDePago _tipoPagoSeleccionado = TipoDePago.Efectivo;

    [ObservableProperty]
    private bool _esEfectivo = true;

    [ObservableProperty]
    private bool _esTarjeta;

    [ObservableProperty]
    private bool _esTransferencia;

    [ObservableProperty]
    private string _montoRecibidoTexto = "";

    [ObservableProperty]
    private decimal _montoRecibido;

    [ObservableProperty]
    private decimal _cambio;

    [ObservableProperty]
    private bool _montoEsSuficiente = true;

    [ObservableProperty]
    private bool _puedeConfirmarCobro;

    public ObservableCollection<CuentaItemDetalleModel> Items { get; } = new();

    public FinalizarCompraViewModel(IVentaService ventaService, IDialogoService dialogoService)
    {
        _ventaService = ventaService;
        _dialogoService = dialogoService;
    }

    public async Task CargarVentaAsync(int ventaId)
    {
        EstaCargando = true;
        try
        {
            var venta = await _ventaService.ObtenerPorIdAsync(ventaId);
            if (venta == null)
            {
                _dialogoService.MostrarMensaje($"No se encontro la cuenta #{ventaId}.", "Error");
                RegresarSolicitado?.Invoke();
                return;
            }

            VentaId = venta.Id;
            Cliente = string.IsNullOrWhiteSpace(venta.IdentificadorCliente) ? "General" : venta.IdentificadorCliente;
            FechaCreacion = venta.FechaCreacion;
            Subtotal = venta.Subtotal;
            Descuento = venta.Descuento;
            Total = venta.Total;

            Items.Clear();
            if (venta.Items != null)
            {
                foreach (var item in venta.Items)
                {
                    Items.Add(new CuentaItemDetalleModel(item));
                }
            }

            // Seleccionar Efectivo por defecto con el monto exacto sugerido
            TipoPagoSeleccionado = TipoDePago.Efectivo;
            EsEfectivo = true;
            EsTarjeta = false;
            EsTransferencia = false;

            MontoRecibido = Total;
            MontoRecibidoTexto = Total.ToString("F2", CultureInfo.InvariantCulture);
            ActualizarCalculos();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al cargar la venta para cobro: {ex.Message}", "Error");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private void SeleccionarTipoPago(string tipoStr)
    {
        if (Enum.TryParse<TipoDePago>(tipoStr, true, out var tipo))
        {
            TipoPagoSeleccionado = tipo;
            EsEfectivo = tipo == TipoDePago.Efectivo;
            EsTarjeta = tipo == TipoDePago.Tarjeta;
            EsTransferencia = tipo == TipoDePago.Transferencia;

            if (!EsEfectivo)
            {
                MontoRecibido = Total;
                MontoRecibidoTexto = Total.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(MontoRecibidoTexto) || MontoRecibido == 0)
                {
                    MontoRecibido = Total;
                    MontoRecibidoTexto = Total.ToString("F2", CultureInfo.InvariantCulture);
                }
            }

            ActualizarCalculos();
        }
    }

    [RelayCommand]
    private void IngresarDigitoKeypad(string digito)
    {
        if (!EsEfectivo) return;

        if (digito == ".")
        {
            if (MontoRecibidoTexto.Contains(".")) return;
            MontoRecibidoTexto = string.IsNullOrEmpty(MontoRecibidoTexto) ? "0." : MontoRecibidoTexto + ".";
        }
        else if (digito == "00")
        {
            if (string.IsNullOrEmpty(MontoRecibidoTexto) || MontoRecibidoTexto == "0") return;
            if (MontoRecibidoTexto.Contains(".") && MontoRecibidoTexto.IndexOf(".") < MontoRecibidoTexto.Length - 1) return;
            MontoRecibidoTexto += "00";
        }
        else
        {
            if (MontoRecibidoTexto == "0")
            {
                MontoRecibidoTexto = digito;
            }
            else
            {
                // Limitar a maximo 2 decimales si ya hay punto
                if (MontoRecibidoTexto.Contains("."))
                {
                    int posPunto = MontoRecibidoTexto.IndexOf(".");
                    if (MontoRecibidoTexto.Length - posPunto > 2)
                        return;
                }
                MontoRecibidoTexto += digito;
            }
        }

        SincronizarMontoDesdeTexto();
    }

    [RelayCommand]
    private void BorrarDigitoKeypad()
    {
        if (!EsEfectivo) return;

        if (!string.IsNullOrEmpty(MontoRecibidoTexto))
        {
            MontoRecibidoTexto = MontoRecibidoTexto.Substring(0, MontoRecibidoTexto.Length - 1);
        }

        SincronizarMontoDesdeTexto();
    }

    [RelayCommand]
    private void LimpiarKeypad()
    {
        if (!EsEfectivo) return;
        MontoRecibidoTexto = "";
        SincronizarMontoDesdeTexto();
    }

    [RelayCommand]
    private void EstablecerMontoRapido(string montoStr)
    {
        if (!EsEfectivo) return;

        if (montoStr.Equals("Exacto", StringComparison.OrdinalIgnoreCase))
        {
            MontoRecibido = Total;
        }
        else if (decimal.TryParse(montoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto))
        {
            MontoRecibido = monto;
        }

        MontoRecibidoTexto = MontoRecibido.ToString("F2", CultureInfo.InvariantCulture);
        ActualizarCalculos();
    }

    private void SincronizarMontoDesdeTexto()
    {
        if (decimal.TryParse(MontoRecibidoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
        {
            MontoRecibido = val;
        }
        else
        {
            MontoRecibido = 0m;
        }

        ActualizarCalculos();
    }

    private void ActualizarCalculos()
    {
        if (EsEfectivo)
        {
            if (MontoRecibido >= Total)
            {
                Cambio = MontoRecibido - Total;
                MontoEsSuficiente = true;
            }
            else
            {
                Cambio = 0m;
                MontoEsSuficiente = false;
            }
        }
        else
        {
            MontoRecibido = Total;
            Cambio = 0m;
            MontoEsSuficiente = true;
        }

        PuedeConfirmarCobro = Total > 0 && MontoEsSuficiente && !EstaProcesando;
    }

    [RelayCommand]
    private async Task CobrarAsync()
    {
        if (!PuedeConfirmarCobro || EstaProcesando) return;

        EstaProcesando = true;
        try
        {
            var dto = new CobrarVentaDto
            {
                VentaId = VentaId,
                TipoDePago = TipoPagoSeleccionado,
                MontoRecibido = EsEfectivo ? MontoRecibido : Total
            };

            await _ventaService.CobrarAsync(dto);

            string mensaje = EsEfectivo
                ? $"Cobro registrado exitosamente.\n\nTicket #{VentaId} - {Cliente}\nTotal: ${Total:F2}\nEfectivo Recibido: ${MontoRecibido:F2}\nCambio a Entregar: ${Cambio:F2}"
                : $"Cobro con {TipoPagoSeleccionado} registrado exitosamente.\n\nTicket #{VentaId} - {Cliente}\nTotal: ${Total:F2}";

            _dialogoService.MostrarMensaje(mensaje, "Cobro Exitoso");

            CobroFinalizado?.Invoke();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al procesar el cobro: {ex.Message}", "Error");
        }
        finally
        {
            EstaProcesando = false;
            ActualizarCalculos();
        }
    }

    [RelayCommand]
    private void Regresar()
    {
        RegresarSolicitado?.Invoke();
    }
}
