using Core.Domain.Enums;

namespace Core.Application.Dtos.Ventas;

public class VentaResumenDto
{
    public int Id { get; set; }
    public string IdentificadorCliente { get; set; } = "General";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCierre { get; set; }
    public EstadoVenta Estado { get; set; }
    public TipoDePago? TipoDePago { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public decimal? MontoRecibido { get; set; }
    public decimal? Cambio { get; set; }
    public List<VentaItemDto> Items { get; set; } = new();
}
