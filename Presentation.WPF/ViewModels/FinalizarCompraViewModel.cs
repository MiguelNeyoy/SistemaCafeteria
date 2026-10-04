using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces;
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
    private readonly IPrinterService _printerService;
    private readonly ITicketService _ticketService;

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

    [ObservableProperty]
    private bool _mostrarModalDescuento;

    [ObservableProperty]
    private string _montoDescuentoInput = "";

    public ObservableCollection<CuentaItemDetalleModel> Items { get; } = new();

    public FinalizarCompraViewModel(
        IVentaService ventaService, 
        IDialogoService dialogoService,
        IPrinterService printerService,
        ITicketService ticketService)
    {
        _ventaService = ventaService;
        _dialogoService = dialogoService;
        _printerService = printerService;
        _ticketService = ticketService;
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

            var ventaResumen = await _ventaService.CobrarAsync(dto);

            // Generar folio de ticket y mandar a imprimir ticket físico
            try
            {
                var ticket = await _ticketService.GenerarTicketAsync(VentaId);
                await _printerService.ImprimirTicketAsync(ventaResumen, ticket.Folio);
            }
            catch (Exception exHardware)
            {
                _dialogoService.MostrarMensaje(
                    $"Cobro registrado exitosamente.\n\n⚠️ Aviso de Impresora: No se pudo imprimir el ticket ni abrir el cajón automáticamente:\n{exHardware.Message}\n\n(Verifique que la impresora esté conectada y encendida)",
                    "Aviso de Hardware");
            }

            string mensajeToast = EsEfectivo
                ? $"Total: ${Total:F2} • Efectivo: ${MontoRecibido:F2} • Cambio: ${Cambio:F2}"
                : $"Ticket #{VentaId} pagado con {TipoPagoSeleccionado} (${Total:F2})";

            _dialogoService.NotificarExito(mensajeToast, "Cobro Exitoso", 4);

            CobroFinalizado?.Invoke();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarError($"Error al procesar el cobro: {ex.Message}");
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

    #region Comandos de Descuento

    [RelayCommand]
    private void AbrirModalDescuento()
    {
        MontoDescuentoInput = Descuento > 0 ? Descuento.ToString("F2", CultureInfo.InvariantCulture) : "";
        MostrarModalDescuento = true;
    }

    [RelayCommand]
    private void CerrarModalDescuento()
    {
        MostrarModalDescuento = false;
    }

    [RelayCommand]
    private async Task AplicarPorcentajeRapidoAsync(string porcentajeStr)
    {
        if (decimal.TryParse(porcentajeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var pct))
        {
            if (pct < 0 || pct > 100) return;
            decimal monto = Math.Round(Subtotal * (pct / 100m), 2, MidpointRounding.AwayFromZero);
            await AplicarDescuentoInternoAsync(monto);
        }
    }

    [RelayCommand]
    private async Task AplicarDescuentoMontoAsync()
    {
        if (string.IsNullOrWhiteSpace(MontoDescuentoInput))
        {
            await AplicarDescuentoInternoAsync(0m);
            return;
        }

        if (!decimal.TryParse(MontoDescuentoInput, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto))
        {
            _dialogoService.MostrarAdvertencia("Por favor ingrese un monto de descuento válido.", "Monto Inválido");
            return;
        }

        if (monto < 0)
        {
            _dialogoService.MostrarAdvertencia("El descuento no puede ser negativo.", "Monto Inválido");
            return;
        }

        if (monto > Subtotal)
        {
            _dialogoService.MostrarAdvertencia($"El descuento (${monto:F2}) no puede superar el subtotal de la cuenta (${Subtotal:F2}).", "Descuento Excesivo");
            return;
        }

        await AplicarDescuentoInternoAsync(monto);
    }

    [RelayCommand]
    private async Task QuitarDescuentoAsync()
    {
        await AplicarDescuentoInternoAsync(0m);
    }

    [RelayCommand]
    private void IngresarDigitoDescuento(string digito)
    {
        if (digito == ".")
        {
            if (MontoDescuentoInput.Contains(".")) return;
            MontoDescuentoInput = string.IsNullOrEmpty(MontoDescuentoInput) ? "0." : MontoDescuentoInput + ".";
        }
        else
        {
            if (MontoDescuentoInput == "0")
            {
                MontoDescuentoInput = digito;
            }
            else
            {
                if (MontoDescuentoInput.Contains("."))
                {
                    int posPunto = MontoDescuentoInput.IndexOf(".");
                    if (MontoDescuentoInput.Length - posPunto > 2) return;
                }
                MontoDescuentoInput += digito;
            }
        }
    }

    [RelayCommand]
    private void BorrarDigitoDescuento()
    {
        if (!string.IsNullOrEmpty(MontoDescuentoInput))
        {
            MontoDescuentoInput = MontoDescuentoInput.Substring(0, MontoDescuentoInput.Length - 1);
        }
    }

    private async Task AplicarDescuentoInternoAsync(decimal monto)
    {
        try
        {
            EstaProcesando = true;
            var ventaActualizada = await _ventaService.AplicarDescuentoAsync(VentaId, monto);
            Descuento = ventaActualizada.Descuento;
            Total = ventaActualizada.Total;
            Subtotal = ventaActualizada.Subtotal;

            // Si estamos en efectivo o el monto recibido sugerido correspondía al total previo, ajustar monto recibido
            if (!EsEfectivo || MontoRecibido < Total || MontoRecibido == Subtotal)
            {
                MontoRecibido = Total;
                MontoRecibidoTexto = Total.ToString("F2", CultureInfo.InvariantCulture);
            }

            ActualizarCalculos();
            MostrarModalDescuento = false;

            if (monto > 0)
            {
                _dialogoService.NotificarExito($"Descuento de {monto:C2} aplicado a la cuenta.", "Descuento Aplicado");
            }
            else
            {
                _dialogoService.NotificarInformacion("Descuento removido de la cuenta.", "Descuento");
            }
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarError($"Error al aplicar descuento: {ex.Message}", "Descuento");
        }
        finally
        {
            EstaProcesando = false;
            ActualizarCalculos();
        }
    }

    #endregion
}
