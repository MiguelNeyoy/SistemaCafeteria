using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> ObtenerPorIdAsync(int id)
    {
        return await _context.Tickets.FindAsync(id);
    }

    public async Task<Ticket?> ObtenerPorVentaIdAsync(int ventaId)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.VentaId == ventaId);
    }

    public async Task AgregarAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
    }

    public async Task<string> GenerarSiguienteFolioAsync()
    {
        var hoy = DateTime.Now;
        var prefijo = $"T-{hoy:yyyyMMdd}-";

        var totalHoy = await _context.Tickets
            .Where(t => t.Folio.StartsWith(prefijo))
            .CountAsync();

        return $"{prefijo}{(totalHoy + 1):D4}";
    }
}
