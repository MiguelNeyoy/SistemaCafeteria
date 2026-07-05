namespace Presentation.WPF.Services;

public class ComandaPrintService : IComandaPrintService
{
    public async Task ImprimirComandaAsync(Guid pedidoId)
    {
        await Task.Run(() =>
        {
            // Lógica de impresión hacia la impresora de comandas
        });
    }

    public async Task ImprimirTicketAsync(Guid ventaId)
    {
        await Task.Run(() =>
        {
            // Lógica de impresión hacia la impresora de tickets
        });
    }
}
