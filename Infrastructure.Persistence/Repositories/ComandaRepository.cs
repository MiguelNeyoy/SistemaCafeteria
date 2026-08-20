using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ComandaRepository : IComandaRepository
{
    private readonly AppDbContext _context;

    public ComandaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comanda?> ObtenerPorIdAsync(int id)
    {
        return await _context.Comandas
            .Include(c => c.Items)
            .ThenInclude(i => i.Extras)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Comanda>> ObtenerPorVentaIdAsync(int ventaId)
    {
        return await _context.Comandas
            .Include(c => c.Items)
            .ThenInclude(i => i.Extras)
            .Where(c => c.VentaId == ventaId)
            .OrderBy(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Comanda>> ObtenerPendientesAsync()
    {
        return await _context.Comandas
            .Include(c => c.Items)
            .ThenInclude(i => i.Extras)
            .Where(c => c.Estado == EstadoComanda.Pendiente)
            .OrderBy(c => c.FechaCreacion)
            .ToListAsync();
    }

    public async Task AgregarAsync(Comanda comanda)
    {
        await _context.Comandas.AddAsync(comanda);
    }

    public void Actualizar(Comanda comanda)
    {
        _context.Comandas.Update(comanda);
    }
}
