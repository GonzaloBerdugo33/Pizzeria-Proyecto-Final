namespace Pizzeria.API.Modelos
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public static class MetodoPagoId
    {
        public const int Efectivo = 1;
        public const int Tarjeta = 2;
        public const int MercadoPago = 3;
    }

    public static class TipoEntrega
    {
        public const int Retiro = 1;
        public const int Envio = 2;
    }
}
