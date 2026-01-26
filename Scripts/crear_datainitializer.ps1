# Script para crear DataInitializer
$basePath = "c:\asis\SistemIA.ServidorCentral"

$dataInitContent = @'
using Microsoft.EntityFrameworkCore;
using SistemIA.ServidorCentral.Models;
using System.Security.Cryptography;

namespace SistemIA.ServidorCentral.Data;

public static class DataInitializer
{
    public static async Task InitializeAsync(ServidorCentralDbContext db, ILogger logger)
    {
        try
        {
            // Aplicar migraciones pendientes
            await db.Database.MigrateAsync();
            logger.LogInformation("Base de datos inicializada correctamente");

            // Crear cliente API por defecto si no existe ninguno
            if (!await db.ClientesAPI.AnyAsync())
            {
                var clienteDefault = new ClienteAPI
                {
                    Nombre = "SistemIA Default",
                    ApiKey = GenerarApiKey(),
                    ApiSecret = GenerarApiKey(),
                    Descripcion = "Cliente por defecto creado automaticamente",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                db.ClientesAPI.Add(clienteDefault);
                await db.SaveChangesAsync();

                logger.LogInformation("==============================================");
                logger.LogInformation("CLIENTE API POR DEFECTO CREADO:");
                logger.LogInformation("  Nombre: {Nombre}", clienteDefault.Nombre);
                logger.LogInformation("  API Key: {ApiKey}", clienteDefault.ApiKey);
                logger.LogInformation("  API Secret: {ApiSecret}", clienteDefault.ApiSecret);
                logger.LogInformation("==============================================");
                logger.LogWarning("IMPORTANTE: Guarde estas credenciales en un lugar seguro!");
            }

            // Crear articulos de ejemplo si no existen
            if (!await db.ArticulosConocimiento.AnyAsync())
            {
                var articulosIniciales = new List<ArticuloConocimiento>
                {
                    new()
                    {
                        Categoria = "Sistema",
                        Subcategoria = "General",
                        Titulo = "Bienvenido a SistemIA",
                        Contenido = "SistemIA es un sistema de gestion empresarial con asistente IA integrado.\n\n" +
                                   "**Modulos principales:**\n" +
                                   "- Ventas y Facturacion Electronica (SIFEN)\n" +
                                   "- Compras y Proveedores\n" +
                                   "- Inventario y Stock\n" +
                                   "- Caja y Cierres\n" +
                                   "- Clientes y Cuentas por Cobrar",
                        PalabrasClave = "sistemia, inicio, bienvenida, modulos",
                        Icono = "bi-info-circle",
                        Prioridad = 10,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    },
                    new()
                    {
                        Categoria = "Ventas",
                        Subcategoria = "SIFEN",
                        Titulo = "Como enviar una factura a SIFEN",
                        Contenido = "Para enviar una factura electronica a SIFEN:\n\n" +
                                   "1. Crear la venta normalmente\n" +
                                   "2. Confirmar la venta (cambiar estado a Confirmada)\n" +
                                   "3. Ir al Explorador de Ventas\n" +
                                   "4. Buscar la venta y hacer clic en **Enviar SIFEN**\n" +
                                   "5. Esperar la respuesta del SET\n\n" +
                                   "💡 **Tip:** Las ventas ACEPTADAS tienen validez legal y no pueden eliminarse.",
                        PalabrasClave = "sifen, factura electronica, enviar, set",
                        RutaNavegacion = "/ventas/explorar",
                        Icono = "bi-send",
                        Prioridad = 9,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    },
                    new()
                    {
                        Categoria = "Caja",
                        Subcategoria = "Cierre",
                        Titulo = "Como realizar un cierre de caja",
                        Contenido = "Para realizar el cierre de caja:\n\n" +
                                   "1. Ir a **Caja > Cierre de Caja**\n" +
                                   "2. Seleccionar la caja y turno a cerrar\n" +
                                   "3. Contar el efectivo fisico\n" +
                                   "4. Ingresar el monto contado\n" +
                                   "5. El sistema calculara la diferencia\n" +
                                   "6. Confirmar el cierre\n\n" +
                                   "⚠️ **Importante:** No se pueden modificar operaciones de turnos cerrados.",
                        PalabrasClave = "cierre, caja, arqueo, turno",
                        RutaNavegacion = "/caja/cierre",
                        Icono = "bi-cash-stack",
                        Prioridad = 8,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    }
                };

                db.ArticulosConocimiento.AddRange(articulosIniciales);
                await db.SaveChangesAsync();

                logger.LogInformation("Se crearon {Count} articulos de ejemplo", articulosIniciales.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inicializando la base de datos");
            throw;
        }
    }

    private static string GenerarApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }
}
'@
Set-Content -Path "$basePath\Data\DataInitializer.cs" -Value $dataInitContent -Encoding UTF8
Write-Host "Creado: Data\DataInitializer.cs"

# Tambien actualizar Program.cs para usar DataInitializer
$programContent = @'
using Microsoft.EntityFrameworkCore;
using Serilog;
using SistemIA.ServidorCentral.Data;
using SistemIA.ServidorCentral.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configurar como servicio de Windows
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "SistemIA.ServidorCentral";
});

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "Logs", "servidor-central-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar Kestrel con puertos del appsettings
builder.WebHost.ConfigureKestrel((context, options) =>
{
    var kestrelConfig = context.Configuration.GetSection("Kestrel:Endpoints");
    if (kestrelConfig.Exists())
    {
        options.Configure(context.Configuration.GetSection("Kestrel"));
    }
    else
    {
        options.ListenAnyIP(5100); // HTTP por defecto
    }
});

// Agregar servicios
builder.Services.AddDbContext<ServidorCentralDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SistemIA Servidor Central API", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key en el header",
        Name = "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityDefinition("ApiSecret", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Secret en el header",
        Name = "X-API-Secret",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServidorCentralDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DataInitializer.InitializeAsync(db, logger);
}

// Configurar pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.RoutePrefix = "swagger");
}

app.UseCors();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));
app.MapGet("/", () => Results.Redirect("/swagger"));

Log.Information("SistemIA Servidor Central API iniciado");
app.Run();
'@
Set-Content -Path "$basePath\Program.cs" -Value $programContent -Encoding UTF8
Write-Host "Actualizado: Program.cs"

Write-Host ""
Write-Host "Archivos finales creados!" -ForegroundColor Green
