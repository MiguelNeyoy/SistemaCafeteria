namespace Core.Application.Dtos.Ventas;

public class AgregarItemVentaDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; } = 1;
    public string? Notas { get; set; }
    public List<int> ExtraIds { get; set; } = new();
}
