using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using System.Collections.ObjectModel;

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
    private bool mostrarFormularioCategoria;

    [ObservableProperty]
    private bool mostrarFormularioProducto;
    

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
}