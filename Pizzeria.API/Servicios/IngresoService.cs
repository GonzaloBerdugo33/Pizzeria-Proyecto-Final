using Microsoft.EntityFrameworkCore;
using Pizzeria.API.Data;
using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;

namespace Pizzeria.API.Servicios
{
    public class IngresoService
    {
        private readonly IIngresoProductoRepository _ingresoRepository;
        private readonly MiApiContext _context;

        public IngresoService(IIngresoProductoRepository ingresoRepository, MiApiContext context)
        {
            _ingresoRepository = ingresoRepository;
            _context = context;
        }

        public async Task<List<ListarIngresoDTO>> ListarIngresosAsync()
        {
            var ingresos = await _ingresoRepository.ObtenerTodosAsync();

            return ingresos
                .OrderByDescending(i => i.Fecha)
                .Select(i => new ListarIngresoDTO
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    ProveedorRazonSocial = i.Proveedor?.RazonSocial ?? "Desconocido",
                    ProductoNombre = i.Producto?.Nombre ?? "Desconocido",
                    UsuarioEmail = i.Usuario?.Email ?? "Desconocido",
                    TotalItems = i.Cantidad,
                    CostoUnitario = i.CostoUnitario
                }).ToList();
        }

        public async Task<IngresoProducto> ObtenerPorId(int id)
        {
            return await _ingresoRepository.ObtenerPorIdAsync(id);
        }

        // Cada detalle del DTO se guarda como un ingreso de una sola línea
        // y suma el stock del producto. Todo en una transacción.
        public async Task<int> RegistrarIngresoAsync(CrearIngresoDTO dto, int usuarioId)
        {
            if (dto.ProveedorId <= 0)
                throw new ArgumentException("Debe indicar un proveedor válido.");

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ArgumentException("El ingreso debe contener al menos un producto.");

            if (dto.Detalles.Any(d => d.Cantidad <= 0))
                throw new ArgumentException("Las cantidades deben ser mayores a cero.");

            if (dto.Detalles.Any(d => d.CostoUnitario < 0))
                throw new ArgumentException("El costo unitario no puede ser negativo.");

            if (!await _context.Proveedores.AnyAsync(p => p.Id == dto.ProveedorId))
                throw new ArgumentException("El proveedor indicado no existe.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            int ultimoId = 0;

            foreach (var d in dto.Detalles)
            {
                // Suma atómica en la base: no pisa lo que otro pedido haya descontado en paralelo
                var filas = await _context.Productos
                    .Where(p => p.Id == d.ProductoId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + d.Cantidad));

                if (filas == 0)
                    throw new ArgumentException($"El producto con ID {d.ProductoId} no existe.");

                var ingreso = new IngresoProducto
                {
                    ProveedorId = dto.ProveedorId,
                    ProductoId = d.ProductoId,
                    UsuarioId = usuarioId,
                    Cantidad = d.Cantidad,
                    CostoUnitario = d.CostoUnitario,
                    Fecha = DateTime.UtcNow
                };

                _context.Ingresos.Add(ingreso);
                await _context.SaveChangesAsync();
                ultimoId = ingreso.Id;
            }

            await tx.CommitAsync();
            return ultimoId;
        }
    }
}
