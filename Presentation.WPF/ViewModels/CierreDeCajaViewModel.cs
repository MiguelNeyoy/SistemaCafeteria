using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Reportes;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Presentation.WPF.Services;
using System;
using System.Threading.Tasks;

namespace Presentation.WPF.ViewModels;

public partial class CierreDeCajaViewModel : ObservableObject
{
    private readonly ICorteCajaService _corteCajaService;
    private readonly IPrinterService _printerService;
    private readonly IDialogoService _dialogoService;
    private readonly IVentaService _ventaService;

    private CorteCajaDto? _ultimoCorte;

    [ObservableProperty]
    private decimal _totalVentas;

    public decimal TotalVenta => TotalVentas;

    [ObservableProperty]
    private decimal _totalEfectivo;

    [ObservableProperty]
    private decimal _totalTransferencia;

    [ObservableProperty]
    private decimal _totalTarjeta;

    [ObservableProperty]
    private decimal _totalVentasBruto;

    [ObservableProperty]
    private decimal _totalDescuentos;

    [ObservableProperty]
    private decimal _totalNeto;

    [ObservableProperty]
    private int _cuentasAbiertasCount;

    [ObservableProperty]
    private bool _tieneCuentasAbiertas;

    [ObservableProperty]
    private string _mensajeAlertaCuentas = string.Empty;

    [ObservableProperty]
    private bool _estaCargando;

    public CierreDeCajaViewModel(
        ICorteCajaService corteCajaService,
        IPrinterService printerService,
        IDialogoService dialogoService,
        IVentaService ventaService)
    {
        _corteCajaService = corteCajaService;
        _printerService = printerService;
        _dialogoService = dialogoService;
        _ventaService = ventaService;
    }

    [RelayCommand]
    public async Task CargarDatosAsync()
    {
        EstaCargando = true;
        try
        {
            var corte = await _corteCajaService.GenerarCorteDiarioAsync(DateTime.Today, 0m);
            _ultimoCorte = corte;

            TotalEfectivo = corte.TotalEfectivo;
            TotalTransferencia = corte.TotalTransferencia;
            TotalTarjeta = corte.TotalTarjeta;
            TotalDescuentos = corte.TotalDescuentos;

            TotalNeto = corte.TotalVentas;
            TotalVentas = corte.TotalVentas;
            TotalVentasBruto = corte.TotalVentas + corte.TotalDescuentos;
            OnPropertyChanged(nameof(TotalVenta));

            // Verificar cuentas abiertas pendientes
            var pendientes = await _ventaService.ObtenerPendientesAsync();
            CuentasAbiertasCount = pendientes.Count;
            TieneCuentasAbiertas = CuentasAbiertasCount > 0;
            MensajeAlertaCuentas = TieneCuentasAbiertas
                ? $"Hay {CuentasAbiertasCount} cuenta(s) abierta(s) sin cobrar. Debes cobrarlas o cancelarlas antes de cerrar caja."
                : string.Empty;
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarError($"Error al consultar los datos del corte de caja: {ex.Message}", "Corte de Caja");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task CerrarCajaAsync()
    {
        if (EstaCargando)
            return;

        EstaCargando = true;
        try
        {
            // 1. Refrescar los datos para asegurar que el corte considere las últimas ventas
            await CargarDatosAsync();

            // 2. VALIDACIÓN ESTRICTA: Bloquear si hay cuentas abiertas pendientes
            if (TieneCuentasAbiertas)
            {
                _dialogoService.MostrarAdvertencia(
                    $"No se puede realizar el corte de caja.\n\n" +
                    $"Actualmente hay {CuentasAbiertasCount} cuenta(s) abierta(s) pendientes de cobro o cancelación en el sistema.\n\n" +
                    $"Por favor ve al módulo de 'Cuentas Abiertas' para cobrarlas o cancelarlas antes de proceder con el cierre.",
                    "Cuentas Pendientes de Cobro");
                return;
            }

            // 3. Confirmar con el usuario
            if (_ultimoCorte == null || _ultimoCorte.CantidadVentas == 0)
            {
                bool continuarSinVentas = _dialogoService.Confirmar(
                    "No se registraron ventas pagadas el día de hoy.\n\n¿Deseas realizar el corte de caja de todas formas?",
                    "Sin Ventas Hoy",
                    "Sí, Continuar",
                    "Cancelar");

                if (!continuarSinVentas)
                    return;
            }
            else
            {
                bool confirmar = _dialogoService.Confirmar(
                    $"¿Deseas realizar el corte de caja del día de hoy?\n\n" +
                    $"• Total Ventas: {_ultimoCorte.TotalVentas:C2}\n" +
                    $"• Efectivo: {_ultimoCorte.TotalEfectivo:C2}\n" +
                    $"• Tarjeta: {_ultimoCorte.TotalTarjeta:C2}\n" +
                    $"• Transferencia: {_ultimoCorte.TotalTransferencia:C2}\n\n" +
                    $"Se imprimirá el comprobante oficial y se abrirá el cajón de dinero.",
                    "Confirmar Corte de Caja",
                    "Sí, Cerrar Caja",
                    "Cancelar");

                if (!confirmar)
                    return;
            }

            // 4. Imprimir ticket de corte de caja y abrir cajón
            if (_ultimoCorte != null)
            {
                try
                {
                    await _printerService.ImprimirCorteCajaAsync(_ultimoCorte);

                    try
                    {
                        await _printerService.AbrirCajonDineroAsync();
                    }
                    catch
                    {
                        // Si falla solo el cajón pero la impresora imprimió, no bloquear
                    }

                    _dialogoService.NotificarExito("Corte de caja realizado y ticket impreso con éxito.", "Corte de Caja");
                }
                catch (Exception ex)
                {
                    _dialogoService.MostrarAdvertencia(
                        $"El corte de caja se calculó correctamente, pero ocurrió un problema con la impresora:\n\n{ex.Message}\n\n(Verifique que la impresora térmica esté conectada y encendida)",
                        "Aviso de Hardware");
                }
            }
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarError($"Ocurrió un error inesperado al cerrar la caja: {ex.Message}", "Error en Corte");
        }
        finally
        {
            EstaCargando = false;
        }
    }
}