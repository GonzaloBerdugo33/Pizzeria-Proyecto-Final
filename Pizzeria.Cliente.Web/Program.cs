using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Pizzeria.Cliente.Web;
using Pizzeria.Cliente.Web.Auth;
using Pizzeria.Cliente.Web.Servicios;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

// Agrega el token JWT a cada llamada a la API
builder.Services.AddTransient<JwtInterceptorHandler>();

builder.Services.AddHttpClient("ApiHttpClient", client =>
    client.BaseAddress = new Uri("https://pizzeria-api.runasp.net/"))
    .AddHttpMessageHandler<JwtInterceptorHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();