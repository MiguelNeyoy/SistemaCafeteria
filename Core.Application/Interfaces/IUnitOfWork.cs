namespace Core.Application.Interfaces;

/// <summary>
/// Contrato para coordinar la persistencia atómica de cambios en una sola transacción.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
