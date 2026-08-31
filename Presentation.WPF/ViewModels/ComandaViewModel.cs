using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Drawing.Printing;

namespace Presentation.WPF.ViewModels;

public partial class ComandaViewModel : ObservableObject
{
    private readonly IProductoService _productoService;

    public ObservableCollection<ProductoDto> ProductoCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ComandaItemViewModel> ItemsComanda { get; } = new();
    public decimal TotalComanda => ItemsComanda.Sum(item => item.Subtotal);

    [ObservableProperty]
    private CategoriaDto? categoriaSeleccionada;

    [ObservableProperty]
    private decimal totalComanda;


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

    private void ActualizarTotal()
    {
        TotalComanda = ItemsComanda.Sum(item => item.Subtotal);
    }


    [RelayCommand]
    private void AgregarProducto( ProductoDto producto)
    {
        var itemExistente = ItemsComanda.FirstOrDefault( item => item.ProductoId == producto.Id );

        if( itemExistente is not null)
        {
            itemExistente.Cantidad++;
            ActualizarTotal();
            return;
        }

        ItemsComanda.Add( new ComandaItemViewModel
        {
            ProductoId = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Cantidad = 1
        } );

    }//Fin - AgregarProducto

}


public partial class ComandaItemViewModel : ObservableObject
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor( nameof( Subtotal ) ) ]
    private int cantidad = 1;

   public decimal Subtotal => Precio * Cantidad;

}//Fin - ComandaItemViewModel