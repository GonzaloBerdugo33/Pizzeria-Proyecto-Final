using Pizzeria.Cliente.Movil.DTOs;

namespace Pizzeria.Cliente.Movil.Servicios
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginDTO loginDTO);
        Task LogoutAsync();
    }
}
