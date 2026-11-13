namespace Pizzeria.Cliente.Movil.DTOs
{
    // ---------- Login y usuario ----------
    public class LoginDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResultDTO
    {
        public string Token { get; set; } = string.Empty;
    }

    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    // ---------- Menú ----------
    public class ProductoDTO
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public int ProveedorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public bool Disponible { get; set; } = true;
        public string? CategoriaNombre { get; set; }
        public string? ProveedorNombre { get; set; }
    }

    // ---------- Registro (sin dirección: se carga al hacer el primer envío) ----------
    public class RegistroClienteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }

    // ---------- Pedidos ----------
    public class DireccionDTO
    {
        public int Id { get; set; }
        public string Calle { get; set; } = string.Empty;
        public string Altura { get; set; } = string.Empty;
        public string? PisoDepto { get; set; }
        public string? Aclaraciones { get; set; }

        public string Texto => $"{Calle} {Altura}" + (string.IsNullOrWhiteSpace(PisoDepto) ? "" : $", {PisoDepto}");
    }

    public class CrearPedidoDetalleDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearPedidoDTO
    {
        public int TipoEntrega { get; set; }      // 1 Retiro, 2 Envío
        public int MetodoPagoId { get; set; }     // 1 Efectivo, 2 Tarjeta, 3 Mercado Pago
        public int? DireccionId { get; set; }
        public DireccionDTO? Direccion { get; set; }
        public List<CrearPedidoDetalleDTO> Detalles { get; set; } = new();
    }

    public class PedidoDetalleDTO
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class HistorialEstadoDTO
    {
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string? Usuario { get; set; }
    }

    public class PedidoDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string? ClienteTelefono { get; set; }
        public string? CajeroEmail { get; set; }
        public int EstadoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int MetodoPagoId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public int TipoEntrega { get; set; }
        public string TipoEntregaTexto { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public bool Pagado { get; set; }
        public string? CodigoEntrega { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public List<PedidoDetalleDTO> Detalles { get; set; } = new();
        public List<HistorialEstadoDTO> Historial { get; set; } = new();
    }
}
