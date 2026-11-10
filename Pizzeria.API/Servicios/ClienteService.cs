using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;

namespace Pizzeria.API.Servicios
{
    public class ClienteService
    {
        private readonly IClienteRepository _repositorio;

        public ClienteService(IClienteRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<Cliente>> ObtenerPaginado(int pagina, int tamanoPagina)
        {
            return await _repositorio.ObtenerPaginadoAsync(pagina, tamanoPagina);
        }

        public async Task<Cliente> ObtenerPorId(int id)
        {
            return await _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<List<Cliente>> ObtenerTodos()
        {
            return await _repositorio.ObtenerTodosAsync();
        }

        // El teléfono es la identidad del cliente: solo dígitos y único
        private static void Validar(string nombre, string telefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new InvalidOperationException("El nombre es obligatorio.");

            if (!Regex.IsMatch(telefono ?? "", @"^\d{8,15}$"))
                throw new InvalidOperationException("El teléfono debe tener solo números (entre 8 y 15 dígitos).");
        }

        public async Task CrearCliente(Cliente cliente)
        {
            Validar(cliente.Nombre, cliente.Telefono);

            // Un cliente nuevo nunca nace con cuenta: la vincula el registro desde la app
            cliente.Id = 0;
            cliente.UsuarioId = null;

            try
            {
                await _repositorio.AgregarAsync(cliente);
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Ya existe un cliente con ese teléfono.");
            }
        }

        public async Task ActualizarCliente(Cliente datos)
        {
            Validar(datos.Nombre, datos.Telefono);

            var existente = await _repositorio.ObtenerPorIdAsync(datos.Id);
            if (existente == null)
                throw new InvalidOperationException("El cliente no existe.");

            // Solo se cambian nombre y teléfono: no se pisa el vínculo con el usuario
            existente.Nombre = datos.Nombre;
            existente.Telefono = datos.Telefono;

            try
            {
                await _repositorio.ActualizarAsync(existente);
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Ya existe un cliente con ese teléfono.");
            }
        }

        public async Task EliminarCliente(int id)
        {
            await _repositorio.EliminarAsync(id);
        }
    }
}
