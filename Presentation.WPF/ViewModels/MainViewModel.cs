using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object _vistaActual;

    [RelayCommand]
    private void ShowMenu()
    {
        VistaActual = new MenuViewModel();
    }

    [RelayCommand]
    private void ShowConfiguracionmenu()
    {
        VistaActual = new ConfiguracionMenuViewModel();
    }
}