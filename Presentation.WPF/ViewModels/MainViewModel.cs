using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object _vistaActual;

    [ObservableProperty]
    private string _botonSeleccionado;

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
}