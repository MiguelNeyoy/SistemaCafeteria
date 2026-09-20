using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Línea de detalle de una venta. Congela el nombre y precio del producto al momento de agregarlo.
/// </summary>
public class VentaItem
{
    private readonly List<VentaItemExtra> _extras = new();

    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public int ProductoId { get; private set; }
    public string ProductoNombre { get; private set; } = string.Empty;
    public decimal PrecioUnitario { get; private set; }
    public int Cantidad { get; private set; }
    public string? Notas { get; private set; }
    public IReadOnlyList<VentaItemExtra> Extras => _extras.AsReadOnly();

    public decimal SubtotalExtras => _extras.Sum(e => e.Precio);
    public decimal Subtotal => (PrecioUnitario + SubtotalExtras) * Cantidad;

    // Constructor privado para EF Core
    private VentaItem() { }

    public VentaItem(int productoId, string productoNombre, decimal precioUnitario, int cantidad, string? notas = null, IEnumerable<VentaItemExtra>? extras = null)
    {
        if (productoId <= 0)
        {
            throw new DomainValidationException(nameof(ProductoId), "El Id del producto es inválido.");
        }

        if (string.IsNullOrWhiteSpace(productoNombre))
        {
            throw new DomainValidationException(nameof(ProductoNombre), "El nombre del producto no puede estar vacío.");
        }

        if (precioUnitario <= 0)
        {
            throw new DomainValidationException(nameof(PrecioUnitario), "El precio unitario debe ser mayor a cero.");
        }

        if (cantidad <= 0)
        {
            throw new DomainValidationException(nameof(Cantidad), "La cantidad debe ser mayor a cero.");
        }

        ProductoId = productoId;
        ProductoNombre = productoNombre.Trim();
        PrecioUnitario = precioUnitario;
        Cantidad = cantidad;
        Notas = notas?.Trim();

        if (extras != null)
        {
            _extras.AddRange(extras);
        }
    }

    public VentaItem(int id, int ventaId, int productoId, string productoNombre, decimal precioUnitario, int cantidad, string? notas = null, IEnumerable<VentaItemExtra>? extras = null)
        : this(productoId, productoNombre, precioUnitario, cantidad, notas, extras)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del ítem de venta no puede ser negativo.");
        }

        Id = id;
        VentaId = ventaId;
    }

    public void AgregarExtra(VentaItemExtra extra)
    {
        ArgumentNullException.ThrowIfNull(extra);
        _extras.Add(extra);
    }

    public void ModificarCantidad(int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
        {
            throw new DomainValidationException(nameof(Cantidad), "La cantidad debe ser mayor a cero.");
        }

        Cantidad = nuevaCantidad;
    }
}
