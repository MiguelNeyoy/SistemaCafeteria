using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Reportes;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Services;
using Presentation.WPF.Services;

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
            // 1. Refrescar los datos para asegurar que el corte considere las ultimas ventas
            await CargarDatosAsync();

            // 2. Verificar si hay cuentas abiertas pendientes
            var pendientes = await _ventaService.ObtenerPendientesAsync();
            string mensajeConfirmacion;
            if (pendientes.Count > 0)
            {
                mensajeConfirmacion = $"Atención: Hay {pendientes.Count} cuenta(s) abierta(s) pendientes de cobro en el sistema.\n\nEstas cuentas NO estarán incluidas en el corte de caja.\n\n¿Deseas continuar con el cierre de caja e imprimir el ticket?";
            }
            else
            {
                mensajeConfirmacion = "¿Deseas realizar el corte de caja del día de hoy e imprimir el ticket de comprobante?";
            }

            bool confirmar = _dialogoService.Confirmar(mensajeConfirmacion, "Confirmar Corte de Caja", "Sí, Cerrar Caja", "Cancelar");
            if (!confirmar)
                return;

            // 3. Imprimir ticket de corte de caja
            if (_ultimoCorte != null)
            {
                try
                {
                    await _printerService.ImprimirCorteCajaAsync(_ultimoCorte);
                    _dialogoService.NotificarExito("Corte de caja realizado y ticket impreso con éxito.", "Corte de Caja");
                }
                catch (Exception ex)
                {
                    _dialogoService.MostrarAdvertencia(
                        $"El corte de caja se calculó correctamente, pero ocurrió un problema al imprimir el ticket:\n{ex.Message}",
                        "Aviso de Impresión");
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