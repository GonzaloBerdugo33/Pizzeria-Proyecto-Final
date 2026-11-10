using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;

namespace Pizzeria.API.Servicios
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _repositorio;

        public UsuarioService(IUsuarioRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<Usuario>> ListarUsuarios()
        {
            return await _repositorio.ObtenerTodosAsync();
        }

        public async Task<Usuario> ObtenerPorId(int id)
        {
            return await _repositorio.ObtenerPorIdAsync(id);
        }

        public async Task<Usuario?> ObtenerPorEmail(string email)
        {
            return await _repositorio.ObtenerPorEmailAsync(email);
        }

        public async Task<Usuario> ValidarCredenciales(string email, string password)
        {
            var usuario = await _repositorio.ObtenerPorEmailAsync(email);
            if (usuario != null && usuario.Activo)
            {
                if (BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                {
                    return usuario;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public async Task<UsuarioDTO> RegistrarAsync(CrearUsuarioDTO dto)
        {
            var usuarioExistente = await _repositorio.ObtenerPorEmailAsync(dto.Email);
            if (usuarioExistente != null)
            {
                throw new InvalidOperationException("El email ya se encuentra registrado.");
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol
            };

            await _repositorio.AgregarAsync(usuario);

            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol
            };
        }

        public async Task ActualizarUsuario(Usuario usuario)
        {
            await _repositorio.ActualizarAsync(usuario);
        }

        public async Task EliminarUsuario(int id)
        {
            await _repositorio.EliminarAsync(id);
        }
    }
}