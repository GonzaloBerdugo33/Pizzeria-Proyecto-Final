using Pizzeria.API.Data;
using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Repositorios
{
    public interface ICategoriaProductoRepository
    {
        Task<List<CategoriaProducto>> ObtenerTodosAsync();
        Task<CategoriaProducto> ObtenerPorIdAsync(int id);
        Task AgregarAsync(CategoriaProducto categoria);
        Task ActualizarAsync(CategoriaProducto categoria);
        Task EliminarAsync(int id);

    }
}
