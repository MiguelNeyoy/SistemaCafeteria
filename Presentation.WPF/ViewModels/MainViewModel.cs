using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
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

    private MenuViewModel? _menuViewModel;
    private ComandaViewModel? _comandaViewModel;
    private ConfiguracionMenuViewModel? _configuracionMenuViewModel;
    private CuentasAbiertasViewModel? _cuentasAbiertasViewModel;
    private FinalizarCompraViewModel? _finalizarCompraViewModel;

    private bool HayOrdenEnProceso => _comandaViewModel is not null && _comandaViewModel.ItemsComanda.Count > 0;

    [ObservableProperty]
    private object? _vistaActual;

    [ObservableProperty]
    private string? _botonSeleccionado;

    public MainViewModel(
        IProductoService productoService, 
        ICategoriaService categoriaService, 
        IExtraService extraService, 
        ISeguridadService seguridadService, 
        IPurgaService purgaService, 
        IDialogoService dialogoService,
        IVentaService ventaService,
        IComandaService comandaService)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _dialogoService = dialogoService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;
        _ventaService = ventaService;
        _comandaService = comandaService;

    }//Fin - MainViewModel


    private bool ConfirmarSalidaDeComanda()
    {
        if (!HayOrdenEnProceso)
            return true;

        return _dialogoService.Confirmar(
            "Tienes una comanda en proceso.\n\n" +
            "Si sales ahora, perderas los productos que has agregado.\n\n" +
            "¿Deseas salir de la comanda?",
            "Orden en proceso");

    }//Fin - ConfirmarSalidaDeComanda


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
                _purgaService);

            _configuracionMenuViewModel.ConfiguracionAvanzadaSolicitada += MostrarConfiguracionAvanzada;
        }


        await _configuracionMenuViewModel.CargarDatosAsync();

        VistaActual = _configuracionMenuViewModel;

        BotonSeleccionado = "Configuracion";
    }


    private void MostrarConfiguracionAvanzada()
    {
        if (_configuracionMenuViewModel is null) return;

        var vista = new ConfiguracionAvanzadaView
        {
            DataContext = _configuracionMenuViewModel
        };

        VistaActual = vista;

    }//Fin - MostrarConfiguracionAvanzadaView


    [RelayCommand]
    private void ShowCierreDeCaja()
    {
        VistaActual = new CierreDeCajaViewModel();

        BotonSeleccionado = "CierreDeCaja";
    }

    [RelayCommand]
    private async Task ShowCuentasAbiertas()
    {
        if (!ConfirmarSalidaDeComanda())
            return;

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
            _finalizarCompraViewModel = new FinalizarCompraViewModel(_ventaService, _dialogoService);
            _finalizarCompraViewModel.CobroFinalizado += OnCobroFinalizado;
            _finalizarCompraViewModel.RegresarSolicitado += OnRegresarDeCobro;
        }

        await _finalizarCompraViewModel.CargarVentaAsync(ventaId);
        VistaActual = _finalizarCompraViewModel;
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