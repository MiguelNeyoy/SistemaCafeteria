using Core.Application.Dtos.Tickets;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TicketService(
        ITicketRepository ticketRepository,
        IVentaRepository ventaRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _ventaRepository = ventaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDto> GenerarTicketAsync(int ventaId)
    {
        var ticketExistente = await _ticketRepository.ObtenerPorVentaIdAsync(ventaId);
        if (ticketExistente != null)
        {
            return MapToDto(ticketExistente);
        }

        var venta = await _ventaRepository.ObtenerPorIdAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        if (venta.Estado != EstadoVenta.Pagado)
        {
            throw new DomainException("Solo se pueden generar tickets de comprobante para ventas pagadas.");
        }

        var folio = await _ticketRepository.GenerarSiguienteFolioAsync();
        var ticket = new Ticket(venta.Id, folio);

        await _ticketRepository.AgregarAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<TicketDto> ReimprimirTicketAsync(int ticketId)
    {
        var ticket = await _ticketRepository.ObtenerPorIdAsync(ticketId)
            ?? throw new DomainException($"Ticket con Id {ticketId} no encontrado.");

        ticket.MarcarComoReimpresion();
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    private static TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto
        {
            Id = ticket.Id,
            VentaId = ticket.VentaId,
            Folio = ticket.Folio,
            FechaEmision = ticket.FechaEmision,
            EsReimpresion = ticket.EsReimpresion
        };
    }
}
