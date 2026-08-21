using Core.Application.Dtos.Catalogo;

namespace Core.Application.Interfaces.Services;

public interface IExtraService
{
    Task<ExtraDto> CrearAsync(CrearExtraDto dto);
    Task<ExtraDto> EditarAsync(EditarExtraDto dto);
    Task ActivarAsync(int id);
    Task DesactivarAsync(int id);
    Task<ExtraDto?> ObtenerPorIdAsync(int id);
    Task<List<ExtraDto>> ObtenerTodosAsync();
    Task<List<ExtraDto>> ObtenerActivosAsync();
}
