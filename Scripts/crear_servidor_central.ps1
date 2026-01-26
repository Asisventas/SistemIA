# Script para crear archivos del Servidor Central API
$basePath = "c:\asis\SistemIA.ServidorCentral"

# ApiKeyMiddleware.cs
$middlewareContent = @'
namespace SistemIA.ServidorCentral.Middleware;
using SistemIA.ServidorCentral.Data;
using Microsoft.EntityFrameworkCore;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private const string API_KEY_HEADER = "X-API-Key";
    private const string API_SECRET_HEADER = "X-API-Secret";

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ServidorCentralDbContext db)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/swagger") || path.Contains("/health") || path == "/")
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKey) ||
            !context.Request.Headers.TryGetValue(API_SECRET_HEADER, out var apiSecret))
        {
            _logger.LogWarning("API Key o Secret faltante desde {IP}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Success = false, Message = "API Key y Secret son requeridos" });
            return;
        }

        var cliente = await db.ClientesAPI.FirstOrDefaultAsync(c =>
            c.ApiKey == apiKey.ToString() && c.ApiSecret == apiSecret.ToString() && c.Activo);

        if (cliente == null)
        {
            _logger.LogWarning("Credenciales invalidas desde {IP}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { Success = false, Message = "Credenciales invalidas" });
            return;
        }

        cliente.UltimoAcceso = DateTime.UtcNow;
        cliente.IpUltimoAcceso = context.Connection.RemoteIpAddress?.ToString();
        await db.SaveChangesAsync();
        context.Items["ClienteAPI"] = cliente;
        await _next(context);
    }
}
'@
Set-Content -Path "$basePath\Middleware\ApiKeyMiddleware.cs" -Value $middlewareContent -Encoding UTF8
Write-Host "Creado: Middleware\ApiKeyMiddleware.cs"

# ArticulosController.cs
$controllerContent = @'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemIA.ServidorCentral.Data;
using SistemIA.ServidorCentral.Models;

namespace SistemIA.ServidorCentral.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticulosController : ControllerBase
{
    private readonly ServidorCentralDbContext _db;
    private readonly ILogger<ArticulosController> _logger;

    public ArticulosController(ServidorCentralDbContext db, ILogger<ArticulosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ArticuloDto>>>> GetAll([FromQuery] BusquedaArticuloRequest request)
    {
        var query = _db.ArticulosConocimiento.Where(a => a.Activo);

        if (!string.IsNullOrWhiteSpace(request.Categoria))
            query = query.Where(a => a.Categoria == request.Categoria);

        if (!string.IsNullOrWhiteSpace(request.Termino))
        {
            var termino = request.Termino.ToLower();
            query = query.Where(a =>
                a.Titulo.ToLower().Contains(termino) ||
                a.Contenido.ToLower().Contains(termino) ||
                (a.PalabrasClave != null && a.PalabrasClave.ToLower().Contains(termino)));
        }

        var total = await query.CountAsync();
        var articulos = await query
            .OrderByDescending(a => a.Prioridad)
            .ThenByDescending(a => a.VecesUtilizado)
            .Skip((request.Pagina - 1) * request.TamanioPagina)
            .Take(request.TamanioPagina)
            .Select(a => new ArticuloDto
            {
                IdArticulo = a.IdArticulo,
                Categoria = a.Categoria,
                Subcategoria = a.Subcategoria,
                Titulo = a.Titulo,
                Contenido = a.Contenido,
                PalabrasClave = a.PalabrasClave,
                RutaNavegacion = a.RutaNavegacion,
                Icono = a.Icono,
                Prioridad = a.Prioridad
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ArticuloDto>>.Ok(articulos, total: total));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> GetById(int id)
    {
        var articulo = await _db.ArticulosConocimiento.FindAsync(id);
        if (articulo == null || !articulo.Activo)
            return NotFound(ApiResponse<ArticuloDto>.Error("Articulo no encontrado"));

        articulo.VecesUtilizado++;
        await _db.SaveChangesAsync();

        var dto = new ArticuloDto
        {
            IdArticulo = articulo.IdArticulo,
            Categoria = articulo.Categoria,
            Subcategoria = articulo.Subcategoria,
            Titulo = articulo.Titulo,
            Contenido = articulo.Contenido,
            PalabrasClave = articulo.PalabrasClave,
            RutaNavegacion = articulo.RutaNavegacion,
            Icono = articulo.Icono,
            Prioridad = articulo.Prioridad
        };

        return Ok(ApiResponse<ArticuloDto>.Ok(dto));
    }

    [HttpGet("categorias")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCategorias()
    {
        var categorias = await _db.ArticulosConocimiento
            .Where(a => a.Activo)
            .Select(a => a.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(ApiResponse<List<string>>.Ok(categorias));
    }

    [HttpPost("sincronizar")]
    public async Task<ActionResult<ApiResponse<List<ArticuloDto>>>> Sincronizar([FromBody] SincronizacionRequest request)
    {
        var query = _db.ArticulosConocimiento.Where(a => a.Activo);

        if (request.UltimaSincronizacion.HasValue)
            query = query.Where(a => a.FechaActualizacion > request.UltimaSincronizacion.Value);

        var articulos = await query
            .Select(a => new ArticuloDto
            {
                IdArticulo = a.IdArticulo,
                Categoria = a.Categoria,
                Subcategoria = a.Subcategoria,
                Titulo = a.Titulo,
                Contenido = a.Contenido,
                PalabrasClave = a.PalabrasClave,
                RutaNavegacion = a.RutaNavegacion,
                Icono = a.Icono,
                Prioridad = a.Prioridad
            })
            .ToListAsync();

        _logger.LogInformation("Sincronizacion: {Count} articulos enviados", articulos.Count);
        return Ok(ApiResponse<List<ArticuloDto>>.Ok(articulos, total: articulos.Count));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> Create([FromBody] ArticuloDto dto)
    {
        var articulo = new ArticuloConocimiento
        {
            Categoria = dto.Categoria,
            Subcategoria = dto.Subcategoria,
            Titulo = dto.Titulo,
            Contenido = dto.Contenido,
            PalabrasClave = dto.PalabrasClave,
            RutaNavegacion = dto.RutaNavegacion,
            Icono = dto.Icono,
            Prioridad = dto.Prioridad,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            Activo = true
        };

        _db.ArticulosConocimiento.Add(articulo);
        await _db.SaveChangesAsync();

        dto.IdArticulo = articulo.IdArticulo;
        _logger.LogInformation("Articulo creado: {Id} - {Titulo}", articulo.IdArticulo, articulo.Titulo);
        return CreatedAtAction(nameof(GetById), new { id = articulo.IdArticulo }, ApiResponse<ArticuloDto>.Ok(dto));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ArticuloDto>>> Update(int id, [FromBody] ArticuloDto dto)
    {
        var articulo = await _db.ArticulosConocimiento.FindAsync(id);
        if (articulo == null)
            return NotFound(ApiResponse<ArticuloDto>.Error("Articulo no encontrado"));

        articulo.Categoria = dto.Categoria;
        articulo.Subcategoria = dto.Subcategoria;
        articulo.Titulo = dto.Titulo;
        articulo.Contenido = dto.Contenido;
        articulo.PalabrasClave = dto.PalabrasClave;
        articulo.RutaNavegacion = dto.RutaNavegacion;
        articulo.Icono = dto.Icono;
        articulo.Prioridad = dto.Prioridad;
        articulo.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Articulo actualizado: {Id} - {Titulo}", id, articulo.Titulo);
        return Ok(ApiResponse<ArticuloDto>.Ok(dto));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var articulo = await _db.ArticulosConocimiento.FindAsync(id);
        if (articulo == null)
            return NotFound(ApiResponse<bool>.Error("Articulo no encontrado"));

        articulo.Activo = false;
        articulo.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Articulo eliminado (soft delete): {Id}", id);
        return Ok(ApiResponse<bool>.Ok(true, "Articulo eliminado correctamente"));
    }
}
'@
Set-Content -Path "$basePath\Controllers\ArticulosController.cs" -Value $controllerContent -Encoding UTF8
Write-Host "Creado: Controllers\ArticulosController.cs"

# ClientesController.cs
$clientesControllerContent = @'
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemIA.ServidorCentral.Data;
using SistemIA.ServidorCentral.Models;
using System.Security.Cryptography;

namespace SistemIA.ServidorCentral.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ServidorCentralDbContext _db;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(ServidorCentralDbContext db, ILogger<ClientesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ClienteAPI>>>> GetAll()
    {
        var clientes = await _db.ClientesAPI
            .Select(c => new ClienteAPI
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                ApiKey = c.ApiKey,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                UltimoAcceso = c.UltimoAcceso,
                IpUltimoAcceso = c.IpUltimoAcceso,
                Descripcion = c.Descripcion
            })
            .ToListAsync();

        return Ok(ApiResponse<List<ClienteAPI>>.Ok(clientes));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClienteAPI>>> Create([FromBody] ClienteAPI cliente)
    {
        cliente.ApiKey = GenerarApiKey();
        cliente.ApiSecret = GenerarApiKey();
        cliente.FechaCreacion = DateTime.UtcNow;
        cliente.Activo = true;

        _db.ClientesAPI.Add(cliente);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Cliente API creado: {Id} - {Nombre}", cliente.IdCliente, cliente.Nombre);
        return CreatedAtAction(nameof(GetAll), ApiResponse<ClienteAPI>.Ok(cliente, "Cliente creado correctamente"));
    }

    [HttpPut("{id}/regenerar-credenciales")]
    public async Task<ActionResult<ApiResponse<ClienteAPI>>> RegenerarCredenciales(int id)
    {
        var cliente = await _db.ClientesAPI.FindAsync(id);
        if (cliente == null)
            return NotFound(ApiResponse<ClienteAPI>.Error("Cliente no encontrado"));

        cliente.ApiKey = GenerarApiKey();
        cliente.ApiSecret = GenerarApiKey();
        await _db.SaveChangesAsync();

        _logger.LogInformation("Credenciales regeneradas para cliente: {Id}", id);
        return Ok(ApiResponse<ClienteAPI>.Ok(cliente, "Credenciales regeneradas"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var cliente = await _db.ClientesAPI.FindAsync(id);
        if (cliente == null)
            return NotFound(ApiResponse<bool>.Error("Cliente no encontrado"));

        cliente.Activo = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Cliente desactivado: {Id}", id);
        return Ok(ApiResponse<bool>.Ok(true, "Cliente desactivado correctamente"));
    }

    private static string GenerarApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }
}
'@
Set-Content -Path "$basePath\Controllers\ClientesController.cs" -Value $clientesControllerContent -Encoding UTF8
Write-Host "Creado: Controllers\ClientesController.cs"

# instalar-servicio.ps1
$installerContent = @'
# Instalar SistemIA.ServidorCentral como servicio de Windows
# Ejecutar como Administrador

param(
    [string]$ServiceName = "SistemIA.ServidorCentral",
    [string]$DisplayName = "SistemIA Servidor Central API",
    [string]$Description = "API REST para sincronizacion de base de conocimiento de SistemIA",
    [string]$ExePath = ""
)

$ErrorActionPreference = "Stop"

if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Este script debe ejecutarse como Administrador"
    exit 1
}

if ([string]::IsNullOrEmpty($ExePath)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $ExePath = Join-Path (Split-Path -Parent $scriptDir) "SistemIA.ServidorCentral.exe"
}

if (-not (Test-Path $ExePath)) {
    Write-Error "No se encontro el ejecutable: $ExePath"
    Write-Host "Primero compile el proyecto con: dotnet publish -c Release"
    exit 1
}

$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Deteniendo servicio existente..."
    Stop-Service -Name $ServiceName -Force
    Write-Host "Eliminando servicio existente..."
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
}

Write-Host "Creando servicio: $ServiceName"
Write-Host "Ejecutable: $ExePath"

New-Service -Name $ServiceName `
    -BinaryPathName $ExePath `
    -DisplayName $DisplayName `
    -Description $Description `
    -StartupType Automatic

Write-Host "Iniciando servicio..."
Start-Service -Name $ServiceName

$svc = Get-Service -Name $ServiceName
Write-Host ""
Write-Host "Servicio instalado exitosamente!" -ForegroundColor Green
Write-Host "Estado: $($svc.Status)"
Write-Host ""
Write-Host "El servidor esta disponible en:"
Write-Host "  HTTP:  http://localhost:5100"
Write-Host "  HTTPS: https://localhost:5101"
Write-Host ""
Write-Host "Swagger UI: http://localhost:5100/swagger"
'@
Set-Content -Path "$basePath\Scripts\instalar-servicio.ps1" -Value $installerContent -Encoding UTF8
Write-Host "Creado: Scripts\instalar-servicio.ps1"

# desinstalar-servicio.ps1
$uninstallerContent = @'
# Desinstalar SistemIA.ServidorCentral servicio de Windows
# Ejecutar como Administrador

param(
    [string]$ServiceName = "SistemIA.ServidorCentral"
)

$ErrorActionPreference = "Stop"

if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "Este script debe ejecutarse como Administrador"
    exit 1
}

$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $existingService) {
    Write-Host "El servicio $ServiceName no esta instalado"
    exit 0
}

Write-Host "Deteniendo servicio: $ServiceName"
Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

Write-Host "Eliminando servicio..."
sc.exe delete $ServiceName

Write-Host ""
Write-Host "Servicio desinstalado exitosamente!" -ForegroundColor Green
'@
Set-Content -Path "$basePath\Scripts\desinstalar-servicio.ps1" -Value $uninstallerContent -Encoding UTF8
Write-Host "Creado: Scripts\desinstalar-servicio.ps1"

# README.md
$readmeContent = @'
# SistemIA.ServidorCentral

API REST para sincronizacion centralizada de la base de conocimiento del Asistente IA de SistemIA.

## Caracteristicas

- **API REST** con autenticacion por API Key + Secret
- **Servicio de Windows** para ejecucion en segundo plano
- **Swagger UI** para documentacion y pruebas
- **Entity Framework Core** con SQL Server
- **Serilog** para logging a archivo y consola

## Requisitos

- .NET 8 Runtime o SDK
- SQL Server 2019+
- Windows Server 2019+ (para servicio)

## Instalacion

### 1. Compilar el proyecto

```powershell
cd c:\asis\SistemIA.ServidorCentral
dotnet publish -c Release -o publish --self-contained true -r win-x64
```

### 2. Configurar base de datos

Editar `publish\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=MI_SERVIDOR;Database=SistemIA_Central;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Instalar como servicio

```powershell
cd publish\Scripts
.\instalar-servicio.ps1
```

## Endpoints

| Metodo | Endpoint | Descripcion |
|--------|----------|-------------|
| GET | /api/articulos | Listar articulos (paginado) |
| GET | /api/articulos/{id} | Obtener articulo por ID |
| GET | /api/articulos/categorias | Listar categorias |
| POST | /api/articulos/sincronizar | Sincronizar articulos nuevos |
| POST | /api/articulos | Crear articulo |
| PUT | /api/articulos/{id} | Actualizar articulo |
| DELETE | /api/articulos/{id} | Eliminar articulo |
| GET | /api/clientes | Listar clientes API |
| POST | /api/clientes | Crear cliente API |
| PUT | /api/clientes/{id}/regenerar-credenciales | Regenerar API Key/Secret |
| DELETE | /api/clientes/{id} | Desactivar cliente |

## Autenticacion

Todas las peticiones (excepto /swagger y /health) requieren headers:

```
X-API-Key: tu_api_key
X-API-Secret: tu_api_secret
```

## Configuracion en SistemIA (cliente)

En el modulo Admin Asistente IA:

1. Seleccionar "API REST" como modo de conexion
2. Ingresar URL: `http://servidor:5100` o `https://servidor:5101`
3. Ingresar API Key y API Secret
4. Probar conexion

## Logs

Los logs se guardan en:
- `Logs/servidor-central-YYYYMMDD.log`

## Desinstalar

```powershell
cd publish\Scripts
.\desinstalar-servicio.ps1
```
'@
Set-Content -Path "$basePath\README.md" -Value $readmeContent -Encoding UTF8
Write-Host "Creado: README.md"

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "Archivos creados exitosamente!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Archivos creados:"
Get-ChildItem $basePath -Recurse -File | ForEach-Object { Write-Host "  - $($_.FullName.Replace($basePath, ''))" }
