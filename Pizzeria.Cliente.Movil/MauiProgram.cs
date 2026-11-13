using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Pizzeria.Cliente.Movil.Auth;
using Pizzeria.Cliente.Movil.Servicios;
using Pizzeria.Cliente.Movil;

namespace Pizzeria.Cliente.Movil;

public static class MauiProgram
{
    private const string ApiUrl = "https://pizzeria-api.runasp.net/";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddTransient<JwtInterceptorHandler>();

        builder.Services.AddHttpClient("ApiHttpClient", client => client.BaseAddress = new Uri(ApiUrl))
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
#if DEBUG
                // Solo en desarrollo: acepta el certificado de prueba de la API local
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
                return handler;
            })
            .AddHttpMessageHandler<JwtInterceptorHandler>();

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));

        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddSingleton<CarritoService>();

        return builder.Build();
    }
}