using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Representa un producto o platillo del menú de la cafetería.
/// </summary>
public class Producto
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }
    public int CategoriaId { get; private set; }
    public bool Activo { get; private set; } = true;

    // Constructor privado para EF Core
    private Producto() { }

    // Constructor para nuevo producto en dominio
    public Producto(string nombre, decimal precio, int categoriaId)
    {
        ValidarYAsignarNombre(nombre);
        ValidarYAsignarPrecio(precio);
        ValidarYAsignarCategoria(categoriaId);
        Activo = true;
    }

    // Constructor para reconstrucción desde persistencia
    public Producto(int id, string nombre, decimal precio, int categoriaId, bool activo = true)
    {
        if (id < 0)
        {
            throw new DomainValidationException(nameof(Id), "El Id del producto no puede ser negativo.");
        }

        Id = id;
        ValidarYAsignarNombre(nombre);
        ValidarYAsignarPrecio(precio);
        ValidarYAsignarCategoria(categoriaId);
        Activo = activo;
    }

    public void ModificarPrecio(decimal nuevoPrecio)
    {
        ValidarYAsignarPrecio(nuevoPrecio);
    }

    public void CambiarNombre(string nuevoNombre)
    {
        ValidarYAsignarNombre(nuevoNombre);
    }

    public void CambiarCategoria(int nuevaCategoriaId)
    {
        ValidarYAsignarCategoria(nuevaCategoriaId);
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
            throw new DomainValidationException(nameof(Nombre), "El nombre del producto no puede estar vacío.");
        }

        Nombre = nombre.Trim();
    }

    private void ValidarYAsignarPrecio(decimal precio)
    {
        if (precio <= 0)
        {
            throw new DomainValidationException(nameof(Precio), "El precio del producto debe ser mayor a cero.");
        }

        Precio = precio;
    }

    private void ValidarYAsignarCategoria(int categoriaId)
    {
        if (categoriaId <= 0)
        {
            throw new DomainValidationException(nameof(CategoriaId), "La categoría del producto es inválida o no fue seleccionada.");
        }

        CategoriaId = categoriaId;
    }
}
