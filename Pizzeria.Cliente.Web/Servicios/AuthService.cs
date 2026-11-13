using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Pizzeria.Cliente.Web.Auth;
using Pizzeria.Cliente.Web.DTOs;

namespace Pizzeria.Cliente.Web.Servicios
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;
        private readonly AuthenticationStateProvider _authState;

        public AuthService(HttpClient http, ILocalStorageService storage, AuthenticationStateProvider authState)
        {
            _http = http;
            _storage = storage;
            _authState = authState;
        }

        public async Task<bool> LoginAsync(LoginDTO login)
        {
            var respuesta = await _http.PostAsJsonAsync("api/Usuario/Login", login);
            if (!respuesta.IsSuccessStatusCode) return false;

            var resultado = await respuesta.Content.ReadFromJsonAsync<LoginResultDTO>();
            if (resultado == null || string.IsNullOrEmpty(resultado.Token)) return false;

            await _storage.SetItemAsStringAsync("authToken", resultado.Token);
            ((CustomAuthStateProvider)_authState).NotifyUserAuthentication(resultado.Token);
            return true;
        }

        public async Task LogoutAsync()
        {
            await _storage.RemoveItemAsync("authToken");
            ((CustomAuthStateProvider)_authState).NotifyUserLogout();
        }
    }
}