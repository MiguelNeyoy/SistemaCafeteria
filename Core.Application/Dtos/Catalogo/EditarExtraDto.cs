namespace Core.Application.Dtos.Catalogo;

public class EditarExtraDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}
