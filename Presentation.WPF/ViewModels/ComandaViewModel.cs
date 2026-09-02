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

    [ObservableProperty]
    private ComandaItemViewModel? itemSeleccionado;

    public decimal TotalComanda => ItemsComanda.Sum(item => item.Subtotal);

    [ObservableProperty]
    private CategoriaDto? categoriaSeleccionada;

    public event Action? RegresarACategorias;


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


    public async Task CambiarCategoriaAsync( CategoriaDto categoria )
    {
        CategoriaSeleccionada = categoria;

        await CargarProductosAsync();

    }//Fin - CambiarCategoriaAsync


    private void ActualizarTotal()
    {
        OnPropertyChanged(nameof(TotalComanda));
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

        ActualizarTotal();

    }//Fin - AgregarProducto


    [RelayCommand]
    private void MostrarOcultarComentario()
    {

        if (ItemSeleccionado is null)
            return;

        ItemSeleccionado.MostrarComentario = !ItemSeleccionado.MostrarComentario;

    }//Fin - MostrarOcultarComentario


    [RelayCommand]
    private void DisminuirCantidad( ComandaItemViewModel item)
    {
        if (item.Cantidad <= 1)
            return;

        item.Cantidad--;

        ActualizarTotal( );

    }//Fin - DisminuirCantidad


    [RelayCommand]
    private void EliminarProducto( ComandaItemViewModel item)
    {
        ItemsComanda.Remove( item );

        ActualizarTotal() ;

    }//Fin - EliminarProducto


    [RelayCommand]
    private void LimpiarComanda()
    {
        ItemsComanda.Clear();

        ActualizarTotal( ) ;

    }//Fin - LimpiarComanda


    [RelayCommand]
    private void Regresar()
    {
        RegresarACategorias?.Invoke();

    }//Fin - Regresar

}


public partial class ComandaItemViewModel : ObservableObject
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor( nameof( Subtotal ) ) ]
    private int cantidad = 1;


    [ObservableProperty]
    [NotifyPropertyChangedFor( nameof( TieneNota ) ) ]
    private string? nota;


    [ObservableProperty]
    private bool mostrarComentario;


    public decimal Subtotal => Precio * Cantidad;
    public bool TieneNota => !string.IsNullOrWhiteSpace(Nota);

}//Fin - ComandaItemViewModel