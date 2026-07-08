namespace Core.Domain.Entities
/**
 * Clase que representa un producto en el sistema.
    Nombre: Nombre que recibe el producto
    Precion: Precio que tiene el producto
    Categoria: Categoria a la que pertenece el producto por ejmplo cafe, platillo, jugo Etc.
 */

{
    public class Producto
    {
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; }

        public Producto(string nombre, decimal precio, string categoria)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                throw new ArgumentException("El nombre del producto no puede estar vacío.");
            }

            if (precio <= 0)
            {
                throw new ArgumentException("El precio del producto debe ser mayor a cero.");
            }

            if (string.IsNullOrEmpty(categoria))
            {
                throw new ArgumentException("La categoría del producto no puede estar vacía.");
            }

            Nombre = nombre.Trim();
            Precio = precio;
            Categoria = categoria.Trim();
        }

        public void ModificarPrecio(decimal nuevoPrecio)
        {
            if (nuevoPrecio <= 0)
            {
                throw new ArgumentException("El nuevo precio del producto debe ser mayor a cero.");
            }

            Precio = nuevoPrecio;
        }

        public void CambiarCategoria(string nuevaCategoria)
        {
            if (string.IsNullOrEmpty(nuevaCategoria))
            {
                throw new ArgumentException("La nueva categoría del producto no puede estar vacía.");
            }

            Categoria = nuevaCategoria.Trim();
        }

        public void CambiarNombre(string nuevoNombre)
        {
            if (string.IsNullOrEmpty(nuevoNombre) || !System.Text.RegularExpressions.Regex.IsMatch(nuevoNombre, @"^[a-zA-Z0-9 ]+$"))
            {
                throw new ArgumentException("El nuevo nombre del producto no puede estar vacío.");
            }

            Nombre = nuevoNombre.Trim();
        }

    }

}
