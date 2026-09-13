using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Catalogo;
using Core.Application.Dtos.Comandas;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces.Services;
using Presentation.WPF.Services;
using System.Collections.ObjectModel;

namespace Presentation.WPF.ViewModels;

/// <summary>
/// Opción del selector de cuentas en comanda (Nueva cuenta o cuenta abierta existente).
/// </summary>
public class CuentaOpcionSelector
{
    public int? VentaId { get; }
    public string Titulo { get; }
    public bool EsNuevaCuenta => !VentaId.HasValue;

    public CuentaOpcionSelector()
    {
        VentaId = null;
        Titulo = "Nueva Cuenta";
    }

    public CuentaOpcionSelector(VentaResumenDto venta)
    {
        VentaId = venta.Id;
        Titulo = $"Ticket #{venta.Id} - {venta.IdentificadorCliente} (${venta.Total:F2})";
    }
}

/// <summary>
/// Modelo reactivo para la selección de extras dentro del modal de personalización.
/// </summary>
public partial class ExtraSeleccionableViewModel : ObservableObject
{
    public ExtraDto Extra { get; }
    public int Id => Extra.Id;
    public string Nombre => Extra.Nombre;
    public decimal Precio => Extra.Precio;

    [ObservableProperty]
    private bool seleccionado;

    public ExtraSeleccionableViewModel(ExtraDto extra, bool seleccionado = false)
    {
        Extra = extra;
        Seleccionado = seleccionado;
    }
}

/// <summary>
/// Modelo de ítem individual de la comanda en preparación, con soporte para extras y notas.
/// </summary>
public partial class ComandaItemViewModel : ObservableObject
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public ObservableCollection<ExtraDto> Extras { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int cantidad = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneNota))]
    private string? nota;

    [ObservableProperty]
    private bool mostrarComentario;

    public decimal SubtotalExtras => Extras.Sum(e => e.Precio);

    public decimal Subtotal => (Precio + SubtotalExtras) * Cantidad;

    public bool TieneNota => !string.IsNullOrWhiteSpace(Nota);

    public bool TieneExtras => Extras.Count > 0;

    public string ResumenExtras => Extras.Count == 0 ? string.Empty : string.Join(", ", Extras.Select(e => $"+ {e.Nombre} (${e.Precio:F2})"));

    public void NotificarCambioExtras()
    {
        OnPropertyChanged(nameof(SubtotalExtras));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(TieneExtras));
        OnPropertyChanged(nameof(ResumenExtras));
    }
}

/// <summary>
/// ViewModel para la toma de comandas, selección de productos, personalización con extras y apertura/acumulación de cuentas.
/// </summary>
public partial class ComandaViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly IExtraService _extraService;
    private readonly IVentaService _ventaService;
    private readonly IComandaService _comandaService;
    private readonly IDialogoService _dialogoService;

    public ObservableCollection<ProductoDto> ProductoCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ComandaItemViewModel> ItemsComanda { get; } = new();

    public ObservableCollection<ExtraDto> ExtrasCategoriaSeleccionada { get; } = new();

    public ObservableCollection<ExtraSeleccionableViewModel> ExtrasDisponiblesModal { get; } = new();

    public ObservableCollection<CuentaOpcionSelector> CuentasDisponibles { get; } = new();

    [ObservableProperty]
    private CuentaOpcionSelector? cuentaSeleccionadaOpcion;

    [ObservableProperty]
    private ComandaItemViewModel? itemSeleccionado;

    public decimal TotalComanda => ItemsComanda.Sum(item => item.Subtotal);

    [ObservableProperty]
    private CategoriaDto? categoriaSeleccionada;

    // Estado para el modal de personalización de extras
    [ObservableProperty]
    private bool mostrarModalExtras;

    [ObservableProperty]
    private ProductoDto? productoParaPersonalizar;

    [ObservableProperty]
    private ComandaItemViewModel? itemParaEditarExtras;

    [ObservableProperty]
    private string notaPersonalizada = string.Empty;

    // Estado para el modal de apertura de cuenta al finalizar
    [ObservableProperty]
    private bool mostrarModalAbrirCuenta;

    [ObservableProperty]
    private string identificadorCliente = "General";

    [ObservableProperty]
    private bool estaProcesando;

    public decimal SubtotalPersonalizacion
    {
        get
        {
            decimal precioBase = ProductoParaPersonalizar?.Precio ?? (ItemParaEditarExtras?.Precio ?? 0m);
            decimal extras = ExtrasDisponiblesModal.Where(e => e.Seleccionado).Sum(e => e.Precio);
            return precioBase + extras;
        }
    }

    public event Action? RegresarACategorias;

    public ComandaViewModel(
        IProductoService productoService, 
        IExtraService extraService,
        IVentaService ventaService,
        IComandaService comandaService,
        IDialogoService dialogoService,
        CategoriaDto categoria)
    {
        _productoService = productoService;
        _extraService = extraService;
        _ventaService = ventaService;
        _comandaService = comandaService;
        _dialogoService = dialogoService;
        CategoriaSeleccionada = categoria;
    }

    public async Task CargarProductosAsync()
    {
        if (CategoriaSeleccionada is null) return;

        var productos = await _productoService.ObtenerPorCategoriaAsync(CategoriaSeleccionada.Id);
        var extras = await _extraService.ObtenerPorCategoriaAsync(CategoriaSeleccionada.Id);

        ProductoCategoriaSeleccionada.Clear();
        ExtrasCategoriaSeleccionada.Clear();

        foreach (var producto in productos)
        {
            ProductoCategoriaSeleccionada.Add(producto);
        }

        foreach (var extra in extras)
        {
            ExtrasCategoriaSeleccionada.Add(extra);
        }

        await CargarCuentasDisponiblesAsync();
    }

    [RelayCommand]
    public async Task CargarCuentasDisponiblesAsync()
    {
        try
        {
            var pendientes = await _ventaService.ObtenerPendientesAsync();
            int? idSeleccionado = CuentaSeleccionadaOpcion?.VentaId;

            CuentasDisponibles.Clear();
            CuentasDisponibles.Add(new CuentaOpcionSelector()); // Nueva Cuenta

            foreach (var venta in pendientes.OrderByDescending(v => v.FechaCreacion))
            {
                CuentasDisponibles.Add(new CuentaOpcionSelector(venta));
            }

            if (idSeleccionado.HasValue)
            {
                CuentaSeleccionadaOpcion = CuentasDisponibles.FirstOrDefault(c => c.VentaId == idSeleccionado.Value)
                                          ?? CuentasDisponibles.FirstOrDefault();
            }
            else
            {
                CuentaSeleccionadaOpcion = CuentasDisponibles.FirstOrDefault();
            }
        }
        catch
        {
            if (CuentasDisponibles.Count == 0)
            {
                CuentasDisponibles.Add(new CuentaOpcionSelector());
                CuentaSeleccionadaOpcion = CuentasDisponibles.FirstOrDefault();
            }
        }
    }

    public async Task CambiarCategoriaAsync(CategoriaDto categoria)
    {
        CategoriaSeleccionada = categoria;
        await CargarProductosAsync();
    }

    public void ActualizarTotal()
    {
        OnPropertyChanged(nameof(TotalComanda));
    }

    [RelayCommand]
    private void AgregarProducto(ProductoDto producto)
    {
        // Si la categoría no tiene extras disponibles, se agrega directo
        if (ExtrasCategoriaSeleccionada.Count == 0)
        {
            var itemExistente = ItemsComanda.FirstOrDefault(i => i.ProductoId == producto.Id && !i.TieneExtras);
            if (itemExistente is not null)
            {
                itemExistente.Cantidad++;
                ActualizarTotal();
                return;
            }

            ItemsComanda.Add(new ComandaItemViewModel
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Cantidad = 1
            });

            ActualizarTotal();
            return;
        }

        // Si la categoría tiene extras, se abre el modal de personalización
        ProductoParaPersonalizar = producto;
        ItemParaEditarExtras = null;
        NotaPersonalizada = string.Empty;

        ExtrasDisponiblesModal.Clear();
        foreach (var extra in ExtrasCategoriaSeleccionada)
        {
            var vm = new ExtraSeleccionableViewModel(extra, false);
            vm.PropertyChanged += (_, _) => OnPropertyChanged(nameof(SubtotalPersonalizacion));
            ExtrasDisponiblesModal.Add(vm);
        }

        OnPropertyChanged(nameof(SubtotalPersonalizacion));
        MostrarModalExtras = true;
    }

    [RelayCommand]
    private void AlternarSeleccionExtra(ExtraSeleccionableViewModel extraVm)
    {
        extraVm.Seleccionado = !extraVm.Seleccionado;
        OnPropertyChanged(nameof(SubtotalPersonalizacion));
    }

    [RelayCommand]
    private void ConfirmarExtrasModal()
    {
        var extrasElegidos = ExtrasDisponiblesModal
            .Where(e => e.Seleccionado)
            .Select(e => e.Extra)
            .ToList();

        if (ProductoParaPersonalizar is not null)
        {
            var nuevoItem = new ComandaItemViewModel
            {
                ProductoId = ProductoParaPersonalizar.Id,
                Nombre = ProductoParaPersonalizar.Nombre,
                Precio = ProductoParaPersonalizar.Precio,
                Cantidad = 1,
                Nota = string.IsNullOrWhiteSpace(NotaPersonalizada) ? null : NotaPersonalizada.Trim()
            };

            foreach (var extra in extrasElegidos)
            {
                nuevoItem.Extras.Add(extra);
            }

            nuevoItem.NotificarCambioExtras();
            ItemsComanda.Add(nuevoItem);
        }
        else if (ItemParaEditarExtras is not null)
        {
            ItemParaEditarExtras.Extras.Clear();
            foreach (var extra in extrasElegidos)
            {
                ItemParaEditarExtras.Extras.Add(extra);
            }

            if (!string.IsNullOrWhiteSpace(NotaPersonalizada))
            {
                ItemParaEditarExtras.Nota = NotaPersonalizada.Trim();
            }

            ItemParaEditarExtras.NotificarCambioExtras();
        }

        ActualizarTotal();
        MostrarModalExtras = false;
        ProductoParaPersonalizar = null;
        ItemParaEditarExtras = null;
    }

    [RelayCommand]
    private void AgregarSinExtrasModal()
    {
        if (ProductoParaPersonalizar is not null)
        {
            var itemExistente = ItemsComanda.FirstOrDefault(i => i.ProductoId == ProductoParaPersonalizar.Id && !i.TieneExtras);
            if (itemExistente is not null)
            {
                itemExistente.Cantidad++;
            }
            else
            {
                ItemsComanda.Add(new ComandaItemViewModel
                {
                    ProductoId = ProductoParaPersonalizar.Id,
                    Nombre = ProductoParaPersonalizar.Nombre,
                    Precio = ProductoParaPersonalizar.Precio,
                    Cantidad = 1,
                    Nota = string.IsNullOrWhiteSpace(NotaPersonalizada) ? null : NotaPersonalizada.Trim()
                });
            }

            ActualizarTotal();
        }

        MostrarModalExtras = false;
        ProductoParaPersonalizar = null;
        ItemParaEditarExtras = null;
    }

    [RelayCommand]
    private void CancelarModalExtras()
    {
        MostrarModalExtras = false;
        ProductoParaPersonalizar = null;
        ItemParaEditarExtras = null;
    }

    [RelayCommand]
    private void DisminuirCantidad(ComandaItemViewModel item)
    {
        if (item.Cantidad <= 1) return;
        item.Cantidad--;
        ActualizarTotal();
    }

    [RelayCommand]
    private void EliminarProducto(ComandaItemViewModel item)
    {
        ItemsComanda.Remove(item);
        ActualizarTotal();
    }

    [RelayCommand]
    private void LimpiarComanda()
    {
        ItemsComanda.Clear();
        ActualizarTotal();
    }

    [RelayCommand]
    private async Task SolicitarFinalizarComanda()
    {
        if (ItemsComanda.Count == 0)
        {
            _dialogoService.MostrarMensaje("No hay productos en la comanda para procesar.", "Comanda vacia");
            return;
        }

        if (CuentaSeleccionadaOpcion is null || CuentaSeleccionadaOpcion.EsNuevaCuenta)
        {
            IdentificadorCliente = "General";
            MostrarModalAbrirCuenta = true;
        }
        else
        {
            bool confirmar = _dialogoService.Confirmar(
                $"Se agregaran estos productos a la cuenta existente:\n{CuentaSeleccionadaOpcion.Titulo}\n\nMonto a acumular: ${TotalComanda:F2}\n\n¿Deseas continuar?",
                "Confirmar acumulacion");

            if (!confirmar) return;

            await AgregarProductosACuentaExistenteAsync(CuentaSeleccionadaOpcion.VentaId!.Value);
        }
    }

    private async Task AgregarProductosACuentaExistenteAsync(int ventaId)
    {
        EstaProcesando = true;
        try
        {
            foreach (var item in ItemsComanda)
            {
                var dto = new AgregarItemVentaDto
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    Notas = item.Nota,
                    ExtraIds = item.Extras.Select(e => e.Id).ToList()
                };

                await _ventaService.AgregarItemAsync(ventaId, dto);
            }

            var comandaItems = ItemsComanda.Select(i => new EnviarComandaItemDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                NotasCocina = i.Nota,
                ExtraInstrucciones = i.Extras.Select(e => e.Nombre).ToList()
            }).ToList();

            await _comandaService.EnviarACocinaAsync(ventaId, comandaItems);

            ItemsComanda.Clear();
            ActualizarTotal();

            await CargarCuentasDisponiblesAsync();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al agregar productos a la cuenta: {ex.Message}", "Error");
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    [RelayCommand]
    private void CancelarAbrirCuenta()
    {
        MostrarModalAbrirCuenta = false;
    }

    [RelayCommand]
    private async Task ConfirmarAbrirCuentaAsync()
    {
        if (ItemsComanda.Count == 0) return;

        EstaProcesando = true;
        MostrarModalAbrirCuenta = false;

        try
        {
            string cliente = string.IsNullOrWhiteSpace(IdentificadorCliente) ? "General" : IdentificadorCliente.Trim();

            // 1. Crear la cuenta abierta en el dominio (Estado = Pendiente)
            var venta = await _ventaService.CrearCuentaAsync(cliente);

            // 2. Agregar los ítems con sus extras y notas congelados
            foreach (var item in ItemsComanda)
            {
                var dto = new AgregarItemVentaDto
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    Notas = item.Nota,
                    ExtraIds = item.Extras.Select(e => e.Id).ToList()
                };

                await _ventaService.AgregarItemAsync(venta.Id, dto);
            }

            // 3. Emitir comanda operativa a cocina
            var comandaItems = ItemsComanda.Select(i => new EnviarComandaItemDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                NotasCocina = i.Nota,
                ExtraInstrucciones = i.Extras.Select(e => e.Nombre).ToList()
            }).ToList();

            await _comandaService.EnviarACocinaAsync(venta.Id, comandaItems);

            // 4. Limpiar comanda activa y recargar cuentas disponibles
            ItemsComanda.Clear();
            ActualizarTotal();

            await CargarCuentasDisponiblesAsync();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarMensaje($"Error al abrir la cuenta: {ex.Message}", "Error");
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    [RelayCommand]
    private void Regresar()
    {
        RegresarACategorias?.Invoke();
    }
}