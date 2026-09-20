namespace Core.Application.Dtos.Comandas;

public class EnviarComandaItemDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; } = 1;
    public string? NotasCocina { get; set; }
    public List<string> ExtraInstrucciones { get; set; } = new();
}
