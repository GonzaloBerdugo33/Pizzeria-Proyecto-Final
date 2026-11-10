using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly MiApiContext _context;

        public UsuarioRepository(MiApiContext context) 
        {
            _context = context;
        }

        public async Task<List<Usuario>> ObtenerTodosAsync() 
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario> ObtenerPorIdAsync(int id) 
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario> ObtenerPorEmailAsync(string email) 
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AgregarAsync(Usuario usuario) 
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Usuario usuario) 
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id) 
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Activo = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
