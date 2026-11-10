using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    public class IngresoController : ControllerBase
    {
        private readonly IngresoService _service;
        private readonly AuditoriaService _auditoria;

        public IngresoController(IngresoService service, AuditoriaService auditoria) 
        {
            _service = service;
            _auditoria = auditoria;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos() 
        {
            var dtos = await _service.ListarIngresosAsync();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id) 
        {
            var ingreso = await _service.ObtenerPorId(id);
            if (ingreso == null) return NotFound();
            return Ok(ingreso);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearIngresoDTO dto)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("id")?.Value;

                if (string.IsNullOrEmpty(usuarioIdClaim))
                {
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario autenticado." });
                }

                int usuarioIdLogueado = int.Parse(usuarioIdClaim);

                var id = await _service.RegistrarIngresoAsync(dto, usuarioIdLogueado);

                var email = User.FindFirst(ClaimTypes.Email)?.Value
                         ?? User.FindFirst("email")?.Value
                         ?? "";
                var unidades = dto.Detalles.Sum(d => d.Cantidad);

                await _auditoria.Registrar("Ingreso", "Ingreso", id,
                    $"Ingreso de proveedor {dto.ProveedorId}, {unidades} unidades",
                    usuarioIdLogueado, email);

                return Ok(new { mensaje = "Ingreso registrado con éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
