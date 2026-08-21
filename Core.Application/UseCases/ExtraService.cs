using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class ExtraService : IExtraService
{
    private readonly IExtraRepository _extraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExtraService(IExtraRepository extraRepository, IUnitOfWork unitOfWork)
    {
        _extraRepository = extraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExtraDto> CrearAsync(CrearExtraDto dto)
    {
        var extra = new Extra(dto.Nombre, dto.Precio);
        await _extraRepository.AgregarAsync(extra);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(extra);
    }

    public async Task<ExtraDto> EditarAsync(EditarExtraDto dto)
    {
        var extra = await _extraRepository.ObtenerPorIdAsync(dto.Id)
            ?? throw new DomainException($"Extra con Id {dto.Id} no encontrado.");

        extra.CambiarNombre(dto.Nombre);
        extra.CambiarPrecio(dto.Precio);

        _extraRepository.Actualizar(extra);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(extra);
    }

    public async Task ActivarAsync(int id)
    {
        var extra = await _extraRepository.ObtenerPorIdAsync(id)
            ?? throw new DomainException($"Extra con Id {id} no encontrado.");

        extra.Activar();
        _extraRepository.Actualizar(extra);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var extra = await _extraRepository.ObtenerPorIdAsync(id)
            ?? throw new DomainException($"Extra con Id {id} no encontrado.");

        extra.Desactivar();
        _extraRepository.Actualizar(extra);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ExtraDto?> ObtenerPorIdAsync(int id)
    {
        var extra = await _extraRepository.ObtenerPorIdAsync(id);
        return extra == null ? null : MapToDto(extra);
    }

    public async Task<List<ExtraDto>> ObtenerTodosAsync()
    {
        var extras = await _extraRepository.ObtenerTodosAsync();
        return extras.Select(MapToDto).ToList();
    }

    public async Task<List<ExtraDto>> ObtenerActivosAsync()
    {
        var extras = await _extraRepository.ObtenerActivosAsync();
        return extras.Select(MapToDto).ToList();
    }

    private static ExtraDto MapToDto(Extra extra)
    {
        return new ExtraDto
        {
            Id = extra.Id,
            Nombre = extra.Nombre,
            Precio = extra.Precio,
            Activo = extra.Activo
        };
    }
}
