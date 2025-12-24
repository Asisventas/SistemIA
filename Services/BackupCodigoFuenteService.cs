using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Compression;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para backup completo del código fuente y base de datos.
    /// Diferente de BackupService que se usa para actualizaciones del sistema.
    /// Este servicio copia TODO el código fuente desde la carpeta del proyecto.
    /// </summary>
    public class BackupCodigoFuenteService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupCodigoFuenteService> _logger;
        
        // Ruta fija del proyecto - cambiar si el proyecto está en otra ubicación
        private const string RUTA_PROYECTO = @"C:\asis\SistemIA";
        private const string RUTA_BACKUP = @"C:\backup";

        public BackupCodigoFuenteService(IConfiguration configuration, ILogger<BackupCodigoFuenteService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Crea un backup completo del código fuente y la base de datos
        /// </summary>
        public async Task<BackupCodigoResult> CrearBackupCompleto()
        {
            var result = new BackupCodigoResult();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            try
            {
                // Crear directorio de backup si no existe
                if (!Directory.Exists(RUTA_BACKUP))
                {
                    Directory.CreateDirectory(RUTA_BACKUP);
                }

                // Verificar que existe la carpeta del proyecto
                if (!Directory.Exists(RUTA_PROYECTO))
                {
                    result.Error = $"No se encontró la carpeta del proyecto: {RUTA_PROYECTO}";
                    return result;
                }

                // 1. Obtener nombre de la base de datos desde la cadena de conexión
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    result.Error = "No se encontró la cadena de conexión";
                    return result;
                }
                
                var databaseName = ExtraerNombreBaseDatos(connectionString);
                
                if (string.IsNullOrEmpty(databaseName))
                {
                    result.Error = "No se pudo obtener el nombre de la base de datos";
                    return result;
                }

                // 2. Crear backup de la base de datos
                var backupFileName = $"{databaseName}_backup_{timestamp}.bak";
                var backupPath = Path.Combine(RUTA_BACKUP, backupFileName);
                
                var backupSuccess = await CrearBackupBaseDatos(connectionString, databaseName, backupPath);
                if (!backupSuccess)
                {
                    result.Error = "Error al crear el backup de la base de datos";
                    return result;
                }
                result.BackupBDPath = backupPath;

                // 3. Crear ZIP del código fuente
                var zipFileName = $"SistemIA_codigo_{timestamp}.zip";
                var zipPath = Path.Combine(RUTA_BACKUP, zipFileName);
                
                var (zipExitoso, archivosCopidos) = await CrearZipCodigoFuente(RUTA_PROYECTO, zipPath);
                if (!zipExitoso)
                {
                    result.Error = "Error al crear el ZIP del código fuente";
                    return result;
                }

                result.Success = true;
                result.ZipCodigoPath = zipPath;
                result.DatabaseName = databaseName;
                result.Timestamp = timestamp;
                result.ArchivosCopidos = archivosCopidos;
                
                _logger.LogInformation($"Backup completo creado: BD={backupPath}, Código={zipPath}, Archivos={archivosCopidos}");
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"Error general: {ex.Message}";
                _logger.LogError(ex, "Error al crear backup completo de código fuente");
                return result;
            }
        }

        private async Task<(bool exito, int archivos)> CrearZipCodigoFuente(string origen, string zipPath)
        {
            try
            {
                // Carpetas a excluir (binarios, temporales, etc.)
                var carpetasExcluir = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "bin",
                    "obj",
                    ".vs",
                    ".git",
                    "node_modules",
                    "packages",
                    "TestResults",
                    "publish_temp",
                    "publish_selfcontained",
                    "Releases"
                };

                // Archivos a excluir por extensión
                var extensionesExcluir = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".user",
                    ".suo",
                    ".cache"
                };

                int archivosAgregados = 0;

                // Crear el archivo ZIP
                using (var zipStream = new FileStream(zipPath, FileMode.Create))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    archivosAgregados = await AgregarDirectorioAlZip(archive, origen, "", carpetasExcluir, extensionesExcluir);
                }

                _logger.LogInformation("ZIP creado: {ZipPath} con {Archivos} archivos", zipPath, archivosAgregados);
                return (true, archivosAgregados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ZIP de código fuente");
                return (false, 0);
            }
        }

        private async Task<int> AgregarDirectorioAlZip(ZipArchive archive, string origen, string prefijoEntrada,
            HashSet<string> carpetasExcluir, HashSet<string> extensionesExcluir)
        {
            var dirInfo = new DirectoryInfo(origen);
            int archivosAgregados = 0;

            // Agregar todos los archivos
            foreach (var file in dirInfo.GetFiles())
            {
                var extension = Path.GetExtension(file.Name);
                if (!extensionesExcluir.Contains(extension))
                {
                    try
                    {
                        var entryName = string.IsNullOrEmpty(prefijoEntrada)
                            ? file.Name
                            : $"{prefijoEntrada}/{file.Name}";

                        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        
                        // Usar FileShare.ReadWrite para poder leer archivos en uso
                        using var sourceStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var entryStream = entry.Open();
                        await sourceStream.CopyToAsync(entryStream);
                        
                        archivosAgregados++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("No se pudo agregar {Archivo}: {Error}", file.Name, ex.Message);
                        // Continuar con los demás archivos
                    }
                }
            }

            // Agregar subdirectorios recursivamente
            foreach (var subDir in dirInfo.GetDirectories())
            {
                if (!carpetasExcluir.Contains(subDir.Name))
                {
                    var nuevoPrefijoEntrada = string.IsNullOrEmpty(prefijoEntrada)
                        ? subDir.Name
                        : $"{prefijoEntrada}/{subDir.Name}";
                    
                    archivosAgregados += await AgregarDirectorioAlZip(archive, subDir.FullName, nuevoPrefijoEntrada, 
                        carpetasExcluir, extensionesExcluir);
                }
            }

            return archivosAgregados;
        }

        private string ExtraerNombreBaseDatos(string connectionString)
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                return builder.InitialCatalog;
            }
            catch
            {
                // Fallback: buscar Database= en la cadena
                var parts = connectionString.Split(';');
                var dbPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase));
                return dbPart?.Split('=')[1]?.Trim() ?? "";
            }
        }

        private async Task<bool> CrearBackupBaseDatos(string connectionString, string databaseName, string backupPath)
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                var server = builder.DataSource;
                var userId = builder.UserID;
                var password = builder.Password;
                var useIntegratedSecurity = builder.IntegratedSecurity;

                var sqlCommand = $@"
                    BACKUP DATABASE [{databaseName}] 
                    TO DISK = '{backupPath}' 
                    WITH FORMAT, INIT, 
                    NAME = '{databaseName}-Full Database Backup', 
                    SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                var authArgs = useIntegratedSecurity 
                    ? "-E" 
                    : $"-U \"{userId}\" -P \"{password}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "sqlcmd",
                    Arguments = $"-S \"{server}\" {authArgs} -Q \"{sqlCommand}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    var exitCode = process.ExitCode;
                    if (exitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.LogError("Error sqlcmd: {Error}", error);
                    }
                    return exitCode == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup de base de datos");
                return false;
            }
        }
    }

    public class BackupCodigoResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public string BackupBDPath { get; set; } = "";
        public string ZipCodigoPath { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public int ArchivosCopidos { get; set; }
    }
}
