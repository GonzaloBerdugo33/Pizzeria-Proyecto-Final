namespace Pizzeria.Cliente.Web.Servicios
{

    public static class Formato
    {
        public static string Dinero(decimal valor) => $"$ {valor:N2}";

        // La API guarda las fechas en UTC; Argentina es UTC-3
        public static string Fecha(DateTime fechaUtc) => fechaUtc.AddHours(-3).ToString("dd/MM/yyyy HH:mm");
    }
}