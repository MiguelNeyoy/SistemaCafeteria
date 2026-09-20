namespace Core.Application.Dtos.Reportes;

public class CorteCajaDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal FondoInicial { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalTarjeta { get; set; }
    public decimal TotalTransferencia { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TotalDevoluciones { get; set; }
    public decimal EsperadoEnCajon => FondoInicial + TotalEfectivo - TotalDevoluciones;
    public int CantidadVentas { get; set; }
    public int CantidadCanceladas { get; set; }
    public int CantidadDevueltas { get; set; }
}
