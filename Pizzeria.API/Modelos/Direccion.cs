using System.Text.Json.Serialization;

namespace Pizzeria.API.Modelos
{
    public class Direccion
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        [JsonIgnore]
        public Cliente? Cliente { get; set; }
        public string Calle { get; set; } = string.Empty;
        public string Altura { get; set; } = string.Empty;
        public string? PisoDepto { get; set; }
        public string? Aclaraciones { get; set; }
    }
}
