using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;

namespace Pizzeria.API.Servicios
{
    public class ProductoService
    {
        private readonly IProductoRepository _repositorio;

        public ProductoService(IProductoRepository repositorio) 
        {
            _repositorio = repositorio;
        }

        public async Task<List<Producto>> ObtenerPaginado(int pagina, int tamanoPagina) 
        {
            return await _repositorio.ObtenerPaginadoAsync(pagina, tamanoPagina);
        }

        public async Task<Producto> ObtenerPorId(int id) 
        {
            return await _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<List<Producto>> ObtenerTodos()
        {
            return await _repositorio.ObtenerTodosAsync();
        }

        public async Task CrearProducto(Producto producto) 
        {
            await _repositorio.AgregarAsync(producto);
        }

        public async Task<string> SubirImagen(int id, IFormFile archivo)
        {
            var producto = await _repositorio.ObtenerPorIdAsync(id);
            if (producto == null)
                throw new Exception("Producto no encontrado");

            if (archivo == null || archivo.Length == 0)
                throw new Exception("Archivo invalido");

            // Validar que sea una imagen (extension basica)
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(extension))
                throw new Exception("Formato de imagen no permitido");

            // Carpeta destino: wwwroot/uploads
            var carpetaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(carpetaUploads))
                Directory.CreateDirectory(carpetaUploads);

            // Nombre unico para evitar colisiones/sobrescritura
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpetaUploads, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Ruta relativa que se guarda en la BD
            var rutaRelativa = $"/uploads/{nombreArchivo}";
            producto.ImagenUrl = rutaRelativa;
            await _repositorio.ActualizarAsync(producto);

            return rutaRelativa;
        }

        // Botón de pánico: lo usan Cajero y Administrador
        public async Task<bool> CambiarDisponible(int id, bool disponible)
        {
            var producto = await _repositorio.ObtenerPorIdAsync(id);
            if (producto == null) return false;

            producto.Disponible = disponible;
            await _repositorio.ActualizarAsync(producto);
            return true;
        }

        public async Task ActualizarProducto(Producto producto) 
        {
            await _repositorio.ActualizarAsync(producto);
        }

        public async Task EliminarProducto(int id) 
        {
            await _repositorio.EliminarAsync(id);
        }
    }
}
