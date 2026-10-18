using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;

namespace Presentation.WPF.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    public ObservableCollection<CategoriaItem> Categorias { get; } = new();
    public ObservableCollection<ProductoDto> ProductosBusqueda { get; } = new();

    private readonly ICategoriaService _categoriaService;
    private readonly IProductoService _productoService;
    private int _versionBusqueda;

    public event Action<CategoriaDto>? CategoriaSeleccionada;
    public event Action<ProductoDto>? ProductoSeleccionadoDesdeBusqueda;

    [ObservableProperty]
    private bool tieneCategorias;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneTextoBusqueda))]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private bool sinResultadosBusqueda;

    public bool TieneTextoBusqueda => !string.IsNullOrWhiteSpace(TextoBusqueda);

    public string MensajeResultadosBusqueda => TieneTextoBusqueda
        ? $"Resultados de búsqueda ({ProductosBusqueda.Count})"
        : string.Empty;

    partial void OnTextoBusquedaChanged(string value)
    {
        _ = EjecutarBusquedaReactivaAsync(value);
    }

    public MenuViewModel(ICategoriaService categoriaService, IProductoService productoService)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;

        Categorias.CollectionChanged += (_, _) =>
        {
            TieneCategorias = Categorias.Count > 0;
        };
    }

    public async Task CargarCategoriasAsync()
    {
        _versionBusqueda++;
        textoBusqueda = string.Empty;
        OnPropertyChanged(nameof(TextoBusqueda));
        OnPropertyChanged(nameof(TieneTextoBusqueda));
        ProductosBusqueda.Clear();
        SinResultadosBusqueda = false;
        OnPropertyChanged(nameof(MensajeResultadosBusqueda));

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

    public async Task EjecutarBusquedaReactivaAsync(string? termino)
    {
        int versionActual = ++_versionBusqueda;

        if (string.IsNullOrWhiteSpace(termino))
        {
            ProductosBusqueda.Clear();
            SinResultadosBusqueda = false;
            OnPropertyChanged(nameof(MensajeResultadosBusqueda));
            return;
        }

        var resultados = await _productoService.BuscarActivosAsync(termino);
        if (versionActual != _versionBusqueda)
        {
            return;
        }

        ProductosBusqueda.Clear();
        foreach (var prod in resultados)
        {
            ProductosBusqueda.Add(prod);
        }

        SinResultadosBusqueda = ProductosBusqueda.Count == 0;
        OnPropertyChanged(nameof(MensajeResultadosBusqueda));
    }

    [RelayCommand]
    private void LimpiarBusqueda()
    {
        TextoBusqueda = string.Empty;
    }

    [RelayCommand]
    private void SeleccionarProductoBusqueda(ProductoDto? producto)
    {
        if (producto is null)
        {
            return;
        }

        TextoBusqueda = string.Empty;
        ProductoSeleccionadoDesdeBusqueda?.Invoke(producto);
    }

    [RelayCommand]
    private void SeleccionarPrimerResultadoBusqueda()
    {
        if (ProductosBusqueda.Count == 0)
        {
            return;
        }

        SeleccionarProductoBusqueda(ProductosBusqueda[0]);
    }

    [RelayCommand]
    private void SeleccionarCategoria(CategoriaItem categoria)
    {
        var categoriaDto = new CategoriaDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Activo = true
        };

        CategoriaSeleccionada?.Invoke(categoriaDto);
    }
}

public class CategoriaItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
}