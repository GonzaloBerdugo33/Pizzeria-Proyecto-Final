using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Pizzeria.API.Data;
using Pizzeria.API.DTOs;
using Pizzeria.API.Modelos;

namespace Pizzeria.API.Servicios
{
    public class PedidoService
    {
        private readonly MiApiContext _context;

        public PedidoService(MiApiContext context)
        {
            _context = context;
        }

        // =====================================================
        //  CREAR PEDIDOS
        // =====================================================

        // Cliente desde la app: tarjeta y Mercado Pago se aprueban al instante (simulado)
        public async Task<int> CrearDesdeAppAsync(CrearPedidoDTO dto, int usuarioId)
        {
            var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
            if (cliente == null)
                throw new InvalidOperationException("Tu usuario no tiene un cliente asociado.");

            bool pagado = dto.MetodoPagoId == MetodoPagoId.Tarjeta || dto.MetodoPagoId == MetodoPagoId.MercadoPago;

            return await CrearInternoAsync(cliente.Id, null, dto.TipoEntrega, dto.MetodoPagoId,
                dto.DireccionId, dto.Direccion, dto.Detalles, pagado);
        }

        // Cajero (mostrador o teléfono): la tarjeta se aprueba al instante;
        // Mercado Pago queda sin pagar hasta "Confirmar pago"; efectivo se paga al entregar
        public async Task<int> CrearVentaAsync(CrearVentaDTO dto, int cajeroId)
        {
            if (!await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId))
                throw new InvalidOperationException("El cliente indicado no existe.");

            bool pagado = dto.MetodoPagoId == MetodoPagoId.Tarjeta;

            return await CrearInternoAsync(dto.ClienteId, cajeroId, dto.TipoEntrega, dto.MetodoPagoId,
                dto.DireccionId, dto.Direccion, dto.Detalles, pagado);
        }

        private async Task<int> CrearInternoAsync(int clienteId, int? cajeroId, int tipoEntrega, int metodoPagoId,
            int? direccionId, DireccionDTO? direccionNueva, List<CrearPedidoDetalleDTO> detalles, bool pagado)
        {
            // ---------- Validaciones ----------
            if (detalles == null || detalles.Count == 0)
                throw new InvalidOperationException("El pedido debe tener al menos un producto.");
            if (detalles.Any(d => d.Cantidad <= 0))
                throw new InvalidOperationException("Las cantidades deben ser mayores a cero.");
            if (tipoEntrega != TipoEntrega.Retiro && tipoEntrega != TipoEntrega.Envio)
                throw new InvalidOperationException("Tipo de entrega inválido.");
            if (metodoPagoId != MetodoPagoId.Efectivo && metodoPagoId != MetodoPagoId.Tarjeta && metodoPagoId != MetodoPagoId.MercadoPago)
                throw new InvalidOperationException("Método de pago inválido.");

            // Si el mismo producto viene repetido, se suman las cantidades
            var lineas = detalles
                .GroupBy(d => d.ProductoId)
                .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToList();

            // Dirección: solo es obligatoria en los envíos
            int? direccionIdFinal = null;
            Direccion? direccionCreada = null;
            if (tipoEntrega == TipoEntrega.Envio)
            {
                if (direccionId != null)
                {
                    bool esDelCliente = await _context.Direcciones
                        .AnyAsync(d => d.Id == direccionId && d.ClienteId == clienteId);
                    if (!esDelCliente)
                        throw new InvalidOperationException("La dirección indicada no pertenece al cliente.");
                    direccionIdFinal = direccionId;
                }
                else if (direccionNueva != null
                         && !string.IsNullOrWhiteSpace(direccionNueva.Calle)
                         && !string.IsNullOrWhiteSpace(direccionNueva.Altura))
                {
                    direccionCreada = new Direccion
                    {
                        ClienteId = clienteId,
                        Calle = direccionNueva.Calle.Trim(),
                        Altura = direccionNueva.Altura.Trim(),
                        PisoDepto = direccionNueva.PisoDepto,
                        Aclaraciones = direccionNueva.Aclaraciones
                    };
                }
                else
                {
                    throw new InvalidOperationException("Para un envío tenés que indicar la dirección (calle y altura).");
                }
            }

            // ---------- Transacción: stock + pedido + historial ----------
            await using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var linea in lineas)
            {
                int cantidad = linea.Cantidad;
                int productoId = linea.ProductoId;

                // Descuento atómico: solo si está activo, disponible y alcanza el stock.
                // Si dos pedidos compiten por la última unidad, solo uno de los dos pasa.
                var filas = await _context.Productos
                    .Where(p => p.Id == productoId && p.Activo && p.Disponible && p.Stock >= cantidad)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - cantidad));

                if (filas == 0)
                {
                    var producto = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productoId);
                    if (producto == null)
                        throw new InvalidOperationException($"El producto {productoId} no existe.");
                    if (!producto.Activo || !producto.Disponible)
                        throw new InvalidOperationException($"{producto.Nombre} no está disponible.");
                    throw new InvalidOperationException($"No hay stock suficiente de {producto.Nombre}.");
                }
            }

            // Precios al momento de la compra
            var ids = lineas.Select(l => l.ProductoId).ToList();
            var precios = await _context.Productos
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Precio);

            var ahora = DateTime.UtcNow;
            var pedido = new Pedido
            {
                ClienteId = clienteId,
                CajeroId = cajeroId,
                EstadoId = EstadoPedido.Recibido,
                MetodoPagoId = metodoPagoId,
                TipoEntrega = tipoEntrega,
                DireccionId = direccionIdFinal,
                Direccion = direccionCreada,
                Pagado = pagado,
                // Código de 4 dígitos: solo en envíos
                CodigoEntrega = (tipoEntrega == TipoEntrega.Envio && cajeroId == null)
                    ? RandomNumberGenerator.GetInt32(0, 10000).ToString("D4")
                    : null,
                Fecha = ahora,
                Detalles = lineas.Select(l => new DetallePedido
                {
                    ProductoId = l.ProductoId,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = precios[l.ProductoId]
                }).ToList(),
                Historial = new List<HistorialEstado>
                {
                    new HistorialEstado { EstadoId = EstadoPedido.Recibido, UsuarioId = cajeroId, Fecha = ahora }
                }
            };
            pedido.Total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return pedido.Id;
        }

        // Cambios de Estado
        // Retiro: Recibido → En horno → Listo para retirar → Entregado
        // Envío:  Recibido → En horno → En camino → Entregado
        private static int SiguienteEstado(int estadoActual, int tipoEntrega)
        {
            return estadoActual switch
            {
                EstadoPedido.Recibido => EstadoPedido.EnHorno,
                EstadoPedido.EnHorno => tipoEntrega == TipoEntrega.Envio
                    ? EstadoPedido.EnCamino
                    : EstadoPedido.ListoParaRetirar,
                EstadoPedido.EnCamino or EstadoPedido.ListoParaRetirar => EstadoPedido.Entregado,
                _ => throw new InvalidOperationException("El pedido ya no puede avanzar de estado.")
            };
        }

        // Solo el Cajero avanza; el servidor decide cuál es el siguiente estado
        public async Task<int> AvanzarAsync(int pedidoId, string? codigoEntrega, int cajeroId)
        {
            var pedido = await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pedidoId);
            if (pedido == null)
                throw new KeyNotFoundException("El pedido no existe.");

            int actual = pedido.EstadoId;
            int siguiente = SiguienteEstado(actual, pedido.TipoEntrega);

            if (siguiente == EstadoPedido.Entregado)
            {
                if (!pedido.Pagado && pedido.MetodoPagoId != MetodoPagoId.Efectivo)
                    throw new InvalidOperationException("Falta confirmar el pago antes de entregar el pedido.");

                if (pedido.CodigoEntrega != null
                    && (string.IsNullOrWhiteSpace(codigoEntrega) || codigoEntrega.Trim() != pedido.CodigoEntrega))
                    throw new InvalidOperationException("El código de entrega es incorrecto.");
            }

            await using var tx = await _context.Database.BeginTransactionAsync();

            // El WHERE con el estado actual evita que dos cajeros avancen el mismo pedido a la vez
            int filas;
            if (siguiente == EstadoPedido.Entregado)
            {
                // Al entregar, el efectivo queda cobrado
                filas = await _context.Pedidos
                    .Where(p => p.Id == pedidoId && p.EstadoId == actual)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.EstadoId, siguiente)
                        .SetProperty(p => p.Pagado, true));
            }
            else
            {
                filas = await _context.Pedidos
                    .Where(p => p.Id == pedidoId && p.EstadoId == actual)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.EstadoId, siguiente));
            }

            if (filas == 0)
                throw new InvalidOperationException("El pedido cambió de estado. Actualizá la pantalla.");

            _context.HistorialEstados.Add(new HistorialEstado
            {
                PedidoId = pedidoId,
                EstadoId = siguiente,
                UsuarioId = cajeroId,
                Fecha = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return siguiente;
        }

        // Cliente: solo sus pedidos y solo en "Recibido". Cajero: cualquiera antes de "Entregado".
        public async Task CancelarAsync(int pedidoId, int usuarioId, bool esCajero)
        {
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                throw new KeyNotFoundException("El pedido no existe.");

            if (!esCajero)
            {
                // No se revela si el pedido existe cuando es de otro cliente
                if (pedido.Cliente?.UsuarioId != usuarioId)
                    throw new KeyNotFoundException("El pedido no existe.");
                if (pedido.EstadoId != EstadoPedido.Recibido)
                    throw new InvalidOperationException("Solo podés cancelar un pedido que todavía está en estado Recibido.");
            }
            else if (pedido.EstadoId == EstadoPedido.Entregado || pedido.EstadoId == EstadoPedido.Cancelado)
            {
                throw new InvalidOperationException("El pedido ya está finalizado y no se puede cancelar.");
            }

            int actual = pedido.EstadoId;

            await using var tx = await _context.Database.BeginTransactionAsync();

            // Cambio de estado atómico: evita devolver el stock dos veces si se cancela en paralelo
            var filas = await _context.Pedidos
                .Where(p => p.Id == pedidoId && p.EstadoId == actual)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.EstadoId, EstadoPedido.Cancelado));

            if (filas == 0)
                throw new InvalidOperationException("El pedido cambió de estado. Actualizá la pantalla.");

            // Devuelve el stock
            var items = await _context.DetallesPedido
                .AsNoTracking()
                .Where(d => d.PedidoId == pedidoId)
                .Select(d => new { d.ProductoId, d.Cantidad })
                .ToListAsync();

            foreach (var item in items)
            {
                int productoId = item.ProductoId;
                int cantidad = item.Cantidad;
                await _context.Productos
                    .Where(p => p.Id == productoId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + cantidad));
            }

            _context.HistorialEstados.Add(new HistorialEstado
            {
                PedidoId = pedidoId,
                EstadoId = EstadoPedido.Cancelado,
                UsuarioId = esCajero ? usuarioId : null,
                Fecha = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        // El Cajero verifica el comprobante (Mercado Pago) y confirma el pago
        public async Task ConfirmarPagoAsync(int pedidoId)
        {
            var filas = await _context.Pedidos
                .Where(p => p.Id == pedidoId
                         && !p.Pagado
                         && p.EstadoId != EstadoPedido.Cancelado
                         && p.MetodoPagoId != MetodoPagoId.Efectivo)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Pagado, true));

            if (filas == 0)
                throw new InvalidOperationException(
                    "No se puede confirmar el pago: el pedido no existe, ya está pagado, está cancelado o es en efectivo.");
        }

        // =====================================================
        //  CONSULTAS
        // =====================================================

        private static IQueryable<PedidoDTO> Proyectar(IQueryable<Pedido> query)
        {
            return query.Select(p => new PedidoDTO
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                ClienteNombre = p.Cliente!.Nombre,
                ClienteTelefono = p.Cliente!.Telefono,
                CajeroEmail = p.Cajero != null ? p.Cajero.Email : null,
                EstadoId = p.EstadoId,
                Estado = p.Estado!.Nombre,
                MetodoPagoId = p.MetodoPagoId,
                MetodoPago = p.MetodoPago!.Nombre,
                TipoEntrega = p.TipoEntrega,
                Direccion = p.Direccion != null
                    ? p.Direccion.Calle + " " + p.Direccion.Altura
                      + (p.Direccion.PisoDepto != null ? ", " + p.Direccion.PisoDepto : "")
                    : null,
                Pagado = p.Pagado,
                CodigoEntrega = p.CodigoEntrega,
                Fecha = p.Fecha,
                Total = p.Total,
                Detalles = p.Detalles.Select(d => new PedidoDetalleDTO
                {
                    ProductoId = d.ProductoId,
                    NombreProducto = d.Producto!.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                }).ToList(),
                Historial = p.Historial.OrderBy(h => h.Fecha).Select(h => new HistorialEstadoDTO
                {
                    Estado = h.Estado!.Nombre,
                    Fecha = h.Fecha,
                    Usuario = h.Usuario != null ? h.Usuario.Email : null
                }).ToList()
            });
        }

        // El código de entrega solo lo ve el cliente dueño del pedido
        private static void OcultarCodigo(IEnumerable<PedidoDTO> pedidos)
        {
            foreach (var p in pedidos) p.CodigoEntrega = null;
        }

        public async Task<List<PedidoDTO>> MisPedidosAsync(int usuarioId)
        {
            return await Proyectar(_context.Pedidos
                    .Where(p => p.Cliente!.UsuarioId == usuarioId)
                    .OrderByDescending(p => p.Fecha))
                .ToListAsync();
        }

        // Cliente: solo el suyo (con código). Cajero/Administrador: cualquiera (sin código).
        public async Task<PedidoDTO?> ObtenerAsync(int pedidoId, int usuarioId, string rol)
        {
            var query = _context.Pedidos.Where(p => p.Id == pedidoId);
            if (rol == "Cliente")
                query = query.Where(p => p.Cliente!.UsuarioId == usuarioId);

            var pedido = await Proyectar(query).FirstOrDefaultAsync();
            if (pedido != null && rol != "Cliente")
                OcultarCodigo(new[] { pedido });
            return pedido;
        }

        // Panel del Cajero: los pedidos en curso, del más viejo al más nuevo
        public async Task<List<PedidoDTO>> PanelAsync(bool incluirFinalizados)
        {
            List<PedidoDTO> pedidos;

            if (incluirFinalizados)
            {
                pedidos = await Proyectar(_context.Pedidos
                        .OrderByDescending(p => p.Fecha)
                        .Take(100))
                    .ToListAsync();
            }
            else
            {
                pedidos = await Proyectar(_context.Pedidos
                        .Where(p => p.EstadoId != EstadoPedido.Entregado && p.EstadoId != EstadoPedido.Cancelado)
                        .OrderBy(p => p.Fecha))
                    .ToListAsync();
            }

            OcultarCodigo(pedidos);
            return pedidos;
        }

        // ---------- Direcciones ----------
        public async Task<List<DireccionDTO>> MisDireccionesAsync(int usuarioId)
        {
            return await _context.Direcciones
                .AsNoTracking()
                .Where(d => d.Cliente!.UsuarioId == usuarioId)
                .Select(d => new DireccionDTO
                {
                    Id = d.Id,
                    Calle = d.Calle,
                    Altura = d.Altura,
                    PisoDepto = d.PisoDepto,
                    Aclaraciones = d.Aclaraciones
                })
                .ToListAsync();
        }

        public async Task<List<DireccionDTO>> DireccionesDeClienteAsync(int clienteId)
        {
            return await _context.Direcciones
                .AsNoTracking()
                .Where(d => d.ClienteId == clienteId)
                .Select(d => new DireccionDTO
                {
                    Id = d.Id,
                    Calle = d.Calle,
                    Altura = d.Altura,
                    PisoDepto = d.PisoDepto,
                    Aclaraciones = d.Aclaraciones
                })
                .ToListAsync();
        }
    }
}
