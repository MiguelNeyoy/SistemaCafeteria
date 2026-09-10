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
    private readonly IExtraService _extraService;
    private readonly ISeguridadService _seguridadService;
    private readonly IPurgaService _purgaService;

    // Colecciones de Catálogo
    public ObservableCollection<CategoriaDto> Categorias { get; } = new();
    public ObservableCollection<ProductoDto> Productos { get; } = new();
    public ObservableCollection<ProductoDto> ProductosCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ExtraDto> Extras { get; } = new();
    public ObservableCollection<CategoriaCheckItem> CategoriasParaExtra { get; } = new();

    // Contadores y banderas para la interfaz
    public int TotalProductos => Productos.Count;
    public int TotalExtras => Extras.Count;
    public bool TieneCategorias => Categorias.Count > 0;
    public bool TieneProductos => Productos.Count > 0;
    public bool TieneCategoriaSeleccionada => CategoriaSeleccionada is not null;
    public bool TieneProductosCategoria => ProductosCategoriaSeleccionada.Count > 0;
    public bool TieneExtras => Extras.Count > 0;

    // Visibilidad de formularios en panel lateral derecho
    [ObservableProperty]
    private bool mostrarFormulario;


    [ObservableProperty]
    private bool formularioCategoria;

    [ObservableProperty]
    private bool formularioProducto;

    [ObservableProperty]
    private bool formularioExtra;

    [ObservableProperty]
    private bool formularioSeguridad;

    // Formulario Categoría
    [ObservableProperty]
    private CategoriaDto? categoriaSeleccionada;

    [ObservableProperty]
    private string nombreCategoria = string.Empty;

    [ObservableProperty]
    private bool modoEdicionCategoria;

    // Formulario Producto
    [ObservableProperty]
    private ProductoDto? productoSeleccionado;

    [ObservableProperty]
    private string nombreProducto = string.Empty;

    [ObservableProperty]
    private decimal precioProducto;

    [ObservableProperty]
    private CategoriaDto? categoriaProductoSeleccionado;

    [ObservableProperty]
    private bool modoEdicionProducto;

    // Formulario Extra
    [ObservableProperty]
    private ExtraDto? extraSeleccionado;

    [ObservableProperty]
    private string nombreExtra = string.Empty;

    [ObservableProperty]
    private decimal precioExtra;

    [ObservableProperty]
    private bool modoEdicionExtra;

    // Sección Seguridad y Purga
    [ObservableProperty]
    private string pinActual = string.Empty;

    [ObservableProperty]
    private string pinNuevo = string.Empty;

    [ObservableProperty]
    private string mensajeSeguridad = string.Empty;

    [ObservableProperty]
    private bool esMensajeSeguridadError;

    // Opciones rápidas de días para purga
    public ObservableCollection<string> OpcionesDiasPurga { get; } = new() { "5", "10", "15", "30", "60", "90", "120" };

    [ObservableProperty]
    private string diasPurgaTexto = "30";

    [ObservableProperty]
    private int ventasAPurgar;

    [ObservableProperty]
    private string mensajeResultadoPurga = string.Empty;

    // Modal de PIN táctil
    [ObservableProperty]
    private bool mostrarModalPin;

    [ObservableProperty]
    private string pinIngresado = string.Empty;

    public string PinEnmascarado => new string('●', PinIngresado.Length);

    [ObservableProperty]
    private string mensajeErrorPin = string.Empty;

    private Func<Task>? _accionPendientePostPin;

    public ConfiguracionMenuViewModel(
        ICategoriaService categoriaService,
        IProductoService productoService,
        IExtraService extraService,
        ISeguridadService seguridadService,
        IPurgaService purgaService)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;

        Productos.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalProductos));
            OnPropertyChanged(nameof(TieneProductos));
        };

        Categorias.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TieneCategorias));
        };

        ProductosCategoriaSeleccionada.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TieneProductosCategoria));
        };


        Extras.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalExtras));
            OnPropertyChanged(nameof(TieneExtras));
        };
    }

    public async Task CargarDatosAsync()
    {
        var categorias = await _categoriaService.ObtenerActivasAsync();
        var productos = await _productoService.ObtenerActivosAsync();
        var extras = await _extraService.ObtenerTodosAsync();

        Categorias.Clear();
        Productos.Clear();
        Extras.Clear();

        foreach (var categoria in categorias)
        {
            Categorias.Add(categoria);
        }

        foreach (var producto in productos)
        {
            Productos.Add(producto);
        }

        foreach (var extra in extras)
        {
            Extras.Add(extra);
        }
    }

    private async Task CargarProductosCategoriaAsync()
    {
        ProductosCategoriaSeleccionada.Clear();

        if (CategoriaSeleccionada is null)
            return;

        var productos = await _productoService .ObtenerPorCategoriaAsync(CategoriaSeleccionada.Id);

        foreach (var producto in productos)
        {
            ProductosCategoriaSeleccionada.Add(producto);
        }

        OnPropertyChanged(nameof(TieneCategoriaSeleccionada));
        OnPropertyChanged(nameof(TieneProductosCategoria));
    }

    partial void OnCategoriaSeleccionadaChanged(CategoriaDto? value)
    {
        OnPropertyChanged(nameof(TieneCategoriaSeleccionada));

        _ = CargarProductosCategoriaAsync();
    }


    private void LimpiarFormulario()
    {
        NombreCategoria = string.Empty;
        NombreProducto = string.Empty;
        PrecioProducto = 0;
        NombreExtra = string.Empty;
        PrecioExtra = 0;
        PinActual = string.Empty;
        PinNuevo = string.Empty;
        MensajeSeguridad = string.Empty;

        CategoriaProductoSeleccionado = null;
        CategoriaSeleccionada = null;
        ProductoSeleccionado = null;
        ExtraSeleccionado = null;

        ModoEdicionCategoria = false;
        ModoEdicionProducto = false;
        ModoEdicionProducto = false;
        ModoEdicionExtra = false;
    }

    #region CRUD Categorias
    [RelayCommand]
    private void MostrarFormularioCategoria()
    {
        LimpiarFormulario();

        MostrarFormulario = true;
        FormularioCategoria = true;
        FormularioProducto = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionCategoria = false;
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

        CancelarFormulario();
    }

    [RelayCommand]
    private void EditarCategoria(CategoriaDto categoria)
    {
        CategoriaSeleccionada = categoria;
        NombreCategoria = categoria.Nombre;

        MostrarFormulario = true;
        FormularioCategoria = true;
        FormularioProducto = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionCategoria = true;
    }
    #endregion

    #region CRUD Productos
    [RelayCommand]
    private void MostrarFormularioProducto()
    {
        LimpiarFormulario();

        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionProducto = false;
    }

    [RelayCommand]
    private async Task GuardarProducto()
    {
        if (string.IsNullOrWhiteSpace(NombreProducto))
            return;

        if (PrecioProducto <= 0)
            return;

        if (CategoriaProductoSeleccionado is null)
            return;

        if (ProductoSeleccionado is not null)
        {
            var dtoProducto = new EditarProductoDto
            {
                Id = ProductoSeleccionado.Id,
                Nombre = NombreProducto.Trim(),
                Precio = PrecioProducto,
                CategoriaId = CategoriaProductoSeleccionado.Id
            };

            var productoEditado = await _productoService.EditarAsync(dtoProducto);
            var productoEnLista = Productos.FirstOrDefault(p => p.Id == productoEditado.Id);

            if (productoEnLista is not null)
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

        CancelarFormulario();
    }

    [RelayCommand]
    private void EditarProducto(ProductoDto producto)
    {
        ProductoSeleccionado = producto;
        NombreProducto = producto.Nombre;
        PrecioProducto = producto.Precio;
        CategoriaProductoSeleccionado = Categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);

        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionProducto = true;
    }
    #endregion

    #region CRUD Extras
    [RelayCommand]
    private void MostrarFormularioExtra()
    {
        LimpiarFormulario();
        PrepararCategoriasParaExtra();

        MostrarFormulario = true;
        FormularioExtra = true;
        FormularioCategoria = false;
        FormularioProducto = false;
        FormularioSeguridad = false;

        ModoEdicionExtra = false;
    }

    [RelayCommand]
    private void EditarExtra(ExtraDto extra)
    {
        ExtraSeleccionado = extra;
        NombreExtra = extra.Nombre;
        PrecioExtra = extra.Precio;

        PrepararCategoriasParaExtra();

        MostrarFormulario = true;
        FormularioExtra = true;
        FormularioCategoria = false;

        ModoEdicionProducto = true;
        FormularioProducto = false;
        FormularioSeguridad = false;

        ModoEdicionExtra = true;
    }

    [RelayCommand]
    private async Task GuardarExtra()
    {
        if (string.IsNullOrWhiteSpace(NombreExtra) || PrecioExtra < 0)
            return;

        if (ModoEdicionExtra && ExtraSeleccionado is not null)
        {
            var dto = new EditarExtraDto
            {
                Id = ExtraSeleccionado.Id,
                Nombre = NombreExtra.Trim(),
                Precio = PrecioExtra
            };

            var extraEditado = await _extraService.EditarAsync(dto);
            var index = Extras.IndexOf(ExtraSeleccionado);
            if (index >= 0)
            {
                Extras[index] = extraEditado;
            }
        }
        else
        {
            var dto = new CrearExtraDto
            {
                Nombre = NombreExtra.Trim(),
                Precio = PrecioExtra
            };

            var nuevoExtra = await _extraService.CrearAsync(dto);
            Extras.Add(nuevoExtra);
        }

        CancelarFormulario();
    }

    [RelayCommand]
    private async Task ToggleActivoExtra(ExtraDto extra)
    {
        if (extra is null) return;

        var index = Extras.IndexOf(extra);
        if (index < 0) return;

        if (extra.Activo)
        {
            await _extraService.DesactivarAsync(extra.Id);
            Extras[index] = new ExtraDto
            {
                Id = extra.Id,
                Nombre = extra.Nombre,
                Precio = extra.Precio,
                Activo = false
            };
        }
        else
        {
            await _extraService.ActivarAsync(extra.Id);
            Extras[index] = new ExtraDto
            {
                Id = extra.Id,
                Nombre = extra.Nombre,
                Precio = extra.Precio,
                Activo = true
            };
        }
    }

    private void PrepararCategoriasParaExtra()
    {
        CategoriasParaExtra.Clear();
        foreach (var c in Categorias)
        {
            CategoriasParaExtra.Add(new CategoriaCheckItem
            {
                Categoria = c,
                IsChecked = false
            });
        }
    }
    #endregion

    #region Seguridad y Purga de Ventas
    [RelayCommand]
    private async Task MostrarFormularioSeguridad()
    {
        LimpiarFormulario();

        MostrarFormulario = true;
        FormularioSeguridad = true;
        FormularioCategoria = false;
        FormularioProducto = false;
        FormularioExtra = false;

        await ActualizarConteoVentasAPurgarAsync();
    }

    async partial void OnDiasPurgaTextoChanged(string value)
    {
        await ActualizarConteoVentasAPurgarAsync();
    }

    private async Task ActualizarConteoVentasAPurgarAsync()
    {
        if (int.TryParse(DiasPurgaTexto, out int dias) && dias > 0)
        {
            try
            {
                VentasAPurgar = await _purgaService.ContarVentasAntiguasAsync(dias);
            }
            catch
            {
                VentasAPurgar = 0;
            }
        }
        else
        {
            VentasAPurgar = 0;
        }
    }

    [RelayCommand]
    private void SolicitarPurgaVentas()
    {
        if (!int.TryParse(DiasPurgaTexto, out int dias) || dias <= 0)
        {
            MensajeResultadoPurga = "Por favor ingrese un número válido de días.";
            return;
        }

        if (VentasAPurgar == 0)
        {
            MensajeResultadoPurga = $"No existen ventas con más de {dias} días de antigüedad para eliminar.";
            return;
        }

        MensajeErrorPin = string.Empty;
        PinIngresado = string.Empty;
        OnPropertyChanged(nameof(PinEnmascarado));

        _accionPendientePostPin = async () =>
        {
            var eliminadas = await _purgaService.PurgarVentasAntiguasAsync(dias);
            MensajeResultadoPurga = $"Operación exitosa: Se eliminaron {eliminadas} ventas físicamente.";
            await ActualizarConteoVentasAPurgarAsync();
        };

        MostrarModalPin = true;
    }

    [RelayCommand]
    private async Task CambiarPin()
    {
        MensajeSeguridad = string.Empty;

        if (string.IsNullOrWhiteSpace(PinActual) || string.IsNullOrWhiteSpace(PinNuevo))
        {
            MensajeSeguridad = "Debe ingresar el PIN actual y el nuevo PIN.";
            EsMensajeSeguridadError = true;
            return;
        }

        if (PinNuevo.Trim().Length < 4)
        {
            MensajeSeguridad = "El nuevo PIN debe contener al menos 4 dígitos.";
            EsMensajeSeguridadError = true;
            return;
        }

        try
        {
            await _seguridadService.CambiarPinAsync(PinActual.Trim(), PinNuevo.Trim());
            MensajeSeguridad = "PIN de seguridad actualizado correctamente.";
            EsMensajeSeguridadError = false;
            PinActual = string.Empty;
            PinNuevo = string.Empty;
        }
        catch (Exception ex)
        {
            MensajeSeguridad = ex.Message;
            EsMensajeSeguridadError = true;
        }
    }
    #endregion

    #region Modal PIN Táctil
    [RelayCommand]
    private void AgregarDigitoPin(string digito)
    {
        if (PinIngresado.Length < 8)
        {
            PinIngresado += digito;
            MensajeErrorPin = string.Empty;
            OnPropertyChanged(nameof(PinEnmascarado));
        }
    }

    [RelayCommand]
    private void BorrarDigitoPin()
    {
        if (PinIngresado.Length > 0)
        {
            PinIngresado = PinIngresado[..^1];
            OnPropertyChanged(nameof(PinEnmascarado));
        }
    }

    [RelayCommand]
    private void LimpiarPin()
    {
        PinIngresado = string.Empty;
        OnPropertyChanged(nameof(PinEnmascarado));
    }

    [RelayCommand]
    private async Task ConfirmarPin()
    {
        if (string.IsNullOrWhiteSpace(PinIngresado))
        {
            MensajeErrorPin = "Por favor ingrese el PIN.";
            return;
        }

        bool esValido = await _seguridadService.ValidarPinAsync(PinIngresado.Trim());
        if (esValido)
        {
            MostrarModalPin = false;
            PinIngresado = string.Empty;
            OnPropertyChanged(nameof(PinEnmascarado));
            MensajeErrorPin = string.Empty;

            var accion = _accionPendientePostPin;
            _accionPendientePostPin = null;

            if (accion is not null)
            {
                await accion();
            }
        }
        else
        {
            MensajeErrorPin = "PIN incorrecto. Intente de nuevo.";
            PinIngresado = string.Empty;
            OnPropertyChanged(nameof(PinEnmascarado));
        }
    }

    [RelayCommand]
    private void CancelarPin()
    {
        MostrarModalPin = false;
        PinIngresado = string.Empty;
        OnPropertyChanged(nameof(PinEnmascarado));
        MensajeErrorPin = string.Empty;
        _accionPendientePostPin = null;
    }
    #endregion

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;
        FormularioCategoria = false;
        FormularioProducto = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionCategoria = false;
        ModoEdicionProducto = false;

        LimpiarFormulario();
    }
}

public partial class CategoriaCheckItem : ObservableObject
{
    public required CategoriaDto Categoria { get; init; }

    [ObservableProperty]
    private bool isChecked;
}