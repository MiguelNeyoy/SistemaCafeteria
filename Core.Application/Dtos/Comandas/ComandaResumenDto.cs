using Core.Domain.Enums;

namespace Core.Application.Dtos.Comandas;

public class ComandaResumenDto
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string IdentificadorCliente { get; set; } = "General";
    public DateTime FechaCreacion { get; set; }
    public EstadoComanda Estado { get; set; }
    public List<ComandaItemResumenDto> Items { get; set; } = new();
}

public class ComandaItemResumenDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string? NotasCocina { get; set; }
    public List<string> Extras { get; set; } = new();
}
