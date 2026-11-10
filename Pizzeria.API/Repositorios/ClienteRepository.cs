using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly MiApiContext _context;

        public ClienteRepository(MiApiContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ObtenerTodosAsync() 
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<List<Cliente>> ObtenerPaginadoAsync(int pagina, int tamanoPagina)
        {
            return await _context.Clientes
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
        }

        public async Task<Cliente> ObtenerPorIdAsync(int id) 
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task AgregarAsync(Cliente cliente) 
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Cliente cliente) 
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id) 
        {
            var tienePedidos = await _context.Pedidos.AnyAsync(p => p.ClienteId == id);
            if (tienePedidos)
            {
                throw new Exception("No se puede eliminar el cliente porque tiene pedidos");
            }
            else 
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if(cliente != null) 
                {
                    _context.Clientes.Remove(cliente);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
