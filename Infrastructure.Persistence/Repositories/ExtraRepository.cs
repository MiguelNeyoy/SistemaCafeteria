using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ExtraRepository : IExtraRepository
{
    private readonly AppDbContext _context;

    public ExtraRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Extra?> ObtenerPorIdAsync(int id)
    {
        return await _context.Extras.FindAsync(id);
    }

    public async Task<List<Extra>> ObtenerTodosAsync()
    {
        return await _context.Extras.ToListAsync();
    }

    public async Task<List<Extra>> ObtenerActivosAsync()
    {
        return await _context.Extras
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public async Task AgregarAsync(Extra extra)
    {
        await _context.Extras.AddAsync(extra);
    }

    public void Actualizar(Extra extra)
    {
        _context.Extras.Update(extra);
    }
}
