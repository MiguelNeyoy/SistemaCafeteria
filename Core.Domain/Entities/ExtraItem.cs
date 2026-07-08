

namespace Core.Domain.Entities
{
    public class ExtraItem
    {
        public int ExtraId { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }



        public ExtraItem(int extraId, string nombre, decimal precio)
        {
            if (extraId <= 0)
            {
                throw new ArgumentException("El ID del extra debe ser mayor a cero.");
            }
            if (string.IsNullOrEmpty(nombre))
            {
                throw new ArgumentException("El nombre del extra no puede estar vacío.");
            }
            if (precio < 0)
            {
                throw new ArgumentException("El precio del extra no puede ser negativo.");
            }
            ExtraId = extraId;
            Nombre = nombre.Trim();
            Precio = precio;
        }


        public void CambiarNombre(string nuevoNombre)
        {
            if (string.IsNullOrEmpty(nuevoNombre))
            {
                throw new ArgumentException("El nuevo nombre del extra no puede estar vacío.");
            }
            Nombre = nuevoNombre.Trim();
        }

        public void CambiarPrecio(decimal nuevoPrecio)
        {
            if (nuevoPrecio < 0)
            {
                throw new ArgumentException("El nuevo precio del extra no puede ser negativo.");
            }
            Precio = nuevoPrecio;

        }

    }
}
