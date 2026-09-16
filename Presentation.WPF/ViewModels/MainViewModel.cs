using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Presentation.WPF.Services;
using Presentation.WPF.Views;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IDialogoService _dialogoService;
    private readonly IExtraService _extraService;
    private readonly ISeguridadService _seguridadService;
    private readonly IPurgaService _purgaService;
    private readonly IVentaService _ventaService;
    private readonly IComandaService _comandaService;
    private readonly IPrinterService _printerService;
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly ITicketService _ticketService;
    private readonly ICorteCajaService _corteCajaService;

    private readonly DashboardViewModel _dashboardViewModel;
    private readonly ConfiguracionAvanzadaViewModel _configuracionAvanzadaViewModel;
    private MenuViewModel? _menuViewModel;
    private ComandaViewModel? _comandaViewModel;
    private ConfiguracionMenuViewModel? _configuracionMenuViewModel;
    private CuentasAbiertasViewModel? _cuentasAbiertasViewModel;
    private FinalizarCompraViewModel? _finalizarCompraViewModel;
    private CierreDeCajaViewModel? _cierreDeCajaViewModel;


    [ObservableProperty]
    private object? _vistaActual;

    [ObservableProperty]
    private string? _botonSeleccionado;

    public bool EsMenuSeleccionado => BotonSeleccionado == "Menu";
    public bool EsConfigSeleccionado => BotonSeleccionado == "Configuracion";
    public bool EsCierreSeleccionado => BotonSeleccionado == "CierreDeCaja";
    public bool EsCuentasSeleccionado => BotonSeleccionado == "CuentasAbiertas";

    partial void OnBotonSeleccionadoChanged(string? value)
    {
        OnPropertyChanged(nameof(EsMenuSeleccionado));
        OnPropertyChanged(nameof(EsConfigSeleccionado));
        OnPropertyChanged(nameof(EsCierreSeleccionado));
        OnPropertyChanged(nameof(EsCuentasSeleccionado));
    }

    public MainViewModel(
        IProductoService productoService, 
        ICategoriaService categoriaService, 
        IExtraService extraService, 
        ISeguridadService seguridadService, 
        IPurgaService purgaService, 
        IDialogoService dialogoService,
        IVentaService ventaService,
        IComandaService comandaService,
        DashboardViewModel dashboardViewModel,
        ConfiguracionAvanzadaViewModel configuracionAvanzadaViewModel,
        IPrinterService printerService,
        IConfiguracionRepository configuracionRepository,
        ITicketService ticketService,
        ICorteCajaService corteCajaService)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _dialogoService = dialogoService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;
        _ventaService = ventaService;
        _comandaService = comandaService;
        _dashboardViewModel = dashboardViewModel;
        _configuracionAvanzadaViewModel = configuracionAvanzadaViewModel;
        _printerService = printerService;
        _configuracionRepository = configuracionRepository;
        _ticketService = ticketService;
        _corteCajaService = corteCajaService;

        // Conectar eventos de navegacion para Configuracion Avanzada
        _dashboardViewModel.ConfiguracionAvanzadaSolicitada += MostrarConfiguracionAvanzada;
        _configuracionAvanzadaViewModel.RegresarSolicitado += async () => await ShowHome();

        // Establecer el Dashboard de Inicio como vista inicial al arrancar
        VistaActual = _dashboardViewModel;
        BotonSeleccionado = "Home";
        _ = _dashboardViewModel.CargarDatosCommand.ExecuteAsync(null);

    }//Fin - MainViewModel


    [RelayCommand]
    private async Task ShowHome()
    {
        await _dashboardViewModel.CargarDatosCommand.ExecuteAsync(null);

        VistaActual = _dashboardViewModel;

        BotonSeleccionado = "Home";
    }


    [RelayCommand]
    private async Task ShowMenu()
    {
        if (_menuViewModel is null)
        {
            _menuViewModel = new MenuViewModel(_categoriaService, _productoService);

            _menuViewModel.CategoriaSeleccionada += SeleccionarCategoria;
        }


        await _menuViewModel.CargarCategoriasAsync();

        VistaActual = _menuViewModel;

        BotonSeleccionado = "Menu";

    }//Fin - ShowMenu


    private async void SeleccionarCategoria(CategoriaDto categoria)
    {

        if (_comandaViewModel is null)
        {
            _comandaViewModel = new ComandaViewModel(
                _productoService, 
                _extraService, 
                _ventaService, 
                _comandaService, 
                _dialogoService, 
                _printerService,
                categoria);

            _comandaViewModel.RegresarACategorias += RegresarACategorias;

            await _comandaViewModel.CargarProductosAsync();

        }
        else
        {
            await _comandaViewModel.CambiarCategoriaAsync(categoria);
        }

        VistaActual = _comandaViewModel;

    }//Fin - SeleccionarCategoria


    private void RegresarACategorias()
    {

        if (_menuViewModel is null)
            return;

        VistaActual = _menuViewModel;

        BotonSeleccionado = "Menu";

    }//Fin - RegresarACategorias


    [RelayCommand]
    private async Task ShowConfigMenu()
    {
        if (_configuracionMenuViewModel is null)
        {
            _configuracionMenuViewModel = new ConfiguracionMenuViewModel(
                _categoriaService,
                _productoService,
                _extraService,
                _seguridadService,
                _purgaService,
                _dialogoService,
                _printerService,
                _configuracionRepository);

            _configuracionMenuViewModel.ConfiguracionAvanzadaSolicitada += MostrarConfiguracionAvanzada;
        }


        await _configuracionMenuViewModel.CargarDatosAsync();

        VistaActual = _configuracionMenuViewModel;

        BotonSeleccionado = "Configuracion";
    }


    private async void MostrarConfiguracionAvanzada()
    {
        await _configuracionAvanzadaViewModel.CargarDatosAsync();
        VistaActual = _configuracionAvanzadaViewModel;
        BotonSeleccionado = null;
    }//Fin - MostrarConfiguracionAvanzada


    [RelayCommand]
    private async Task ShowCierreDeCaja()
    {
        if (_cierreDeCajaViewModel is null)
        {
            _cierreDeCajaViewModel = new CierreDeCajaViewModel(
                _corteCajaService,
                _printerService,
                _dialogoService,
                _ventaService);
        }

        await _cierreDeCajaViewModel.CargarDatosAsync();

        VistaActual = _cierreDeCajaViewModel;
        BotonSeleccionado = "CierreDeCaja";
    }

    [RelayCommand]
    private async Task ShowCuentasAbiertas()
    {
        if (_cuentasAbiertasViewModel is null)
        {
            _cuentasAbiertasViewModel = new CuentasAbiertasViewModel(_ventaService, _dialogoService);
            _cuentasAbiertasViewModel.CobrarCuentaSolicitado += IniciarCobroCuenta;
        }

        await _cuentasAbiertasViewModel.CargarCuentasAsync();

        VistaActual = _cuentasAbiertasViewModel;

        BotonSeleccionado = "CuentasAbiertas";
    }

    private async void IniciarCobroCuenta(int ventaId)
    {
        if (_finalizarCompraViewModel is null)
        {
            _finalizarCompraViewModel = new FinalizarCompraViewModel(
                _ventaService, 
                _dialogoService, 
                _printerService, 
                _ticketService);
            _finalizarCompraViewModel.CobroFinalizado += OnCobroFinalizado;
            _finalizarCompraViewModel.RegresarSolicitado += OnRegresarDeCobro;
        }

        await _finalizarCompraViewModel.CargarVentaAsync(ventaId);
        VistaActual = _finalizarCompraViewModel;
    }

    [RelayCommand]
    private async Task AbrirCajon()
    {
        try
        {
            await _printerService.AbrirCajonDineroAsync();
            _dialogoService.NotificarExito("Señal de apertura enviada al cajón de dinero.", "Cajón", 2);
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje(
                $"No se pudo abrir el cajón de dinero:\n\n{ex.Message}\n\n(Verifique que la impresora térmica esté configurada en Configuración Avanzada y conectada por USB)",
                "Hardware No Detectado");
        }
    }

    private async void OnCobroFinalizado()
    {
        if (_cuentasAbiertasViewModel is not null)
        {
            await _cuentasAbiertasViewModel.CargarCuentasAsync();
            VistaActual = _cuentasAbiertasViewModel;
            BotonSeleccionado = "CuentasAbiertas";
        }
    }

    private void OnRegresarDeCobro()
    {
        if (_cuentasAbiertasViewModel is not null)
        {
            VistaActual = _cuentasAbiertasViewModel;
            BotonSeleccionado = "CuentasAbiertas";
        }
    }
}