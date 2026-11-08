using Core.Domain.Enums;

namespace Core.Application.Dtos.Ventas;

public class ModificarVentaDto
{
    public int VentaId { get; set; }
    public TipoDePago TipoDePago { get; set; }
    public string IdentificadorCliente { get; set; } = string.Empty;
    public decimal? Descuento { get; set; }
    public decimal? MontoRecibido { get; set; }
}
