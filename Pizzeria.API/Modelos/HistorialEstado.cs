using System.Text.Json.Serialization;

namespace Pizzeria.API.Modelos
{
    public class HistorialEstado
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }
        [JsonIgnore]
        public Pedido? Pedido { get; set; }

        public int EstadoId { get; set; }
        public Estado? Estado { get; set; }

        // Null si lo hizo el sistema (pedido creado desde la app)
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime Fecha { get; set; }
    }
}
