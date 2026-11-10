using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizzeria.API.DTOs;
using Pizzeria.API.Servicios;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiendaController : ControllerBase
    {
        private readonly TiendaService _service;
        private readonly AuditoriaService _auditoria;

        public TiendaController(TiendaService service, AuditoriaService auditoria)
        {
            _service = service;
            _auditoria = auditoria;
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroClienteDTO dto)
        {
            try
            {
                var usuario = await _service.RegistrarClienteAsync(dto);

                await _auditoria.Registrar("RegistroCliente", "Usuario", usuario.Id,
                    $"Registro de cliente desde la app: {usuario.Email}",
                    usuario.Id, usuario.Email);

                return Ok(usuario);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
