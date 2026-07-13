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

    public MenuViewModel() {

        Categorias = new ObservableCollection<CategoriaItem>();

        CategoriaItem Categoria = new CategoriaItem();

        Categoria.Nombre = string.Empty;
        Categoria.Imagen = string.Empty;

        Categorias.Add(Categoria);

        TieneCategorias = Categorias.Count > 0;
    }
}

public class CategoriaItem
{
    public string Nombre { get; set; }
    public string Imagen { get; set; }
}