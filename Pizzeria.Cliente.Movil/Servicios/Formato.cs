namespace Pizzeria.Cliente.Movil.Servicios;

// Funciones chicas para mostrar datos igual en todas las pantallas
public static class Formato
{
    // Precio sin depender de la moneda del celular (ToString("C") mostraba euros)
    public static string Dinero(decimal valor) => $"$ {valor:N2}";

    // La API guarda las fechas en UTC; Argentina es UTC-3
    public static string Fecha(DateTime fechaUtc) => fechaUtc.AddHours(-3).ToString("dd/MM/yyyy HH:mm");

    // La API devuelve rutas relativas (/uploads/xxx.png): se les antepone la dirección de la API
    public static string Imagen(Uri? baseApi, string? ruta)
    {
        if (string.IsNullOrEmpty(ruta)) return string.Empty;
        if (ruta.StartsWith("http")) return ruta;
        return $"{baseApi}{ruta.TrimStart('/')}";
    }
}
