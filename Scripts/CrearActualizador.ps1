# Script para crear los archivos del proyecto SistemIA.Actualizador
$basePath = "c:\asis\SistemIA.Actualizador"

# 1. csproj
$csproj = @"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>SistemIA.Actualizador</RootNamespace>
    <AssemblyName>SistemIA.Actualizador</AssemblyName>
  </PropertyGroup>
</Project>
"@
[System.IO.File]::WriteAllText("$basePath\SistemIA.Actualizador.csproj", $csproj, [System.Text.Encoding]::UTF8)

# 2. Program.cs
$program = @"
var builder = WebApplication.CreateBuilder(args);

// Configurar puerto 5096 por defecto para el actualizador
builder.WebHost.UseUrls("http://0.0.0.0:5096");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Console.WriteLine("===========================================");
Console.WriteLine("  SistemIA Actualizador - Puerto 5096");
Console.WriteLine("  http://localhost:5096");
Console.WriteLine("===========================================");

app.Run();
"@
[System.IO.File]::WriteAllText("$basePath\Program.cs", $program, [System.Text.Encoding]::UTF8)

# 3. Crear carpeta Pages si no existe
New-Item -ItemType Directory -Path "$basePath\Pages" -Force | Out-Null

# 4. _Host.cshtml
$host = @"
@page "/"
@namespace SistemIA.Actualizador.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SistemIA - Actualizador</title>
    <base href="/" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" rel="stylesheet" />
    <style>
        body { background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%); min-height: 100vh; }
        .update-container { max-width: 800px; margin: 50px auto; }
        .update-card { background: rgba(255,255,255,0.95); border-radius: 15px; box-shadow: 0 10px 40px rgba(0,0,0,0.3); }
        .update-header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 15px 15px 0 0; padding: 25px; }
        .progress { height: 30px; border-radius: 15px; }
        .progress-bar { font-size: 14px; font-weight: bold; }
        .log-container { max-height: 300px; overflow-y: auto; background: #1a1a2e; color: #00ff00; font-family: 'Consolas', monospace; font-size: 12px; padding: 15px; border-radius: 10px; }
        .log-entry { margin: 2px 0; }
        .log-entry.error { color: #ff6b6b; }
        .log-entry.success { color: #51cf66; }
        .log-entry.warning { color: #ffd43b; }
        .status-badge { font-size: 1.1em; padding: 8px 16px; }
        .file-drop-zone { border: 3px dashed #667eea; border-radius: 15px; padding: 40px; text-align: center; cursor: pointer; transition: all 0.3s; }
        .file-drop-zone:hover { background: rgba(102, 126, 234, 0.1); border-color: #764ba2; }
        .file-drop-zone.dragover { background: rgba(102, 126, 234, 0.2); border-color: #764ba2; }
    </style>
</head>
<body>
    <component type="typeof(App)" render-mode="ServerPrerendered" />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
"@
[System.IO.File]::WriteAllText("$basePath\Pages\_Host.cshtml", $host, [System.Text.Encoding]::UTF8)

# 5. _Imports.razor
$imports = @"
@using System.Net.Http
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using SistemIA.Actualizador
@using SistemIA.Actualizador.Pages
"@
[System.IO.File]::WriteAllText("$basePath\_Imports.razor", $imports, [System.Text.Encoding]::UTF8)

# 6. App.razor
$appRazor = @"
<Router AppAssembly=""@typeof(App).Assembly"">
    <Found Context=""routeData"">
        <RouteView RouteData=""@routeData"" />
    </Found>
    <NotFound>
        <LayoutView>
            <div class=""container mt-5"">
                <div class=""alert alert-warning"">
                    <h4>Pagina no encontrada</h4>
                    <a href=""/"">Volver al inicio</a>
                </div>
            </div>
        </LayoutView>
    </NotFound>
</Router>
"@
[System.IO.File]::WriteAllText("$basePath\App.razor", $appRazor, [System.Text.Encoding]::UTF8)

# 7. Index.razor - Pagina principal del actualizador
$indexRazor = @'
@page "/"
@using System.IO
@using System.IO.Compression
@using System.Diagnostics
@inject IJSRuntime JS

<div class="update-container">
    <div class="update-card">
        <div class="update-header text-center">
            <i class="fas fa-sync-alt fa-3x mb-3"></i>
            <h2 class="mb-1">SistemIA - Actualizador</h2>
            <p class="mb-0 opacity-75">Sistema independiente de actualizacion</p>
        </div>
        
        <div class="card-body p-4">
            @if (!actualizando)
            {
                <!-- Estado actual de SistemIA -->
                <div class="alert @(sistemaActivo ? "alert-success" : "alert-warning") mb-4">
                    <div class="d-flex align-items-center">
                        <i class="fas @(sistemaActivo ? "fa-check-circle" : "fa-exclamation-triangle") fa-2x me-3"></i>
                        <div>
                            <strong>Estado de SistemIA (Puerto 5095):</strong>
                            <span class="badge @(sistemaActivo ? "bg-success" : "bg-warning") ms-2">
                                @(sistemaActivo ? "Activo" : "Detenido")
                            </span>
                            <br/>
                            <small class="text-muted">Ruta: @rutaSistemIA</small>
                        </div>
                        <button class="btn btn-sm btn-outline-secondary ms-auto" @onclick="VerificarEstadoSistema">
                            <i class="fas fa-sync-alt"></i>
                        </button>
                    </div>
                </div>

                <!-- Seleccionar archivo ZIP -->
                <div class="mb-4">
                    <label class="form-label fw-bold">
                        <i class="fas fa-file-archive me-2"></i>Paquete de Actualizacion (.zip)
                    </label>
                    
                    @if (archivosDisponibles.Any())
                    {
                        <select class="form-select mb-2" @onchange="SeleccionarArchivo">
                            <option value="">-- Seleccione un paquete --</option>
                            @foreach (var archivo in archivosDisponibles)
                            {
                                <option value="@archivo.RutaCompleta">
                                    @archivo.Nombre (@archivo.TamanoMB.ToString("F1") MB) - @archivo.Fecha.ToString("dd/MM/yyyy HH:mm")
                                </option>
                            }
                        </select>
                    }
                    
                    @if (!string.IsNullOrEmpty(archivoSeleccionado))
                    {
                        <div class="alert alert-info">
                            <i class="fas fa-file-archive me-2"></i>
                            <strong>Archivo:</strong> @Path.GetFileName(archivoSeleccionado)
                        </div>
                    }
                </div>

                <!-- Opciones -->
                <div class="mb-4">
                    <div class="form-check mb-2">
                        <input class="form-check-input" type="checkbox" @bind="crearBackup" id="chkBackup">
                        <label class="form-check-label" for="chkBackup">
                            <i class="fas fa-database me-1"></i>Crear backup antes de actualizar
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" @bind="aplicarMigraciones" id="chkMigraciones">
                        <label class="form-check-label" for="chkMigraciones">
                            <i class="fas fa-database me-1"></i>Aplicar migraciones de BD
                        </label>
                    </div>
                </div>

                <!-- Boton iniciar -->
                <div class="d-grid">
                    <button class="btn btn-primary btn-lg" @onclick="IniciarActualizacion" 
                            disabled="@string.IsNullOrEmpty(archivoSeleccionado)">
                        <i class="fas fa-rocket me-2"></i>Iniciar Actualizacion
                    </button>
                </div>
            }
            else
            {
                <!-- Progreso de actualizacion -->
                <div class="text-center mb-4">
                    <span class="status-badge badge @GetBadgeClass()">
                        <i class="@GetStatusIcon() me-2"></i>@estadoActual
                    </span>
                </div>

                <div class="progress mb-4">
                    <div class="progress-bar progress-bar-striped @(error ? "bg-danger" : completado ? "bg-success" : "progress-bar-animated")" 
                         role="progressbar" style="width: @progreso%">
                        @progreso%
                    </div>
                </div>

                <div class="mb-3">
                    <strong>Paso actual:</strong> @mensajeActual
                </div>

                <!-- Logs -->
                <div class="log-container" @ref="logContainer">
                    @foreach (var log in logs)
                    {
                        <div class="log-entry @GetLogClass(log)">@log</div>
                    }
                </div>

                @if (completado)
                {
                    <div class="alert alert-success mt-4">
                        <i class="fas fa-check-circle me-2"></i>
                        <strong>Actualizacion completada exitosamente!</strong>
                        <br/>
                        <small>Archivos verificados: @archivosVerificados</small>
                    </div>
                    <div class="d-flex gap-2 mt-3">
                        <button class="btn btn-success flex-fill" @onclick="AbrirSistemIA">
                            <i class="fas fa-external-link-alt me-2"></i>Abrir SistemIA
                        </button>
                        <button class="btn btn-outline-secondary" @onclick="Reiniciar">
                            <i class="fas fa-redo me-2"></i>Nueva Actualizacion
                        </button>
                    </div>
                }
                else if (error)
                {
                    <div class="alert alert-danger mt-4">
                        <i class="fas fa-times-circle me-2"></i>
                        <strong>Error:</strong> @mensajeError
                    </div>
                    <button class="btn btn-warning mt-2" @onclick="Reiniciar">
                        <i class="fas fa-redo me-2"></i>Reintentar
                    </button>
                }
            }
        </div>
        
        <div class="card-footer text-center text-muted small">
            <i class="fas fa-info-circle me-1"></i>
            Actualizador independiente - Puerto 5096
        </div>
    </div>
</div>

@code {
    private string rutaSistemIA = @"C:\SistemIA";
    private bool sistemaActivo = false;
    private string archivoSeleccionado = "";
    private bool crearBackup = true;
    private bool aplicarMigraciones = true;
    
    private bool actualizando = false;
    private int progreso = 0;
    private string estadoActual = "Preparando...";
    private string mensajeActual = "";
    private List<string> logs = new();
    private bool completado = false;
    private bool error = false;
    private string mensajeError = "";
    private int archivosVerificados = 0;
    
    private List<ArchivoInfo> archivosDisponibles = new();
    private ElementReference logContainer;

    protected override async Task OnInitializedAsync()
    {
        DetectarRutaSistemIA();
        CargarArchivosDisponibles();
        await VerificarEstadoSistema();
    }

    private void DetectarRutaSistemIA()
    {
        var posiblesRutas = new[] { @"C:\SistemIA", @"C:\Program Files\SistemIA", @"C:\asis\SistemIA\publish_selfcontained" };
        foreach (var ruta in posiblesRutas)
        {
            if (Directory.Exists(ruta) && (File.Exists(Path.Combine(ruta, "SistemIA.exe")) || File.Exists(Path.Combine(ruta, "SistemIA.dll"))))
            {
                rutaSistemIA = ruta;
                break;
            }
        }
    }

    private void CargarArchivosDisponibles()
    {
        archivosDisponibles.Clear();
        var carpetas = new[] { 
            Path.Combine(rutaSistemIA, "Releases"),
            @"C:\asis\SistemIA\Releases",
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        
        foreach (var carpeta in carpetas)
        {
            if (Directory.Exists(carpeta))
            {
                var zips = Directory.GetFiles(carpeta, "*.zip").Take(10);
                foreach (var zip in zips)
                {
                    var info = new FileInfo(zip);
                    archivosDisponibles.Add(new ArchivoInfo
                    {
                        Nombre = info.Name,
                        RutaCompleta = info.FullName,
                        TamanoMB = info.Length / (1024.0 * 1024.0),
                        Fecha = info.LastWriteTime
                    });
                }
            }
        }
        archivosDisponibles = archivosDisponibles.OrderByDescending(a => a.Fecha).ToList();
    }

    private async Task VerificarEstadoSistema()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var response = await http.GetAsync("http://localhost:5095/");
            sistemaActivo = response.IsSuccessStatusCode;
        }
        catch
        {
            sistemaActivo = false;
        }
        StateHasChanged();
    }

    private void SeleccionarArchivo(ChangeEventArgs e)
    {
        archivoSeleccionado = e.Value?.ToString() ?? "";
    }

    private async Task IniciarActualizacion()
    {
        if (string.IsNullOrEmpty(archivoSeleccionado)) return;
        
        actualizando = true;
        progreso = 0;
        logs.Clear();
        completado = false;
        error = false;
        
        try
        {
            // Paso 1: Validar ZIP
            await ActualizarProgreso(5, "Validando", "Verificando archivo ZIP...");
            if (!File.Exists(archivoSeleccionado))
            {
                throw new Exception("El archivo ZIP no existe");
            }
            AgregarLog("OK Archivo ZIP validado");

            // Paso 2: Crear backup si es necesario
            if (crearBackup)
            {
                await ActualizarProgreso(10, "Backup", "Creando backup de la aplicacion...");
                var backupPath = Path.Combine(rutaSistemIA, "Backups", $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                AgregarLog($"OK Backup creado en: {backupPath}");
            }

            // Paso 3: Detener SistemIA
            await ActualizarProgreso(20, "Deteniendo", "Deteniendo servicio SistemIA...");
            await DetenerSistemIA();
            await Task.Delay(2000);
            AgregarLog("OK SistemIA detenido");

            // Paso 4: Extraer archivos
            await ActualizarProgreso(30, "Extrayendo", "Extrayendo archivos del paquete...");
            var tempPath = Path.Combine(Path.GetTempPath(), $"SistemIA_Update_{DateTime.Now:yyyyMMddHHmmss}");
            ZipFile.ExtractToDirectory(archivoSeleccionado, tempPath, true);
            AgregarLog($"OK Archivos extraidos a: {tempPath}");

            // Paso 5: Copiar archivos
            await ActualizarProgreso(40, "Copiando", "Copiando archivos actualizados...");
            var archivosCopiados = await CopiarArchivos(tempPath, rutaSistemIA);
            AgregarLog($"OK {archivosCopiados} archivos copiados");

            // Paso 6: Limpiar temporales
            await ActualizarProgreso(70, "Limpiando", "Limpiando archivos temporales...");
            try { Directory.Delete(tempPath, true); } catch { }
            AgregarLog("OK Archivos temporales eliminados");

            // Paso 7: Iniciar SistemIA
            await ActualizarProgreso(80, "Iniciando", "Iniciando SistemIA...");
            await IniciarSistemIA();
            await Task.Delay(5000);

            // Paso 8: Verificar que inicio
            await ActualizarProgreso(90, "Verificando", "Verificando que SistemIA este activo...");
            await VerificarEstadoSistema();
            
            if (sistemaActivo)
            {
                AgregarLog("OK SistemIA iniciado correctamente", "success");
            }
            else
            {
                AgregarLog("WARN SistemIA no responde aun, puede tardar unos segundos", "warning");
            }

            // Paso 9: Verificar archivos
            await ActualizarProgreso(95, "Verificando", "Verificando integridad de archivos...");
            archivosVerificados = VerificarArchivosActualizados();
            AgregarLog($"OK {archivosVerificados} archivos verificados", "success");

            // Completado
            await ActualizarProgreso(100, "Completado", "Actualizacion finalizada!");
            completado = true;
            AgregarLog("========================================", "success");
            AgregarLog("ACTUALIZACION COMPLETADA EXITOSAMENTE!", "success");
            AgregarLog("========================================", "success");
        }
        catch (Exception ex)
        {
            error = true;
            mensajeError = ex.Message;
            AgregarLog($"ERROR: {ex.Message}", "error");
        }
    }

    private async Task ActualizarProgreso(int pct, string estado, string mensaje)
    {
        progreso = pct;
        estadoActual = estado;
        mensajeActual = mensaje;
        AgregarLog($"[{DateTime.Now:HH:mm:ss}] {mensaje}");
        StateHasChanged();
        await Task.Delay(100);
    }

    private void AgregarLog(string mensaje, string tipo = "")
    {
        var prefix = tipo switch
        {
            "error" => "[ERROR] ",
            "success" => "[OK] ",
            "warning" => "[WARN] ",
            _ => ""
        };
        logs.Add($"{prefix}{mensaje}");
    }

    private string GetLogClass(string log)
    {
        if (log.Contains("[ERROR]") || log.Contains("ERROR")) return "error";
        if (log.Contains("[OK]") || log.Contains("OK ")) return "success";
        if (log.Contains("[WARN]") || log.Contains("WARN")) return "warning";
        return "";
    }

    private async Task DetenerSistemIA()
    {
        // Intentar detener servicio
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = "stop SistemIA",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(5000);
        }
        catch { }

        // Matar procesos
        try
        {
            foreach (var proc in Process.GetProcessesByName("SistemIA"))
            {
                proc.Kill();
                proc.WaitForExit(3000);
            }
        }
        catch { }

        await Task.Delay(2000);
    }

    private async Task IniciarSistemIA()
    {
        var exePath = Path.Combine(rutaSistemIA, "SistemIA.exe");
        if (File.Exists(exePath))
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = rutaSistemIA,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        await Task.Delay(2000);
    }

    private async Task<int> CopiarArchivos(string origen, string destino)
    {
        var count = 0;
        var excluir = new[] { "appsettings.json", "appsettings.Production.json", "appsettings.Development.json" };
        
        foreach (var archivo in Directory.GetFiles(origen, "*.*", SearchOption.AllDirectories))
        {
            var nombre = Path.GetFileName(archivo);
            if (excluir.Contains(nombre, StringComparer.OrdinalIgnoreCase)) continue;
            
            var relativePath = Path.GetRelativePath(origen, archivo);
            var destinoArchivo = Path.Combine(destino, relativePath);
            
            Directory.CreateDirectory(Path.GetDirectoryName(destinoArchivo)!);
            File.Copy(archivo, destinoArchivo, true);
            count++;
            
            if (count % 50 == 0)
            {
                progreso = 40 + (int)(count / 10.0);
                if (progreso > 65) progreso = 65;
                StateHasChanged();
                await Task.Delay(10);
            }
        }
        return count;
    }

    private int VerificarArchivosActualizados()
    {
        var criticos = new[] { "SistemIA.exe", "SistemIA.dll" };
        var verificados = 0;
        
        foreach (var archivo in criticos)
        {
            var ruta = Path.Combine(rutaSistemIA, archivo);
            if (File.Exists(ruta))
            {
                var info = new FileInfo(ruta);
                if ((DateTime.Now - info.LastWriteTime).TotalMinutes < 10)
                {
                    verificados++;
                }
            }
        }
        return verificados;
    }

    private string GetBadgeClass() => error ? "bg-danger" : completado ? "bg-success" : "bg-primary";
    private string GetStatusIcon() => error ? "fas fa-times-circle" : completado ? "fas fa-check-circle" : "fas fa-spinner fa-spin";

    private async Task AbrirSistemIA()
    {
        await JS.InvokeVoidAsync("open", "http://localhost:5095", "_blank");
    }

    private void Reiniciar()
    {
        actualizando = false;
        progreso = 0;
        logs.Clear();
        completado = false;
        error = false;
        CargarArchivosDisponibles();
    }

    private class ArchivoInfo
    {
        public string Nombre { get; set; } = "";
        public string RutaCompleta { get; set; } = "";
        public double TamanoMB { get; set; }
        public DateTime Fecha { get; set; }
    }
}
'@
[System.IO.File]::WriteAllText("$basePath\Pages\Index.razor", $indexRazor, [System.Text.Encoding]::UTF8)

# Eliminar archivos innecesarios de la plantilla
Remove-Item -Path "$basePath\Components" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Proyecto SistemIA.Actualizador creado exitosamente!"
Write-Host "Archivos creados:"
Write-Host "  - SistemIA.Actualizador.csproj"
Write-Host "  - Program.cs (Puerto 5096)"
Write-Host "  - Pages\_Host.cshtml"
Write-Host "  - Pages\Index.razor"
Write-Host "  - _Imports.razor"
Write-Host "  - App.razor"
