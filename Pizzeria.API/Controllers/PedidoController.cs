using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pizzeria.API.DTOs;
using Pizzeria.API.Servicios;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PedidoController : ControllerBase
    {
        private readonly PedidoService _service;
        private readonly AuditoriaService _auditoria;

        public PedidoController(PedidoService service, AuditoriaService auditoria)
        {
            _service = service;
            _auditoria = auditoria;
        }

        // Id y email del usuario que viene en el token
        private bool TryGetUsuario(out int usuarioId, out string email)
        {
            email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            return int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out usuarioId);
        }

        // =============== CLIENTE (app móvil) ===============

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Crear(CrearPedidoDTO dto)
        {
            if (!TryGetUsuario(out var usuarioId, out var email)) return Unauthorized();

            try
            {
                var id = await _service.CrearDesdeAppAsync(dto, usuarioId);

                await _auditoria.Registrar("PedidoApp", "Pedido", id,
                    $"Pedido #{id} creado desde la app", usuarioId, email);

                return Ok(new { id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("mis-pedidos")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisPedidos()
        {
            if (!TryGetUsuario(out var usuarioId, out _)) return Unauthorized();
            return Ok(await _service.MisPedidosAsync(usuarioId));
        }

        [HttpGet("mis-direcciones")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisDirecciones()
        {
            if (!TryGetUsuario(out var usuarioId, out _)) return Unauthorized();
            return Ok(await _service.MisDireccionesAsync(usuarioId));
        }

        // El cliente consulta el estado de un pedido suyo (la app lo repite cada pocos segundos)
        [HttpGet("{id}")]
        [Authorize(Roles = "Cliente,Cajero,Administrador")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            if (!TryGetUsuario(out var usuarioId, out _)) return Unauthorized();
            var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var pedido = await _service.ObtenerAsync(id, usuarioId, rol);
            if (pedido == null) return NotFound();
            return Ok(pedido);
        }

        // Cliente (solo en "Recibido") o Cajero (antes de "Entregado")
        [HttpPost("{id}/cancelar")]
        [Authorize(Roles = "Cliente,Cajero")]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (!TryGetUsuario(out var usuarioId, out var email)) return Unauthorized();
            bool esCajero = User.IsInRole("Cajero");

            try
            {
                await _service.CancelarAsync(id, usuarioId, esCajero);

                await _auditoria.Registrar("CancelarPedido", "Pedido", id,
                    $"Pedido #{id} cancelado por {(esCajero ? "el cajero" : "el cliente")}", usuarioId, email);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =============== CAJERO (web) ===============

        [HttpPost("venta")]
        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> CrearVenta(CrearVentaDTO dto)
        {
            if (!TryGetUsuario(out var usuarioId, out var email)) return Unauthorized();

            try
            {
                var id = await _service.CrearVentaAsync(dto, usuarioId);

                await _auditoria.Registrar("VentaCajero", "Pedido", id,
                    $"Pedido #{id} registrado por el cajero para el cliente {dto.ClienteId}", usuarioId, email);

                return Ok(new { id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Pedidos en curso (o los últimos 100 si incluirFinalizados = true)
        [HttpGet("panel")]
        [Authorize(Roles = "Cajero,Administrador")]
        public async Task<IActionResult> Panel(bool incluirFinalizados = false)
        {
            return Ok(await _service.PanelAsync(incluirFinalizados));
        }

        [HttpGet("direcciones/{clienteId}")]
        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> DireccionesDeCliente(int clienteId)
        {
            return Ok(await _service.DireccionesDeClienteAsync(clienteId));
        }

        [HttpPost("{id}/avanzar")]
        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> Avanzar(int id, AvanzarPedidoDTO? dto)
        {
            if (!TryGetUsuario(out var usuarioId, out var email)) return Unauthorized();

            try
            {
                var estadoId = await _service.AvanzarAsync(id, dto?.CodigoEntrega, usuarioId);

                await _auditoria.Registrar("AvanzarPedido", "Pedido", id,
                    $"Pedido #{id} pasó al estado {estadoId}", usuarioId, email);

                return Ok(new { estadoId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/confirmar-pago")]
        [Authorize(Roles = "Cajero")]
        public async Task<IActionResult> ConfirmarPago(int id)
        {
            if (!TryGetUsuario(out var usuarioId, out var email)) return Unauthorized();

            try
            {
                await _service.ConfirmarPagoAsync(id);

                await _auditoria.Registrar("ConfirmarPago", "Pedido", id,
                    $"Pago confirmado del pedido #{id}", usuarioId, email);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
