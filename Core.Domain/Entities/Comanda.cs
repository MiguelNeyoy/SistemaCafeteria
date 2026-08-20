using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Aggregate Root del flujo operativo (Cocina). Representa una orden de preparación.
/// NO maneja conceptos monetarios ni precios.
/// </summary>
public class Comanda
{
    private readonly List<ComandaItem> _items = new();

    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public string IdentificadorCliente { get; private set; } = "General";
    public DateTime FechaCreacion { get; private set; }
    public EstadoComanda Estado { get; private set; } = EstadoComanda.Pendiente;
    public IReadOnlyList<ComandaItem> Items => _items.AsReadOnly();

    // Constructor privado para EF Core
    private Comanda() { }

    // Constructor para emitir comanda a cocina
    public Comanda(int ventaId, string? identificadorCliente, IEnumerable<ComandaItem> items, DateTime? fechaCreacion = null)
    {
        if (ventaId <= 0)
        {
            throw new DomainValidationException(nameof(VentaId), "La comanda debe estar vinculada a una venta válida.");
        }

        ArgumentNullException.ThrowIfNull(items);

        var itemsList = items.ToList();
        if (!itemsList.Any())
        {
            throw new DomainException("No se puede enviar una comanda a cocina sin productos.");
        }

        VentaId = ventaId;
        IdentificadorCliente = string.IsNullOrWhiteSpace(identificadorCliente) ? "General" : identificadorCliente.Trim();
        FechaCreacion = fechaCreacion ?? DateTime.Now;
        Estado = EstadoComanda.Pendiente;
        _items.AddRange(itemsList);
    }

    // Constructor para reconstrucción desde persistencia
    public Comanda(
        int id,
        int ventaId,
        string identificadorCliente,
        DateTime fechaCreacion,
        EstadoComanda estado,
        IEnumerable<ComandaItem>? items = null)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id de la comanda no puede ser negativo.");
        }

        if (ventaId <= 0)
        {
            throw new DomainValidationException(nameof(VentaId), "El Id de la venta vinculada es inválido.");
        }

        Id = id;
        VentaId = ventaId;
        IdentificadorCliente = string.IsNullOrWhiteSpace(identificadorCliente) ? "General" : identificadorCliente.Trim();
        FechaCreacion = fechaCreacion;
        Estado = estado;

        if (items != null)
        {
            _items.AddRange(items);
        }
    }

    public void MarcarEntregada()
    {
        Estado = EstadoComanda.Entregado;
    }

    public void AgregarItem(ComandaItem item)
    {
        if (Estado != EstadoComanda.Pendiente)
        {
            throw new DomainException("No se pueden agregar productos a una comanda ya entregada.");
        }

        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }
}
