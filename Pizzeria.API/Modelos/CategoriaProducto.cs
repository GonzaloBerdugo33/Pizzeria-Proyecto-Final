namespace Pizzeria.API.Modelos
{
   public class CategoriaProducto
   {
      public int Id { get; set; }
      public string Nombre { get; set; } = string.Empty;
      public bool Activo { get; set; } = true;
   }
}
