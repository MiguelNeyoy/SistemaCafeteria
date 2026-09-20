using Core.Application.Dtos.Catalogo;

namespace Core.Application.Interfaces.Services;

public interface ICategoriaService
{
    Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto);
    Task<CategoriaDto> EditarAsync(EditarCategoriaDto dto);
    Task ActivarAsync(int id);
    Task DesactivarAsync(int id);
    Task<CategoriaDto?> ObtenerPorIdAsync(int id);
    Task<List<CategoriaDto>> ObtenerTodasAsync();
    Task<List<CategoriaDto>> ObtenerActivasAsync();
}
