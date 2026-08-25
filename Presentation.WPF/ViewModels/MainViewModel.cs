using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Core.Application.Interfaces.Services;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductoService _productoService;

    [ObservableProperty]
    private object _vistaActual;

    [ObservableProperty]
    private string _botonSeleccionado;

    public MainViewModel( IProductoService productoService)
    {
        _productoService = productoService;
    }


    [RelayCommand]
    private void ShowMenu()
    {
        VistaActual = new MenuViewModel();

        BotonSeleccionado = "Menu";
    }

    [RelayCommand]
    private void ShowConfigMenu()
    {
        VistaActual = new ConfiguracionMenuViewModel();

        BotonSeleccionado = "Configuracion";
    }

    [RelayCommand]
    private void ShowCierreDeCaja()
    {
        VistaActual = new CierreDeCajaViewModel();

        BotonSeleccionado = "CierreDeCaja";
    }
}