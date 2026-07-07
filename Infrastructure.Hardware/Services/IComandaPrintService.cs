namespace Infrastructure.Hardware.Services;

public interface IComandaPrintService
{
    Task ImprimirComandaAsync(Guid pedidoId);
    Task ImprimirTicketAsync(Guid ventaId);
}
