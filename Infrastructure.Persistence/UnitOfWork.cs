using Core.Application.Interfaces;
using Infrastructure.Persistence.Data;

namespace Infrastructure.Persistence;

/// <summary>
/// Implementación de IUnitOfWork usando EF Core para transacciones de persistencia.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
