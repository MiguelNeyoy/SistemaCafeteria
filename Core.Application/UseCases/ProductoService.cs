using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductoService(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoDto dto)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(dto.CategoriaId)
            ?? throw new DomainValidationException(nameof(dto.CategoriaId), "La categoría seleccionada no existe.");

        var producto = new Producto(dto.Nombre, dto.Precio, dto.CategoriaId);
        await _productoRepository.AgregarAsync(producto);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(producto, categoria.Nombre);
    }

    public async Task<ProductoDto> EditarAsync(EditarProductoDto dto)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(dto.Id)
            ?? throw new DomainException($"Producto con Id {dto.Id} no encontrado.");

        var categoria = await _categoriaRepository.ObtenerPorIdAsync(dto.CategoriaId)
            ?? throw new DomainValidationException(nameof(dto.CategoriaId), "La categoría seleccionada no existe.");

        producto.CambiarNombre(dto.Nombre);
        producto.ModificarPrecio(dto.Precio);
        producto.CambiarCategoria(dto.CategoriaId);

        _productoRepository.Actualizar(producto);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(producto, categoria.Nombre);
    }

    public async Task ActivarAsync(int id)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(id)
            ?? throw new DomainException($"Producto con Id {id} no encontrado.");

        producto.Activar();
        _productoRepository.Actualizar(producto);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(id)
            ?? throw new DomainException($"Producto con Id {id} no encontrado.");

        producto.Desactivar();
        _productoRepository.Actualizar(producto);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(id);
        if (producto == null)
        {
            return null;
        }

        var categoria = await _categoriaRepository.ObtenerPorIdAsync(producto.CategoriaId);
        return MapToDto(producto, categoria?.Nombre ?? string.Empty);
    }

    public async Task<List<ProductoDto>> ObtenerTodosAsync()
    {
        var productos = await _productoRepository.ObtenerTodosAsync();
        var categorias = (await _categoriaRepository.ObtenerTodasAsync())
            .ToDictionary(c => c.Id, c => c.Nombre);

        return productos.Select(p => MapToDto(p, categorias.GetValueOrDefault(p.CategoriaId, string.Empty))).ToList();
    }

    public async Task<List<ProductoDto>> ObtenerActivosAsync()
    {
        var productos = await _productoRepository.ObtenerActivosAsync();
        var categorias = (await _categoriaRepository.ObtenerTodasAsync())
            .ToDictionary(c => c.Id, c => c.Nombre);

        return productos.Select(p => MapToDto(p, categorias.GetValueOrDefault(p.CategoriaId, string.Empty))).ToList();
    }

    public async Task<List<ProductoDto>> ObtenerPorCategoriaAsync(int categoriaId)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(categoriaId);
        var categoriaNombre = categoria?.Nombre ?? string.Empty;

        var productos = await _productoRepository.ObtenerPorCategoriaAsync(categoriaId);
        return productos.Select(p => MapToDto(p, categoriaNombre)).ToList();
    }

    private static ProductoDto MapToDto(Producto producto, string categoriaNombre)
    {
        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            CategoriaId = producto.CategoriaId,
            CategoriaNombre = categoriaNombre,
            Activo = producto.Activo
        };
    }
}
