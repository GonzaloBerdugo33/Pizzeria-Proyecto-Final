using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;
using Pizzeria.API.Modelos;
using Pizzeria.API.Servicios;
using Pizzeria.API.DTOs;

namespace Pizzeria.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;
        private readonly JwtService _jwtService;
        private readonly AuditoriaService _auditoria;

        public UsuarioController(UsuarioService service, JwtService jwtService, AuditoriaService auditoria)
        {
            _service = service;
            _jwtService = jwtService;
            _auditoria = auditoria;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var usuarios = await _service.ListarUsuarios();
            var dtos = usuarios.Select(u => new UsuarioDTO
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = u.Rol
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles ="Administrador")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var usuario = await _service.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            var dto = new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol
            };
            return Ok(dto);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO dto) 
        {
            var usuario = await _service.ValidarCredenciales(dto.Email, dto.Password);
            if(usuario == null) 
            {
                await _auditoria.Registrar("LoginFallido", "Usuario", null,
                    "Intento de inicio de sesión con credenciales inválidas", null, dto.Email);
                return Unauthorized("Credenciales Invalidas");
            }

            await _auditoria.Registrar("Login", "Usuario", usuario.Id,
                "Inicio de sesión correcto", usuario.Id, usuario.Email);

            var token = _jwtService.GenerarToken(usuario);
            return Ok(new { token = token });
        }

        [HttpPost("Register")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Registrar(CrearUsuarioDTO dto)
        {

            var resultado = await _service.RegistrarAsync(dto);

            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

            await _auditoria.Registrar("RegistroUsuario", "Usuario", resultado.Id,
                $"Alta de usuario {resultado.Email} con rol {resultado.Rol}", adminId, adminEmail);

            return Ok(resultado);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, CrearUsuarioDTO dto)
        {
            var usuario = new Usuario
            {
                Id = id,
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = dto.Rol
            };
            await _service.ActualizarUsuario(usuario);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _service.EliminarUsuario(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}