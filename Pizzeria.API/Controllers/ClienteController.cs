using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteService _service;

        public ClienteController(ClienteService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public async Task<IActionResult> ObtenerPaginado(int pagina = 1, int tamanoPagina = 10)
        {
            var clientes = await _service.ObtenerPaginado(pagina, tamanoPagina);
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Cajero")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _service.ObtenerPorId(id);
            if (cliente == null) return NotFound();
            return Ok(cliente);
        }

        // Alta de cliente (también sirve como "alta rápida" desde el mostrador)
        [HttpPost]
        [Authorize(Roles = "Administrador,Cajero")]
        public async Task<IActionResult> Crear(Cliente cliente)
        {
            try
            {
                await _service.CrearCliente(cliente);
                return Ok(cliente);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Cajero")]
        public async Task<IActionResult> Actualizar(int id, Cliente cliente)
        {
            if (id != cliente.Id) return BadRequest();
            try
            {
                await _service.ActualizarCliente(cliente);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _service.EliminarCliente(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
