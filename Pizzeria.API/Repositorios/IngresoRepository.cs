using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public class IngresoRepository : IIngresoProductoRepository
    {
        private readonly MiApiContext _context;

        public IngresoRepository(MiApiContext context) 
        {
            _context = context;
        }

        public async Task<List<IngresoProducto>> ObtenerTodosAsync()
        {
            return await _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Usuario)
                .Include(i => i.Producto)
                .ToListAsync();
        }

        public async Task<IngresoProducto> ObtenerPorIdAsync(int id) 
        {
            return await _context.Ingresos.FindAsync(id);
        }

        public async Task AgregarAsync(IngresoProducto ingreso)
        {
            _context.Ingresos.Add(ingreso);
            await _context.SaveChangesAsync();
        }
    }
}
