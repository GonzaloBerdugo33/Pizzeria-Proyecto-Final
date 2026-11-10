using Pizzeria.API.DTOs;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaService _service;

        public AuditoriaController(AuditoriaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPaginado(int pagina = 1, int tamanoPagina = 20)
        {
            if (pagina < 1) pagina = 1;
            if (tamanoPagina < 1) tamanoPagina = 20;
            if (tamanoPagina > 100) tamanoPagina = 100;

            var (items, total) = await _service.ObtenerPaginado(pagina, tamanoPagina);

            var dto = new AuditoriaPaginadaDTO
            {
                Total = total,
                Pagina = pagina,
                TamanoPagina = tamanoPagina,
                Items = items.Select(a => new AuditoriaDTO
                {
                    Id = a.Id,
                    Fecha = a.Fecha,
                    UsuarioId = a.UsuarioId,
                    UsuarioEmail = a.UsuarioEmail,
                    Accion = a.Accion,
                    Entidad = a.Entidad,
                    EntidadId = a.EntidadId,
                    Detalle = a.Detalle,
                    Ip = a.Ip
                }).ToList()
            };

            return Ok(dto);
        }
    }
}