using CommunityToolkit.Mvvm.ComponentModel;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

public partial class ComandaViewModel : ObservableObject
{
    private readonly IProductoService _productoService;

    public ObservableCollection<ProductoDto> ProductoCategoriaSeleccionada { get; } = new();

    [ObservableProperty]
    private CategoriaDto? categoriaSeleccionada;


    public ComandaViewModel( IProductoService productoService, CategoriaDto categoria )
    {
        _productoService = productoService;
        CategoriaSeleccionada = categoria;

    }//Fin - ComandaViewModel


    public async Task CargarProductosAsync()
    {
        if ( CategoriaSeleccionada is null )
            return;

        var productos = await _productoService.ObtenerPorCategoriaAsync( CategoriaSeleccionada.Id );

        ProductoCategoriaSeleccionada.Clear();

        foreach ( var producto in productos )
        {
            ProductoCategoriaSeleccionada.Add( producto );
        }

    }//Fin - CargarProductosAsync

}