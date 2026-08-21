namespace Core.Application.Dtos.Ventas;

public class VentaItemDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public string? Notas { get; set; }
    public decimal Subtotal { get; set; }
    public List<VentaItemExtraDto> Extras { get; set; } = new();
}

public class VentaItemExtraDto
{
    public int Id { get; set; }
    public int ExtraId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}
