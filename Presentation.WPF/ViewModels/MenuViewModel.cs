using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    public ObservableCollection<CategoriaItem> Categorias { get; } = new();

    [ObservableProperty]
    private bool tieneCategorias;


    public MenuViewModel() {

        Categorias.CollectionChanged += (_, _) =>
        {
            TieneCategorias = Categorias.Count > 0;
        };
    }
    
}//Fin - MenuViewModel


public class CategoriaItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
}