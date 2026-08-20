using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para Tickets de comprobante.
/// </summary>
public interface ITicketRepository
{
    Task<Ticket?> ObtenerPorIdAsync(int id);
    Task<Ticket?> ObtenerPorVentaIdAsync(int ventaId);
    Task AgregarAsync(Ticket ticket);
    Task<string> GenerarSiguienteFolioAsync();
}
