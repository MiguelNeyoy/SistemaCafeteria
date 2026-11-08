using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces.Services;
using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Presentation.WPF.Services;

namespace Presentation.WPF.ViewModels;

public class VentaDetalleLineaViewModel
{
    public int Cantidad { get; init; }
    public string ProductoNombre { get; init; } = string.Empty;
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
    public string ExtrasTexto { get; init; } = string.Empty;
    public bool TieneExtras => !string.IsNullOrWhiteSpace(ExtrasTexto);
    public string NotasTexto { get; init; } = string.Empty;
    public bool TieneNotas => !string.IsNullOrWhiteSpace(NotasTexto);
}

public class VentaHistorialItemViewModel
{
    public VentaResumenDto Dto { get; }

    public int Id => Dto.Id;
    public string FolioTexto => $"#{Dto.Id}";
    public DateTime FechaReferencia => Dto.FechaCierre ?? Dto.FechaCreacion;
    public string FechaTexto { get; }
    public string HoraTexto { get; }
    public string Cliente => string.IsNullOrWhiteSpace(Dto.IdentificadorCliente) ? "General" : Dto.IdentificadorCliente;
    public string ResumenProductos { get; }
    public int CantidadItems { get; }
    public EstadoVenta Estado => Dto.Estado;
    public string EstadoTexto { get; }
    public TipoDePago? TipoDePago => Dto.TipoDePago;
    public string TipoPagoTexto { get; }
    public decimal Subtotal => Dto.Subtotal;
    public decimal Descuento => Dto.Descuento;
    public bool TieneDescuento => Dto.Descuento > 0m;
    public decimal Total => Dto.Total;
    public bool EsEditable => Dto.Estado == EstadoVenta.Pagado;
    public string TextoAccion => EsEditable ? "Ver / Editar" : "Ver Detalle";

    public VentaHistorialItemViewModel(VentaResumenDto dto, CultureInfo cultura)
    {
        Dto = dto;
        FechaTexto = FechaReferencia.ToString("dd/MM/yyyy", cultura);
        HoraTexto = FechaReferencia.ToString("hh:mm tt", cultura);
        CantidadItems = dto.Items.Sum(i => i.Cantidad);

        if (dto.Items.Count == 0)
        {
            ResumenProductos = "Sin productos";
        }
        else
        {
            var partes = dto.Items
                .Take(3)
                .Select(i => $"{i.Cantidad}x {i.ProductoNombre}")
                .ToList();

            string textoBase = string.Join(", ", partes);
            if (dto.Items.Count > 3)
            {
                textoBase += $" (+{dto.Items.Count - 3} más)";
            }

            ResumenProductos = textoBase;
        }

        EstadoTexto = dto.Estado switch
        {
            EstadoVenta.Pagado => "Pagada",
            EstadoVenta.Cancelado => "Cancelada",
            EstadoVenta.Devuelto => "Devuelta",
            EstadoVenta.Pendiente => "Pendiente",
            _ => dto.Estado.ToString()
        };

        TipoPagoTexto = dto.Estado == EstadoVenta.Pagado
            ? (dto.TipoDePago?.ToString() ?? "Efectivo")
            : "—";
    }
}

public partial class HistorialVentasViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly IDialogoService _dialogoService;
    private readonly CultureInfo _cultura = new("es-MX");

    private List<VentaResumenDto> _ventasCargadas = new();
    private bool _suprimirSanitizacion;

    public event Action? RegresarSolicitado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string _periodoActivo = "Hoy";

    [ObservableProperty]
    private DateTime _fechaDesde = DateTime.Today;

    [ObservableProperty]
    private DateTime _fechaHasta = DateTime.Today;

    [ObservableProperty]
    private string _descripcionPeriodo = string.Empty;

    [ObservableProperty]
    private string _filtroMetodoPago = "Todos";

    [ObservableProperty]
    private string _textoBusqueda = string.Empty;

    // KPIs del periodo cargado (ventas pagadas)
    [ObservableProperty]
    private decimal _totalVentasPeriodo;

    [ObservableProperty]
    private decimal _totalEfectivoPeriodo;

    [ObservableProperty]
    private decimal _totalTarjetaPeriodo;

    [ObservableProperty]
    private decimal _totalTransferenciaPeriodo;

    [ObservableProperty]
    private decimal _totalDescuentosPeriodo;

    [ObservableProperty]
    private int _cantidadPagadasPeriodo;

    [ObservableProperty]
    private int _cantidadCanceladasPeriodo;

    [ObservableProperty]
    private bool _tieneVentasFiltradas;

    // Estado del Modal de Detalle y Edición
    [ObservableProperty]
    private bool _modalDetalleAbierto;

    [ObservableProperty]
    private VentaResumenDto? _ventaSeleccionada;

    [ObservableProperty]
    private string _folioModalTexto = string.Empty;

    [ObservableProperty]
    private string _fechaModalTexto = string.Empty;

    [ObservableProperty]
    private string _estadoModalTexto = string.Empty;

    [ObservableProperty]
    private bool _esVentaEditable;

    [ObservableProperty]
    private string _clienteEdicion = string.Empty;

    [ObservableProperty]
    private TipoDePago _tipoPagoEdicion = TipoDePago.Efectivo;

    [ObservableProperty]
    private string _descuentoEdicionTexto = "0";

    [ObservableProperty]
    private string _montoRecibidoEdicionTexto = string.Empty;

    [ObservableProperty]
    private decimal _subtotalModal;

    [ObservableProperty]
    private decimal _totalCalculadoEdicion;

    [ObservableProperty]
    private decimal _cambioCalculadoEdicion;

    [ObservableProperty]
    private string _mensajeErrorEdicion = string.Empty;

    public bool TieneErrorEdicion => !string.IsNullOrWhiteSpace(MensajeErrorEdicion);

    public bool EsPeriodoHoy => PeriodoActivo == "Hoy";
    public bool EsPeriodoAyer => PeriodoActivo == "Ayer";
    public bool EsPeriodoSemana => PeriodoActivo == "Semana";
    public bool EsPeriodoMes => PeriodoActivo == "Mes";
    public bool EsPeriodoPersonalizado => PeriodoActivo == "Personalizado";

    public bool EsFiltroPagoTodos => FiltroMetodoPago == "Todos";
    public bool EsFiltroPagoEfectivo => FiltroMetodoPago == "Efectivo";
    public bool EsFiltroPagoTarjeta => FiltroMetodoPago == "Tarjeta";
    public bool EsFiltroPagoTransferencia => FiltroMetodoPago == "Transferencia";

    public bool TieneTextoBusqueda => !string.IsNullOrWhiteSpace(TextoBusqueda);

    public bool EsPagoEdicionEfectivo => TipoPagoEdicion == TipoDePago.Efectivo;
    public bool EsPagoEdicionTarjeta => TipoPagoEdicion == TipoDePago.Tarjeta;
    public bool EsPagoEdicionTransferencia => TipoPagoEdicion == TipoDePago.Transferencia;

    public ObservableCollection<VentaHistorialItemViewModel> VentasFiltradas { get; } = new();
    public ObservableCollection<VentaDetalleLineaViewModel> LineasDetalleModal { get; } = new();

    public HistorialVentasViewModel(
        IVentaService ventaService,
        IDialogoService dialogoService)
    {
        _ventaService = ventaService;
        _dialogoService = dialogoService;
    }

    partial void OnPeriodoActivoChanged(string value)
    {
        OnPropertyChanged(nameof(EsPeriodoHoy));
        OnPropertyChanged(nameof(EsPeriodoAyer));
        OnPropertyChanged(nameof(EsPeriodoSemana));
        OnPropertyChanged(nameof(EsPeriodoMes));
        OnPropertyChanged(nameof(EsPeriodoPersonalizado));
    }

    partial void OnFiltroMetodoPagoChanged(string value)
    {
        OnPropertyChanged(nameof(EsFiltroPagoTodos));
        OnPropertyChanged(nameof(EsFiltroPagoEfectivo));
        OnPropertyChanged(nameof(EsFiltroPagoTarjeta));
        OnPropertyChanged(nameof(EsFiltroPagoTransferencia));
        AplicarFiltrosEnMemoria();
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        OnPropertyChanged(nameof(TieneTextoBusqueda));
        AplicarFiltrosEnMemoria();
    }

    partial void OnMensajeErrorEdicionChanged(string value)
    {
        OnPropertyChanged(nameof(TieneErrorEdicion));
    }

    partial void OnTipoPagoEdicionChanged(TipoDePago value)
    {
        OnPropertyChanged(nameof(EsPagoEdicionEfectivo));
        OnPropertyChanged(nameof(EsPagoEdicionTarjeta));
        OnPropertyChanged(nameof(EsPagoEdicionTransferencia));

        MensajeErrorEdicion = string.Empty;

        if (value == TipoDePago.Efectivo && !_suprimirSanitizacion)
        {
            if (!decimal.TryParse(MontoRecibidoEdicionTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out var actual) ||
                actual < TotalCalculadoEdicion)
            {
                _suprimirSanitizacion = true;
                MontoRecibidoEdicionTexto = TotalCalculadoEdicion.ToString("0.##", CultureInfo.InvariantCulture);
                _suprimirSanitizacion = false;
            }
        }

        RecalcularVistaPreviaEdicion();
    }

    partial void OnClienteEdicionChanged(string value)
    {
        if (!_suprimirSanitizacion)
        {
            MensajeErrorEdicion = string.Empty;
        }
    }

    partial void OnDescuentoEdicionTextoChanged(string value)
    {
        if (_suprimirSanitizacion)
        {
            return;
        }

        bool intentoNegativo = !string.IsNullOrEmpty(value) && value.Contains('-');
        string limpio = FormateadorTextoCatalogo.SanitizarTextoPrecio(value);

        if (limpio != value)
        {
            _suprimirSanitizacion = true;
            DescuentoEdicionTexto = limpio;
            _suprimirSanitizacion = false;
        }

        MensajeErrorEdicion = intentoNegativo
            ? "No se permiten descuentos negativos."
            : string.Empty;

        RecalcularVistaPreviaEdicion();
    }

    partial void OnMontoRecibidoEdicionTextoChanged(string value)
    {
        if (_suprimirSanitizacion)
        {
            return;
        }

        bool intentoNegativo = !string.IsNullOrEmpty(value) && value.Contains('-');
        string limpio = FormateadorTextoCatalogo.SanitizarTextoPrecio(value);

        if (limpio != value)
        {
            _suprimirSanitizacion = true;
            MontoRecibidoEdicionTexto = limpio;
            _suprimirSanitizacion = false;
        }

        MensajeErrorEdicion = intentoNegativo
            ? "No se permiten montos negativos."
            : string.Empty;

        RecalcularVistaPreviaEdicion();
    }

    public async Task InicializarHoyAsync()
    {
        PeriodoActivo = "Hoy";
        FechaDesde = DateTime.Today;
        FechaHasta = DateTime.Today;
        FiltroMetodoPago = "Todos";
        TextoBusqueda = string.Empty;
        ModalDetalleAbierto = false;
        await CargarVentasRangoAsync();
    }

    [RelayCommand]
    private async Task SeleccionarPeriodoAsync(string? periodo)
    {
        if (string.IsNullOrWhiteSpace(periodo))
        {
            return;
        }

        var hoy = DateTime.Today;
        PeriodoActivo = periodo;

        switch (periodo)
        {
            case "Hoy":
                FechaDesde = hoy;
                FechaHasta = hoy;
                break;
            case "Ayer":
                FechaDesde = hoy.AddDays(-1);
                FechaHasta = hoy.AddDays(-1);
                break;
            case "Semana":
                FechaDesde = hoy.AddDays(-6);
                FechaHasta = hoy;
                break;
            case "Mes":
                FechaDesde = new DateTime(hoy.Year, hoy.Month, 1);
                FechaHasta = hoy;
                break;
        }

        await CargarVentasRangoAsync();
    }

    [RelayCommand]
    private async Task AplicarRangoPersonalizadoAsync()
    {
        if (FechaHasta.Date < FechaDesde.Date)
        {
            _dialogoService.MostrarAdvertencia(
                "La fecha final no puede ser anterior a la fecha inicial.",
                "Rango de Fechas Inválido");
            return;
        }

        PeriodoActivo = "Personalizado";
        await CargarVentasRangoAsync();
    }

    [RelayCommand]
    public async Task CargarVentasRangoAsync()
    {
        if (EstaCargando)
        {
            return;
        }

        try
        {
            EstaCargando = true;

            var inicio = FechaDesde.Date;
            var fin = FechaHasta.Date;
            if (fin < inicio)
            {
                fin = inicio;
                FechaHasta = inicio;
            }

            DescripcionPeriodo = inicio == fin
                ? inicio.ToString("dddd, dd 'de' MMMM 'de' yyyy", _cultura)
                : $"Del {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}";

            _ventasCargadas = await _ventaService.ObtenerPorRangoFechasAsync(inicio, fin);

            RecalcularTotalesPeriodo();
            AplicarFiltrosEnMemoria();
        }
        catch (Exception ex)
        {
            _dialogoService.MostrarError($"Error al consultar el historial de ventas: {ex.Message}", "Historial de Ventas");
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private void CambiarFiltroMetodoPago(string? metodo)
    {
        if (!string.IsNullOrWhiteSpace(metodo))
        {
            FiltroMetodoPago = metodo;
        }
    }

    [RelayCommand]
    private void LimpiarBusqueda()
    {
        TextoBusqueda = string.Empty;
    }

    [RelayCommand]
    private void AbrirDetalleVenta(VentaHistorialItemViewModel? item)
    {
        if (item == null)
        {
            return;
        }

        var dto = item.Dto;
        VentaSeleccionada = dto;
        FolioModalTexto = $"Venta #{dto.Id}";
        var fechaRef = dto.FechaCierre ?? dto.FechaCreacion;
        FechaModalTexto = fechaRef.ToString("dd/MM/yyyy hh:mm tt", _cultura);
        EstadoModalTexto = item.EstadoTexto;
        EsVentaEditable = dto.Estado == EstadoVenta.Pagado;
        SubtotalModal = dto.Subtotal;

        LineasDetalleModal.Clear();
        foreach (var linea in dto.Items)
        {
            string extras = linea.Extras.Count > 0
                ? string.Join(", ", linea.Extras.Select(e => $"+ {e.Nombre} ({e.Precio:C})"))
                : string.Empty;

            LineasDetalleModal.Add(new VentaDetalleLineaViewModel
            {
                Cantidad = linea.Cantidad,
                ProductoNombre = linea.ProductoNombre,
                PrecioUnitario = linea.PrecioUnitario,
                Subtotal = linea.Subtotal,
                ExtrasTexto = extras,
                NotasTexto = linea.Notas ?? string.Empty
            });
        }

        _suprimirSanitizacion = true;
        ClienteEdicion = string.IsNullOrWhiteSpace(dto.IdentificadorCliente) ? "General" : dto.IdentificadorCliente;
        TipoPagoEdicion = dto.TipoDePago ?? TipoDePago.Efectivo;
        DescuentoEdicionTexto = dto.Descuento > 0m
            ? dto.Descuento.ToString("0.##", CultureInfo.InvariantCulture)
            : "0";
        decimal recibidoInicial = dto.MontoRecibido ?? dto.Total;
        MontoRecibidoEdicionTexto = recibidoInicial.ToString("0.##", CultureInfo.InvariantCulture);
        MensajeErrorEdicion = string.Empty;
        _suprimirSanitizacion = false;

        OnPropertyChanged(nameof(EsPagoEdicionEfectivo));
        OnPropertyChanged(nameof(EsPagoEdicionTarjeta));
        OnPropertyChanged(nameof(EsPagoEdicionTransferencia));

        RecalcularVistaPreviaEdicion();
        ModalDetalleAbierto = true;
    }

    [RelayCommand]
    private void SeleccionarMetodoPagoEdicion(string? metodo)
    {
        if (string.IsNullOrWhiteSpace(metodo))
        {
            return;
        }

        if (Enum.TryParse<TipoDePago>(metodo, ignoreCase: true, out var parsed))
        {
            TipoPagoEdicion = parsed;
        }
    }

    [RelayCommand]
    private void CerrarModalDetalle()
    {
        ModalDetalleAbierto = false;
        MensajeErrorEdicion = string.Empty;
    }

    [RelayCommand]
    private async Task GuardarCorreccionPagoAsync()
    {
        if (VentaSeleccionada == null || !EsVentaEditable)
        {
            return;
        }

        MensajeErrorEdicion = string.Empty;

        string clienteLimpio = FormateadorTextoCatalogo.FormatearNombre(ClienteEdicion);
        if (string.IsNullOrWhiteSpace(clienteLimpio))
        {
            MensajeErrorEdicion = "El nombre del cliente no puede quedar en blanco.";
            return;
        }

        decimal descuento = 0m;
        if (!string.IsNullOrWhiteSpace(DescuentoEdicionTexto))
        {
            if (!decimal.TryParse(DescuentoEdicionTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out descuento))
            {
                MensajeErrorEdicion = "El descuento ingresado no tiene un formato numérico válido.";
                return;
            }
        }

        if (descuento < 0m)
        {
            MensajeErrorEdicion = "El descuento no puede ser un número negativo.";
            return;
        }

        if (descuento > SubtotalModal)
        {
            MensajeErrorEdicion = $"El descuento ({descuento:C}) no puede superar el subtotal ({SubtotalModal:C}).";
            return;
        }

        decimal totalEsperado = Math.Max(0m, SubtotalModal - Math.Round(descuento, 2));
        decimal? montoRecibido = null;

        if (TipoPagoEdicion == TipoDePago.Efectivo)
        {
            if (string.IsNullOrWhiteSpace(MontoRecibidoEdicionTexto) ||
                !decimal.TryParse(MontoRecibidoEdicionTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out var recibidoParsed))
            {
                MensajeErrorEdicion = "Ingresa un monto recibido válido para el pago en efectivo.";
                return;
            }

            if (recibidoParsed < 0m)
            {
                MensajeErrorEdicion = "El monto recibido no puede ser negativo.";
                return;
            }

            if (Math.Round(recibidoParsed, 2) < totalEsperado)
            {
                MensajeErrorEdicion = $"El monto recibido ({recibidoParsed:C}) no cubre el total de la venta ({totalEsperado:C}).";
                return;
            }

            montoRecibido = Math.Round(recibidoParsed, 2);
        }

        try
        {
            EstaCargando = true;

            var actualizada = await _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
            {
                VentaId = VentaSeleccionada.Id,
                IdentificadorCliente = clienteLimpio,
                TipoDePago = TipoPagoEdicion,
                Descuento = Math.Round(descuento, 2),
                MontoRecibido = montoRecibido
            });

            int idx = _ventasCargadas.FindIndex(v => v.Id == actualizada.Id);
            if (idx >= 0)
            {
                _ventasCargadas[idx] = actualizada;
            }

            RecalcularTotalesPeriodo();
            AplicarFiltrosEnMemoria();

            ModalDetalleAbierto = false;
            _dialogoService.NotificarExito(
                $"La venta #{actualizada.Id} se actualizó correctamente ({actualizada.TipoDePago}, {actualizada.Total:C}).",
                "Venta Actualizada");
        }
        catch (DomainValidationException dvEx)
        {
            MensajeErrorEdicion = dvEx.Message;
        }
        catch (DomainException dEx)
        {
            MensajeErrorEdicion = dEx.Message;
        }
        catch (Exception ex)
        {
            MensajeErrorEdicion = $"Error inesperado al guardar: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private void Regresar()
    {
        RegresarSolicitado?.Invoke();
    }

    private void RecalcularVistaPreviaEdicion()
    {
        decimal descuento = 0m;
        if (!string.IsNullOrWhiteSpace(DescuentoEdicionTexto))
        {
            decimal.TryParse(DescuentoEdicionTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out descuento);
        }

        descuento = Math.Clamp(Math.Round(descuento, 2), 0m, SubtotalModal);
        TotalCalculadoEdicion = Math.Max(0m, SubtotalModal - descuento);

        if (TipoPagoEdicion == TipoDePago.Efectivo &&
            decimal.TryParse(MontoRecibidoEdicionTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out var recibido) &&
            recibido >= TotalCalculadoEdicion)
        {
            CambioCalculadoEdicion = Math.Round(recibido, 2) - TotalCalculadoEdicion;
        }
        else
        {
            CambioCalculadoEdicion = 0m;
        }
    }

    private void RecalcularTotalesPeriodo()
    {
        var pagadas = _ventasCargadas.Where(v => v.Estado == EstadoVenta.Pagado).ToList();
        var canceladas = _ventasCargadas.Where(v => v.Estado == EstadoVenta.Cancelado).ToList();

        TotalVentasPeriodo = pagadas.Sum(v => v.Total);
        TotalEfectivoPeriodo = pagadas.Where(v => v.TipoDePago == TipoDePago.Efectivo).Sum(v => v.Total);
        TotalTarjetaPeriodo = pagadas.Where(v => v.TipoDePago == TipoDePago.Tarjeta).Sum(v => v.Total);
        TotalTransferenciaPeriodo = pagadas.Where(v => v.TipoDePago == TipoDePago.Transferencia).Sum(v => v.Total);
        TotalDescuentosPeriodo = pagadas.Sum(v => v.Descuento);
        CantidadPagadasPeriodo = pagadas.Count;
        CantidadCanceladasPeriodo = canceladas.Count;
    }

    private void AplicarFiltrosEnMemoria()
    {
        IEnumerable<VentaResumenDto> consulta = _ventasCargadas;

        if (FiltroMetodoPago != "Todos")
        {
            if (Enum.TryParse<TipoDePago>(FiltroMetodoPago, ignoreCase: true, out var tipoFiltro))
            {
                consulta = consulta.Where(v => v.Estado == EstadoVenta.Pagado && v.TipoDePago == tipoFiltro);
            }
        }

        string busquedaNormalizada = NormalizarParaBusqueda(TextoBusqueda);
        if (!string.IsNullOrWhiteSpace(busquedaNormalizada))
        {
            string busquedaSinGato = busquedaNormalizada.TrimStart('#');
            consulta = consulta.Where(v =>
                v.Id.ToString(CultureInfo.InvariantCulture).Contains(busquedaSinGato, StringComparison.OrdinalIgnoreCase) ||
                NormalizarParaBusqueda(v.IdentificadorCliente).Contains(busquedaNormalizada, StringComparison.Ordinal) ||
                v.Items.Any(i => NormalizarParaBusqueda(i.ProductoNombre).Contains(busquedaNormalizada, StringComparison.Ordinal)));
        }

        var listaFinal = consulta
            .OrderByDescending(v => v.FechaCierre ?? v.FechaCreacion)
            .Select(v => new VentaHistorialItemViewModel(v, _cultura))
            .ToList();

        VentasFiltradas.Clear();
        foreach (var item in listaFinal)
        {
            VentasFiltradas.Add(item);
        }

        TieneVentasFiltradas = VentasFiltradas.Count > 0;
    }

    private static string NormalizarParaBusqueda(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        string descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);

        foreach (char c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
