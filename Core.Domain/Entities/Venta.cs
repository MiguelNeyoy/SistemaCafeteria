using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Aggregate Root del flujo financiero. Representa la cuenta abierta o transacción cobrada.
/// </summary>
public class Venta
{
    private readonly List<VentaItem> _items = new();

    public int Id { get; private set; }
    public string IdentificadorCliente { get; private set; } = "General";
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaCierre { get; private set; }
    public EstadoVenta Estado { get; private set; } = EstadoVenta.Pendiente;
    public TipoDePago? TipoDePago { get; private set; }
    public decimal Descuento { get; private set; }
    public decimal? MontoRecibido { get; private set; }
    public IReadOnlyList<VentaItem> Items => _items.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.Subtotal);
    public decimal Total => Math.Max(0m, Subtotal - Descuento);
    public decimal? Cambio => (TipoDePago == Enums.TipoDePago.Efectivo && MontoRecibido.HasValue)
        ? MontoRecibido.Value - Total
        : null;

    // Constructor privado para EF Core
    private Venta() { }

    // Constructor para nueva venta en el sistema
    public Venta(string? identificadorCliente = null, DateTime? fechaCreacion = null)
    {
        IdentificadorCliente = string.IsNullOrWhiteSpace(identificadorCliente) ? "General" : identificadorCliente.Trim();
        FechaCreacion = fechaCreacion ?? DateTime.Now;
        Estado = EstadoVenta.Pendiente;
        Descuento = 0m;
    }

    // Constructor para reconstrucción desde persistencia
    public Venta(
        int id,
        string identificadorCliente,
        DateTime fechaCreacion,
        DateTime? fechaCierre,
        EstadoVenta estado,
        TipoDePago? tipoDePago,
        decimal descuento,
        decimal? montoRecibido,
        IEnumerable<VentaItem>? items = null)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id de la venta no puede ser negativo.");
        }

        Id = id;
        IdentificadorCliente = string.IsNullOrWhiteSpace(identificadorCliente) ? "General" : identificadorCliente.Trim();
        FechaCreacion = fechaCreacion;
        FechaCierre = fechaCierre;
        Estado = estado;
        TipoDePago = tipoDePago;
        Descuento = descuento;
        MontoRecibido = montoRecibido;

        if (items != null)
        {
            _items.AddRange(items);
        }
    }

    public void AgregarItem(VentaItem item)
    {
        AsegurarEstadoPendiente("No se pueden agregar productos a una cuenta cerrada o cancelada.");
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public VentaItem AgregarItem(int productoId, string productoNombre, decimal precioUnitario, int cantidad, string? notas = null, IEnumerable<VentaItemExtra>? extras = null)
    {
        AsegurarEstadoPendiente("No se pueden agregar productos a una cuenta cerrada o cancelada.");
        var item = new VentaItem(productoId, productoNombre, precioUnitario, cantidad, notas, extras);
        _items.Add(item);
        return item;
    }

    public void RemoverItem(VentaItem item)
    {
        AsegurarEstadoPendiente("No se pueden eliminar productos de una cuenta cerrada o cancelada.");
        _items.Remove(item);
    }

    public void AplicarDescuento(decimal montoDescuento)
    {
        AsegurarEstadoPendiente("No se puede modificar el descuento de una cuenta cerrada.");

        if (montoDescuento < 0)
        {
            throw new DomainValidationException(nameof(Descuento), "El descuento no puede ser negativo.");
        }

        if (montoDescuento > Subtotal)
        {
            throw new DomainValidationException(nameof(Descuento), "El descuento no puede ser mayor al subtotal de la venta.");
        }

        Descuento = montoDescuento;
    }

    public void Cobrar(TipoDePago tipoDePago, decimal? montoRecibido = null)
    {
        AsegurarEstadoPendiente("La cuenta ya ha sido procesada previamente.");

        if (!_items.Any())
        {
            throw new DomainException("No se puede cobrar una cuenta sin productos.");
        }

        if (tipoDePago == Enums.TipoDePago.Efectivo)
        {
            if (!montoRecibido.HasValue || montoRecibido.Value < Total)
            {
                throw new DomainValidationException(
                    nameof(MontoRecibido),
                    $"El monto recibido (${montoRecibido ?? 0:F2}) es insuficiente para cubrir el total (${Total:F2}).");
            }
        }

        TipoDePago = tipoDePago;
        MontoRecibido = montoRecibido;
        Estado = EstadoVenta.Pagado;
        FechaCierre = DateTime.Now;
    }

    public void Devolver()
    {
        if (Estado != EstadoVenta.Pagado)
        {
            throw new DomainException("Solo se pueden realizar devoluciones de ventas pagadas.");
        }

        Estado = EstadoVenta.Devuelto;
    }

    public void Cancelar()
    {
        AsegurarEstadoPendiente("Solo se pueden cancelar cuentas pendientes.");

        Estado = EstadoVenta.Cancelado;
        FechaCierre = DateTime.Now;
    }

    private void AsegurarEstadoPendiente(string mensajeError)
    {
        if (Estado != EstadoVenta.Pendiente)
        {
            throw new DomainException(mensajeError);
        }
    }
}
