namespace Pizzeria.Cliente.Web.DTOs
{
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

    public class CrearVentaDTO
    {
        public int ClienteId { get; set; }
        public int TipoEntrega { get; set; }       // 1 Retiro, 2 Envío
        public int MetodoPagoId { get; set; }      // 1 Efectivo, 2 Tarjeta, 3 Mercado Pago
        public int? DireccionId { get; set; }
        public DireccionDTO? Direccion { get; set; }
        public List<CrearPedidoDetalleDTO> Detalles { get; set; } = new();
    }

    public class AvanzarPedidoDTO
    {
        public string? CodigoEntrega { get; set; }
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
        public string ClienteTelefono { get; set; } = string.Empty;
        public string? CajeroEmail { get; set; }
        public int EstadoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int MetodoPagoId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public int TipoEntrega { get; set; }
        public string TipoEntregaTexto { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public bool Pagado { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public List<PedidoDetalleDTO> Detalles { get; set; } = new();
        public List<HistorialEstadoDTO> Historial { get; set; } = new();
    }
}