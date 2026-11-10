using Microsoft.EntityFrameworkCore;
using Pizzeria.API.Data;
using Pizzeria.API.Modelos;

namespace Pizzeria.API.Repositorios
{
    public interface IPedidoRepository
    {
        // Para los reportes: solo cuentan los pedidos Entregados
        Task<List<Pedido>> ObtenerEntregadosAsync();
    }

    public class PedidoRepository : IPedidoRepository
    {
        private readonly MiApiContext _context;

        public PedidoRepository(MiApiContext context)
        {
            _context = context;
        }

        public async Task<List<Pedido>> ObtenerEntregadosAsync()
        {
            return await _context.Pedidos
                .Where(p => p.EstadoId == EstadoPedido.Entregado)
                .Include(p => p.Cliente)
                .Include(p => p.MetodoPago)
                .Include(p => p.Detalles)
                .ToListAsync();
        }
    }
}
