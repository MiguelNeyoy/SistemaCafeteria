using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;

namespace Presentation.WPF.ViewModels;

public partial class ConfiguracionMenuViewModel : ObservableObject
{
    private readonly ICategoriaService _categoriaService;
    private readonly IProductoService _productoService;

    public ObservableCollection<CategoriaDto> Categorias { get; } = new();
    public ObservableCollection<ProductoDto> Productos { get; } = new();
    public int TotalProductos => Productos.Count;
    public bool TieneCategorias => Categorias.Count > 0;
    public bool TieneProductos => Productos.Count > 0;


    [ObservableProperty]
    private CategoriaDto? categoriaSeleccionada;

    [ObservableProperty]
    private ProductoDto? productoSeleccionado;

    [ObservableProperty]
    private bool mostrarFormulario;

    [ObservableProperty]
    private bool formularioCategoria;

    [ObservableProperty]
    private bool formularioProducto;

    [ObservableProperty]
    private string nombreProducto = string.Empty;

    [ObservableProperty]
    private decimal precioProducto;

    [ObservableProperty]
    private string nombreCategoria = string.Empty;

    [ObservableProperty]
    private CategoriaDto? categoriaProductoSeleccionado;

    [ObservableProperty]
    private bool modoEdicionCategoria;

    public ConfiguracionMenuViewModel(
        ICategoriaService categoriaService,
        IProductoService productoService)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;

        Productos.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalProductos));
            OnPropertyChanged(nameof(TieneProductos));
        };

        Categorias.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TieneCategorias));
        };
    }

    public async Task CargarDatosAsync()
    {
        var categorias = await _categoriaService.ObtenerActivasAsync();
        var productos = await _productoService.ObtenerActivosAsync();

        Categorias.Clear();
        Productos.Clear();

        foreach (var categoria in categorias)
        {
            Categorias.Add(categoria);
        }

        foreach (var producto in productos)
        {
            Productos.Add(producto);
        }
    }

    private void LimpiarFormulario()
    {
        NombreCategoria = string.Empty;
        NombreProducto = string.Empty;
        PrecioProducto = 0;

        CategoriaProductoSeleccionado = null;

        CategoriaSeleccionada = null;
        ProductoSeleccionado = null;

        ModoEdicionCategoria = false;
    }

    [RelayCommand]
    private void MostrarFormularioCategoria()
    {
        LimpiarFormulario();

        MostrarFormulario = true;
        FormularioCategoria = true;
        FormularioProducto = false;
    }

    [RelayCommand]
    private async Task GuardarCategoria()
    {

        if (string.IsNullOrWhiteSpace(NombreCategoria))
            return;

        if (ModoEdicionCategoria)
        {
            if (CategoriaSeleccionada is null)
                return;

            var dtoCategoria = new EditarCategoriaDto
            {
                Id = CategoriaSeleccionada.Id,
                Nombre = NombreCategoria.Trim()
            };

            var categoriaEditada = await _categoriaService.EditarAsync(dtoCategoria);

            var categoriaEnLista = Categorias.FirstOrDefault(c => c.Id == categoriaEditada.Id);


            if (categoriaEnLista is not null)
            {
                categoriaEnLista.Nombre = categoriaEditada.Nombre;
            }


        }
        else
        {
            var dtoCategoria = new CrearCategoriaDto
            {
                Nombre = NombreCategoria.Trim()
            };


            var categoria = await _categoriaService.CrearAsync(dtoCategoria);

            Categorias.Add(categoria);
        }

        NombreCategoria = string.Empty;
        CategoriaSeleccionada = null;
        ModoEdicionCategoria = false;

        MostrarFormulario = false;
        FormularioCategoria = false;
    }

    [RelayCommand]
    private void EditarCategoria(CategoriaDto categoria)
    {
        CategoriaSeleccionada = categoria;

        NombreCategoria = categoria.Nombre;

        MostrarFormulario = true;
        FormularioCategoria = true;
        FormularioProducto = false;

        ModoEdicionCategoria = true;
    }

    [RelayCommand]
    private void MostrarFormularioProducto()
    {
        LimpiarFormulario();

        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
    }

    [RelayCommand]
    private async Task GuardarProducto()
    {
        if ( string.IsNullOrWhiteSpace( NombreProducto ) )
            return;

        if ( PrecioProducto <= 0 )
            return;


        if ( CategoriaProductoSeleccionado is null )
            return;


        if( ProductoSeleccionado is not null)
        {
            var dtoProducto = new EditarProductoDto
            {
                Id = ProductoSeleccionado.Id,
                Nombre = NombreProducto.Trim(),
                Precio = PrecioProducto,
                CategoriaId = CategoriaProductoSeleccionado.Id
            };

            var productoEditado = await _productoService.EditarAsync( dtoProducto );

            var productoEnLista = Productos.FirstOrDefault( p => p.Id == productoEditado.Id );

            if( productoEnLista is not null)
            {
                productoEnLista.Nombre = productoEditado.Nombre;
                productoEnLista.Precio = productoEditado.Precio;
                productoEnLista.CategoriaId = productoEditado.CategoriaId;
                productoEnLista.CategoriaNombre = productoEditado.CategoriaNombre;
            }
        }
        else
        {
            var dtoProducto = new CrearProductoDto
            {
                Nombre = NombreProducto.Trim(),
                Precio = PrecioProducto,
                CategoriaId = CategoriaProductoSeleccionado.Id
            };

            var producto = await _productoService.CrearAsync(dtoProducto);

            Productos.Add(producto);
        }


        NombreProducto = string.Empty;
        PrecioProducto = 0;
        CategoriaProductoSeleccionado = null;
        ProductoSeleccionado = null;

        MostrarFormulario = false;
        FormularioProducto = false;

    }

    [RelayCommand]
    private void EditarProducto( ProductoDto producto )
    {
        ProductoSeleccionado = producto;

        NombreProducto = producto.Nombre;
        PrecioProducto = producto.Precio;

        CategoriaProductoSeleccionado = Categorias.FirstOrDefault( c => c.Id == producto.CategoriaId );

        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
    }



    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;

        FormularioCategoria = false;
        FormularioProducto = false;

        NombreCategoria = string.Empty;

        NombreProducto = string.Empty;
        PrecioProducto = 0;
        CategoriaProductoSeleccionado = null;

        CategoriaSeleccionada = null;
        ProductoSeleccionado = null;

        ModoEdicionCategoria = false;
    }

}