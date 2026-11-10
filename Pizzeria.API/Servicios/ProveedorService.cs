using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;

namespace Pizzeria.API.Servicios
{
    public class ProveedorService
    {
        private readonly IProveedorRepository _repositorio;

        public ProveedorService(IProveedorRepository repositorio) 
        {
            _repositorio = repositorio;
        }

        public async Task<List<Proveedor>> ObtenerPaginado(int pagina, int tamanoPagina)
        {
            return await _repositorio.ObtenerPaginadoAsync(pagina, tamanoPagina);
        }

        public async Task<Proveedor> ObtenerPorId(int id) 
        {
            return await _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<List<Proveedor>> ObtenerTodos()
        {
            return await _repositorio.ObtenerTodosAsync();
        }

        public async Task CrearProveedor(Proveedor proveedor) 
        {
            await _repositorio.AgregarAsync(proveedor);
        }

        public async Task ActualizarProveedor(Proveedor proveedor) 
        {
            await _repositorio.ActualizarAsync(proveedor);
        }

        public async Task EliminarProveedor(int id) 
        {
            await _repositorio.EliminarAsync(id);
        }
    }
}
