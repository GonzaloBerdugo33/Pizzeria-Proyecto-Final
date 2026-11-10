namespace Pizzeria.API.Modelos
{
    public class Pedido
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Null si el pedido lo hizo el cliente desde la app
        public int? CajeroId { get; set; }
        public Usuario? Cajero { get; set; }

        public int EstadoId { get; set; }
        public Estado? Estado { get; set; }

        public int MetodoPagoId { get; set; }
        public MetodoPago? MetodoPago { get; set; }

        // 1 = Retiro, 2 = Envío (ver TipoEntrega)
        public int TipoEntrega { get; set; }

        // Obligatoria solo si es envío
        public int? DireccionId { get; set; }
        public Direccion? Direccion { get; set; }

        public bool Pagado { get; set; }

        // 4 dígitos, solo en envíos
        public string? CodigoEntrega { get; set; }

        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        public List<DetallePedido> Detalles { get; set; } = new();
        public List<HistorialEstado> Historial { get; set; } = new();
    }
}
