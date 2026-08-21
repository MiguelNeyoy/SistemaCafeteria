namespace Core.Application.Dtos.Tickets;

public class TicketDto
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string Folio { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public bool EsReimpresion { get; set; }
}
