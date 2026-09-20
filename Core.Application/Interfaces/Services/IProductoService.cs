using Core.Application.Dtos.Catalogo;

namespace Core.Application.Interfaces.Services;

public interface IProductoService
{
    Task<ProductoDto> CrearAsync(CrearProductoDto dto);
    Task<ProductoDto> EditarAsync(EditarProductoDto dto);
    Task ActivarAsync(int id);
    Task DesactivarAsync(int id);
    Task<ProductoDto?> ObtenerPorIdAsync(int id);
    Task<List<ProductoDto>> ObtenerTodosAsync();
    Task<List<ProductoDto>> ObtenerActivosAsync();
    Task<List<ProductoDto>> ObtenerPorCategoriaAsync(int categoriaId);
}
