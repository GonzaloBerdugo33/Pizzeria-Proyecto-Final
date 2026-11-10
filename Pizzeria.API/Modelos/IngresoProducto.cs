namespace Pizzeria.API.Modelos
{
    // Una fila = un producto ingresado (sin tabla de detalle)
    public class IngresoProducto
    {
        public int Id { get; set; }
        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime Fecha { get; set; }
    }
}
