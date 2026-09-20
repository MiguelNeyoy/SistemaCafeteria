using Core.Application.Dtos.Tickets;

namespace Core.Application.Interfaces.Services;

public interface ITicketService
{
    Task<TicketDto> GenerarTicketAsync(int ventaId);
    Task<TicketDto> ReimprimirTicketAsync(int ticketId);
}
