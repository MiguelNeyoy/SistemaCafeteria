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


    private MenuViewModel? _menuViewModel;
    private ComandaViewModel? _comandaViewModel;

    private ConfiguracionMenuViewModel? _configuracionMenuViewModel;

    private bool HayOrdenEnProceso => _comandaViewModel is not null && _comandaViewModel.ItemsComanda.Count > 0;


    [ObservableProperty]
    private object? _vistaActual;

    [ObservableProperty]
    private string? _botonSeleccionado;

    public MainViewModel(IProductoService productoService, ICategoriaService categoriaService, IExtraService extraService, ISeguridadService seguridadService, IPurgaService purgaService, IDialogoService dialogoService)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _dialogoService = dialogoService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;

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
            _comandaViewModel = new ComandaViewModel(_productoService, categoria);

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
}