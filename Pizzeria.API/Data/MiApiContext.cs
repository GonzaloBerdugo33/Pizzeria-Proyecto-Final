using Pizzeria.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Pizzeria.API.Data
{
    public class MiApiContext : DbContext
    {
        public MiApiContext(DbContextOptions<MiApiContext> options) : base(options)
        {
        }

        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<CategoriaProducto> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<IngresoProducto> Ingresos { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<HistorialEstado> HistorialEstados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Usuarios ----------
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<CategoriaProducto>()
                .HasIndex(c => c.Nombre)
                .IsUnique();

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.CUIT)
                .IsUnique();

            // ---------- Clientes ----------
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Telefono)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Direccion>()
                .HasOne(d => d.Cliente)
                .WithMany(c => c.Direcciones)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---------- Productos ----------
            modelBuilder.Entity<Producto>(e =>
            {
                e.Property(p => p.Precio).HasPrecision(12, 2);
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Productos_Stock", "\"Stock\" >= 0");
                    t.HasCheckConstraint("CK_Productos_Precio", "\"Precio\" > 0");
                });
            });

            // ---------- Ingresos (una línea por producto) ----------
            modelBuilder.Entity<IngresoProducto>(e =>
            {
                e.Property(i => i.CostoUnitario).HasPrecision(12, 2);
                e.HasOne(i => i.Proveedor).WithMany().HasForeignKey(i => i.ProveedorId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(i => i.Producto).WithMany().HasForeignKey(i => i.ProductoId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(i => i.Usuario).WithMany(u => u.Ingresos).HasForeignKey(i => i.UsuarioId).OnDelete(DeleteBehavior.Restrict);
                e.ToTable(t => t.HasCheckConstraint("CK_Ingresos_Cantidad", "\"Cantidad\" > 0"));
            });

            // ---------- Pedidos ----------
            modelBuilder.Entity<Pedido>(e =>
            {
                e.Property(p => p.Total).HasPrecision(12, 2);
                e.Property(p => p.CodigoEntrega).HasMaxLength(4);
                e.HasOne(p => p.Cliente).WithMany(c => c.Pedidos).HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Cajero).WithMany().HasForeignKey(p => p.CajeroId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Estado).WithMany().HasForeignKey(p => p.EstadoId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.MetodoPago).WithMany().HasForeignKey(p => p.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Direccion).WithMany().HasForeignKey(p => p.DireccionId).OnDelete(DeleteBehavior.Restrict);
                e.ToTable(t => t.HasCheckConstraint("CK_Pedidos_TipoEntrega", "\"TipoEntrega\" IN (1, 2)"));
            });

            modelBuilder.Entity<DetallePedido>(e =>
            {
                e.Property(d => d.PrecioUnitario).HasPrecision(12, 2);
                e.HasOne(d => d.Pedido).WithMany(p => p.Detalles).HasForeignKey(d => d.PedidoId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.ProductoId).OnDelete(DeleteBehavior.Restrict);
                e.ToTable(t => t.HasCheckConstraint("CK_Detalles_Cantidad", "\"Cantidad\" > 0"));
            });

            modelBuilder.Entity<HistorialEstado>(e =>
            {
                e.HasOne(h => h.Pedido).WithMany(p => p.Historial).HasForeignKey(h => h.PedidoId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(h => h.Estado).WithMany().HasForeignKey(h => h.EstadoId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(h => h.Usuario).WithMany().HasForeignKey(h => h.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Datos fijos (se insertan con la migración) ----------
            modelBuilder.Entity<Estado>().HasData(
                new Estado { Id = EstadoPedido.Recibido, Nombre = "Recibido" },
                new Estado { Id = EstadoPedido.EnHorno, Nombre = "En horno" },
                new Estado { Id = EstadoPedido.EnCamino, Nombre = "En camino" },
                new Estado { Id = EstadoPedido.ListoParaRetirar, Nombre = "Listo para retirar" },
                new Estado { Id = EstadoPedido.Entregado, Nombre = "Entregado" },
                new Estado { Id = EstadoPedido.Cancelado, Nombre = "Cancelado" });

            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { Id = MetodoPagoId.Efectivo, Nombre = "Efectivo" },
                new MetodoPago { Id = MetodoPagoId.Tarjeta, Nombre = "Tarjeta" },
                new MetodoPago { Id = MetodoPagoId.MercadoPago, Nombre = "Mercado Pago" });
        }
    }
}
