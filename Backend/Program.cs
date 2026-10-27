using System.Text;
using Backend.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configuro los servicios necesarios para el contenedor de la aplicación.
builder.Services.AddControllers();
// Aprendo sobre la configuración de OpenAPI en https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuro CORS para permitir que mi frontend se comunique con esta API sin bloqueos de seguridad.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== AUTENTICACIÓN CON JWT =====
// La clave secreta NO va en appsettings.json: en desarrollo está en appsettings.Development.json
// y en producción se pasa como variable de entorno (Jwt__Clave).
string claveJwt = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException("Falta la configuración Jwt:Clave (ver appsettings.Development.json).");

builder.Services.AddSingleton<ServicioTokens>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Usa los nombres de los claims tal como están en el token (id_usuario, nombre, rol...)
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emisor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateLifetime = true,                 // Rechaza tokens vencidos
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,         // Rechaza tokens modificados o firmados con otra clave
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            NameClaimType = "nombre",
            RoleClaimType = "rol"
        };
    });

// ===== AUTORIZACIÓN (PERMISOS) =====
builder.Services.AddAuthorization(opciones =>
{
    // Por defecto TODAS las rutas exigen haber iniciado sesión (salvo las marcadas con [AllowAnonymous])
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

    opciones.AddPolicy(Politicas.Administrador, politica =>
        politica.RequireAssertion(contexto => contexto.User.EsAdmin()));

    opciones.AddPolicy(Politicas.GestionTareas, politica =>
        politica.RequireAssertion(contexto => contexto.User.GestionaTareas()));
});

var app = builder.Build();

// Configuro el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

// Activo la política de CORS que definí antes de mapear los controladores.
app.UseCors("PermitirFrontend");

// En desarrollo NO se redirige a HTTPS: la app móvil en el emulador usa http://10.0.2.2:5293
// y el emulador no confía en el certificado de desarrollo. En producción sí se redirige.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();   // 1. ¿Quién es? (lee y valida el token)
app.UseAuthorization();    // 2. ¿Tiene permiso? (aplica las políticas)

// Mapeo los controladores para que los endpoints queden accesibles.
app.MapControllers();

app.Run();
