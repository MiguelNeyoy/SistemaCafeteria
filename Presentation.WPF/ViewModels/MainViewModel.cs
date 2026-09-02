using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly ICategoriaService _categoriaService;

    private MenuViewModel? _menuViewModel;
    private ComandaViewModel? _comandaViewModel;

    [ObservableProperty]
    private object? _vistaActual;

    [ObservableProperty]
    private string? _botonSeleccionado;

    public MainViewModel( IProductoService productoService, ICategoriaService categoriaService )
    {
        _productoService = productoService;
        _categoriaService = categoriaService;

    }//Fin - MainViewModel


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


    private async void SeleccionarCategoria( CategoriaDto categoria )
    {

        if(_comandaViewModel is null)
        {
            _comandaViewModel = new ComandaViewModel( _productoService, categoria );

            _comandaViewModel.RegresarACategorias += RegresarACategorias;

            await _comandaViewModel.CargarProductosAsync();

        }
        else
        {
            await _comandaViewModel.CambiarCategoriaAsync( categoria );
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
        var viewModel = new ConfiguracionMenuViewModel( _categoriaService, _productoService );

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