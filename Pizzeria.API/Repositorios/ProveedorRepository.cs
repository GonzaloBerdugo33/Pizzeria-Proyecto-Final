using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly MiApiContext _context;

        public ProveedorRepository(MiApiContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return await _context.Proveedores.ToListAsync();
        }

        public async Task<List<Proveedor>> ObtenerPaginadoAsync(int pagina, int tamanoPagina)
        {
            return await _context.Proveedores
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();
        }

        public async Task<Proveedor> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedores.FindAsync(id);
        }

        public async Task AgregarAsync(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Proveedor proveedor) 
        {
            _context.Proveedores.Update(proveedor);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id) 
        {
            bool tieneProductos = await _context.Productos.AnyAsync(p => p.ProveedorId ==  id);
            bool tieneIngresos = await _context.Ingresos.AnyAsync(i => i.ProveedorId == id);

            if(tieneProductos || tieneIngresos)
            {
                throw new Exception("No se puede eliminar proveedor porque cuenta con Productos o Ingresos");
            }
            else 
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor != null) 
                {
                    _context.Proveedores.Remove(proveedor);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
