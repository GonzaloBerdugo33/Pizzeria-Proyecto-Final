using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Pizzeria.API.Data;
using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;

namespace Pizzeria.API.Servicios
{
    // Lógica de la app móvil del cliente. (Pedidos: se suman en el paso 4)
    public class TiendaService
    {
        private readonly MiApiContext _context;
        private readonly UsuarioService _usuarioService;

        public TiendaService(MiApiContext context, UsuarioService usuarioService)
        {
            _context = context;
            _usuarioService = usuarioService;
        }

        // ---------- Registro de cliente desde la app ----------
        // El teléfono es la identidad: si el local ya lo cargó (pedido por mostrador o teléfono),
        // se vincula esa ficha a la cuenta nueva. Si esa ficha ya tiene cuenta, se rechaza.
        public async Task<UsuarioDTO> RegistrarClienteAsync(RegistroClienteDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new InvalidOperationException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidOperationException("El email es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new InvalidOperationException("La contraseña debe tener al menos 6 caracteres.");
            if (!Regex.IsMatch(dto.Telefono ?? "", @"^\d{8,15}$"))
                throw new InvalidOperationException("El teléfono debe tener solo números (entre 8 y 15 dígitos).");

            var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c => c.Telefono == dto.Telefono);
            if (clienteExistente != null && clienteExistente.UsuarioId != null)
                throw new InvalidOperationException("Ese teléfono ya tiene una cuenta registrada.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            // 1) Usuario con rol "Cliente" fijo (la app no puede elegir otro).
            //    RegistrarAsync ya controla que el email no esté repetido.
            var usuario = await _usuarioService.RegistrarAsync(new CrearUsuarioDTO
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password = dto.Password,
                Rol = "Cliente"
            });

            // 2) Se vincula la ficha existente o se crea una nueva
            if (clienteExistente != null)
            {
                clienteExistente.UsuarioId = usuario.Id;
            }
            else
            {
                _context.Clientes.Add(new Cliente
                {
                    Nombre = dto.Nombre,
                    Telefono = dto.Telefono,
                    UsuarioId = usuario.Id
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return usuario;
        }
    }
}
