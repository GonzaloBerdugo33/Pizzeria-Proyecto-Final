using System.ComponentModel.DataAnnotations;

namespace Pizzeria.API.Modelos 
{
    public class Auditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int? UsuarioId { get; set; }
        public string UsuarioEmail { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public int? EntidadId { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public string? Ip { get; set; }
    }
}