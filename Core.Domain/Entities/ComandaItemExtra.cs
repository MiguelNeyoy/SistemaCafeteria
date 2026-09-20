using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Extra o modificador para cocina (ej. Sin Cebolla, Leche Deslactosada). No contiene precios.
/// </summary>
public class ComandaItemExtra
{
    public int Id { get; private set; }
    public int ComandaItemId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    // Constructor privado para EF Core
    private ComandaItemExtra() { }

    public ComandaItemExtra(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainValidationException(nameof(Nombre), "El nombre de la instrucción de cocina no puede estar vacío.");
        }

        Nombre = nombre.Trim();
    }

    public ComandaItemExtra(int id, int comandaItemId, string nombre) : this(nombre)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del extra de comanda no puede ser negativo.");
        }

        Id = id;
        ComandaItemId = comandaItemId;
    }
}
