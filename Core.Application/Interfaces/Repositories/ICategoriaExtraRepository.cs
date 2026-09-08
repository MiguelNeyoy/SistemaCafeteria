using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para la relación asociativa entre Categorías y Extras.
/// </summary>
public interface ICategoriaExtraRepository
{
    Task<List<int>> ObtenerExtraIdsPorCategoriaAsync(int categoriaId);
    Task<List<Extra>> ObtenerExtrasPorCategoriaAsync(int categoriaId);
    Task SincronizarExtrasAsync(int categoriaId, IEnumerable<int> extraIds);
    Task<bool> ExisteRelacionAsync(int categoriaId, int extraId);
}
