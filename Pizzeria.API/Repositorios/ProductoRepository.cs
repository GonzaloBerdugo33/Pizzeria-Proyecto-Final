using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly MiApiContext _context;

        public ProductoRepository(MiApiContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerTodosAsync() 
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .ToListAsync();

        }

        public async Task<List<Producto>> ObtenerPaginadoAsync(int pagina, int tamanoPagina)
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
        }

        public async Task<Producto> ObtenerPorIdAsync(int id) 
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AgregarAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Producto producto) 
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id) 
        {
           bool tienePedidos = await _context.DetallesPedido.AnyAsync(d => d.ProductoId == id);
           bool tieneIngresos = await _context.Ingresos.AnyAsync(i => i.ProductoId == id);

            if (tienePedidos || tieneIngresos) 
            {
                throw new Exception("No se puede eliminar el producto porque tiene pedidos o ingresos. Desactivalo (Activo = false) en su lugar");
            }
            else
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto != null)
                {
                    _context.Productos.Remove(producto);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
