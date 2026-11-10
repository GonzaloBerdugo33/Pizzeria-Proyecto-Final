using System.Globalization;
using Pizzeria.API.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pizzeria.API.Servicios
{
    public interface IReporteService
    {
        byte[] GenerarListadoStock(IReadOnlyCollection<Producto> productos);
        byte[] GenerarListadoEntradas(IReadOnlyCollection<IngresoProducto> ingresos);
        byte[] GenerarListadoVentas(IReadOnlyCollection<Pedido> pedidos);
    }

    public class ReporteService : IReporteService
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        public byte[] GenerarListadoStock(IReadOnlyCollection<Producto> productos)
        {
            var ahora = DateTime.UtcNow.AddHours(-3);
            var totalUnidades = 0;
            foreach (var p in productos)
            {
                totalUnidades += p.Stock;
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Header().Text("Reporte de stock").FontSize(20).SemiBold();
                    page.Content().PaddingVertical(16).Column(column =>
                    {
                        column.Item().Text($"Generado el {ahora:dd/MM/yyyy HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                        column.Item().PaddingTop(12).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloEncabezado).Text("ID");
                                header.Cell().Element(EstiloEncabezado).Text("Producto");
                                header.Cell().Element(EstiloEncabezado).Text("Categoría");
                                header.Cell().Element(EstiloEncabezado).Text("Proveedor");
                                header.Cell().Element(EstiloEncabezado).AlignRight().Text("Precio");
                                header.Cell().Element(EstiloEncabezado).AlignRight().Text("Stock");
                            });

                            foreach (var producto in productos)
                            {
                                table.Cell().Element(EstiloCelda).Text(producto.Id.ToString());
                                table.Cell().Element(EstiloCelda).Text(producto.Nombre);
                                table.Cell().Element(EstiloCelda).Text(producto.Categoria?.Nombre ?? "-");
                                table.Cell().Element(EstiloCelda).Text(producto.Proveedor?.RazonSocial ?? "-");
                                table.Cell().Element(EstiloCelda).AlignRight().Text(producto.Precio.ToString("C2", Cultura));
                                table.Cell().Element(EstiloCelda).AlignRight().Text(producto.Stock.ToString());
                            }
                        });
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span($"Total de productos: {productos.Count} | Unidades en stock: ");
                        text.Span(totalUnidades.ToString()).SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerarListadoEntradas(IReadOnlyCollection<IngresoProducto> ingresos)
        {
            var ahora = DateTime.UtcNow.AddHours(-3);
            var totalUnidades = 0;
            foreach (var i in ingresos)
            {
                totalUnidades += UnidadesDe(i);
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Header().Text("Reporte de entradas").FontSize(20).SemiBold();
                    page.Content().PaddingVertical(16).Column(column =>
                    {
                        column.Item().Text($"Generado el {ahora:dd/MM/yyyy HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                        column.Item().PaddingTop(12).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloEncabezado).Text("Nº");
                                header.Cell().Element(EstiloEncabezado).Text("Fecha");
                                header.Cell().Element(EstiloEncabezado).Text("Proveedor");
                                header.Cell().Element(EstiloEncabezado).Text("Producto");
                                header.Cell().Element(EstiloEncabezado).Text("Registrado por");
                                header.Cell().Element(EstiloEncabezado).AlignRight().Text("Unidades");
                            });

                            foreach (var ingreso in ingresos)
                            {
                                table.Cell().Element(EstiloCelda).Text(ingreso.Id.ToString());
                                table.Cell().Element(EstiloCelda).Text(ingreso.Fecha.AddHours(-3).ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(EstiloCelda).Text(ingreso.Proveedor?.RazonSocial ?? "-");
                                table.Cell().Element(EstiloCelda).Text(ingreso.Producto?.Nombre ?? "-");
                                table.Cell().Element(EstiloCelda).Text(ingreso.Usuario?.Email ?? "-");
                                table.Cell().Element(EstiloCelda).AlignRight().Text(UnidadesDe(ingreso).ToString());
                            }
                        });
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span($"Total de ingresos: {ingresos.Count} | Unidades ingresadas: ");
                        text.Span(totalUnidades.ToString()).SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerarListadoVentas(IReadOnlyCollection<Pedido> pedidos)
        {
            var ahora = DateTime.UtcNow.AddHours(-3);
            decimal totalGeneral = 0;
            foreach (var p in pedidos)
            {
                totalGeneral += p.Total;
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(36);
                    page.Header().Text("Reporte de ventas (pedidos entregados)").FontSize(20).SemiBold();
                    page.Content().PaddingVertical(16).Column(column =>
                    {
                        column.Item().Text($"Generado el {ahora:dd/MM/yyyy HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                        column.Item().PaddingTop(12).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(EstiloEncabezado).Text("Nº");
                                header.Cell().Element(EstiloEncabezado).Text("Fecha");
                                header.Cell().Element(EstiloEncabezado).Text("Cliente");
                                header.Cell().Element(EstiloEncabezado).Text("Entrega");
                                header.Cell().Element(EstiloEncabezado).Text("Forma de pago");
                                header.Cell().Element(EstiloEncabezado).AlignRight().Text("Unidades");
                                header.Cell().Element(EstiloEncabezado).AlignRight().Text("Total");
                            });

                            foreach (var pedido in pedidos)
                            {
                                table.Cell().Element(EstiloCelda).Text(pedido.Id.ToString());
                                table.Cell().Element(EstiloCelda).Text(pedido.Fecha.AddHours(-3).ToString("dd/MM/yyyy HH:mm"));
                                table.Cell().Element(EstiloCelda).Text(pedido.Cliente?.Nombre ?? "-");
                                table.Cell().Element(EstiloCelda).Text(pedido.TipoEntrega == TipoEntrega.Envio ? "Envío" : "Retiro");
                                table.Cell().Element(EstiloCelda).Text(pedido.MetodoPago?.Nombre ?? "-");
                                table.Cell().Element(EstiloCelda).AlignRight().Text(UnidadesDe(pedido).ToString());
                                table.Cell().Element(EstiloCelda).AlignRight().Text(pedido.Total.ToString("C2", Cultura));
                            }
                        });
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span($"Total de pedidos: {pedidos.Count} | Total vendido: ");
                        text.Span(totalGeneral.ToString("C2", Cultura)).SemiBold();
                    });
                });
            }).GeneratePdf();
        }

        private static int UnidadesDe(IngresoProducto ingreso)
        {
            return ingreso.Cantidad;
        }

        private static int UnidadesDe(Pedido pedido)
        {
            var unidades = 0;
            foreach (var d in pedido.Detalles)
            {
                unidades += d.Cantidad;
            }
            return unidades;
        }

        private static IContainer EstiloEncabezado(IContainer container)
        {
            return container.Background(Colors.Blue.Darken2).Padding(6)
                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());
        }

        private static IContainer EstiloCelda(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
        }
    }
}