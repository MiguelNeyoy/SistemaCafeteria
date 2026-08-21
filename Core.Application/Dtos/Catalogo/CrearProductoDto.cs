namespace Core.Application.Dtos.Catalogo;

public class CrearProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CategoriaId { get; set; }
}
