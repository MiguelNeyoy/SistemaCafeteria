using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Representa una categoría del menú (ej. Café, Platillos, Bebidas, Postres).
/// </summary>
public class Categoria
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    // Constructor privado para EF Core
    private Categoria() { }

    // Constructor para creación en el dominio
    public Categoria(string nombre)
    {
        ValidarYAsignarNombre(nombre);
        Activo = true;
    }

    // Constructor para reconstrucción desde persistencia
    public Categoria(int id, string nombre, bool activo = true)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id de la categoría no puede ser negativo.");
        }

        Id = id;
        ValidarYAsignarNombre(nombre);
        Activo = activo;
    }

    public void CambiarNombre(string nuevoNombre)
    {
        ValidarYAsignarNombre(nuevoNombre);
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
            throw new DomainValidationException(nameof(Nombre), "El nombre de la categoría no puede estar vacío.");
        }

        Nombre = nombre.Trim();
    }
}
