using Pizzeria.API.Data;
using Pizzeria.API.Middleware;
using Pizzeria.API.Modelos;
using Pizzeria.API.Repositorios;
using Pizzeria.API.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;
using System.Security.Claims;
using QuestPDF.Infrastructure;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<MiApiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT (sin la palabra 'Bearer', Swagger la agrega sola)"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Repositorios
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<ICategoriaProductoRepository, CategoriaProductoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IIngresoProductoRepository, IngresoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

// Servicios
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CategoriaProductoService>();
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<IngresoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<TiendaService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
    ),

        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
});

app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure the HTTP request pipeline.
app.MapOpenApi();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pizzeria.API API");
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseCors("PermitirTodo");

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MiApiContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones en la base de datos.");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MiApiContext>();

        // Admin Inicial
        if (!context.Usuarios.Any(u => u.Rol == "Administrador"))
        {
            var adminUser = new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@pizzeria.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(builder.Configuration["Admin:Password"] ?? "Admin123*"),
                Rol = "Administrador"
            };

            context.Usuarios.Add(adminUser);
            context.SaveChanges();
        }

        // Cajero inicial
        if (!context.Usuarios.Any(u => u.Rol == "Cajero"))
        {
            var cajero = new Usuario
            {
                Nombre = "Cajero",
                Email = "cajero@pizzeria.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(builder.Configuration["Cajero:Password"] ?? "Cajero123*"),
                Rol = "Cajero"
            };

            context.Usuarios.Add(cajero);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al sembrar los usuarios iniciales.");
    }
}

try
{
    app.Logger.LogInformation("API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fallo crítico no controlado durante el inicio de la aplicación.");
}
finally
{
    Log.CloseAndFlush();
}