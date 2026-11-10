namespace Pizzeria.API.Modelos
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; } = true;      // existe en el catálogo
        public bool Disponible { get; set; } = true;  // botón de pánico (Cajero/Admin)

        public int CategoriaId { get; set; }
        public CategoriaProducto? Categoria { get; set; }

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}
