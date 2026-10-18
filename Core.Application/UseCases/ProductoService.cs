using System.Globalization;
using System.Text;
using Core.Application.Dtos.Catalogo;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Common;
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

        var nombreFormateado = FormateadorTextoCatalogo.FormatearNombre(dto.Nombre);

        if (await _productoRepository.ExisteNombreEnCategoriaAsync(nombreFormateado, dto.CategoriaId))
        {
            throw new DomainValidationException(nameof(dto.Nombre), 
                $"Ya existe un producto con el nombre '{nombreFormateado}' en la categoría '{categoria.Nombre}'.");
        }

        var producto = new Producto(nombreFormateado, dto.Precio, dto.CategoriaId);
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

        var nombreFormateado = FormateadorTextoCatalogo.FormatearNombre(dto.Nombre);

        if (await _productoRepository.ExisteNombreEnCategoriaAsync(nombreFormateado, dto.CategoriaId, dto.Id))
        {
            throw new DomainValidationException(nameof(dto.Nombre), 
                $"Ya existe otro producto con el nombre '{nombreFormateado}' en la categoría '{categoria.Nombre}'.");
        }

        producto.CambiarNombre(nombreFormateado);
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

    public async Task<List<ProductoDto>> BuscarActivosAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
        {
            return new List<ProductoDto>();
        }

        string consultaNormalizada = NormalizarParaBusqueda(termino);
        string[] tokens = consultaNormalizada.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return new List<ProductoDto>();
        }

        var categoriasActivas = (await _categoriaRepository.ObtenerActivasAsync())
            .ToDictionary(c => c.Id, c => c.Nombre);

        var productosActivos = await _productoRepository.ObtenerActivosDeCategoriasActivasAsync();

        var coincidencias = new List<(ProductoDto Dto, int Prioridad)>();

        foreach (var p in productosActivos)
        {
            if (!categoriasActivas.TryGetValue(p.CategoriaId, out var categoriaNombre))
            {
                continue;
            }

            string nombreNorm = NormalizarParaBusqueda(p.Nombre);
            string catNorm = NormalizarParaBusqueda(categoriaNombre);

            bool todosEnNombre = tokens.All(t => nombreNorm.Contains(t, StringComparison.Ordinal));
            bool todosEnNombreOCategoria = todosEnNombre || tokens.All(t =>
                nombreNorm.Contains(t, StringComparison.Ordinal) || catNorm.Contains(t, StringComparison.Ordinal));

            if (!todosEnNombreOCategoria)
            {
                continue;
            }

            int prioridad = nombreNorm.StartsWith(consultaNormalizada, StringComparison.Ordinal) ? 0
                          : nombreNorm.StartsWith(tokens[0], StringComparison.Ordinal) ? 1
                          : todosEnNombre ? 2
                          : 3;

            coincidencias.Add((MapToDto(p, categoriaNombre), prioridad));
        }

        return coincidencias
            .OrderBy(x => x.Prioridad)
            .ThenBy(x => x.Dto.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => x.Dto)
            .ToList();
    }

    private static string NormalizarParaBusqueda(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        string descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);

        foreach (char c in descompuesto)
        {
            var categoriaUnicode = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoriaUnicode != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
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
