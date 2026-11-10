using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public class CategoriaProductoRepository : ICategoriaProductoRepository
    {
        private readonly MiApiContext _context;

        public CategoriaProductoRepository(MiApiContext context)
        {
            _context = context;
        }

        public async Task<List<CategoriaProducto>> ObtenerTodosAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<CategoriaProducto> ObtenerPorIdAsync(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }

        public async Task AgregarAsync(CategoriaProducto categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(CategoriaProducto categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            bool tieneProductos = await _context.Productos.AnyAsync(p => p.CategoriaId == id);

            if (tieneProductos)
            {
                throw new Exception("No podemos eliminar la Categoria porque contiene productos asociados");
            }
            else
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria != null)
                {
                    _context.Categorias.Remove(categoria);
                    await _context.SaveChangesAsync();
                }
            }

        }
    }
}
