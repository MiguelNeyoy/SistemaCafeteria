using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Core.Application.Interfaces.Services;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IExtraService _extraService;
    private readonly ISeguridadService _seguridadService;
    private readonly IPurgaService _purgaService;

    [ObservableProperty]
    private object? _vistaActual;

    [ObservableProperty]
    private string? _botonSeleccionado;

    public MainViewModel(
        IProductoService productoService,
        ICategoriaService categoriaService,
        IExtraService extraService,
        ISeguridadService seguridadService,
        IPurgaService purgaService)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;
    }


    [RelayCommand]
    private async Task ShowMenu()
    {
        var viewModel = new MenuViewModel( _categoriaService, _productoService );

        viewModel.CategoriaSeleccionada += async categoria =>
        {
            var comandaViewModel = new ComandaViewModel(_productoService, categoria);

            await comandaViewModel.CargarProductosAsync();

            VistaActual = comandaViewModel;
        };

        await viewModel.CargarCategoriasAsync();

        VistaActual = viewModel;

        BotonSeleccionado = "Menu";
    }

    [RelayCommand]
    private async Task ShowConfigMenu()
    {
        var viewModel = new ConfiguracionMenuViewModel(
            _categoriaService,
            _productoService,
            _extraService,
            _seguridadService,
            _purgaService);

        await viewModel.CargarDatosAsync();

        VistaActual = viewModel;

        BotonSeleccionado = "Configuracion";
    }

    [RelayCommand]
    private void ShowCierreDeCaja()
    {
        VistaActual = new CierreDeCajaViewModel();

        BotonSeleccionado = "CierreDeCaja";
    }
}