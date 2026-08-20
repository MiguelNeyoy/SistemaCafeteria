using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Representa el comprobante impreso emitido para el cliente tras el cobro de una venta.
/// </summary>
public class Ticket
{
    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public string Folio { get; private set; } = string.Empty;
    public DateTime FechaEmision { get; private set; }
    public bool EsReimpresion { get; private set; }

    // Constructor privado para EF Core
    private Ticket() { }

    public Ticket(int ventaId, string folio, DateTime? fechaEmision = null, bool esReimpresion = false)
    {
        if (ventaId <= 0)
        {
            throw new DomainValidationException(nameof(VentaId), "El Id de la venta vinculada al ticket es inválido.");
        }

        if (string.IsNullOrWhiteSpace(folio))
        {
            throw new DomainValidationException(nameof(Folio), "El folio del ticket no puede estar vacío.");
        }

        VentaId = ventaId;
        Folio = folio.Trim();
        FechaEmision = fechaEmision ?? DateTime.Now;
        EsReimpresion = esReimpresion;
    }

    public Ticket(int id, int ventaId, string folio, DateTime fechaEmision, bool esReimpresion)
        : this(ventaId, folio, fechaEmision, esReimpresion)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del ticket no puede ser negativo.");
        }

        Id = id;
    }

    public void MarcarComoReimpresion()
    {
        EsReimpresion = true;
    }
}
