using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Representa la asignación y validación de un extra o modificador para una categoría específica.
/// Define qué extras están permitidos para los productos que pertenecen a dicha categoría.
/// </summary>
public class CategoriaExtra
{
    public int CategoriaId { get; private set; }
    public int ExtraId { get; private set; }

    // Constructor privado para EF Core
    private CategoriaExtra() { }

    public CategoriaExtra(int categoriaId, int extraId)
    {
        if (categoriaId <= 0)
        {
            throw new DomainValidationException(nameof(CategoriaId), "El Id de categoría debe ser mayor a cero.");
        }

        if (extraId <= 0)
        {
            throw new DomainValidationException(nameof(ExtraId), "El Id del extra debe ser mayor a cero.");
        }

        CategoriaId = categoriaId;
        ExtraId = extraId;
    }
}
