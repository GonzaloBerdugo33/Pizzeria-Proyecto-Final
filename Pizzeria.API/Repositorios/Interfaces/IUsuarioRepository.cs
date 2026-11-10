using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> ObtenerTodosAsync();
        Task<Usuario> ObtenerPorIdAsync(int id);
        Task<Usuario> ObtenerPorEmailAsync(string email);
        Task AgregarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task EliminarAsync(int id);
    }
}