using System;
using System.Collections.Generic;

namespace Core.Application.Dtos.Reportes;

/// <summary>
/// DTO con la información consolidada y el desglose de ventas del día para el Resumen Operativo.
/// </summary>
public class ResumenOperativoDto
{
    public DateTime Fecha { get; set; }
    public decimal TotalVentas { get; set; }
    public int CantidadVentas { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalTarjeta { get; set; }
    public decimal TotalTransferencia { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TicketPromedio { get; set; }
    public List<TicketResumenOperativoDto> Tickets { get; set; } = new();
}

/// <summary>
/// Representa el desglose individual de un ticket cobrado en el día.
/// </summary>
public class TicketResumenOperativoDto
{
    public int Folio { get; set; }
    public DateTime Hora { get; set; }
    public string Cliente { get; set; } = "General";
    public string TipoPago { get; set; } = "Efectivo";
    public decimal Total { get; set; }
    public int CantidadItems { get; set; }
}
