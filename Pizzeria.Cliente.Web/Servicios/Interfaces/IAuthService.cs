using Pizzeria.Cliente.Web.DTOs;

namespace Pizzeria.Cliente.Web.Servicios
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginDTO login);
        Task LogoutAsync();
    }
}