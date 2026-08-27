using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Windows;

namespace Presentation.WPF.ViewModels;

public partial class ConfiguracionMenuViewModel : ObservableObject
{
    private readonly ICategoriaService _categoriaService;
    private readonly IProductoService _productoService;

    public ObservableCollection<CategoriaDto> Categorias { get; } = new();
    public ObservableCollection<ProductoDto> Productos { get; } = new();

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


    public ConfiguracionMenuViewModel(
        ICategoriaService categoriaService,
        IProductoService productoService)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;
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

    [RelayCommand]
    private void MostrarFormularioCategoria()
    {
        MostrarFormulario = true;
        FormularioCategoria = true;
        FormularioProducto = false;
    }

    [RelayCommand]
    private async Task GuardarCategoria()
    {

        var dtoCategoria = new CrearCategoriaDto
        {
            Nombre = NombreCategoria
        };

        var categoria = await _categoriaService.CrearAsync( dtoCategoria );

        Categorias.Add( categoria );

        NombreCategoria = string.Empty;

        MostrarFormulario = false;
        FormularioCategoria = false;

    }

    [RelayCommand]
    private void MostrarFormularioProducto()
    {
        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
    }

    [RelayCommand]
    private async Task GuardarProducto()
    {

        if ( CategoriaProductoSeleccionado == null )
            return;


        var dtoProducto = new CrearProductoDto
        {
            Nombre = NombreProducto,
            Precio = PrecioProducto,
            CategoriaId = CategoriaProductoSeleccionado.Id
        };


        var producto = await _productoService.CrearAsync( dtoProducto );

        Productos.Add( producto );

        NombreProducto = string.Empty;
        PrecioProducto = 0;
        CategoriaProductoSeleccionado = null;

        MostrarFormulario = false;
        FormularioProducto = false;

    }

}