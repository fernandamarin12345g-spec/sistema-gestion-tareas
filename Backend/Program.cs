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

var app = builder.Build();

// Configuro el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Activo la política de CORS que definí antes de mapear los controladores.
app.UseCors("PermitirFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();

// Mapeo los controladores para que los endpoints queden accesibles.
app.MapControllers();

app.Run();