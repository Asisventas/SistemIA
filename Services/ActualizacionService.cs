using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace SistemIA.Services
{
    public class ActualizacionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActualizacionService> _logger;
        private readonly BackupService _backupService;
        private readonly IWebHostEnvironment _env;
        
        // Rutas configurables
        private readonly string _backupDir = @"C:\Backups\SistemIA";
        private readonly string _tempDir = @"C:\Temp\SistemIA";
        private readonly string _appDir;

        public ActualizacionService(
            IConfiguration configuration,
            ILogger<ActualizacionService> logger,
            BackupService backupService,
            IWebHostEnvironment env)
        {
            _configuration = configuration;
            _logger = logger;
            _backupService = backupService;
            _env = env;
            _appDir = _env.ContentRootPath;

            // Crear directorios si no existen
            Directory.CreateDirectory(_backupDir);
            Directory.CreateDirectory(_tempDir);
        }

        /// <summary>
        /// Obtiene información de la versión actual del sistema
        /// </summary>
        public VersionInfo ObtenerVersionActual()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var buildDate = File.GetLastWriteTime(assembly.Location);

                return new VersionInfo
                {
                    Version = version?.ToString() ?? "1.0.0.0",
                    FechaCompilacion = buildDate,
                    Entorno = _env.EnvironmentName,
                    RutaAplicacion = _appDir
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener versión actual");
                return new VersionInfo
                {
                    Version = "Desconocida",
                    FechaCompilacion = DateTime.Now,
                    Entorno = _env.EnvironmentName,
                    RutaAplicacion = _appDir
                };
            }
        }

        /// <summary>
        /// Proceso completo de actualización con backups y rollback
        /// </summary>
        public async Task<ResultadoActualizacion> ActualizarSistema(
            string archivoZipPath,
            bool aplicarMigraciones = true,
            IProgress<string>? progress = null)
        {
            var resultado = new ResultadoActualizacion
            {
                FechaInicio = DateTime.Now
            };

            var backupAppPath = "";
            var backupBdPath = "";

            try
            {
                progress?.Report("📋 Iniciando proceso de actualización...");

                // 1. Validar archivo ZIP
                progress?.Report("🔍 Validando archivo de actualización...");
                if (!File.Exists(archivoZipPath))
                {
                    resultado.Error = "Archivo ZIP no encontrado";
                    return resultado;
                }

                var (esValido, mensajeValidacion) = await ValidarZipActualizacionConMensaje(archivoZipPath);
                if (!esValido)
                {
                    resultado.Error = mensajeValidacion;
                    return resultado;
                }
                progress?.Report("✅ Archivo ZIP válido");

                // 2. Crear backup de aplicación
                progress?.Report("💾 Creando backup de la aplicación...");
                backupAppPath = await CrearBackupAplicacion();
                if (string.IsNullOrEmpty(backupAppPath))
                {
                    resultado.Error = "Error al crear backup de la aplicación";
                    return resultado;
                }
                resultado.BackupAppPath = backupAppPath;
                progress?.Report($"✅ Backup de aplicación creado: {Path.GetFileName(backupAppPath)}");

                // 3. Crear backup de base de datos
                progress?.Report("🗄️ Creando backup de la base de datos...");
                var backupResult = await _backupService.CrearBackupSoloBD();
                if (!backupResult.Success)
                {
                    resultado.Error = $"Error al crear backup de BD: {backupResult.Error}";
                    return resultado;
                }
                backupBdPath = backupResult.BackupPath;
                resultado.BackupBdPath = backupBdPath;
                progress?.Report($"✅ Backup de BD creado: {Path.GetFileName(backupBdPath)}");

                // 4. Extraer actualización a temporal
                progress?.Report("📦 Extrayendo archivos de actualización...");
                var tempExtractPath = Path.Combine(_tempDir, $"update_{DateTime.Now:yyyyMMddHHmmss}");
                Directory.CreateDirectory(tempExtractPath);
                ZipFile.ExtractToDirectory(archivoZipPath, tempExtractPath, true);
                progress?.Report("✅ Archivos extraídos correctamente");

                // 5. Guardar configuración actual
                progress?.Report("⚙️ Preservando configuración...");
                var configFiles = new[] { "appsettings.json", "appsettings.Production.json", "appsettings.Development.json" };
                var configBackup = new Dictionary<string, string>();
                
                foreach (var configFile in configFiles)
                {
                    var configPath = Path.Combine(_appDir, configFile);
                    if (File.Exists(configPath))
                    {
                        configBackup[configFile] = await File.ReadAllTextAsync(configPath);
                    }
                }

                // 6. Copiar nuevos archivos (excepto configuración)
                progress?.Report("📂 Actualizando archivos de la aplicación...");
                await CopiarArchivosActualizacion(tempExtractPath, _appDir, configFiles);
                progress?.Report("✅ Archivos actualizados correctamente");

                // 7. Restaurar configuración
                progress?.Report("🔧 Restaurando configuración...");
                foreach (var config in configBackup)
                {
                    var configPath = Path.Combine(_appDir, config.Key);
                    await File.WriteAllTextAsync(configPath, config.Value);
                }

                // 8. Aplicar migraciones de base de datos
                if (aplicarMigraciones)
                {
                    progress?.Report("🗄️ Aplicando migraciones de base de datos...");
                    var migracionResult = await AplicarMigraciones();
                    if (!migracionResult.Success)
                    {
                        throw new Exception($"Error en migraciones: {migracionResult.Error}");
                    }
                    resultado.MigracionesAplicadas = migracionResult.MigracionesAplicadas;
                    progress?.Report($"✅ Migraciones aplicadas: {migracionResult.MigracionesAplicadas}");
                }

                // 9. Limpiar temporales
                progress?.Report("🧹 Limpiando archivos temporales...");
                if (Directory.Exists(tempExtractPath))
                {
                    Directory.Delete(tempExtractPath, true);
                }

                // 10. Éxito
                resultado.Exitoso = true;
                resultado.FechaFin = DateTime.Now;
                resultado.TiempoTotal = resultado.FechaFin - resultado.FechaInicio;
                
                progress?.Report($"✅ ¡Actualización completada exitosamente en {resultado.TiempoTotal.TotalSeconds:F1} segundos!");
                progress?.Report("⚠️ Se recomienda reiniciar la aplicación para aplicar todos los cambios.");

                _logger.LogInformation("Actualización completada exitosamente");
                
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Error = ex.Message;
                resultado.FechaFin = DateTime.Now;
                resultado.TiempoTotal = resultado.FechaFin - resultado.FechaInicio;

                _logger.LogError(ex, "Error durante la actualización del sistema");
                progress?.Report($"❌ Error: {ex.Message}");

                // Intentar rollback
                progress?.Report("🔄 Iniciando rollback...");
                try
                {
                    if (!string.IsNullOrEmpty(backupAppPath) && File.Exists(backupAppPath))
                    {
                        await RestaurarBackupAplicacion(backupAppPath);
                        progress?.Report("✅ Aplicación restaurada desde backup");
                    }

                    if (!string.IsNullOrEmpty(backupBdPath) && File.Exists(backupBdPath))
                    {
                        progress?.Report("⚠️ Para restaurar la BD, ejecute manualmente:");
                        progress?.Report($"   RESTORE DATABASE [NombreBD] FROM DISK = '{backupBdPath}' WITH REPLACE");
                    }

                    resultado.RollbackAplicado = true;
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error durante el rollback");
                    progress?.Report($"❌ Error en rollback: {rollbackEx.Message}");
                }

                return resultado;
            }
        }

        private async Task<bool> ValidarZipActualizacion(string zipPath)
        {
            try
            {
                _logger.LogInformation("Validando archivo ZIP: {Path}", zipPath);
                
                // Verificar que el archivo existe y tiene tamaño
                var fileInfo = new FileInfo(zipPath);
                if (!fileInfo.Exists)
                {
                    _logger.LogError("El archivo ZIP no existe: {Path}", zipPath);
                    return false;
                }
                
                if (fileInfo.Length == 0)
                {
                    _logger.LogError("El archivo ZIP está vacío: {Path}", zipPath);
                    return false;
                }
                
                _logger.LogInformation("Tamaño del archivo: {Size} bytes", fileInfo.Length);

                using var archive = ZipFile.OpenRead(zipPath);
                
                _logger.LogInformation("Total de entradas en el ZIP: {Count}", archive.Entries.Count);
                
                if (archive.Entries.Count == 0)
                {
                    _logger.LogError("El archivo ZIP no contiene ninguna entrada");
                    return false;
                }
                
                // Mostrar algunos archivos para debug
                var primeros5 = archive.Entries.Take(5).Select(e => e.FullName).ToList();
                _logger.LogInformation("Primeros archivos en el ZIP: {Files}", string.Join(", ", primeros5));
                
                // Validar que contenga archivos esenciales
                var hasMainDll = archive.Entries.Any(e => e.Name.EndsWith("SistemIA.dll", StringComparison.OrdinalIgnoreCase));
                var hasExe = archive.Entries.Any(e => e.Name.EndsWith("SistemIA.exe", StringComparison.OrdinalIgnoreCase));
                
                _logger.LogInformation("Contiene SistemIA.dll: {HasDll}, Contiene SistemIA.exe: {HasExe}", hasMainDll, hasExe);
                
                // Aceptar si tiene la DLL, el EXE o al menos archivos
                return hasMainDll || hasExe || archive.Entries.Count > 0;
            }
            catch (InvalidDataException ex)
            {
                _logger.LogError(ex, "El archivo ZIP está corrupto o tiene un formato inválido: {Path}", zipPath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando ZIP de actualización: {Path}", zipPath);
                return false;
            }
        }

        private async Task<(bool esValido, string mensaje)> ValidarZipActualizacionConMensaje(string zipPath)
        {
            // Reintentar varias veces en caso de que el archivo esté bloqueado temporalmente
            for (int intento = 0; intento < 3; intento++)
            {
                try
                {
                    _logger.LogInformation("Validando archivo ZIP (intento {Intento}): {Path}", intento + 1, zipPath);
                    
                    // Verificar que el archivo existe y tiene tamaño
                    var fileInfo = new FileInfo(zipPath);
                    if (!fileInfo.Exists)
                    {
                        return (false, $"El archivo ZIP no existe: {zipPath}");
                    }
                    
                    if (fileInfo.Length == 0)
                    {
                        return (false, "El archivo ZIP está vacío (0 bytes)");
                    }
                    
                    _logger.LogInformation("Tamaño del archivo: {Size} bytes", fileInfo.Length);

                    // Abrir con FileShare.Read para permitir acceso compartido
                    using var fileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
                    
                    _logger.LogInformation("Total de entradas en el ZIP: {Count}", archive.Entries.Count);
                    
                    if (archive.Entries.Count == 0)
                    {
                        return (false, "El archivo ZIP no contiene ningún archivo");
                    }
                    
                    // Mostrar algunos archivos para debug
                    var primeros5 = archive.Entries.Take(5).Select(e => e.FullName).ToList();
                    _logger.LogInformation("Primeros archivos en el ZIP: {Files}", string.Join(", ", primeros5));
                    
                    // Validar que contenga archivos esenciales
                    var hasMainDll = archive.Entries.Any(e => e.Name.EndsWith("SistemIA.dll", StringComparison.OrdinalIgnoreCase));
                    var hasExe = archive.Entries.Any(e => e.Name.EndsWith("SistemIA.exe", StringComparison.OrdinalIgnoreCase));
                    
                    _logger.LogInformation("Contiene SistemIA.dll: {HasDll}, Contiene SistemIA.exe: {HasExe}", hasMainDll, hasExe);
                    
                    // Aceptar si tiene la DLL, el EXE o al menos archivos
                    if (hasMainDll || hasExe || archive.Entries.Count > 0)
                    {
                        return (true, "OK");
                    }
                    
                    return (false, "El archivo ZIP no contiene los archivos de SistemIA");
                }
                catch (IOException ex) when (intento < 2)
                {
                    _logger.LogWarning("Archivo bloqueado, reintentando en 500ms... (intento {Intento})", intento + 1);
                    await Task.Delay(500);
                    continue;
                }
                catch (InvalidDataException ex)
                {
                    _logger.LogError(ex, "El archivo ZIP está corrupto: {Path}", zipPath);
                    return (false, $"El archivo ZIP está corrupto o tiene un formato inválido: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validando ZIP: {Path}", zipPath);
                    return (false, $"Error al validar el archivo ZIP: {ex.Message}");
                }
            }
            
            return (false, "No se pudo acceder al archivo ZIP después de varios intentos");
        }

        private async Task<string> CrearBackupAplicacion()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"SistemIA_App_Backup_{timestamp}.zip";
                var backupPath = Path.Combine(_backupDir, backupFileName);

                // Crear ZIP de la aplicación actual
                if (File.Exists(backupPath))
                    File.Delete(backupPath);

                using var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create);
                
                var filesToBackup = Directory.GetFiles(_appDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\logs\\") && 
                                !f.Contains("\\temp\\") &&
                                !f.Contains("\\Temp\\") &&
                                !f.Contains("\\.vs\\") &&
                                !Path.GetFileName(f).EndsWith(".log"));

                foreach (var file in filesToBackup)
                {
                    var entryName = Path.GetRelativePath(_appDir, file);
                    archive.CreateEntryFromFile(file, entryName);
                }

                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup de la aplicación");
                return "";
            }
        }

        private async Task RestaurarBackupAplicacion(string backupPath)
        {
            try
            {
                // Eliminar archivos actuales (excepto configuración y logs)
                var filesToDelete = Directory.GetFiles(_appDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\logs\\") && 
                                !f.Contains("\\temp\\") &&
                                !f.EndsWith("appsettings.json") &&
                                !f.EndsWith("appsettings.Production.json"));

                foreach (var file in filesToDelete)
                {
                    try { File.Delete(file); } catch { }
                }

                // Extraer backup
                ZipFile.ExtractToDirectory(backupPath, _appDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar backup de la aplicación");
                throw;
            }
        }

        private async Task CopiarArchivosActualizacion(string origen, string destino, string[] excluir)
        {
            var archivos = Directory.GetFiles(origen, "*.*", SearchOption.AllDirectories);

            foreach (var archivo in archivos)
            {
                var nombreArchivo = Path.GetFileName(archivo);
                
                // Saltar archivos de configuración
                if (excluir.Contains(nombreArchivo, StringComparer.OrdinalIgnoreCase))
                    continue;

                var relativePath = Path.GetRelativePath(origen, archivo);
                var destinoArchivo = Path.Combine(destino, relativePath);

                // Crear directorio si no existe
                var dirDestino = Path.GetDirectoryName(destinoArchivo);
                if (!string.IsNullOrEmpty(dirDestino))
                    Directory.CreateDirectory(dirDestino);

                // Copiar archivo
                File.Copy(archivo, destinoArchivo, true);
            }
        }

        private async Task<MigracionResult> AplicarMigraciones()
        {
            var result = new MigracionResult();

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    result.Error = "Connection string no encontrada";
                    return result;
                }

                // Intentar aplicar migraciones usando dotnet ef
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "ef database update --no-build",
                    WorkingDirectory = _appDir,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        result.Success = true;
                        result.MigracionesAplicadas = ContarMigracionesEnOutput(output);
                        _logger.LogInformation($"Migraciones aplicadas correctamente: {output}");
                    }
                    else
                    {
                        result.Error = $"Error al aplicar migraciones: {error}";
                        _logger.LogError($"Error en migraciones: {error}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                _logger.LogError(ex, "Excepción al aplicar migraciones");
                return result;
            }
        }

        private int ContarMigracionesEnOutput(string output)
        {
            if (string.IsNullOrEmpty(output))
                return 0;

            var lines = output.Split('\n');
            return lines.Count(l => l.Contains("Applied migration", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Lista los backups disponibles
        /// </summary>
        public List<BackupInfo> ListarBackups()
        {
            try
            {
                if (!Directory.Exists(_backupDir))
                    return new List<BackupInfo>();

                var backups = new List<BackupInfo>();

                // Backups de aplicación
                var appBackups = Directory.GetFiles(_backupDir, "SistemIA_App_Backup_*.zip");
                foreach (var file in appBackups)
                {
                    var fileInfo = new FileInfo(file);
                    backups.Add(new BackupInfo
                    {
                        Tipo = "Aplicación",
                        NombreArchivo = fileInfo.Name,
                        RutaCompleta = fileInfo.FullName,
                        Fecha = fileInfo.CreationTime,
                        TamanoMB = fileInfo.Length / (1024.0 * 1024.0)
                    });
                }

                // Backups de BD
                var bdBackups = Directory.GetFiles(_backupDir, "*.bak");
                foreach (var file in bdBackups)
                {
                    var fileInfo = new FileInfo(file);
                    backups.Add(new BackupInfo
                    {
                        Tipo = "Base de Datos",
                        NombreArchivo = fileInfo.Name,
                        RutaCompleta = fileInfo.FullName,
                        Fecha = fileInfo.CreationTime,
                        TamanoMB = fileInfo.Length / (1024.0 * 1024.0)
                    });
                }

                return backups.OrderByDescending(b => b.Fecha).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar backups");
                return new List<BackupInfo>();
            }
        }

        /// <summary>
        /// Limpia backups antiguos (mantiene últimos N)
        /// </summary>
        public async Task<int> LimpiarBackupsAntiguos(int mantenerUltimos = 5)
        {
            try
            {
                var backups = ListarBackups();
                var eliminados = 0;

                var porTipo = backups.GroupBy(b => b.Tipo);
                
                foreach (var grupo in porTipo)
                {
                    var aEliminar = grupo.OrderByDescending(b => b.Fecha).Skip(mantenerUltimos);
                    
                    foreach (var backup in aEliminar)
                    {
                        try
                        {
                            File.Delete(backup.RutaCompleta);
                            eliminados++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"No se pudo eliminar backup: {backup.NombreArchivo}");
                        }
                    }
                }

                return eliminados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar backups antiguos");
                return 0;
            }
        }

        /// <summary>
        /// Genera un paquete de actualización listo para distribuir
        /// </summary>
        public async Task<ResultadoGeneracionPaquete> GenerarPaqueteActualizacion(
            string version,
            string descripcionCambios,
            IProgress<string>? progress = null)
        {
            var resultado = new ResultadoGeneracionPaquete
            {
                FechaInicio = DateTime.Now
            };

            try
            {
                progress?.Report("Iniciando generación de paquete de actualización...");
                
                var publishDir = Path.Combine(_tempDir, "publish_" + Guid.NewGuid().ToString("N"));
                var outputDir = Path.Combine(_appDir, "Releases");
                Directory.CreateDirectory(publishDir);
                Directory.CreateDirectory(outputDir);

                var fecha = DateTime.Now.ToString("yyyyMMdd_HHmm");
                var nombreZip = $"SistemIA_Update_v{version}_{fecha}.zip";
                var rutaZip = Path.Combine(outputDir, nombreZip);

                // Paso 1: Verificar migraciones
                progress?.Report("[1/7] Verificando migraciones de base de datos...");
                var migracionesInfo = await VerificarMigracionesPendientes();
                progress?.Report($"  ✓ Migraciones totales: {migracionesInfo.Total}");

                // Paso 2: Limpiar compilaciones anteriores
                progress?.Report("[2/7] Limpiando compilaciones anteriores...");
                await EjecutarComandoDotnet("clean", "-c Release");

                // Paso 3: Compilar en modo Release
                progress?.Report("[3/7] Compilando en modo Release...");
                var buildResult = await EjecutarComandoDotnet("build", "-c Release");
                if (!buildResult.Success)
                {
                    throw new Exception($"Error en compilación: {buildResult.Error}");
                }

                // Paso 4: Publicar aplicación (framework-dependent - usa runtime existente)
                progress?.Report("[4/7] Publicando aplicación (usa runtime existente)...");
                
                // Limpiar directorio de publicación si existe
                if (Directory.Exists(publishDir))
                {
                    Directory.Delete(publishDir, true);
                }
                Directory.CreateDirectory(publishDir);
                
                var publishResult = await EjecutarComandoDotnet("publish", 
                    $"\"{Path.Combine(_appDir, "SistemIA.csproj")}\"",
                    "-c", "Release",
                    "--no-self-contained",
                    "-p:PublishSingleFile=false",
                    "-p:IncludeAllContentForSelfExtract=false",
                    $"-o \"{publishDir}\"");
                
                if (!publishResult.Success)
                {
                    throw new Exception($"Error en publicación: {publishResult.Error}");
                }

                // Paso 5: Preparar paquete (excluir config, agregar script)
                progress?.Report("[5/7] Preparando paquete (excluyendo configuración)...");
                
                // Eliminar subdirectorios que no deben estar en el paquete de actualización
                var directoriosExcluir = new[] { 
                    "runtimes", "publish_selfcontained", "publish_temp", 
                    "test_fdd", "test_publish_fdd", "Releases", "obj", "bin",
                    "Installer", "ManualSifen", "Scripts", "Templates"
                };
                foreach (var subdir in directoriosExcluir)
                {
                    var subdirPath = Path.Combine(publishDir, subdir);
                    if (Directory.Exists(subdirPath))
                    {
                        Directory.Delete(subdirPath, true);
                        progress?.Report($"  → Excluido directorio: {subdir}");
                    }
                }
                
                // Eliminar archivos de configuración del paquete
                var configFiles = new[] { "appsettings.json", "appsettings.Development.json", "appsettings.Production.json" };
                foreach (var configFile in configFiles)
                {
                    var configPath = Path.Combine(publishDir, configFile);
                    if (File.Exists(configPath))
                    {
                        File.Delete(configPath);
                        progress?.Report($"  → Excluido: {configFile}");
                    }
                }

                // Crear archivo version.json
                var versionInfo = new
                {
                    Version = version,
                    BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    DotNetVersion = "8.0",
                    Platform = "win-x64",
                    SelfContained = true,
                    MigrationsIncluded = true,
                    Descripcion = descripcionCambios
                };
                var versionJson = System.Text.Json.JsonSerializer.Serialize(versionInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Path.Combine(publishDir, "version.json"), versionJson);

                // Copiar script Actualizar.bat (como fallback)
                // NOTA: El ActualizadorApp GUI NO se incluye en paquetes de actualización
                // porque ya está instalado en el cliente (se instala con el instalador inicial)
                var batSource = Path.Combine(_appDir, "Installer", "Actualizar.bat");
                var batDest = Path.Combine(publishDir, "Actualizar.bat");
                if (File.Exists(batSource))
                {
                    File.Copy(batSource, batDest, true);
                    progress?.Report("  ✓ Script Actualizar.bat incluido");
                }
                else
                {
                    await CrearScriptActualizacionBat(batDest, version);
                    progress?.Report("  ✓ Script Actualizar.bat generado");
                }

                // Paso 6: Verificar archivos críticos
                progress?.Report("[6/7] Verificando archivos críticos...");
                var archivosCriticos = new[]
                {
                    "SistemIA.dll",
                    "SistemIA.exe",
                    "SistemIA.deps.json",
                    "SistemIA.runtimeconfig.json",
                    "Actualizar.bat"
                };

                var archivosFaltantes = new List<string>();
                foreach (var archivo in archivosCriticos)
                {
                    if (!File.Exists(Path.Combine(publishDir, archivo)))
                    {
                        archivosFaltantes.Add(archivo);
                    }
                }

                if (archivosFaltantes.Any())
                {
                    throw new Exception($"Faltan archivos críticos: {string.Join(", ", archivosFaltantes)}");
                }

                // Contar archivos
                var archivos = Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories);
                resultado.TotalArchivos = archivos.Length;

                // Paso 7: Crear archivo ZIP
                progress?.Report($"[7/7] Creando archivo ZIP ({resultado.TotalArchivos} archivos)...");
                
                if (File.Exists(rutaZip))
                {
                    File.Delete(rutaZip);
                }

                ZipFile.CreateFromDirectory(publishDir, rutaZip, CompressionLevel.Optimal, false);

                // Calcular tamaño y hash
                var zipInfo = new FileInfo(rutaZip);
                resultado.TamanoMB = zipInfo.Length / (1024.0 * 1024.0);
                resultado.RutaZip = rutaZip;
                resultado.NombreArchivo = nombreZip;

                // Calcular hash SHA256
                progress?.Report("  Calculando hash SHA256...");
                using (var stream = File.OpenRead(rutaZip))
                {
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    {
                        var hashBytes = await sha256.ComputeHashAsync(stream);
                        resultado.HashSHA256 = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    }
                }

                // Guardar archivo de hash
                var hashFile = rutaZip + ".sha256";
                await File.WriteAllTextAsync(hashFile, $"{resultado.HashSHA256}  {nombreZip}");

                // Crear CHANGELOG
                var changelogPath = Path.Combine(outputDir, $"CHANGELOG_v{version}.txt");
                await CrearChangelog(changelogPath, version, descripcionCambios, resultado);

                resultado.RutaChangelog = changelogPath;
                resultado.Exitoso = true;
                resultado.FechaFin = DateTime.Now;
                resultado.TiempoTotal = resultado.FechaFin - resultado.FechaInicio;

                // Limpiar directorio temporal
                try
                {
                    Directory.Delete(publishDir, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar directorio temporal");
                }

                progress?.Report($"✓ Paquete generado exitosamente: {nombreZip}");
                progress?.Report($"  Tamaño: {resultado.TamanoMB:F2} MB");
                progress?.Report($"  Migraciones incluidas: Sí (se aplican automáticamente)");
                progress?.Report($"  Configuración: NO incluida (preserva appsettings.json del servidor)");
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar paquete de actualización");
                resultado.Exitoso = false;
                resultado.Error = ex.Message;
                resultado.FechaFin = DateTime.Now;
                resultado.TiempoTotal = resultado.FechaFin - resultado.FechaInicio;
                return resultado;
            }
        }

        /// <summary>
        /// Verifica migraciones pendientes en el proyecto
        /// </summary>
        private async Task<(int Total, int Pendientes, List<string> Nombres)> VerificarMigracionesPendientes()
        {
            try
            {
                var result = await EjecutarComandoDotnet("ef", "migrations", "list", "--no-build");
                if (!result.Success)
                {
                    return (0, 0, new List<string>());
                }

                var lineas = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var migraciones = lineas.Where(l => System.Text.RegularExpressions.Regex.IsMatch(l.Trim(), @"^\d{14}_")).ToList();
                var pendientes = migraciones.Where(m => m.Contains("(Pending)")).ToList();

                return (migraciones.Count, pendientes.Count, pendientes);
            }
            catch
            {
                return (0, 0, new List<string>());
            }
        }

        /// <summary>
        /// Crea el script Actualizar.bat si no existe
        /// </summary>
        private async Task CrearScriptActualizacionBat(string rutaDestino, string version)
        {
            var contenido = $@"@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ======================================================================
echo              ACTUALIZACION DE SISTEMIA - v{version}
echo ======================================================================
echo.

:: Verificar permisos de administrador
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Este script requiere permisos de Administrador
    echo         Clic derecho - Ejecutar como administrador
    pause
    exit /b 1
)

:: Configuracion
set ""SERVICE_NAME=SistemIA""
set ""INSTALL_PATH=C:\SistemIA""
set ""BACKUP_PATH=C:\backup""
set ""SCRIPT_DIR=%~dp0""

:: Verificar que existe la carpeta de instalacion
if not exist ""%INSTALL_PATH%"" (
    echo [ERROR] No se encuentra la carpeta de instalacion: %INSTALL_PATH%
    pause
    exit /b 1
)

echo [INFO] Carpeta de instalacion: %INSTALL_PATH%
echo [INFO] Carpeta de origen: %SCRIPT_DIR%
echo.

:: ============================================================
:: PASO 1: Crear backup
:: ============================================================
echo [1/4] Creando backup...

if not exist ""%BACKUP_PATH%"" mkdir ""%BACKUP_PATH%""

for /f ""tokens=2 delims=="" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set ""BACKUP_FILE=%BACKUP_PATH%\SistemIA_Backup_%datetime:~0,8%_%datetime:~8,6%.zip""

powershell -Command ""Compress-Archive -Path $env:INSTALL_PATH -DestinationPath $env:BACKUP_FILE -Force"" 2>nul
if %errorlevel% equ 0 (
    echo       Backup creado: %BACKUP_FILE%
) else (
    echo       [ADVERTENCIA] No se pudo crear backup, continuando...
)

:: ============================================================
:: PASO 2: Detener servicio
:: ============================================================
echo.
echo [2/4] Deteniendo servicio %SERVICE_NAME%...

sc query ""%SERVICE_NAME%"" >nul 2>&1
if %errorlevel% equ 0 (
    net stop ""%SERVICE_NAME%"" >nul 2>&1
    timeout /t 3 /nobreak >nul
    echo       Servicio detenido
) else (
    echo       Servicio no encontrado, deteniendo procesos...
    taskkill /F /IM SistemIA.exe >nul 2>&1
    timeout /t 2 /nobreak >nul
)

timeout /t 3 /nobreak >nul

:: ============================================================
:: PASO 3: Copiar archivos (sin appsettings.json)
:: ============================================================
echo.
echo [3/4] Actualizando archivos...

:: Respaldar appsettings.json actual
if exist ""%INSTALL_PATH%\appsettings.json"" (
    copy /Y ""%INSTALL_PATH%\appsettings.json"" ""%INSTALL_PATH%\appsettings.json.bak"" >nul
    echo       Configuracion respaldada
)

:: Copiar archivos excepto .bat y appsettings
for %%F in (""%SCRIPT_DIR%*.*"") do (
    set ""filename=%%~nxF""
    if /I not ""!filename!""==""Actualizar.bat"" (
        if /I not ""!filename!""==""appsettings.json"" (
            if /I not ""!filename!""==""appsettings.Development.json"" (
                if /I not ""!filename!""==""appsettings.Production.json"" (
                    copy /Y ""%%F"" ""%INSTALL_PATH%\"" >nul 2>&1
                )
            )
        )
    )
)

:: Copiar subcarpetas
for /D %%D in (""%SCRIPT_DIR%*"") do (
    set ""dirname=%%~nxD""
    xcopy /E /Y /I /Q ""%%D"" ""%INSTALL_PATH%\!dirname!\"" >nul 2>&1
)

echo       Archivos actualizados correctamente

:: ============================================================
:: PASO 4: Iniciar servicio
:: ============================================================
echo.
echo [4/4] Iniciando servicio...

sc query ""%SERVICE_NAME%"" >nul 2>&1
if %errorlevel% equ 0 (
    net start ""%SERVICE_NAME%""
    if %errorlevel% equ 0 (
        echo       Servicio iniciado correctamente
    ) else (
        echo       [ERROR] No se pudo iniciar el servicio
        echo       Revise los logs en: %INSTALL_PATH%\Logs
    )
) else (
    echo       [INFO] No hay servicio configurado.
    echo       Inicie la aplicacion manualmente: %INSTALL_PATH%\SistemIA.exe
)

:: ============================================================
:: FINALIZADO
:: ============================================================
echo.
echo ======================================================================
echo              ACTUALIZACION COMPLETADA
echo ======================================================================
echo.
echo Las migraciones de base de datos se aplicaran automaticamente
echo cuando el servicio inicie.
echo.
echo Backup guardado en: %BACKUP_FILE%
echo.

pause
";
            await File.WriteAllTextAsync(rutaDestino, contenido, Encoding.UTF8);
        }

        private async Task<ComandoResult> EjecutarComandoDotnet(string comando, params string[] argumentos)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = comando + " " + string.Join(" ", argumentos),
                    WorkingDirectory = _appDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    return new ComandoResult { Success = false, Error = "No se pudo iniciar proceso dotnet" };
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                return new ComandoResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    Error = string.IsNullOrEmpty(error) ? "" : error
                };
            }
            catch (Exception ex)
            {
                return new ComandoResult { Success = false, Error = ex.Message };
            }
        }

        private async Task CrearChangelog(string rutaArchivo, string version, string descripcionCambios, ResultadoGeneracionPaquete resultado)
        {
            var versionActual = ObtenerVersionActual();
            
            var changelog = $@"========================================
SistemIA - Paquete de Actualización
Versión: {version}
Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}
Archivo: {resultado.NombreArchivo}
Tamaño: {resultado.TamanoMB:F2} MB
Hash SHA256: {resultado.HashSHA256}
========================================

DESCRIPCIÓN DE CAMBIOS:
{descripcionCambios}

INFORMACIÓN DEL PAQUETE:
- Total de archivos: {resultado.TotalArchivos}
- Archivos principales verificados: ✓
- Generado desde versión: {versionActual.Version}

REQUISITOS:
- Windows Server 2019+ o Windows 10/11
- .NET 8.0 Runtime (ASP.NET Core)
- SQL Server 2019 o superior
- Espacio en disco: Mínimo {Math.Round(resultado.TamanoMB * 3, 0)} MB libre

INSTRUCCIONES DE INSTALACIÓN:

Método 1 - Interfaz Web (Recomendado):
1. Acceder a https://servidor:7060/actualizacion-sistema
2. Seleccionar el archivo {resultado.NombreArchivo}
3. Marcar ""Aplicar migraciones de BD"" si hay cambios en BD
4. Hacer clic en ""Iniciar Actualización""
5. Esperar a que complete (NO cerrar navegador)
6. Reiniciar aplicación cuando se indique

Método 2 - Script PowerShell:
1. Copiar {resultado.NombreArchivo} al servidor
2. Ejecutar PowerShell como Administrador
3. Ejecutar: .\Scripts\ActualizarSistemIA.ps1 -ArchivoZip ""ruta\{resultado.NombreArchivo}""
4. Seguir instrucciones en pantalla

VERIFICACIÓN POST-ACTUALIZACIÓN:
□ Servicio/aplicación inició correctamente
□ Login funciona
□ Funcionalidades principales operativas
□ No hay errores en logs
□ Base de datos actualizada correctamente

VERIFICACIÓN DE INTEGRIDAD:
Después de transferir el archivo, verificar el hash SHA256:
PowerShell> (Get-FileHash '{resultado.NombreArchivo}').Hash
Debe coincidir con: {resultado.HashSHA256}

ROLLBACK (si es necesario):
Los backups se crean automáticamente en:
- Aplicación: C:\Backups\SistemIA\SistemIA_App_Backup_*.zip
- Base de datos: C:\Backups\SistemIA\SistemIA_backup_*.bak

Para restaurar, consultar: MODULO_ACTUALIZACION_README.md

SOPORTE:
- Documentación: Ver archivos README en el proyecto
- Generado automáticamente por SistemIA

========================================
FECHA DE GENERACIÓN: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
========================================
";

            await File.WriteAllTextAsync(rutaArchivo, changelog, Encoding.UTF8);
        }

        /// <summary>
        /// Inicia una actualización con reinicio automático del servicio usando PowerShell
        /// </summary>
        public async Task<(bool iniciado, string mensaje)> IniciarActualizacionConReinicio(
            string archivoZipPath,
            bool aplicarMigraciones = true,
            string nombreServicio = "SistemIA")
        {
            try
            {
                _logger.LogInformation("Iniciando actualización con reinicio automático...");

                // Validar archivo ZIP
                var (esValido, mensajeValidacion) = await ValidarZipActualizacionConMensaje(archivoZipPath);
                if (!esValido)
                {
                    return (false, mensajeValidacion);
                }

                // Crear archivo de estado inicial
                var estadoPath = GetEstadoActualizacionPath();
                var versionActual = ObtenerVersionActual().Version;
                var estado = new EstadoActualizacion
                {
                    Estado = "iniciando",
                    Progreso = 5,
                    Mensaje = "Preparando actualización...",
                    FechaInicio = DateTime.Now,
                    VersionAnterior = versionActual,
                    Logs = new List<string> { $"[{DateTime.Now:HH:mm:ss}] Iniciando proceso de actualización..." }
                };
                await GuardarEstadoActualizacion(estado);

                // Crear backup de BD antes del reinicio
                _logger.LogInformation("Creando backup de base de datos...");
                estado.Mensaje = "Creando backup de base de datos...";
                estado.Progreso = 10;
                estado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Creando backup de base de datos...");
                await GuardarEstadoActualizacion(estado);

                var backupResult = await _backupService.CrearBackupSoloBD();
                if (!backupResult.Success)
                {
                    estado.Estado = "error";
                    estado.Error = $"Error al crear backup de BD: {backupResult.Error}";
                    estado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] ERROR: {estado.Error}");
                    await GuardarEstadoActualizacion(estado);
                    return (false, estado.Error);
                }
                _logger.LogInformation("Backup de BD creado: {Path}", backupResult.BackupPath);
                estado.Logs.Add($"[{DateTime.Now:HH:mm:ss}] Backup creado exitosamente");
                estado.Progreso = 15;
                await GuardarEstadoActualizacion(estado);

                // Obtener connection string para migraciones
                var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

                // Crear script PowerShell temporal
                var scriptPath = Path.Combine(_tempDir, $"update_script_{DateTime.Now:yyyyMMddHHmmss}.ps1");
                var logPath = Path.Combine(_tempDir, $"update_log_{DateTime.Now:yyyyMMddHHmmss}.txt");

                var scriptContent = GenerarScriptActualizacion(
                    archivoZipPath,
                    _appDir,
                    nombreServicio,
                    aplicarMigraciones,
                    connectionString,
                    backupResult.BackupPath ?? "",
                    logPath,
                    estadoPath // Agregar ruta del archivo de estado
                );

                await File.WriteAllTextAsync(scriptPath, scriptContent, Encoding.UTF8);
                _logger.LogInformation("Script de actualización creado: {Path}", scriptPath);

                // Ejecutar script en segundo plano (oculto)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -NoProfile -WindowStyle Hidden -File \"{scriptPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);
                _logger.LogInformation("Script de actualización iniciado en segundo plano");

                return (true, "Actualización iniciada. El sistema se reiniciará en unos segundos...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar actualización con reinicio");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la ruta del archivo de estado de actualización
        /// </summary>
        public string GetEstadoActualizacionPath()
        {
            return Path.Combine(_tempDir, "update_status.json");
        }

        /// <summary>
        /// Lee el estado actual de la actualización
        /// </summary>
        public async Task<EstadoActualizacion?> LeerEstadoActualizacion()
        {
            try
            {
                var path = GetEstadoActualizacionPath();
                if (!File.Exists(path))
                    return null;

                var json = await File.ReadAllTextAsync(path);
                return System.Text.Json.JsonSerializer.Deserialize<EstadoActualizacion>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Guarda el estado de la actualización
        /// </summary>
        public async Task GuardarEstadoActualizacion(EstadoActualizacion estado)
        {
            try
            {
                var path = GetEstadoActualizacionPath();
                var json = System.Text.Json.JsonSerializer.Serialize(estado, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar estado de actualización");
            }
        }

        /// <summary>
        /// Limpia el estado de actualización (después de completar o cancelar)
        /// </summary>
        public void LimpiarEstadoActualizacion()
        {
            try
            {
                var path = GetEstadoActualizacionPath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }

        private string GenerarScriptActualizacion(
            string zipPath,
            string installPath,
            string serviceName,
            bool aplicarMigraciones,
            string connectionString,
            string backupBdPath,
            string logPath,
            string estadoPath)
        {
            // Escapar rutas para PowerShell
            var zipPathEscaped = zipPath.Replace("'", "''");
            var installPathEscaped = installPath.Replace("'", "''");
            var logPathEscaped = logPath.Replace("'", "''");
            var backupPathEscaped = backupBdPath.Replace("'", "''");
            var connectionStringEscaped = connectionString.Replace("'", "''");
            var estadoPathEscaped = estadoPath.Replace("'", "''");

            return $@"
# =============================================================
# Script de Actualización Automática de SistemIA
# Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
# =============================================================

$ErrorActionPreference = 'Continue'
$logFile = '{logPathEscaped}'
$estadoFile = '{estadoPathEscaped}'

# Función para escribir logs
function Write-Log {{
    param([string]$Message)
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $logMessage = ""[$timestamp] $Message""
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage -ErrorAction SilentlyContinue
}}

# Función para actualizar estado JSON (para polling desde la web)
function Update-Estado {{
    param(
        [string]$Estado,
        [int]$Progreso,
        [string]$Mensaje,
        [string]$Error = $null
    )
    
    # Leer estado actual para mantener historial de logs
    $currentState = @{{
        Estado = $Estado
        Progreso = $Progreso
        Mensaje = $Mensaje
        FechaInicio = '{DateTime.Now:yyyy-MM-ddTHH:mm:ss}'
        Logs = @()
    }}
    
    if (Test-Path $estadoFile) {{
        try {{
            $existing = Get-Content $estadoFile -Raw | ConvertFrom-Json
            if ($existing.Logs) {{
                $currentState.Logs = @($existing.Logs)
            }}
            $currentState.FechaInicio = $existing.FechaInicio
            $currentState.VersionAnterior = $existing.VersionAnterior
        }} catch {{ }}
    }}
    
    # Agregar nuevo log
    $timestamp = Get-Date -Format 'HH:mm:ss'
    $currentState.Logs += ""[$timestamp] $Mensaje""
    
    if ($Error) {{
        $currentState.Error = $Error
        $currentState.FechaFin = (Get-Date -Format 'yyyy-MM-ddTHH:mm:ss')
    }}
    
    if ($Estado -eq 'completado') {{
        $currentState.FechaFin = (Get-Date -Format 'yyyy-MM-ddTHH:mm:ss')
    }}
    
    $currentState | ConvertTo-Json -Depth 10 | Set-Content $estadoFile -Encoding UTF8
}}

Write-Log '=============================================='
Write-Log 'INICIO DE ACTUALIZACION DE SISTEMIA'
Write-Log '=============================================='

Update-Estado -Estado 'deteniendo' -Progreso 20 -Mensaje 'Deteniendo el servicio...'

# Esperar a que la aplicación web cierre la conexión
Write-Log 'Esperando 3 segundos para que el servidor cierre conexiones...'
Start-Sleep -Seconds 3

# Variables
$zipPath = '{zipPathEscaped}'
$installPath = '{installPathEscaped}'
$serviceName = '{serviceName}'
$tempPath = ""$env:TEMP\SistemIA_Update_$(Get-Date -Format 'yyyyMMddHHmmss')""

# 1. Detener servicio o proceso
Write-Log 'Deteniendo servicio/proceso...'

$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service -and $service.Status -eq 'Running') {{
    Write-Log ""Deteniendo servicio: $serviceName""
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
}}

# También intentar matar el proceso por si no es servicio
$processes = Get-Process | Where-Object {{ $_.ProcessName -like '*SistemIA*' -or ($_.MainModule.FileName -like '*SistemIA*' -and $_.Id -ne $PID) }} -ErrorAction SilentlyContinue
foreach ($proc in $processes) {{
    Write-Log ""Deteniendo proceso: $($proc.ProcessName) (PID: $($proc.Id))""
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
}}
Start-Sleep -Seconds 3

# Esperar a que se liberen los archivos
Write-Log 'Esperando liberación de archivos...'
Update-Estado -Estado 'extrayendo' -Progreso 30 -Mensaje 'Extrayendo archivos del paquete...'
Start-Sleep -Seconds 2

# 2. Extraer ZIP
Write-Log ""Extrayendo archivos desde: $zipPath""
try {{
    Expand-Archive -Path $zipPath -DestinationPath $tempPath -Force
    Write-Log 'Archivos extraídos correctamente'
}} catch {{
    Write-Log ""ERROR al extraer ZIP: $_""
    Update-Estado -Estado 'error' -Progreso 30 -Mensaje 'Error al extraer archivos' -Error $_.ToString()
    exit 1
}}

Update-Estado -Estado 'copiando' -Progreso 45 -Mensaje 'Copiando archivos actualizados...'

# 3. Copiar archivos (preservando configuración)
Write-Log 'Copiando archivos actualizados...'

$excludeFiles = @('appsettings.json', 'appsettings.Production.json', 'appsettings.Development.json')
$copiedCount = 0
$errorCount = 0
$totalFiles = (Get-ChildItem -Path $tempPath -Recurse -File).Count

Get-ChildItem -Path $tempPath -Recurse -File | ForEach-Object {{
    $relativePath = $_.FullName.Substring($tempPath.Length + 1)
    $destPath = Join-Path $installPath $relativePath
    
    if ($excludeFiles -contains $_.Name) {{
        Write-Log ""  Omitiendo configuración: $($_.Name)""
        return
    }}
    
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {{
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }}
    
    try {{
        Copy-Item -Path $_.FullName -Destination $destPath -Force
        $copiedCount++
        
        # Actualizar progreso cada 20 archivos
        if ($copiedCount % 20 -eq 0) {{
            $pct = [math]::Round(45 + (($copiedCount / $totalFiles) * 25))
            Update-Estado -Estado 'copiando' -Progreso $pct -Mensaje ""Copiando archivos... ($copiedCount de $totalFiles)""
        }}
    }} catch {{
        Write-Log ""  ERROR copiando: $relativePath - $_""
        $errorCount++
    }}
}}

Write-Log ""Archivos copiados: $copiedCount""
if ($errorCount -gt 0) {{
    Write-Log ""Archivos con error: $errorCount""
}}

# 4. Aplicar migraciones si está habilitado
{(aplicarMigraciones ? $@"
Update-Estado -Estado 'migrando' -Progreso 75 -Mensaje 'Aplicando migraciones de base de datos...'
Write-Log 'Aplicando migraciones de base de datos...'
try {{
    $dotnetPath = Join-Path $installPath 'SistemIA.dll'
    if (Test-Path $dotnetPath) {{
        Push-Location $installPath
        $env:ConnectionStrings__DefaultConnection = '{connectionStringEscaped}'
        
        # Intentar aplicar migraciones usando dotnet ef o el propio ejecutable
        $efResult = & dotnet ef database update --no-build 2>&1
        if ($LASTEXITCODE -eq 0) {{
            Write-Log 'Migraciones aplicadas correctamente'
        }} else {{
            Write-Log ""Nota: Las migraciones se aplicarán al iniciar la aplicación""
        }}
        
        Pop-Location
    }}
}} catch {{
    Write-Log ""Nota: Migraciones se aplicarán al iniciar: $_""
}}" : "Update-Estado -Estado 'reiniciando' -Progreso 80 -Mensaje 'Migraciones deshabilitadas'\nWrite-Log 'Migraciones deshabilitadas por el usuario'")}

Update-Estado -Estado 'reiniciando' -Progreso 85 -Mensaje 'Limpiando archivos temporales...'

# 5. Limpiar temporal
Write-Log 'Limpiando archivos temporales...'
Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue

Update-Estado -Estado 'reiniciando' -Progreso 90 -Mensaje 'Iniciando la aplicación...'

# 6. Reiniciar servicio
Write-Log 'Iniciando servicio/aplicación...'

$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service) {{
    Start-Service -Name $serviceName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service.Status -eq 'Running') {{
        Write-Log 'Servicio iniciado correctamente'
        Update-Estado -Estado 'completado' -Progreso 100 -Mensaje '¡Actualización completada exitosamente!'
    }} else {{
        Write-Log 'ADVERTENCIA: El servicio no inició. Iniciando manualmente...'
        # Intentar iniciar el exe directamente
        $exePath = Join-Path $installPath 'SistemIA.exe'
        if (Test-Path $exePath) {{
            Start-Process -FilePath $exePath -WorkingDirectory $installPath
            Write-Log 'Aplicación iniciada manualmente'
            Update-Estado -Estado 'completado' -Progreso 100 -Mensaje '¡Actualización completada! Aplicación iniciada manualmente.'
        }} else {{
            Update-Estado -Estado 'error' -Progreso 95 -Mensaje 'Actualización aplicada pero no se pudo iniciar la aplicación' -Error 'No se encontró el ejecutable'
        }}
    }}
}} else {{
    # No hay servicio, intentar iniciar el exe
    $exePath = Join-Path $installPath 'SistemIA.exe'
    if (Test-Path $exePath) {{
        Write-Log 'Iniciando aplicación directamente...'
        Start-Process -FilePath $exePath -WorkingDirectory $installPath
        Write-Log 'Aplicación iniciada'
        # Esperar un poco a que arranque
        Start-Sleep -Seconds 5
        Update-Estado -Estado 'completado' -Progreso 100 -Mensaje '¡Actualización completada exitosamente!'
    }} else {{
        # Intentar con dotnet run
        Write-Log 'Intentando iniciar con dotnet...'
        $dllPath = Join-Path $installPath 'SistemIA.dll'
        if (Test-Path $dllPath) {{
            Start-Process -FilePath 'dotnet' -ArgumentList """"""$dllPath"""""" -WorkingDirectory $installPath
            Start-Sleep -Seconds 5
            Update-Estado -Estado 'completado' -Progreso 100 -Mensaje '¡Actualización completada exitosamente!'
        }} else {{
            Update-Estado -Estado 'error' -Progreso 95 -Mensaje 'Actualización aplicada pero no se pudo iniciar la aplicación' -Error 'No se encontró ejecutable ni DLL'
        }}
    }}
}}

Write-Log '=============================================='
Write-Log 'PROCESO DE ACTUALIZACION FINALIZADO'
Write-Log '=============================================='
Write-Log ""Log guardado en: $logFile""
Write-Log ""Backup de BD: {backupPathEscaped}""

# Script termina silenciosamente (sin esperar entrada del usuario)
";
        }

        /// <summary>
        /// Compila el actualizador GUI desde el proyecto ActualizadorApp
        /// </summary>
        private async Task<bool> CompilarActualizadorGUI(string publishDir, IProgress<string>? progress = null)
        {
            try
            {
                // Ruta al proyecto ActualizadorApp (hermano de SistemIA)
                var actualizadorProjectDir = Path.GetFullPath(Path.Combine(_appDir, "..", "ActualizadorApp"));
                var actualizadorCsproj = Path.Combine(actualizadorProjectDir, "SistemIAUpdater.csproj");

                // Verificar si existe el proyecto
                if (!File.Exists(actualizadorCsproj))
                {
                    _logger.LogWarning("Proyecto ActualizadorApp no encontrado en: {Path}", actualizadorProjectDir);
                    return false;
                }

                // Directorio temporal para publicar el actualizador
                var actualizadorPublishDir = Path.Combine(_tempDir, "actualizador_publish_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(actualizadorPublishDir);

                try
                {
                    // Compilar el actualizador (framework-dependent, usa el runtime del paquete SistemIA)
                    var psi = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"publish \"{actualizadorCsproj}\" -c Release -r win-x64 --self-contained false -o \"{actualizadorPublishDir}\"",
                        WorkingDirectory = actualizadorProjectDir,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    if (process == null)
                    {
                        _logger.LogError("No se pudo iniciar el proceso de compilación del actualizador");
                        return false;
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("Error al compilar actualizador: {Error}", error);
                        return false;
                    }

                    // Copiar solo los archivos específicos del actualizador al paquete
                    // El runtime y DLLs compartidas ya están en el paquete de SistemIA
                    var archivosActualizador = new[]
                    {
                        "Actualizar-SistemIA.exe",
                        "Actualizar-SistemIA.dll",
                        "Actualizar-SistemIA.deps.json",
                        "Actualizar-SistemIA.runtimeconfig.json"
                    };

                    int copiedCount = 0;
                    foreach (var archivo in archivosActualizador)
                    {
                        var source = Path.Combine(actualizadorPublishDir, archivo);
                        var dest = Path.Combine(publishDir, archivo);
                        if (File.Exists(source))
                        {
                            File.Copy(source, dest, true);
                            copiedCount++;
                        }
                    }

                    // Copiar DLLs adicionales que el actualizador necesita y no están en SistemIA
                    // (en este caso System.ServiceProcess.ServiceController.dll que SistemIA no usa)
                    var dllsAdicionales = Directory.GetFiles(actualizadorPublishDir, "*.dll")
                        .Where(f => !File.Exists(Path.Combine(publishDir, Path.GetFileName(f))))
                        .ToList();

                    foreach (var dll in dllsAdicionales)
                    {
                        var dest = Path.Combine(publishDir, Path.GetFileName(dll));
                        File.Copy(dll, dest, true);
                        copiedCount++;
                    }

                    if (copiedCount > 0)
                    {
                        _logger.LogInformation("Actualizador GUI compilado e incluido: {Count} archivos", copiedCount);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("No se encontraron archivos del actualizador para copiar");
                        return false;
                    }
                }
                finally
                {
                    // Limpiar directorio temporal
                    try
                    {
                        if (Directory.Exists(actualizadorPublishDir))
                            Directory.Delete(actualizadorPublishDir, true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al compilar actualizador GUI");
                return false;
            }
        }
    }

    // Clases de datos
    public class VersionInfo
    {
        public string Version { get; set; } = "";
        public DateTime FechaCompilacion { get; set; }
        public string Entorno { get; set; } = "";
        public string RutaAplicacion { get; set; } = "";
    }

    public class ResultadoActualizacion
    {
        public bool Exitoso { get; set; }
        public string Error { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan TiempoTotal { get; set; }
        public string BackupAppPath { get; set; } = "";
        public string BackupBdPath { get; set; } = "";
        public int MigracionesAplicadas { get; set; }
        public bool RollbackAplicado { get; set; }
    }

    public class MigracionResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public int MigracionesAplicadas { get; set; }
    }

    public class BackupInfo
    {
        public string Tipo { get; set; } = "";
        public string NombreArchivo { get; set; } = "";
        public string RutaCompleta { get; set; } = "";
        public DateTime Fecha { get; set; }
        public double TamanoMB { get; set; }
    }

    public class ResultadoGeneracionPaquete
    {
        public bool Exitoso { get; set; }
        public string Error { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public TimeSpan TiempoTotal { get; set; }
        public string RutaZip { get; set; } = "";
        public string NombreArchivo { get; set; } = "";
        public string RutaChangelog { get; set; } = "";
        public double TamanoMB { get; set; }
        public string HashSHA256 { get; set; } = "";
        public int TotalArchivos { get; set; }
    }

    public class ComandoResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
    }

    /// <summary>
    /// Estado de actualización que se persiste en archivo JSON para seguimiento en tiempo real
    /// </summary>
    public class EstadoActualizacion
    {
        public string Estado { get; set; } = "idle"; // idle, iniciando, deteniendo, extrayendo, copiando, migrando, reiniciando, completado, error
        public int Progreso { get; set; } = 0;
        public string Mensaje { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Error { get; set; }
        public string? VersionAnterior { get; set; }
        public string? VersionNueva { get; set; }
        public List<string> Logs { get; set; } = new();
    }
}
