using CommunityToolkit.Mvvm.ComponentModel;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Drawing.Printing;

namespace Presentation.WPF.ViewModels;

public partial class ComandaViewModel : ObservableObject
{
    private readonly IProductoService _productoService;

    public ObservableCollection<ProductoDto> ProductoCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ComandaItemViewModel> ItemsComanda { get; } = new(); 

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


public partial class ComandaItemViewModel : ObservableObject
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    [ObservableProperty]
    private int cantidad = 1;

   public decimal Subtotal => Precio * Cantidad;

}//Fin - ComandaItemViewModel