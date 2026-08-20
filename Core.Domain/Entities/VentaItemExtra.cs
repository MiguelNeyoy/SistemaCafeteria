using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Snapshot congelado de un extra o modificador aplicado a una línea de venta.
/// </summary>
public class VentaItemExtra
{
    public int Id { get; private set; }
    public int VentaItemId { get; private set; }
    public int ExtraId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }

    // Constructor privado para EF Core
    private VentaItemExtra() { }

    public VentaItemExtra(int extraId, string nombre, decimal precio)
    {
        if (extraId <= 0)
        {
            throw new DomainValidationException(nameof(ExtraId), "El Id del extra es inválido.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainValidationException(nameof(Nombre), "El nombre del extra no puede estar vacío.");
        }

        if (precio < 0)
        {
            throw new DomainValidationException(nameof(Precio), "El precio del extra no puede ser negativo.");
        }

        ExtraId = extraId;
        Nombre = nombre.Trim();
        Precio = precio;
    }

    public VentaItemExtra(int id, int ventaItemId, int extraId, string nombre, decimal precio)
        : this(extraId, nombre, precio)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del extra de venta no puede ser negativo.");
        }

        Id = id;
        VentaItemId = ventaItemId;
    }
}
