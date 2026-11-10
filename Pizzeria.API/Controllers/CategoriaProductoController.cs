using Pizzeria.API.Modelos;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriaProductoController : ControllerBase
    {
        private readonly CategoriaProductoService _service;

        public CategoriaProductoController(CategoriaProductoService service) 
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos() 
        {
            var categorias = await _service.ListarCategorias();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerPorId(int id) 
        {
            var categoria = await _service.ObtenerPorId(id);
            if (categoria == null) return NotFound();
            return Ok(categoria);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear(CategoriaProducto categoria) 
        {
            await _service.CrearCategoria(categoria);
            return Ok(categoria);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, CategoriaProducto categoria) 
        {
            if (id != categoria.Id) return BadRequest();
            await _service.ActualizarCategoria(categoria);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id) 
        {
            try 
            {
                await _service.EliminarCategoria(id);
                return NoContent();
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
