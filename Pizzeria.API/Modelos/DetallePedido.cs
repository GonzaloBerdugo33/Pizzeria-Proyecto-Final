using System.Text.Json.Serialization;

namespace Pizzeria.API.Modelos
{
    public class DetallePedido
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }
        [JsonIgnore]
        public Pedido? Pedido { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }
        // Precio al momento de la compra
        public decimal PrecioUnitario { get; set; }
    }
}
