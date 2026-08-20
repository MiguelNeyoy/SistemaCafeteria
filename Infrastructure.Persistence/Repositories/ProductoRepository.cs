using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;

    public ProductoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos.FindAsync(id);
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        return await _context.Productos.ToListAsync();
    }

    public async Task<List<Producto>> ObtenerActivosAsync()
    {
        return await _context.Productos
            .Where(p => p.Activo)
            .ToListAsync();
    }

    public async Task<List<Producto>> ObtenerPorCategoriaAsync(int categoriaId)
    {
        return await _context.Productos
            .Where(p => p.CategoriaId == categoriaId && p.Activo)
            .ToListAsync();
    }

    public async Task AgregarAsync(Producto producto)
    {
        await _context.Productos.AddAsync(producto);
    }

    public void Actualizar(Producto producto)
    {
        _context.Productos.Update(producto);
    }
}
