using Core.Domain.Enums;

namespace Core.Application.Dtos.Ventas;

public class CobrarVentaDto
{
    public int VentaId { get; set; }
    public TipoDePago TipoDePago { get; set; }
    public decimal? MontoRecibido { get; set; }
}
