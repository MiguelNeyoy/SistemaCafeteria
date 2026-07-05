using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Presentation.WPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private string _bienvenidoMessage = "Bienvenido al Sistema de Cafetería";

    [RelayCommand]
    private void CargarDatos()
    {
        BienvenidoMessage = $"Datos cargados: {DateTime.Now:T}";
    }
}
