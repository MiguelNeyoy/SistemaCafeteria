using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Core.Application.Interfaces.Services;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly ICategoriaService _categoriaService;

    [ObservableProperty]
    private object? _vistaActual;

    [ObservableProperty]
    private string? _botonSeleccionado;

    public MainViewModel( IProductoService productoService, ICategoriaService categoriaService )
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
    }


    [RelayCommand]
    private async Task ShowMenu()
    {
        var viewModel = new MenuViewModel( _categoriaService, _productoService );

        await viewModel.CargarCategoriasAsync();

        VistaActual = viewModel;

        BotonSeleccionado = "Menu";
    }

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