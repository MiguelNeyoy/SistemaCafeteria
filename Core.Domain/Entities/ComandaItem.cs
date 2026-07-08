namespace Core.Domain.Entities
{
    public class ComandaItem
    {
        public int Id { get; set; }

        /**
         * Esto es el profucto principal
         */
        public string ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }

        //Lista de productos adicionales

        public List<ExtraItem> Extras { get; set; } = new();



        public ComandaItem (string productoId, string productoNombre, string cantdad, DateTime fecha, string descripcion, List<ExtraItem> extras)
        {
            if (string.IsNullOrEmpty(productoId))
            {
                throw new ArgumentException("El ID del producto no puede estar vacío.");
            }
            if (string.IsNullOrEmpty(productoNombre))
            {
                throw new ArgumentException("El nombre del producto no puede estar vacío.");
            }
            if (string.IsNullOrEmpty(cantdad))
            {
                throw new ArgumentException("La cantidad no puede estar vacía.");
            }
            if (fecha == default)
            {
                throw new ArgumentException("La fecha no puede ser la predeterminada.");
            }
            ProductoId = productoId.Trim();
            ProductoNombre = productoNombre.Trim();
            Cantidad = cantdad.Trim();
            Fecha = fecha;
            Descripcion = descripcion?.Trim() ?? string.Empty;
            Extras = extras ?? new List<ExtraItem>();
        }

    }
     


}