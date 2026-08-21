namespace Core.Application.Dtos.Catalogo;

public class ExtraDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}
