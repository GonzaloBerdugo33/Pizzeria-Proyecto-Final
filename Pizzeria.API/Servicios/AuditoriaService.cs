using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Servicios
{
    public class AuditoriaService
    {
        private readonly MiApiContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(
            MiApiContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditoriaService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task Registrar(string accion, string entidad, int? entidadId,
            string detalle, int? usuarioId = null, string usuarioEmail = "")
        {
            try
            {
                var auditoria = new Auditoria
                {
                    Fecha = DateTime.UtcNow,
                    UsuarioId = usuarioId,
                    UsuarioEmail = usuarioEmail,
                    Accion = accion,
                    Entidad = entidad,
                    EntidadId = entidadId,
                    Detalle = detalle.Length > 500 ? detalle.Substring(0, 500) : detalle,
                    Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                };

                _context.Auditorias.Add(auditoria);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo registrar la auditoría {Accion} {Entidad}", accion, entidad);
            }
        }

        public async Task<(List<Auditoria> items, int total)> ObtenerPaginado(int pagina, int tamanoPagina)
        {
            var total = await _context.Auditorias.CountAsync();

            var items = await _context.Auditorias
                .AsNoTracking()
                .OrderByDescending(a => a.Fecha)
                .ThenByDescending(a => a.Id)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            return (items, total);
        }
    }
}