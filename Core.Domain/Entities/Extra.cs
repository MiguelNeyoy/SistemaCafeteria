using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Representa un extra o modificador del menú (ej. Extra Queso $15, Sin Cebolla $0, Leche de Almendras $10).
/// Si Precio = 0, actúa como modificador / instrucción de cocina sin costo adicional.
/// </summary>
public class Extra
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; } = true;

    // Constructor privado para EF Core
    private Extra() { }

    // Constructor para nuevo extra en dominio
    public Extra(string nombre, decimal precio)
    {
        ValidarYAsignarNombre(nombre);
        ValidarYAsignarPrecio(precio);
        Activo = true;
    }

    // Constructor para reconstrucción desde persistencia
    public Extra(int id, string nombre, decimal precio, bool activo = true)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del extra no puede ser negativo.");
        }

        Id = id;
        ValidarYAsignarNombre(nombre);
        ValidarYAsignarPrecio(precio);
        Activo = activo;
    }

    public void CambiarNombre(string nuevoNombre)
    {
        ValidarYAsignarNombre(nuevoNombre);
    }

    public void CambiarPrecio(decimal nuevoPrecio)
    {
        ValidarYAsignarPrecio(nuevoPrecio);
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public void Activar()
    {
        Activo = true;
    }

    private void ValidarYAsignarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainValidationException(nameof(Nombre), "El nombre del extra no puede estar vacío.");
        }

        Nombre = nombre.Trim();
    }

    private void ValidarYAsignarPrecio(decimal precio)
    {
        if (precio < 0)
        {
            throw new DomainValidationException(nameof(Precio), "El precio del extra no puede ser negativo.");
        }

        Precio = precio;
    }
}
