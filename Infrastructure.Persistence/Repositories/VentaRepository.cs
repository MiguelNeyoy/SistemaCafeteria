using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _context;

    public VentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> ObtenerPorIdAsync(int id)
    {
        return await _context.Ventas.FindAsync(id);
    }

    public async Task<Venta?> ObtenerPorIdConDetallesAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Items)
            .ThenInclude(i => i.Extras)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Venta>> ObtenerPendientesAsync()
    {
        return await _context.Ventas
            .Include(v => v.Items)
            .ThenInclude(i => i.Extras)
            .Where(v => v.Estado == EstadoVenta.Pendiente)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Venta>> ObtenerPorFechaAsync(DateTime fecha)
    {
        var inicioDia = fecha.Date;
        var finDia = inicioDia.AddDays(1).AddTicks(-1);

        return await _context.Ventas
            .Include(v => v.Items)
            .ThenInclude(i => i.Extras)
            .Where(v => v.FechaCreacion >= inicioDia && v.FechaCreacion <= finDia)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Venta>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta)
    {
        return await _context.Ventas
            .Include(v => v.Items)
            .ThenInclude(i => i.Extras)
            .Where(v => v.FechaCreacion >= desde && v.FechaCreacion <= hasta)
            .OrderByDescending(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task AgregarAsync(Venta venta)
    {
        await _context.Ventas.AddAsync(venta);
    }

    public void Actualizar(Venta venta)
    {
        _context.Ventas.Update(venta);
    }

    public async Task EliminarVentasAnterioresAAsync(DateTime fecha)
    {
        var ventasAntiguas = await _context.Ventas
            .Include(v => v.Items)
            .ThenInclude(i => i.Extras)
            .Where(v => v.FechaCreacion < fecha)
            .ToListAsync();

        if (ventasAntiguas.Any())
        {
            _context.Ventas.RemoveRange(ventasAntiguas);
        }
    }
}
