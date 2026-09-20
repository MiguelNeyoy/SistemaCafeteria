using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Ítem individual enviado a cocina para su preparación. No contiene información de precios.
/// </summary>
public class ComandaItem
{
    private readonly List<ComandaItemExtra> _extras = new();

    public int Id { get; private set; }
    public int ComandaId { get; private set; }
    public int ProductoId { get; private set; }
    public string ProductoNombre { get; private set; } = string.Empty;
    public int Cantidad { get; private set; }
    public string? NotasCocina { get; private set; }
    public IReadOnlyList<ComandaItemExtra> Extras => _extras.AsReadOnly();

    // Constructor privado para EF Core
    private ComandaItem() { }

    public ComandaItem(int productoId, string productoNombre, int cantidad, string? notasCocina = null, IEnumerable<ComandaItemExtra>? extras = null)
    {
        if (productoId <= 0)
        {
            throw new DomainValidationException(nameof(ProductoId), "El Id del producto es inválido.");
        }

        if (string.IsNullOrWhiteSpace(productoNombre))
        {
            throw new DomainValidationException(nameof(ProductoNombre), "El nombre del producto no puede estar vacío.");
        }

        if (cantidad <= 0)
        {
            throw new DomainValidationException(nameof(Cantidad), "La cantidad para cocina debe ser mayor a cero.");
        }

        ProductoId = productoId;
        ProductoNombre = productoNombre.Trim();
        Cantidad = cantidad;
        NotasCocina = notasCocina?.Trim();

        if (extras != null)
        {
            _extras.AddRange(extras);
        }
    }

    public ComandaItem(int id, int comandaId, int productoId, string productoNombre, int cantidad, string? notasCocina = null, IEnumerable<ComandaItemExtra>? extras = null)
        : this(productoId, productoNombre, cantidad, notasCocina, extras)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del ítem de comanda no puede ser negativo.");
        }

        Id = id;
        ComandaId = comandaId;
    }

    public void AgregarExtra(ComandaItemExtra extra)
    {
        ArgumentNullException.ThrowIfNull(extra);
        _extras.Add(extra);
    }
}