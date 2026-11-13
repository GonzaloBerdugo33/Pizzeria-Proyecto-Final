using Pizzeria.Cliente.Movil.DTOs;

namespace Pizzeria.Cliente.Movil.Servicios;

public class CarritoItem
{
    public ProductoDTO Producto { get; set; } = default!;
    public int Cantidad { get; set; }
    public decimal Subtotal => Producto.Precio * Cantidad;
}

public class CarritoService
{
    private readonly List<CarritoItem> _items = new();

    public IReadOnlyList<CarritoItem> Items => _items;
    public decimal Total => _items.Sum(i => i.Subtotal);
    public int CantidadTotal => _items.Sum(i => i.Cantidad);

    // Un producto se puede pedir si está activo, disponible y con stock
    public static bool PuedePedirse(ProductoDTO p) => p.Activo && p.Disponible && p.Stock > 0;

    public void Agregar(ProductoDTO producto)
    {
        if (!PuedePedirse(producto)) return;

        var item = _items.FirstOrDefault(i => i.Producto.Id == producto.Id);
        if (item is null)
            _items.Add(new CarritoItem { Producto = producto, Cantidad = 1 });
        else
            Aumentar(producto.Id);
    }

    public void Aumentar(int productoId)
    {
        var item = _items.FirstOrDefault(i => i.Producto.Id == productoId);
        if (item is not null && item.Cantidad < item.Producto.Stock)
            item.Cantidad++;
    }

    public void Disminuir(int productoId)
    {
        var item = _items.FirstOrDefault(i => i.Producto.Id == productoId);
        if (item is not null && item.Cantidad > 1)
            item.Cantidad--;
    }

    public void Quitar(int productoId) => _items.RemoveAll(i => i.Producto.Id == productoId);

    public void Vaciar() => _items.Clear();
}
