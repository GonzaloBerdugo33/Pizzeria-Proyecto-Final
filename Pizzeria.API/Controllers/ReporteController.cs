using Pizzeria.API.Repositorios;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly ProductoService _productoService;
        private readonly IIngresoProductoRepository _ingresoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IReporteService _pdf;

        public ReporteController(ProductoService productoService,
            IIngresoProductoRepository ingresoRepository,
            IPedidoRepository pedidoRepository,
            IReporteService pdf)
        {
            _productoService = productoService;
            _ingresoRepository = ingresoRepository;
            _pedidoRepository = pedidoRepository;
            _pdf = pdf;
        }

        [HttpGet("Stock")]
        public async Task<IActionResult> Stock()
        {
            var productos = await _productoService.ObtenerTodos();
            var archivo = _pdf.GenerarListadoStock(productos.OrderBy(p => p.Nombre).ToList());
            return File(archivo, "application/pdf", $"stock-{DateTime.UtcNow.AddHours(-3):yyyyMMdd}.pdf");
        }

        [HttpGet("Entradas")]
        public async Task<IActionResult> Entradas()
        {
            var ingresos = await _ingresoRepository.ObtenerTodosAsync();
            var archivo = _pdf.GenerarListadoEntradas(ingresos.OrderByDescending(i => i.Fecha).ToList());
            return File(archivo, "application/pdf", $"entradas-{DateTime.UtcNow.AddHours(-3):yyyyMMdd}.pdf");
        }

        [HttpGet("Ventas")]
        public async Task<IActionResult> Ventas()
        {
            var pedidos = await _pedidoRepository.ObtenerEntregadosAsync();
            var archivo = _pdf.GenerarListadoVentas(pedidos.OrderByDescending(p => p.Fecha).ToList());
            return File(archivo, "application/pdf", $"ventas-{DateTime.UtcNow.AddHours(-3):yyyyMMdd}.pdf");
        }
    }
}