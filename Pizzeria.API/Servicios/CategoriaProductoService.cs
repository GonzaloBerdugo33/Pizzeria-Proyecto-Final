using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;

namespace Pizzeria.API.Servicios
{
    public class CategoriaProductoService
    {
        private readonly ICategoriaProductoRepository _repositorio;

        public CategoriaProductoService(ICategoriaProductoRepository repoitorio) 
        {
            _repositorio = repoitorio;
        }

        public async Task<List<CategoriaProducto>> ListarCategorias()
        {
            return await _repositorio.ObtenerTodosAsync();
        }

        public async Task<CategoriaProducto> ObtenerPorId(int id)
        {
            return await _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task CrearCategoria(CategoriaProducto categoria)
        {
            await _repositorio.AgregarAsync(categoria);
        }

        public async Task ActualizarCategoria(CategoriaProducto categoria)
        {
            await _repositorio.ActualizarAsync(categoria);
        }

        public async Task EliminarCategoria(int id)
        {
            await _repositorio.EliminarAsync(id);
        }
    }
}
