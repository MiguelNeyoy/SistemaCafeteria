using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;

namespace Presentation.WPF.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    public ObservableCollection<CategoriaItem> Categorias { get; } = new();

    private readonly ICategoriaService _categoriaService;

    [ObservableProperty]
    private bool tieneCategorias;


    public MenuViewModel( ICategoriaService categoriaService ) 
    {
        _categoriaService = categoriaService;

        Categorias.CollectionChanged += (_, _) =>
        {
            TieneCategorias = Categorias.Count > 0;
        };
    }
    
}//Fin - MenuViewModel


public class CategoriaItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
}


public async Task CargarCategoriasAsync()
    {
        var categorias = await _categoriaService.ObtenerActivasAsync();

        Categorias.Clear();

        foreach (var categoria in categorias)
        {
            Categorias.Add(new CategoriaItem
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre
            });
        }
    }