using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly ProveedorService _service;

        public ProveedorController(ProveedorService service) 
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPaginado(int pagina = 1, int tamanoPagina = 10) 
        {
            var proveedores = await _service.ObtenerPaginado(pagina, tamanoPagina);
            return Ok(proveedores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id) 
        {
            var proveedor = await _service.ObtenerPorId(id);
            if (proveedor == null) return NotFound();
            return Ok(proveedor);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Proveedor proveedor) 
        {
            await _service.CrearProveedor(proveedor);
            return Ok(proveedor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, Proveedor proveedor) 
        {
            if (id != proveedor.Id) return BadRequest();
            await _service.ActualizarProveedor(proveedor);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _service.EliminarProveedor(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
