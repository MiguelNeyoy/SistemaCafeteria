using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces.Services;
using Core.Domain.Exceptions;
using Presentation.WPF.Services;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

public partial class ConfiguracionMenuViewModel : ObservableObject
{
    private readonly ICategoriaService _categoriaService;
    private readonly IProductoService _productoService;
    private readonly IExtraService _extraService;
    private readonly ISeguridadService _seguridadService;
    private readonly IPurgaService _purgaService;
    private readonly IDialogoService _dialogoService;

    // Colecciones de Catálogo
    public ObservableCollection<CategoriaDto> Categorias { get; } = new();
    public ObservableCollection<ProductoDto> Productos { get; } = new();
    public ObservableCollection<ProductoDto> ProductosCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ExtraDto> Extras { get; } = new();
    public ObservableCollection<ExtraDto> ExtrasCategoriaSeleccionada { get; } = new();
    public ObservableCollection<CategoriaCheckItem> CategoriasParaExtra { get; } = new();

    // Contadores y banderas para la interfaz
    public int TotalProductos => Productos.Count;
    public int TotalExtras => Extras.Count;
    public bool TieneCategorias => Categorias.Count > 0;
    public bool TieneProductos => Productos.Count > 0;
    public bool TieneCategoriaSeleccionada => CategoriaSeleccionada is not null;
    public bool TieneProductosCategoria => ProductosCategoriaSeleccionada.Count > 0;
    public bool TieneExtras => Extras.Count > 0;

    public bool TieneExtrasCategoria => ExtrasCategoriaSeleccionada.Count > 0;

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneErrorCategoria))]
    private string? mensajeErrorCategoria;

    public bool TieneErrorCategoria => !string.IsNullOrWhiteSpace(MensajeErrorCategoria);

    partial void OnNombreCategoriaChanged(string value)
    {
        MensajeErrorCategoria = null;
    }

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneErrorProducto))]
    private string? mensajeErrorProducto;

    public bool TieneErrorProducto => !string.IsNullOrWhiteSpace(MensajeErrorProducto);

    partial void OnNombreProductoChanged(string value)
    {
        MensajeErrorProducto = null;
    }

    public event Action? ConfiguracionAvanzadaSolicitada;

    // Formulario Extra
    [ObservableProperty]
    private ExtraDto? extraSeleccionado;

    [ObservableProperty]
    private string nombreExtra = string.Empty;

    [ObservableProperty]
    private decimal precioExtra;

    [ObservableProperty]
    private bool modoEdicionExtra;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneErrorExtra))]
    private string? mensajeErrorExtra;

    public bool TieneErrorExtra => !string.IsNullOrWhiteSpace(MensajeErrorExtra);

    partial void OnNombreExtraChanged(string value)
    {
        MensajeErrorExtra = null;
    }

    public bool CategoriaSeleccionadaEsActiva => CategoriaSeleccionada?.Activo ?? true;
    public bool ProductoSeleccionadoEsActivo => ProductoSeleccionado?.Activo ?? true;
    public bool ExtraSeleccionadoEsActivo => ExtraSeleccionado?.Activo ?? true;

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
        IPurgaService purgaService,
        IDialogoService dialogoService)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;
        _dialogoService = dialogoService;

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

        ExtrasCategoriaSeleccionada.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TieneExtrasCategoria));
        };
    }


    public void SolicitarAccesoConfiguracion()
    {
        MensajeErrorPin = string.Empty;
        PinIngresado = string.Empty;

        OnPropertyChanged(nameof(PinEnmascarado));

        _accionPendientePostPin = null;

        MostrarModalPin = true;

    }//Fin - SolicitarAccesoConfiguracion

    public async Task CargarDatosAsync()
    {
        var categorias = await _categoriaService.ObtenerTodasAsync();
        var productos = await _productoService.ObtenerTodosAsync();
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

    private async Task CargarExtrasCategoriaAsync()
    {
        ExtrasCategoriaSeleccionada.Clear();

        if (CategoriaSeleccionada is null)
        {
            OnPropertyChanged(nameof(TieneExtrasCategoria));
            return;
        }

        var extras = await _extraService.ObtenerPorCategoriaAsync(CategoriaSeleccionada.Id);

        foreach (var extra in extras)
        {
            ExtrasCategoriaSeleccionada.Add(extra);
        }

        OnPropertyChanged(nameof(TieneExtrasCategoria));
    }


    partial void OnCategoriaSeleccionadaChanged(CategoriaDto? value)
    {
        OnPropertyChanged(nameof(TieneCategoriaSeleccionada));

        _ = CargarProductosCategoriaAsync();
        _ = CargarExtrasCategoriaAsync();
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
        ModoEdicionExtra = false;
        MensajeErrorProducto = null;
        MensajeErrorCategoria = null;
        MensajeErrorExtra = null;

        OnPropertyChanged(nameof(CategoriaSeleccionadaEsActiva));
        OnPropertyChanged(nameof(ProductoSeleccionadoEsActivo));
        OnPropertyChanged(nameof(ExtraSeleccionadoEsActivo));
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
        MensajeErrorCategoria = null;

        if (string.IsNullOrWhiteSpace(NombreCategoria))
        {
            MensajeErrorCategoria = "El nombre de la categoría es obligatorio.";
            return;
        }

        try
        {
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

            // Mantener la lista observable ordenada alfabéticamente
            var categoriasOrdenadas = Categorias.OrderBy(c => c.Nombre).ToList();
            Categorias.Clear();
            foreach (var item in categoriasOrdenadas)
            {
                Categorias.Add(item);
            }

            CancelarFormulario();
        }
        catch (DomainValidationException ex)
        {
            MensajeErrorCategoria = ex.Message;
        }
        catch (Exception ex)
        {
            MensajeErrorCategoria = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void EditarCategoria(CategoriaDto categoria)
    {
        CategoriaSeleccionada = categoria;
        NombreCategoria = categoria.Nombre;
        MensajeErrorCategoria = null;

        MostrarFormulario = true;
        FormularioCategoria = true;
        FormularioProducto = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionCategoria = true;
        OnPropertyChanged(nameof(CategoriaSeleccionadaEsActiva));
    }

    [RelayCommand]
    private async Task AlternarEstadoCategoria()
    {
        if (CategoriaSeleccionada is null) return;

        if (CategoriaSeleccionada.Activo)
        {
            var confirmacion = _dialogoService.Confirmar(
                $"¿Desea deshabilitar la categoría '{CategoriaSeleccionada.Nombre}'? Los productos de esta categoría no se mostrarán en la comanda.",
                "Confirmar Deshabilitación");

            if (!confirmacion) return;

            await _categoriaService.DesactivarAsync(CategoriaSeleccionada.Id);
        }
        else
        {
            var confirmacion = _dialogoService.Confirmar(
                $"¿Desea habilitar la categoría '{CategoriaSeleccionada.Nombre}'?",
                "Confirmar Habilitación");

            if (!confirmacion) return;

            await _categoriaService.ActivarAsync(CategoriaSeleccionada.Id);
        }

        CancelarFormulario();
        await CargarDatosAsync();
    }
    #endregion

    #region CRUD Productos
    [RelayCommand]
    private void MostrarFormularioProducto()
    {
        var categoriaActual = CategoriaSeleccionada;

        LimpiarFormulario();

        CategoriaSeleccionada = categoriaActual;

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
        MensajeErrorProducto = null;

        if (string.IsNullOrWhiteSpace(NombreProducto))
        {
            MensajeErrorProducto = "El nombre del producto es obligatorio.";
            return;
        }

        if (PrecioProducto <= 0)
        {
            MensajeErrorProducto = "El precio debe ser mayor a $0.";
            return;
        }

        var categoria = CategoriaProductoSeleccionado ?? CategoriaSeleccionada;
        if (categoria is null)
        {
            MensajeErrorProducto = "Debes seleccionar una categoría para el producto.";
            return;
        }

        try
        {
            if (ProductoSeleccionado is not null)
            {
                var dtoProducto = new EditarProductoDto
                {
                    Id = ProductoSeleccionado.Id,
                    Nombre = NombreProducto.Trim(),
                    Precio = PrecioProducto,
                    CategoriaId = categoria.Id
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
                    CategoriaId = categoria.Id
                };

                var producto = await _productoService.CrearAsync(dtoProducto);
                Productos.Add(producto);
            }

            // Mantener la lista observable global ordenada alfabéticamente
            var productosOrdenados = Productos.OrderBy(p => p.Nombre).ToList();
            Productos.Clear();
            foreach (var p in productosOrdenados)
            {
                Productos.Add(p);
            }

            // Refrescar y ordenar productos de la categoría seleccionada
            if (CategoriaSeleccionada is not null)
            {
                await CargarProductosCategoriaAsync();
            }

            CancelarFormulario();
        }
        catch (DomainValidationException ex)
        {
            MensajeErrorProducto = ex.Message;
        }
        catch (Exception ex)
        {
            MensajeErrorProducto = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void EditarProducto(ProductoDto producto)
    {
        ProductoSeleccionado = producto;
        NombreProducto = producto.Nombre;
        PrecioProducto = producto.Precio;
        CategoriaProductoSeleccionado = Categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);
        MensajeErrorProducto = null;

        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionProducto = true;
        OnPropertyChanged(nameof(ProductoSeleccionadoEsActivo));
    }

    [RelayCommand]
    private async Task AlternarEstadoProducto()
    {
        if (ProductoSeleccionado is null) return;

        if (ProductoSeleccionado.Activo)
        {
            var confirmacion = _dialogoService.Confirmar(
                $"¿Desea deshabilitar el producto '{ProductoSeleccionado.Nombre}'? No aparecerá en el menú al tomar comandas.",
                "Confirmar Deshabilitación");

            if (!confirmacion) return;

            await _productoService.DesactivarAsync(ProductoSeleccionado.Id);
        }
        else
        {
            var confirmacion = _dialogoService.Confirmar(
                $"¿Desea habilitar el producto '{ProductoSeleccionado.Nombre}'?",
                "Confirmar Habilitación");

            if (!confirmacion) return;

            await _productoService.ActivarAsync(ProductoSeleccionado.Id);
        }

        CancelarFormulario();
        await CargarDatosAsync();
    }
    #endregion

    #region CRUD Extras
    [RelayCommand]
    private async Task MostrarFormularioExtra()
    {
        LimpiarFormulario();

        await PrepararCategoriasParaExtraAsync();

        MostrarFormulario = true;
        FormularioExtra = true;
        FormularioCategoria = false;
        FormularioProducto = false;
        FormularioSeguridad = false;

        ModoEdicionExtra = false;
    }

    [RelayCommand]
    private async Task EditarExtra(ExtraDto extra)
    {
        ExtraSeleccionado = extra;
        NombreExtra = extra.Nombre;
        PrecioExtra = extra.Precio;
        MensajeErrorExtra = null;

        await PrepararCategoriasParaExtraAsync();

        MostrarFormulario = true;
        FormularioExtra = true;
        FormularioCategoria = false;
        FormularioProducto = false;
        FormularioSeguridad = false;

        ModoEdicionExtra = true;
        OnPropertyChanged(nameof(ExtraSeleccionadoEsActivo));
    }

    [RelayCommand]
    private async Task AlternarEstadoExtra()
    {
        if (ExtraSeleccionado is null) return;

        if (ExtraSeleccionado.Activo)
        {
            var confirmacion = _dialogoService.Confirmar(
                $"¿Desea deshabilitar el extra '{ExtraSeleccionado.Nombre}'? No aparecerá en el menú al tomar comandas.",
                "Confirmar Deshabilitación");

            if (!confirmacion) return;

            await _extraService.DesactivarAsync(ExtraSeleccionado.Id);
        }
        else
        {
            var confirmacion = _dialogoService.Confirmar(
                $"¿Desea habilitar el extra '{ExtraSeleccionado.Nombre}'?",
                "Confirmar Habilitación");

            if (!confirmacion) return;

            await _extraService.ActivarAsync(ExtraSeleccionado.Id);
        }

        CancelarFormulario();
        await CargarDatosAsync();
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

            // 1. Crear el Extra
            var nuevoExtra = await _extraService.CrearAsync(dto);

            // 2. Agregarlo a la colección de Extras
            Extras.Add(nuevoExtra);

            // 3. Obtener las categorías seleccionadas
            var categoriasSeleccionadas = CategoriasParaExtra
                .Where(c => c.IsChecked)
                .Select(c => c.Categoria.Id)
                .ToList();

            // 4. Crear las relaciones Categoría <-> Extra
            foreach (var categoriaId in categoriasSeleccionadas)
            {
                await _extraService.SincronizarExtrasCategoriaAsync(
                    categoriaId,
                    new List<int> { nuevoExtra.Id });
            }
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

    private async Task PrepararCategoriasParaExtraAsync()
    {
        CategoriasParaExtra.Clear();

        if (Categorias.Count == 0)
            return;

        int? extraId = ExtraSeleccionado?.Id;

        foreach (var categoria in Categorias)
        {
            bool estaSeleccionada = false;

            if (extraId.HasValue)
            {
                var extraIds = await _extraService.ObtenerExtraIdsPorCategoriaAsync( categoria.Id );

                estaSeleccionada = extraIds.Contains(extraId.Value);
            }

            CategoriasParaExtra.Add(new CategoriaCheckItem
            {
                Categoria = categoria,
                IsChecked = estaSeleccionada
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
        var categoriaActual = CategoriaSeleccionada;

        MostrarFormulario = false;
        FormularioCategoria = false;
        FormularioProducto = false;
        FormularioExtra = false;
        FormularioSeguridad = false;

        ModoEdicionCategoria = false;
        ModoEdicionProducto = false;
        ModoEdicionExtra = false;

        LimpiarFormulario();

        CategoriaSeleccionada = categoriaActual;
    }


    [RelayCommand]
    private void MostrarConfiguracionAvanzada()
    {
        ConfiguracionAvanzadaSolicitada?.Invoke();

    } //Fin - MostrarConfiguracionAvanzada


}

public partial class CategoriaCheckItem : ObservableObject
{
    public required CategoriaDto Categoria { get; init; }

    [ObservableProperty]
    private bool isChecked;
}