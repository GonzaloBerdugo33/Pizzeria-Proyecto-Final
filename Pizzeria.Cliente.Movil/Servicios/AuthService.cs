using Pizzeria.Cliente.Movil.Auth;
using Pizzeria.Cliente.Movil.DTOs;
using Pizzeria.Cliente.Movil.Servicios;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace Pizzeria.Cliente.Movil.Servicios
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> LoginAsync(LoginDTO loginDTO)
        {

            var response = await _http.PostAsJsonAsync("api/Usuario/Login", loginDTO);

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResultDTO>();

            if (result == null || string.IsNullOrEmpty(result.Token)) return false;


            await SecureStorage.Default.SetAsync("authToken", result.Token);

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);

            return true;
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("authToken");
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }
    }
}