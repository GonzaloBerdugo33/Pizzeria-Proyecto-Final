namespace Pizzeria.API.DTOs
{
    public class DireccionDTO
    {
        public int Id { get; set; }
        public string Calle { get; set; } = string.Empty;
        public string Altura { get; set; } = string.Empty;
        public string? PisoDepto { get; set; }
        public string? Aclaraciones { get; set; }
    }

    public class CrearPedidoDetalleDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearPedidoDTO
    {
        public int TipoEntrega { get; set; }      // 1 = Retiro, 2 = Envío
        public int MetodoPagoId { get; set; }     // 1 = Efectivo, 2 = Tarjeta, 3 = Mercado Pago
        public int? DireccionId { get; set; }     // dirección guardada del cliente (envío)
        public DireccionDTO? Direccion { get; set; } // o una dirección nueva (envío)
        public List<CrearPedidoDetalleDTO> Detalles { get; set; } = new();
    }

    // Venta que registra el Cajero (mostrador o teléfono)
    public class CrearVentaDTO
    {
        public int ClienteId { get; set; }        // el cliente siempre existe (nombre + teléfono)
        public int TipoEntrega { get; set; }
        public int MetodoPagoId { get; set; }
        public int? DireccionId { get; set; }
        public DireccionDTO? Direccion { get; set; }
        public List<CrearPedidoDetalleDTO> Detalles { get; set; } = new();
    }

    public class AvanzarPedidoDTO
    {
        // Solo se usa al pasar un envío a "Entregado"
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
        public string TipoEntregaTexto => TipoEntrega == 2 ? "Envío" : "Retiro";
        public string? Direccion { get; set; }
        public bool Pagado { get; set; }
        public string? CodigoEntrega { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public List<PedidoDetalleDTO> Detalles { get; set; } = new();
        public List<HistorialEstadoDTO> Historial { get; set; } = new();
    }
}
