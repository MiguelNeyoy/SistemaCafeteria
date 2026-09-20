using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CategoriaExtraRepository : ICategoriaExtraRepository
{
    private readonly AppDbContext _context;

    public CategoriaExtraRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<int>> ObtenerExtraIdsPorCategoriaAsync(int categoriaId)
    {
        return await _context.CategoriaExtras
            .Where(ce => ce.CategoriaId == categoriaId)
            .Select(ce => ce.ExtraId)
            .ToListAsync();
    }

    public async Task<List<Extra>> ObtenerExtrasPorCategoriaAsync(int categoriaId)
    {
        return await _context.CategoriaExtras
            .Where(ce => ce.CategoriaId == categoriaId)
            .Join(
                _context.Extras.Where(e => e.Activo),
                ce => ce.ExtraId,
                e => e.Id,
                (ce, e) => e
            )
            .ToListAsync();
    }

    public async Task SincronizarExtrasAsync(int categoriaId, IEnumerable<int> extraIds)
    {
        var actuales = await _context.CategoriaExtras
            .Where(ce => ce.CategoriaId == categoriaId)
            .ToListAsync();

        var nuevosIds = extraIds?.Distinct().ToList() ?? new List<int>();

        // Eliminar las que ya no están marcadas
        var aEliminar = actuales
            .Where(ce => !nuevosIds.Contains(ce.ExtraId))
            .ToList();

        if (aEliminar.Any())
        {
            _context.CategoriaExtras.RemoveRange(aEliminar);
        }

        // Agregar las nuevas
        var actualesIds = actuales.Select(ce => ce.ExtraId).ToHashSet();
        var aAgregar = nuevosIds
            .Where(id => !actualesIds.Contains(id))
            .Select(id => new CategoriaExtra(categoriaId, id))
            .ToList();

        if (aAgregar.Any())
        {
            await _context.CategoriaExtras.AddRangeAsync(aAgregar);
        }
    }

    public async Task<bool> ExisteRelacionAsync(int categoriaId, int extraId)
    {
        return await _context.CategoriaExtras
            .AnyAsync(ce => ce.CategoriaId == categoriaId && ce.ExtraId == extraId);
    }

    public async Task<List<int>> ObtenerCategoriaIdsPorExtraAsync(int extraId)
    {
        return await _context.CategoriaExtras
            .Where(ce => ce.ExtraId == extraId)
            .Select(ce => ce.CategoriaId)
            .ToListAsync();
    }

    public async Task SincronizarCategoriasDeExtraAsync(int extraId, IEnumerable<int> categoriaIds)
    {
        var actuales = await _context.CategoriaExtras
            .Where(ce => ce.ExtraId == extraId)
            .ToListAsync();

        var nuevosCatIds = categoriaIds?.Distinct().ToList() ?? new List<int>();

        // Eliminar ÚNICAMENTE las asociaciones de este extra con categorías que se hayan desmarcado
        var aEliminar = actuales
            .Where(ce => !nuevosCatIds.Contains(ce.CategoriaId))
            .ToList();

        if (aEliminar.Any())
        {
            _context.CategoriaExtras.RemoveRange(aEliminar);
        }

        // Agregar este extra ÚNICAMENTE a las nuevas categorías marcadas (sin tocar los demás extras existentes)
        var actualesCatIds = actuales.Select(ce => ce.CategoriaId).ToHashSet();
        var aAgregar = nuevosCatIds
            .Where(catId => !actualesCatIds.Contains(catId))
            .Select(catId => new CategoriaExtra(catId, extraId))
            .ToList();

        if (aAgregar.Any())
        {
            await _context.CategoriaExtras.AddRangeAsync(aAgregar);
        }
    }
}
