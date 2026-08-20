using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Categoria?> ObtenerPorIdAsync(int id)
    {
        return await _context.Categorias.FindAsync(id);
    }

    public async Task<List<Categoria>> ObtenerTodasAsync()
    {
        return await _context.Categorias.ToListAsync();
    }

    public async Task<List<Categoria>> ObtenerActivasAsync()
    {
        return await _context.Categorias
            .Where(c => c.Activo)
            .ToListAsync();
    }

    public async Task AgregarAsync(Categoria categoria)
    {
        await _context.Categorias.AddAsync(categoria);
    }

    public void Actualizar(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
    {
        var nombreLimpio = nombre.Trim().ToUpper();

        var query = _context.Categorias
            .Where(c => c.Nombre.ToUpper() == nombreLimpio);

        if (excluirId.HasValue)
        {
            query = query.Where(c => c.Id != excluirId.Value);
        }

        return await query.AnyAsync();
    }
}
