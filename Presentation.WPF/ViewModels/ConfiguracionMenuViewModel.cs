using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Common;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Common;
using Core.Domain.Exceptions;
using Presentation.WPF.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Presentation.WPF.ViewModels;

public partial class ConfiguracionMenuViewModel : ObservableObject
{
    private readonly ICategoriaService _categoriaService;
    private readonly IProductoService _productoService;
    private readonly IExtraService _extraService;
    private readonly ISeguridadService _seguridadService;
    private readonly IPurgaService _purgaService;
    private readonly IDialogoService _dialogoService;
    private readonly IPrinterService _printerService;
    private readonly IConfiguracionRepository _configuracionRepository;

    // Colecciones de Catálogo
    public ObservableCollection<CategoriaDto> Categorias { get; } = new();
    public ObservableCollection<ProductoDto> Productos { get; } = new();
    public ObservableCollection<ProductoDto> ProductosCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ExtraDto> Extras { get; } = new();
    public ObservableCollection<ExtraDto> ExtrasCategoriaSeleccionada { get; } = new();
    public ObservableCollection<CategoriaCheckItem> CategoriasParaExtra { get; } = new();

    // Colecciones y estado del Tablero Kanban por Categorías (Issue #8)
    public const int ColumnasPorVistaKanban = 3;
    private readonly HashSet<int> _categoriasOcultasIds = new();
    private readonly Dictionary<int, List<ExtraDto>> _extrasPorCategoriaCache = new();

    public ObservableCollection<CategoriaKanbanItem> ColumnasKanban { get; } = new();
    public ObservableCollection<CategoriaKanbanItem> ColumnasKanbanVisibles { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneTextoBusquedaProducto))]
    private string textoBusquedaProducto = string.Empty;

    public bool TieneTextoBusquedaProducto => !string.IsNullOrWhiteSpace(TextoBusquedaProducto);

    [ObservableProperty]
    private int indiceInicioKanban;

    partial void OnTextoBusquedaProductoChanged(string value)
    {
        IndiceInicioKanban = 0;
        SincronizarTableroKanban();
    }

    // Contadores y banderas para la interfaz
    public int TotalCategorias => Categorias.Count;
    public string EtiquetaBotonTodas => $"Todas ({TotalCategorias})";
    public bool TodasLasColumnasVisibles => _categoriasOcultasIds.Count == 0;
    public bool TieneColumnasKanbanVisibles => ColumnasKanbanVisibles.Count > 0;
    public bool PuedeIrAnteriorKanban => IndiceInicioKanban > 0;
    public bool PuedeIrSiguienteKanban => ObtenerColumnasActivasParaTablero().Count > IndiceInicioKanban + ColumnasPorVistaKanban;

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
    private string precioProductoTexto = string.Empty;

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

    partial void OnPrecioProductoTextoChanged(string value)
    {
        MensajeErrorProducto = null;

        string limpio = FormateadorTextoCatalogo.SanitizarTextoPrecio(value);
        if (limpio != value)
        {
            PrecioProductoTexto = limpio;
            return;
        }

        if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto))
        {
            PrecioProducto = monto;
        }
        else
        {
            PrecioProducto = 0m;
        }
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
    private string precioExtraTexto = "0";

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

    partial void OnPrecioExtraTextoChanged(string value)
    {
        MensajeErrorExtra = null;

        string limpio = FormateadorTextoCatalogo.SanitizarTextoPrecio(value);
        if (limpio != value)
        {
            PrecioExtraTexto = limpio;
            return;
        }

        if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto))
        {
            PrecioExtra = monto;
        }
        else
        {
            PrecioExtra = 0m;
        }
    }

    public bool CategoriaSeleccionadaEsActiva => CategoriaSeleccionada?.Activo ?? true;
    public bool ProductoSeleccionadoEsActivo => ProductoSeleccionado?.Activo ?? true;
    public bool ExtraSeleccionadoEsActivo => ExtraSeleccionado?.Activo ?? true;



    // Modal de PIN táctil
    [ObservableProperty]
    private bool mostrarModalPin;

    [ObservableProperty]
    private string pinIngresado = string.Empty;

    public string PinEnmascarado => new string('●', PinIngresado.Length);

    [ObservableProperty]
    private string mensajeErrorPin = string.Empty;

    private Func<Task>? _accionPendientePostPin;

    // Sección Hardware / Impresora Térmica
    public ObservableCollection<string> ImpresorasDisponibles { get; } = new();

    [ObservableProperty]
    private string? impresoraSeleccionada;

    [ObservableProperty]
    private string? impresoraComandaSeleccionada;

    [ObservableProperty]
    private bool esPapel58mm = true;

    [ObservableProperty]
    private bool esPapel80mm;

    [ObservableProperty]
    private bool abrirCajonAutomatico = true;

    [ObservableProperty]
    private bool cortarPapelAutomatico = true;

    [ObservableProperty]
    private string mensajeResultadoImpresora = string.Empty;

    [ObservableProperty]
    private bool tieneErrorImpresora;

    public ConfiguracionMenuViewModel(
        ICategoriaService categoriaService,
        IProductoService productoService,
        IExtraService extraService,
        ISeguridadService seguridadService,
        IPurgaService purgaService,
        IDialogoService dialogoService,
        IPrinterService printerService,
        IConfiguracionRepository configuracionRepository)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;
        _extraService = extraService;
        _seguridadService = seguridadService;
        _purgaService = purgaService;
        _dialogoService = dialogoService;
        _printerService = printerService;
        _configuracionRepository = configuracionRepository;

        Productos.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalProductos));
            OnPropertyChanged(nameof(TieneProductos));
        };

        Categorias.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TieneCategorias));
            OnPropertyChanged(nameof(TotalCategorias));
            OnPropertyChanged(nameof(EtiquetaBotonTodas));
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

        foreach (var categoria in categorias.OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase))
        {
            Categorias.Add(categoria);
        }

        foreach (var producto in productos.OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase))
        {
            Productos.Add(producto);
        }

        foreach (var extra in extras.OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase))
        {
            Extras.Add(extra);
        }

        await RecargarExtrasPorCategoriaAsync();
        SincronizarTableroKanban();

        // Cargar impresoras del sistema y configuración persistida
        RefrescarImpresoras();
        await CargarConfiguracionImpresoraAsync();
    }

    private async Task RecargarExtrasPorCategoriaAsync()
    {
        _extrasPorCategoriaCache.Clear();
        foreach (var cat in Categorias)
        {
            var extrasCat = await _extraService.ObtenerPorCategoriaAsync(cat.Id);
            _extrasPorCategoriaCache[cat.Id] = extrasCat
                .OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }

    public void SincronizarTableroKanban()
    {
        var estadoVistaExtrasPrevio = ColumnasKanban.ToDictionary(c => c.Categoria.Id, c => c.MostrarVistaExtras);

        ColumnasKanban.Clear();

        foreach (var cat in Categorias.OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase))
        {
            int totalEnCategoria = Productos.Count(p => p.CategoriaId == cat.Id);
            var productosFiltrados = FiltroKanbanCatalogo.FiltrarProductosDeCategoria(
                Productos,
                cat.Id,
                TextoBusquedaProducto);

            var columna = new CategoriaKanbanItem
            {
                Categoria = cat,
                TotalProductosCategoria = totalEnCategoria,
                EstaOculta = _categoriasOcultasIds.Contains(cat.Id),
                MostrarVistaExtras = estadoVistaExtrasPrevio.TryGetValue(cat.Id, out bool verExtras) && verExtras
            };

            foreach (var prod in productosFiltrados)
            {
                columna.Productos.Add(prod);
            }

            if (_extrasPorCategoriaCache.TryGetValue(cat.Id, out var extrasDeCat))
            {
                foreach (var ext in extrasDeCat)
                {
                    columna.Extras.Add(ext);
                }
            }

            columna.NotificarCambiosColecciones();
            ColumnasKanban.Add(columna);
        }

        ActualizarColumnasKanbanVisibles();
    }

    private List<CategoriaKanbanItem> ObtenerColumnasActivasParaTablero()
    {
        bool filtrandoPorTexto = TieneTextoBusquedaProducto;

        return ColumnasKanban
            .Where(c => !c.EstaOculta && (!filtrandoPorTexto || c.Productos.Count > 0))
            .ToList();
    }

    public void ActualizarColumnasKanbanVisibles()
    {
        var activas = ObtenerColumnasActivasParaTablero();
        var ventana = FiltroKanbanCatalogo.ObtenerVentanaColumnas(
            activas,
            IndiceInicioKanban,
            ColumnasPorVistaKanban,
            out int indiceAjustado);

        if (indiceAjustado != IndiceInicioKanban)
        {
            IndiceInicioKanban = indiceAjustado;
        }

        var listaVentana = ventana.ToList();
        var conjuntoVentana = listaVentana.ToHashSet();

        foreach (var col in ColumnasKanban)
        {
            int idxEnVentana = listaVentana.IndexOf(col);
            col.EstaEnVentanaActual = !col.EstaOculta && conjuntoVentana.Contains(col);
            col.OrdenEnVentanaActual = idxEnVentana >= 0 ? idxEnVentana + 1 : 0;
        }

        ColumnasKanbanVisibles.Clear();
        foreach (var col in listaVentana)
        {
            ColumnasKanbanVisibles.Add(col);
        }

        OnPropertyChanged(nameof(TotalCategorias));
        OnPropertyChanged(nameof(EtiquetaBotonTodas));
        OnPropertyChanged(nameof(TodasLasColumnasVisibles));
        OnPropertyChanged(nameof(TieneColumnasKanbanVisibles));
        OnPropertyChanged(nameof(PuedeIrAnteriorKanban));
        OnPropertyChanged(nameof(PuedeIrSiguienteKanban));
    }

    [RelayCommand]
    private void LimpiarBusquedaProducto()
    {
        TextoBusquedaProducto = string.Empty;
    }

    [RelayCommand]
    private void MostrarTodasLasCategorias()
    {
        _categoriasOcultasIds.Clear();
        foreach (var col in ColumnasKanban)
        {
            col.EstaOculta = false;
        }

        IndiceInicioKanban = 0;
        ActualizarColumnasKanbanVisibles();
    }

    [RelayCommand]
    private void OcultarColumnaKanban(CategoriaKanbanItem? columna)
    {
        if (columna is null) return;

        _categoriasOcultasIds.Add(columna.Categoria.Id);
        columna.EstaOculta = true;
        ActualizarColumnasKanbanVisibles();
    }

    [RelayCommand]
    private void AlternarPildoraCategoria(CategoriaKanbanItem? columna)
    {
        if (columna is null) return;

        // Si estaba oculta, al darle clic en su píldora se vuelve a mostrar
        if (columna.EstaOculta)
        {
            _categoriasOcultasIds.Remove(columna.Categoria.Id);
            columna.EstaOculta = false;
        }

        // Posicionar la ventana de 3 columnas empezando en la categoría seleccionada
        var activas = ObtenerColumnasActivasParaTablero();
        int idx = activas.IndexOf(columna);
        if (idx >= 0)
        {
            IndiceInicioKanban = Math.Clamp(idx, 0, Math.Max(0, activas.Count - ColumnasPorVistaKanban));
        }

        ActualizarColumnasKanbanVisibles();
    }

    [RelayCommand]
    private void CategoriasAnteriores()
    {
        if (IndiceInicioKanban > 0)
        {
            IndiceInicioKanban--;
            ActualizarColumnasKanbanVisibles();
        }
    }

    [RelayCommand]
    private void CategoriasSiguientes()
    {
        var activas = ObtenerColumnasActivasParaTablero();
        if (IndiceInicioKanban + ColumnasPorVistaKanban < activas.Count)
        {
            IndiceInicioKanban++;
            ActualizarColumnasKanbanVisibles();
        }
    }

    [RelayCommand]
    private async Task AgregarAquiEnColumna(CategoriaKanbanItem? columna)
    {
        if (columna is null) return;

        if (columna.MostrarVistaExtras)
        {
            LimpiarFormulario();
            await PrepararCategoriasParaExtraAsync();

            var itemCheck = CategoriasParaExtra.FirstOrDefault(c => c.Categoria.Id == columna.Categoria.Id);
            if (itemCheck is not null)
            {
                itemCheck.IsChecked = true;
            }

            MostrarFormulario = true;
            FormularioExtra = true;
            FormularioCategoria = false;
            FormularioProducto = false;
            ModoEdicionExtra = false;
        }
        else
        {
            LimpiarFormulario();
            var catEnLista = Categorias.FirstOrDefault(c => c.Id == columna.Categoria.Id) ?? columna.Categoria;
            CategoriaSeleccionada = catEnLista;
            CategoriaProductoSeleccionado = catEnLista;

            MostrarFormulario = true;
            FormularioProducto = true;
            FormularioCategoria = false;
            FormularioExtra = false;
            ModoEdicionProducto = false;
        }
    }

    [RelayCommand]
    private void AlternarVistaExtrasColumna(CategoriaKanbanItem? columna)
    {
        if (columna is null) return;
        columna.MostrarVistaExtras = !columna.MostrarVistaExtras;
    }

    public void RefrescarImpresoras()
    {
        ImpresorasDisponibles.Clear();
        var lista = _printerService.ObtenerImpresorasInstaladas();
        foreach (var item in lista)
        {
            ImpresorasDisponibles.Add(item);
        }
    }

    private async Task CargarConfiguracionImpresoraAsync()
    {
        try
        {
            ImpresoraSeleccionada = await _configuracionRepository.ObtenerValorAsync("Impresora_NombreTicket");
            ImpresoraComandaSeleccionada = await _configuracionRepository.ObtenerValorAsync("Impresora_NombreComanda");

            string ancho = await _configuracionRepository.ObtenerValorAsync("Impresora_AnchoPapel") ?? "58mm";
            EsPapel80mm = ancho == "80mm";
            EsPapel58mm = !EsPapel80mm;

            string abrirCajon = await _configuracionRepository.ObtenerValorAsync("Impresora_AbrirCajon") ?? "true";
            AbrirCajonAutomatico = abrirCajon != "false";

            string cortarPapel = await _configuracionRepository.ObtenerValorAsync("Impresora_CortarPapel") ?? "true";
            CortarPapelAutomatico = cortarPapel != "false";
        }
        catch
        {
            // Valores por defecto
            EsPapel58mm = true;
            AbrirCajonAutomatico = true;
            CortarPapelAutomatico = true;
        }
    }

    private async Task CargarProductosCategoriaAsync()
    {
        ProductosCategoriaSeleccionada.Clear();

        if (CategoriaSeleccionada is null)
            return;

        var productos = await _productoService.ObtenerPorCategoriaAsync(CategoriaSeleccionada.Id);

        foreach (var producto in productos.OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase))
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

        foreach (var extra in extras.OrderBy(e => e.Nombre, StringComparer.CurrentCultureIgnoreCase))
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
        PrecioProductoTexto = string.Empty;
        NombreExtra = string.Empty;
        PrecioExtra = 0;
        PrecioExtraTexto = "0";

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

        ModoEdicionCategoria = false;
    }

    [RelayCommand]
    private async Task GuardarCategoria()
    {
        MensajeErrorCategoria = null;
        NombreCategoria = FormateadorTextoCatalogo.FormatearNombre(NombreCategoria);

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
                    Nombre = NombreCategoria
                };

                var categoriaEditada = await _categoriaService.EditarAsync(dtoCategoria);
                var index = Categorias.IndexOf(CategoriaSeleccionada);
                if (index >= 0)
                {
                    Categorias[index] = categoriaEditada;
                }
                else
                {
                    var categoriaEnLista = Categorias.FirstOrDefault(c => c.Id == categoriaEditada.Id);
                    if (categoriaEnLista is not null)
                    {
                        var idx = Categorias.IndexOf(categoriaEnLista);
                        if (idx >= 0) Categorias[idx] = categoriaEditada;
                    }
                }

                foreach (var prod in Productos.Where(p => p.CategoriaId == categoriaEditada.Id))
                {
                    prod.CategoriaNombre = categoriaEditada.Nombre;
                }

                Categorias.ReordenarColeccion(c => c.Nombre);
            }
            else
            {
                var dtoCategoria = new CrearCategoriaDto
                {
                    Nombre = NombreCategoria
                };

                var categoria = await _categoriaService.CrearAsync(dtoCategoria);
                Categorias.InsertarOrdenado(categoria, c => c.Nombre);
            }

            SincronizarTableroKanban();
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

        ModoEdicionProducto = false;
    }

    [RelayCommand]
    private async Task GuardarProducto()
    {
        MensajeErrorProducto = null;
        NombreProducto = FormateadorTextoCatalogo.FormatearNombre(NombreProducto);
        PrecioProducto = FormateadorTextoCatalogo.RedondearPrecio(PrecioProducto);

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
                    Nombre = NombreProducto,
                    Precio = PrecioProducto,
                    CategoriaId = categoria.Id
                };

                var productoEditado = await _productoService.EditarAsync(dtoProducto);
                var index = Productos.IndexOf(ProductoSeleccionado);
                if (index >= 0)
                {
                    Productos[index] = productoEditado;
                }
                else
                {
                    var productoEnLista = Productos.FirstOrDefault(p => p.Id == productoEditado.Id);
                    if (productoEnLista is not null)
                    {
                        var idx = Productos.IndexOf(productoEnLista);
                        if (idx >= 0) Productos[idx] = productoEditado;
                    }
                }

                Productos.ReordenarColeccion(p => p.Nombre);
            }
            else
            {
                var dtoProducto = new CrearProductoDto
                {
                    Nombre = NombreProducto,
                    Precio = PrecioProducto,
                    CategoriaId = categoria.Id
                };

                var producto = await _productoService.CrearAsync(dtoProducto);
                Productos.InsertarOrdenado(producto, p => p.Nombre);
            }

            // Refrescar y ordenar productos de la categoría seleccionada
            if (CategoriaSeleccionada is not null)
            {
                await CargarProductosCategoriaAsync();
            }

            SincronizarTableroKanban();
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
        PrecioProductoTexto = producto.Precio.ToString("0.##", CultureInfo.InvariantCulture);
        CategoriaProductoSeleccionado = Categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);
        MensajeErrorProducto = null;

        MostrarFormulario = true;
        FormularioProducto = true;
        FormularioCategoria = false;
        FormularioExtra = false;

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

        ModoEdicionExtra = false;
    }

    [RelayCommand]
    private async Task EditarExtra(ExtraDto extra)
    {
        ExtraSeleccionado = extra;
        NombreExtra = extra.Nombre;
        PrecioExtra = extra.Precio;
        PrecioExtraTexto = extra.Precio.ToString("0.##", CultureInfo.InvariantCulture);
        MensajeErrorExtra = null;

        await PrepararCategoriasParaExtraAsync();

        MostrarFormulario = true;
        FormularioExtra = true;
        FormularioCategoria = false;
        FormularioProducto = false;

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
        MensajeErrorExtra = null;
        NombreExtra = FormateadorTextoCatalogo.FormatearNombre(NombreExtra);
        PrecioExtra = FormateadorTextoCatalogo.RedondearPrecio(PrecioExtra);

        if (string.IsNullOrWhiteSpace(NombreExtra))
        {
            MensajeErrorExtra = "El nombre del extra es obligatorio.";
            return;
        }

        if (PrecioExtra < 0)
        {
            MensajeErrorExtra = "El precio del extra no puede ser negativo.";
            return;
        }

        try
        {
            var categoriasSeleccionadas = CategoriasParaExtra
                .Where(c => c.IsChecked)
                .Select(c => c.Categoria.Id)
                .ToList();

            if (ModoEdicionExtra && ExtraSeleccionado is not null)
            {
                var dto = new EditarExtraDto
                {
                    Id = ExtraSeleccionado.Id,
                    Nombre = NombreExtra,
                    Precio = PrecioExtra
                };

                var extraEditado = await _extraService.EditarAsync(dto);
                var index = Extras.IndexOf(ExtraSeleccionado);
                if (index >= 0)
                {
                    Extras[index] = extraEditado;
                }
                else
                {
                    var extraEnLista = Extras.FirstOrDefault(e => e.Id == extraEditado.Id);
                    if (extraEnLista is not null)
                    {
                        var idx = Extras.IndexOf(extraEnLista);
                        if (idx >= 0) Extras[idx] = extraEditado;
                    }
                }

                Extras.ReordenarColeccion(e => e.Nombre);

                // Sincronizar las categorías asociadas a este extra sin afectar a los demás extras
                await _extraService.SincronizarCategoriasDeExtraAsync(extraEditado.Id, categoriasSeleccionadas);
            }
            else
            {
                var dto = new CrearExtraDto
                {
                    Nombre = NombreExtra,
                    Precio = PrecioExtra
                };

                // 1. Crear el Extra
                var nuevoExtra = await _extraService.CrearAsync(dto);

                // 2. Agregarlo a la colección observable de Extras manteniendo orden alfabético
                Extras.InsertarOrdenado(nuevoExtra, e => e.Nombre);

                // 3. Asociar este nuevo extra a las categorías seleccionadas sin desmarcar los extras existentes
                await _extraService.SincronizarCategoriasDeExtraAsync(nuevoExtra.Id, categoriasSeleccionadas);
            }

            // Refrescar los extras de la categoría actualmente seleccionada si la hay
            if (CategoriaSeleccionada is not null)
            {
                await CargarExtrasCategoriaAsync();
            }

            await RecargarExtrasPorCategoriaAsync();
            SincronizarTableroKanban();
            CancelarFormulario();
        }
        catch (DomainValidationException ex)
        {
            MensajeErrorExtra = ex.Message;
        }
        catch (Exception ex)
        {
            MensajeErrorExtra = $"Error: {ex.Message}";
        }
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

        await RecargarExtrasPorCategoriaAsync();
        SincronizarTableroKanban();
    }

    private async Task PrepararCategoriasParaExtraAsync()
    {
        CategoriasParaExtra.Clear();

        if (Categorias.Count == 0)
            return;

        int? extraId = ExtraSeleccionado?.Id;
        List<int> categoriasDelExtra = new();

        if (extraId.HasValue)
        {
            categoriasDelExtra = await _extraService.ObtenerCategoriaIdsPorExtraAsync(extraId.Value);
        }

        foreach (var categoria in Categorias.OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase))
        {
            CategoriasParaExtra.Add(new CategoriaCheckItem
            {
                Categoria = categoria,
                IsChecked = extraId.HasValue && categoriasDelExtra.Contains(categoria.Id)
            });
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

    [RelayCommand]
    private async Task GuardarConfiguracionImpresora()
    {
        try
        {
            await _configuracionRepository.GuardarValorAsync("Impresora_NombreTicket", ImpresoraSeleccionada ?? string.Empty);
            await _configuracionRepository.GuardarValorAsync("Impresora_NombreComanda", ImpresoraComandaSeleccionada ?? string.Empty);
            await _configuracionRepository.GuardarValorAsync("Impresora_AnchoPapel", EsPapel80mm ? "80mm" : "58mm");
            await _configuracionRepository.GuardarValorAsync("Impresora_AbrirCajon", AbrirCajonAutomatico ? "true" : "false");
            await _configuracionRepository.GuardarValorAsync("Impresora_CortarPapel", CortarPapelAutomatico ? "true" : "false");

            MensajeResultadoImpresora = "Configuración de hardware guardada correctamente.";
            TieneErrorImpresora = false;
            _dialogoService.NotificarExito("Configuración de impresora y cajón guardada.", "Hardware Guardado", 3);
        }
        catch (Exception ex)
        {
            MensajeResultadoImpresora = $"Error al guardar: {ex.Message}";
            TieneErrorImpresora = true;
            _dialogoService.MostrarMensaje($"Error al guardar configuración: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private async Task ProbarImpresion()
    {
        if (string.IsNullOrWhiteSpace(ImpresoraSeleccionada))
        {
            MensajeResultadoImpresora = "Debes seleccionar una impresora en la lista primero.";
            TieneErrorImpresora = true;
            _dialogoService.MostrarMensaje("Selecciona una impresora térmica antes de imprimir.", "Aviso");
            return;
        }

        try
        {
            await _printerService.ImprimirTicketPruebaAsync(ImpresoraSeleccionada, EsPapel80mm ? "80mm" : "58mm");
            MensajeResultadoImpresora = "Ticket de prueba enviado exitosamente.";
            TieneErrorImpresora = false;
            _dialogoService.NotificarExito("Ticket de prueba enviado al spooler.", "Éxito");
        }
        catch (Exception ex)
        {
            MensajeResultadoImpresora = $"Error: {ex.Message}";
            TieneErrorImpresora = true;
            _dialogoService.MostrarMensaje(
                $"Error al imprimir ticket de prueba:\n\n{ex.Message}\n\n(Verifique que la impresora esté encendida y conectada)",
                "Fallo de Hardware");
        }
    }

    [RelayCommand]
    private async Task ProbarAperturaCajon()
    {
        try
        {
            await _printerService.AbrirCajonDineroAsync();
            MensajeResultadoImpresora = "Señal de apertura enviada al cajón.";
            TieneErrorImpresora = false;
            _dialogoService.NotificarExito("Señal enviada al cajón de dinero.", "Cajón");
        }
        catch (Exception ex)
        {
            MensajeResultadoImpresora = $"Error: {ex.Message}";
            TieneErrorImpresora = true;
            _dialogoService.MostrarMensaje(
                $"Error al intentar abrir el cajón de dinero:\n\n{ex.Message}\n\n(Verifique que la impresora térmica esté conectada)",
                "Fallo de Hardware");
        }
    }
}

public partial class CategoriaCheckItem : ObservableObject
{
    public required CategoriaDto Categoria { get; init; }

    [ObservableProperty]
    private bool isChecked;
}

public partial class CategoriaKanbanItem : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EtiquetaPastilla))]
    private CategoriaDto categoria = null!;

    public ObservableCollection<ProductoDto> Productos { get; } = new();
    public ObservableCollection<ExtraDto> Extras { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EtiquetaPastilla))]
    private int totalProductosCategoria;

    [ObservableProperty]
    private bool estaOculta;

    [ObservableProperty]
    private bool estaEnVentanaActual;

    [ObservableProperty]
    private int ordenEnVentanaActual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonExtras))]
    [NotifyPropertyChangedFor(nameof(TextoBotonAgregarAqui))]
    private bool mostrarVistaExtras;

    public string EtiquetaPastilla => $"{Categoria?.Nombre} ({TotalProductosCategoria})";
    public string TextoBotonExtras => MostrarVistaExtras ? "Productos" : "Extras";
    public string TextoBotonAgregarAqui => MostrarVistaExtras ? "+ Agregar Extra" : "+ Agregar aquí";
    public bool TieneProductos => Productos.Count > 0;
    public bool TieneExtras => Extras.Count > 0;

    public void NotificarCambiosColecciones()
    {
        OnPropertyChanged(nameof(EtiquetaPastilla));
        OnPropertyChanged(nameof(TextoBotonExtras));
        OnPropertyChanged(nameof(TextoBotonAgregarAqui));
        OnPropertyChanged(nameof(TieneProductos));
        OnPropertyChanged(nameof(TieneExtras));
    }
}