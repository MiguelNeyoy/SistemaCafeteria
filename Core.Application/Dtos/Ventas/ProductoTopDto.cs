namespace Core.Application.Dtos.Ventas;

/// <summary>
/// Representa un producto dentro del ranking de productos más vendidos en un periodo.
/// </summary>
public class ProductoTopDto
{
    public int Posicion { get; set; }
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalRecaudado { get; set; }
}
