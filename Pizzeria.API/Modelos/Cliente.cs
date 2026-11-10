using System.Text.Json.Serialization;

namespace Pizzeria.API.Modelos
{
    public class Cliente
    {
        public int Id { get; set; }
        // FK opcional: un cliente de mostrador/teléfono puede no tener cuenta
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        // Identidad del cliente: única, solo dígitos
        public string Telefono { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Direccion> Direcciones { get; set; } = new();
        [JsonIgnore]
        public List<Pedido> Pedidos { get; set; } = new();
    }
}
