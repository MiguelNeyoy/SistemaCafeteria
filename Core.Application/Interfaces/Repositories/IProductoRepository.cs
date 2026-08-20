using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para Productos.
/// </summary>
public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<List<Producto>> ObtenerTodosAsync();
    Task<List<Producto>> ObtenerActivosAsync();
    Task<List<Producto>> ObtenerPorCategoriaAsync(int categoriaId);
    Task AgregarAsync(Producto producto);
    void Actualizar(Producto producto);
}
