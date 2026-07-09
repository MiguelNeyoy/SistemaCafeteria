using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CategoriaItem> _categorias;

    [ObservableProperty]
    private bool _tieneCategorias;
}

