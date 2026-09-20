using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriaService(ICategoriaRepository categoriaRepository, IUnitOfWork unitOfWork)
    {
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto)
    {
        if (await _categoriaRepository.ExisteNombreAsync(dto.Nombre))
        {
            throw new DomainValidationException(nameof(dto.Nombre), "Ya existe una categoría con ese nombre.");
        }

        var categoria = new Categoria(dto.Nombre);
        await _categoriaRepository.AgregarAsync(categoria);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(categoria);
    }

    public async Task<CategoriaDto> EditarAsync(EditarCategoriaDto dto)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(dto.Id)
            ?? throw new DomainException($"Categoría con Id {dto.Id} no encontrada.");

        if (await _categoriaRepository.ExisteNombreAsync(dto.Nombre, dto.Id))
        {
            throw new DomainValidationException(nameof(dto.Nombre), "Ya existe otra categoría con ese nombre.");
        }

        categoria.CambiarNombre(dto.Nombre);
        _categoriaRepository.Actualizar(categoria);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(categoria);
    }

    public async Task ActivarAsync(int id)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(id)
            ?? throw new DomainException($"Categoría con Id {id} no encontrada.");

        categoria.Activar();
        _categoriaRepository.Actualizar(categoria);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(id)
            ?? throw new DomainException($"Categoría con Id {id} no encontrada.");

        categoria.Desactivar();
        _categoriaRepository.Actualizar(categoria);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CategoriaDto?> ObtenerPorIdAsync(int id)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(id);
        return categoria == null ? null : MapToDto(categoria);
    }

    public async Task<List<CategoriaDto>> ObtenerTodasAsync()
    {
        var categorias = await _categoriaRepository.ObtenerTodasAsync();
        return categorias.Select(MapToDto).ToList();
    }

    public async Task<List<CategoriaDto>> ObtenerActivasAsync()
    {
        var categorias = await _categoriaRepository.ObtenerActivasAsync();
        return categorias.Select(MapToDto).ToList();
    }

    private static CategoriaDto MapToDto(Categoria categoria)
    {
        return new CategoriaDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Activo = categoria.Activo
        };
    }
}
