namespace Pizzeria.API.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        // "Cliente", "Cajero" o "Administrador"
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public List<IngresoProducto> Ingresos { get; set; } = new();
    }
}
