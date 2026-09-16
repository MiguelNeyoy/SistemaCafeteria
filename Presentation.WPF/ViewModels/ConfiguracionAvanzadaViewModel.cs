using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Presentation.WPF.Services;

namespace Presentation.WPF.ViewModels;

public partial class ConfiguracionAvanzadaViewModel : ObservableObject
{
    private readonly ISeguridadService _seguridadService;
    private readonly IPurgaService _purgaService;
    private readonly IPrinterService _printerService;
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IDialogoService _dialogoService;
    private readonly IActualizadorService _actualizadorService;

    public event Action? RegresarSolicitado;

    #region Propiedades - Seguridad y PIN
    [ObservableProperty]
    private string pinActual = string.Empty;

    [ObservableProperty]
    private string pinNuevo = string.Empty;

    [ObservableProperty]
    private string mensajeSeguridad = string.Empty;

    [ObservableProperty]
    private bool esMensajeSeguridadError;
    #endregion

    #region Propiedades - Purga de Ventas
    [ObservableProperty]
    private string diasPurgaTexto = "90";

    public ObservableCollection<string> OpcionesDiasPurga { get; } = new()
    {
        "30",
        "60",
        "90",
        "180",
        "365"
    };

    [ObservableProperty]
    private int ventasAPurgar;

    [ObservableProperty]
    private string mensajeResultadoPurga = string.Empty;
    #endregion

    #region Propiedades - Modal PIN Táctil
    [ObservableProperty]
    private bool mostrarModalPin;

    [ObservableProperty]
    private string pinIngresado = string.Empty;

    public string PinEnmascarado => new string('●', PinIngresado.Length);

    [ObservableProperty]
    private string mensajeErrorPin = string.Empty;

    private Func<Task>? _accionPendientePostPin;
    #endregion

    #region Propiedades - Hardware Impresora y Cajon
    public ObservableCollection<string> ImpresorasDisponibles { get; } = new();

    [ObservableProperty]
    private string? impresoraSeleccionada;

    [ObservableProperty]
    private string? impresoraComandaSeleccionada;

    [ObservableProperty]
    private bool esPapel58mm = true;

    [ObservableProperty]
    private bool esPapel80mm;

    [ObservableProperty]
    private bool abrirCajonAutomatico = true;

    [ObservableProperty]
    private bool cortarPapelAutomatico = true;

    [ObservableProperty]
    private string mensajeResultadoImpresora = string.Empty;

    [ObservableProperty]
    private bool tieneErrorImpresora;
    #endregion

    #region Propiedades - Actualizaciones del Sistema
    public string VersionActual => _actualizadorService.VersionActual;

    [ObservableProperty]
    private bool estaBuscandoActualizacion;

    [ObservableProperty]
    private bool estaDescargandoActualizacion;

    [ObservableProperty]
    private int progresoActualizacion;

    [ObservableProperty]
    private string mensajeActualizacion = "Presione para buscar actualizaciones disponibles.";

    [ObservableProperty]
    private bool actualizacionLista;

    [ObservableProperty]
    private bool tieneErrorActualizacion;

    [ObservableProperty]
    private string? versionNuevaDisponible;
    #endregion

    public ConfiguracionAvanzadaViewModel(
        ISeguridadService seguridadService,
        IPurgaService purgaService,
        IPrinterService printerService,
        IConfiguracionRepository configuracionRepository,
        IDialogoService dialogoService,
        IActualizadorService actualizadorService)
    {
        _seguridadService = seguridadService;
        _purgaService = purgaService;
        _printerService = printerService;
        _configuracionRepository = configuracionRepository;
        _dialogoService = dialogoService;
        _actualizadorService = actualizadorService;
    }

    public async Task CargarDatosAsync()
    {
        RefrescarImpresoras();
        await CargarConfiguracionImpresoraAsync();
        await ActualizarConteoVentasAPurgarAsync();
    }

    [RelayCommand]
    private void Regresar()
    {
        RegresarSolicitado?.Invoke();
    }

    #region Comandos - Seguridad PIN
    [RelayCommand]
    private async Task CambiarPin()
    {
        MensajeSeguridad = string.Empty;

        if (string.IsNullOrWhiteSpace(PinActual) || string.IsNullOrWhiteSpace(PinNuevo))
        {
            MensajeSeguridad = "Debe ingresar el PIN actual y el nuevo PIN.";
            EsMensajeSeguridadError = true;
            return;
        }

        if (PinNuevo.Trim().Length < 4)
        {
            MensajeSeguridad = "El nuevo PIN debe contener al menos 4 dígitos.";
            EsMensajeSeguridadError = true;
            return;
        }

        try
        {
            await _seguridadService.CambiarPinAsync(PinActual.Trim(), PinNuevo.Trim());
            MensajeSeguridad = "PIN de seguridad actualizado correctamente.";
            EsMensajeSeguridadError = false;
            PinActual = string.Empty;
            PinNuevo = string.Empty;
            _dialogoService.NotificarExito("PIN de administrador actualizado.", "Seguridad");
        }
        catch (Exception ex)
        {
            MensajeSeguridad = ex.Message;
            EsMensajeSeguridadError = true;
        }
    }
    #endregion

    #region Comandos - Purga de Ventas
    async partial void OnDiasPurgaTextoChanged(string value)
    {
        await ActualizarConteoVentasAPurgarAsync();
    }

    private async Task ActualizarConteoVentasAPurgarAsync()
    {
        if (int.TryParse(DiasPurgaTexto, out int dias) && dias > 0)
        {
            try
            {
                VentasAPurgar = await _purgaService.ContarVentasAntiguasAsync(dias);
            }
            catch
            {
                VentasAPurgar = 0;
            }
        }
        else
        {
            VentasAPurgar = 0;
        }
    }

    [RelayCommand]
    private void SolicitarPurgaVentas()
    {
        if (!int.TryParse(DiasPurgaTexto, out int dias) || dias <= 0)
        {
            MensajeResultadoPurga = "Por favor ingrese un número válido de días.";
            return;
        }

        if (VentasAPurgar == 0)
        {
            MensajeResultadoPurga = $"No existen ventas con más de {dias} días de antigüedad para eliminar.";
            return;
        }

        MensajeErrorPin = string.Empty;
        PinIngresado = string.Empty;
        OnPropertyChanged(nameof(PinEnmascarado));

        _accionPendientePostPin = async () =>
        {
            try
            {
                var eliminadas = await _purgaService.PurgarVentasAntiguasAsync(dias);
                MensajeResultadoPurga = $"Operación exitosa: Se eliminaron {eliminadas} ventas físicamente.";
                _dialogoService.NotificarAdvertencia($"Se purgaron {eliminadas} ventas de la base de datos.", "Purga Completada");
                await ActualizarConteoVentasAPurgarAsync();
            }
            catch (Exception ex)
            {
                MensajeResultadoPurga = $"Error al purgar ventas: {ex.Message}";
                _dialogoService.MostrarError($"Error durante el borrado de ventas:\n\n{ex.Message}", "Fallo de Purga");
            }
        };

        MostrarModalPin = true;
    }
    #endregion

    #region Modal PIN Táctil
    [RelayCommand]
    private void AgregarDigitoPin(string digito)
    {
        if (PinIngresado.Length < 8)
        {
            PinIngresado += digito;
            MensajeErrorPin = string.Empty;
            OnPropertyChanged(nameof(PinEnmascarado));
        }
    }

    [RelayCommand]
    private void BorrarDigitoPin()
    {
        if (PinIngresado.Length > 0)
        {
            PinIngresado = PinIngresado[..^1];
            OnPropertyChanged(nameof(PinEnmascarado));
        }
    }

    [RelayCommand]
    private void LimpiarPin()
    {
        PinIngresado = string.Empty;
        OnPropertyChanged(nameof(PinEnmascarado));
    }

    [RelayCommand]
    private async Task ConfirmarPin()
    {
        if (string.IsNullOrWhiteSpace(PinIngresado))
        {
            MensajeErrorPin = "Por favor ingrese el PIN.";
            return;
        }

        bool esValido = await _seguridadService.ValidarPinAsync(PinIngresado.Trim());
        if (esValido)
        {
            MostrarModalPin = false;
            PinIngresado = string.Empty;
            OnPropertyChanged(nameof(PinEnmascarado));
            MensajeErrorPin = string.Empty;

            var accion = _accionPendientePostPin;
            _accionPendientePostPin = null;

            if (accion is not null)
            {
                await accion();
            }
        }
        else
        {
            MensajeErrorPin = "PIN incorrecto. Intente de nuevo.";
            PinIngresado = string.Empty;
            OnPropertyChanged(nameof(PinEnmascarado));
        }
    }

    [RelayCommand]
    private void CancelarPin()
    {
        MostrarModalPin = false;
        PinIngresado = string.Empty;
        OnPropertyChanged(nameof(PinEnmascarado));
        MensajeErrorPin = string.Empty;
        _accionPendientePostPin = null;
    }
    #endregion

    #region Hardware Impresoras y Cajon
    public void RefrescarImpresoras()
    {
        ImpresorasDisponibles.Clear();
        var lista = _printerService.ObtenerImpresorasInstaladas();
        foreach (var item in lista)
        {
            ImpresorasDisponibles.Add(item);
        }
    }

    private async Task CargarConfiguracionImpresoraAsync()
    {
        try
        {
            ImpresoraSeleccionada = await _configuracionRepository.ObtenerValorAsync("Impresora_NombreTicket");
            ImpresoraComandaSeleccionada = await _configuracionRepository.ObtenerValorAsync("Impresora_NombreComanda");

            string ancho = await _configuracionRepository.ObtenerValorAsync("Impresora_AnchoPapel") ?? "58mm";
            EsPapel80mm = ancho == "80mm";
            EsPapel58mm = !EsPapel80mm;

            string abrirCajon = await _configuracionRepository.ObtenerValorAsync("Impresora_AbrirCajon") ?? "true";
            AbrirCajonAutomatico = abrirCajon != "false";

            string cortarPapel = await _configuracionRepository.ObtenerValorAsync("Impresora_CortarPapel") ?? "true";
            CortarPapelAutomatico = cortarPapel != "false";
        }
        catch
        {
            EsPapel58mm = true;
            AbrirCajonAutomatico = true;
            CortarPapelAutomatico = true;
        }
    }

    [RelayCommand]
    private async Task GuardarConfiguracionImpresora()
    {
        try
        {
            await _configuracionRepository.GuardarValorAsync("Impresora_NombreTicket", ImpresoraSeleccionada ?? string.Empty);
            await _configuracionRepository.GuardarValorAsync("Impresora_NombreComanda", ImpresoraComandaSeleccionada ?? string.Empty);
            await _configuracionRepository.GuardarValorAsync("Impresora_AnchoPapel", EsPapel80mm ? "80mm" : "58mm");
            await _configuracionRepository.GuardarValorAsync("Impresora_AbrirCajon", AbrirCajonAutomatico ? "true" : "false");
            await _configuracionRepository.GuardarValorAsync("Impresora_CortarPapel", CortarPapelAutomatico ? "true" : "false");

            MensajeResultadoImpresora = "Configuración de hardware guardada correctamente.";
            TieneErrorImpresora = false;
            _dialogoService.NotificarExito("Configuración de impresora y cajón guardada.", "Hardware Guardado", 3);
        }
        catch (Exception ex)
        {
            MensajeResultadoImpresora = $"Error al guardar: {ex.Message}";
            TieneErrorImpresora = true;
            _dialogoService.MostrarError($"Error al guardar configuración: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private async Task ProbarImpresion()
    {
        if (string.IsNullOrWhiteSpace(ImpresoraSeleccionada))
        {
            MensajeResultadoImpresora = "Debes seleccionar una impresora en la lista primero.";
            TieneErrorImpresora = true;
            _dialogoService.MostrarAdvertencia("Selecciona una impresora térmica antes de imprimir.", "Aviso");
            return;
        }

        try
        {
            await _printerService.ImprimirTicketPruebaAsync(ImpresoraSeleccionada, EsPapel80mm ? "80mm" : "58mm");
            MensajeResultadoImpresora = "Ticket de prueba enviado exitosamente.";
            TieneErrorImpresora = false;
            _dialogoService.NotificarExito("Ticket de prueba enviado al spooler.", "Éxito");
        }
        catch (Exception ex)
        {
            MensajeResultadoImpresora = $"Error: {ex.Message}";
            TieneErrorImpresora = true;
            _dialogoService.MostrarError(
                $"Error al imprimir ticket de prueba:\n\n{ex.Message}\n\n(Verifique que la impresora esté encendida y conectada)",
                "Fallo de Hardware");
        }
    }

    [RelayCommand]
    private async Task ProbarAperturaCajon()
    {
        try
        {
            await _printerService.AbrirCajonDineroAsync();
            MensajeResultadoImpresora = "Señal de apertura enviada al cajón.";
            TieneErrorImpresora = false;
            _dialogoService.NotificarExito("Señal enviada al cajón de dinero.", "Cajón");
        }
        catch (Exception ex)
        {
            MensajeResultadoImpresora = $"Error: {ex.Message}";
            TieneErrorImpresora = true;
            _dialogoService.MostrarError(
                $"Error al intentar abrir el cajón de dinero:\n\n{ex.Message}\n\n(Verifique que la impresora térmica esté conectada)",
                "Fallo de Hardware");
        }
    }
    #endregion

    #region Comandos - Actualizaciones del Sistema
    [RelayCommand]
    private async Task BuscarActualizaciones()
    {
        if (EstaBuscandoActualizacion || EstaDescargandoActualizacion) return;

        try
        {
            EstaBuscandoActualizacion = true;
            TieneErrorActualizacion = false;
            ActualizacionLista = false;
            ProgresoActualizacion = 0;
            MensajeActualizacion = "Verificando si existen actualizaciones disponibles...";

            bool hayActualizacion = await _actualizadorService.HayActualizacionDisponibleAsync();
            if (!hayActualizacion)
            {
                EstaBuscandoActualizacion = false;
                ProgresoActualizacion = 100;
                MensajeActualizacion = "El sistema se encuentra en la versión más reciente.";
                _dialogoService.NotificarInformacion("El sistema se encuentra actualizado a la versión más reciente.", "Sistema al Día");
                return;
            }

            VersionNuevaDisponible = await _actualizadorService.ObtenerNuevaVersionAsync();
            EstaBuscandoActualizacion = false;
            EstaDescargandoActualizacion = true;
            MensajeActualizacion = $"Descargando actualización ({VersionNuevaDisponible ?? "Nueva versión"})...";

            await _actualizadorService.DescargarActualizacionAsync(p =>
            {
                ProgresoActualizacion = p;
                MensajeActualizacion = $"Descargando actualización ({p}%)...";
            });

            EstaDescargandoActualizacion = false;
            ActualizacionLista = true;
            ProgresoActualizacion = 100;
            MensajeActualizacion = $"Actualización {VersionNuevaDisponible ?? "lista"} descargada con éxito. Presione 'Reiniciar y Aplicar' para finalizar.";
            _dialogoService.NotificarExito("Actualización descargada y lista para instalar.", "Actualización Lista");
        }
        catch (Exception ex)
        {
            EstaBuscandoActualizacion = false;
            EstaDescargandoActualizacion = false;
            TieneErrorActualizacion = true;
            MensajeActualizacion = $"Error al buscar actualización: {ex.Message}";
            _dialogoService.MostrarError($"No se pudo comprobar la actualización:\n\n{ex.Message}", "Error de Actualización");
        }
    }

    [RelayCommand]
    private void ReiniciarYAplicar()
    {
        try
        {
            _actualizadorService.AplicarActualizacionYReiniciar();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarError($"Error al reiniciar la aplicación:\n\n{ex.Message}", "Error de Reinicio");
        }
    }
    #endregion
}
