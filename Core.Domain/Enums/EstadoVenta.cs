namespace Core.Domain.Enums;

/// <summary>
/// Representa el estado de una cuenta/venta en el sistema.
/// </summary>
public enum EstadoVenta
{
    Pendiente,    // Cuenta abierta, acumulando ítems y comandas
    Pagado,       // Cuenta cobrada y cerrada exitosamente
    Cancelado,    // Cancelada antes de realizar el cobro
    Devuelto      // Cobrada previamente pero devuelta al 100%
}
