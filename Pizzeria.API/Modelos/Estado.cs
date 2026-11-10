namespace Pizzeria.API.Modelos
{
    public class Estado
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public static class EstadoPedido
    {
        public const int Recibido = 1;
        public const int EnHorno = 2;
        public const int EnCamino = 3;
        public const int ListoParaRetirar = 4;
        public const int Entregado = 5;
        public const int Cancelado = 6;
    }
}
