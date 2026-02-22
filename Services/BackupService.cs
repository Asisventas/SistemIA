using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Compression;

namespace SistemIA.Services
{
    public class BackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;

        public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<BackupResult> CrearBackupCompleto()
        {
            var result = new BackupResult();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            try
            {
                // Crear directorio de backup si no existe
                var backupDir = @"C:\backup";
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
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
                var backupPath = Path.Combine(backupDir, backupFileName);
                
                var backupSuccess = await CrearBackupBaseDatos(connectionString, databaseName, backupPath);
                if (!backupSuccess)
                {
                    result.Error = "Error al crear el backup de la base de datos";
                    return result;
                }

                // 3. Crear ZIP del proyecto
                var zipFileName = $"SistemIA_proyecto_{timestamp}.zip";
                var zipPath = Path.Combine(backupDir, zipFileName);
                
                var zipSuccess = await CrearZipProyecto(zipPath);
                if (!zipSuccess)
                {
                    result.Error = "Error al crear el ZIP del proyecto";
                    return result;
                }

                result.Success = true;
                result.BackupPath = backupPath;
                result.ZipPath = zipPath;
                result.DatabaseName = databaseName;
                result.Timestamp = timestamp;
                
                _logger.LogInformation($"Backup completo creado exitosamente: {backupPath}, {zipPath}");
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"Error general: {ex.Message}";
                _logger.LogError(ex, "Error al crear backup completo");
                return result;
            }
        }

        /// <summary>
        /// Crea solo el backup de la base de datos sin el ZIP del proyecto
        /// </summary>
        public async Task<BackupResult> CrearBackupSoloBD()
        {
            var result = new BackupResult();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            try
            {
                // Crear directorio de backup si no existe
                var backupDir = @"C:\backup";
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
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
                var backupPath = Path.Combine(backupDir, backupFileName);
                
                var backupSuccess = await CrearBackupBaseDatos(connectionString, databaseName, backupPath);
                if (!backupSuccess)
                {
                    result.Error = "Error al crear el backup de la base de datos";
                    return result;
                }

                result.Success = true;
                result.BackupPath = backupPath;
                result.DatabaseName = databaseName;
                result.Timestamp = timestamp;
                
                _logger.LogInformation($"Backup de BD creado exitosamente: {backupPath}");
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"Error general: {ex.Message}";
                _logger.LogError(ex, "Error al crear backup de BD");
                return result;
            }
        }

        /// <summary>
        /// Crea backup de la BD con un nombre fijo (para sobrescribir siempre el mismo archivo)
        /// Usado para CloudSync donde se quiere mantener solo 1 copia en la nube
        /// AHORA: Crea el backup .bak y lo comprime a .zip para reducir tamaño de subida
        /// </summary>
        public async Task<BackupResult> CrearBackupParaCloudSync()
        {
            var result = new BackupResult();
            
            try
            {
                // Crear directorio de backup si no existe
                var backupDir = @"C:\backup";
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
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

                // 2. Crear backup .bak temporal
                var backupFileName = $"SistemIA_CloudSync_Backup.bak";
                var backupPath = Path.Combine(backupDir, backupFileName);
                
                // Si existe el archivo anterior, eliminarlo primero (SQL Server no sobrescribe)
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                
                var backupSuccess = await CrearBackupBaseDatos(connectionString, databaseName, backupPath);
                if (!backupSuccess)
                {
                    result.Error = "Error al crear el backup de la base de datos";
                    return result;
                }

                _logger.LogInformation($"Backup .bak creado: {backupPath}");

                // 3. Comprimir el backup a ZIP
                var zipFileName = "SistemIA_CloudSync_Backup.zip";
                var zipPath = Path.Combine(backupDir, zipFileName);
                
                // Eliminar ZIP anterior si existe
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                // Comprimir el .bak a .zip
                _logger.LogInformation("Comprimiendo backup a ZIP...");
                await Task.Run(() =>
                {
                    using var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                    zipArchive.CreateEntryFromFile(backupPath, backupFileName, CompressionLevel.Optimal);
                });
                
                var bakSize = new FileInfo(backupPath).Length;
                var zipSize = new FileInfo(zipPath).Length;
                var compresion = (1 - (double)zipSize / bakSize) * 100;
                
                _logger.LogInformation($"Backup comprimido: {bakSize / 1024 / 1024:F1} MB → {zipSize / 1024 / 1024:F1} MB ({compresion:F1}% reducción)");

                // 4. Eliminar el .bak temporal (ya tenemos el .zip)
                try
                {
                    File.Delete(backupPath);
                    _logger.LogInformation("Archivo .bak temporal eliminado");
                }
                catch
                {
                    _logger.LogWarning("No se pudo eliminar el archivo .bak temporal");
                }

                result.Success = true;
                result.BackupPath = zipPath; // Retornar la ruta del ZIP comprimido
                result.DatabaseName = databaseName;
                result.Timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                _logger.LogInformation($"Backup para CloudSync listo: {zipPath}");
                
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"Error general: {ex.Message}";
                _logger.LogError(ex, "Error al crear backup para CloudSync");
                return result;
            }
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
                    return process.ExitCode == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup de base de datos");
                return false;
            }
        }

        private async Task<bool> CrearZipProyecto(string zipPath)
        {
            try
            {
                // Usar la ruta de ejecución de la aplicación (donde está desplegada)
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                
                _logger.LogInformation("Creando backup ZIP desde: {AppDir}", appDir);
                
                // Crear ZIP temporal en memoria para filtrar archivos
                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    await AgregarDirectorioAlZipCompleto(archive, appDir, "", new[]
                    {
                        // Excluir carpetas temporales y de logs
                        "logs", "temp", "cache", "runtimes"
                    });
                }

                // Escribir el ZIP final
                await File.WriteAllBytesAsync(zipPath, memoryStream.ToArray());
                _logger.LogInformation("Backup ZIP creado exitosamente: {ZipPath}", zipPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ZIP de la aplicación");
                return false;
            }
        }
        
        /// <summary>
        /// Crea un ZIP incluyendo TODOS los archivos (para backup de aplicación desplegada)
        /// </summary>
        private async Task AgregarDirectorioAlZipCompleto(ZipArchive archive, string sourceDir, string entryPrefix, string[] excludeDirs)
        {
            var dirInfo = new DirectoryInfo(sourceDir);
            
            // Agregar TODOS los archivos (excepto temporales y de log)
            foreach (var file in dirInfo.GetFiles())
            {
                // Excluir solo archivos temporales y de log
                var excludeExtensions = new[] { ".log", ".tmp", ".cache" };
                var ext = Path.GetExtension(file.Name).ToLowerInvariant();
                
                if (!excludeExtensions.Contains(ext))
                {
                    var entryName = string.IsNullOrEmpty(entryPrefix) 
                        ? file.Name 
                        : Path.Combine(entryPrefix, file.Name).Replace('\\', '/');
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = file.OpenRead();
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            // Agregar subdirectorios (recursivo)
            foreach (var subDir in dirInfo.GetDirectories())
            {
                if (!excludeDirs.Contains(subDir.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var newEntryPrefix = string.IsNullOrEmpty(entryPrefix) 
                        ? subDir.Name 
                        : Path.Combine(entryPrefix, subDir.Name);
                    await AgregarDirectorioAlZipCompleto(archive, subDir.FullName, newEntryPrefix, excludeDirs);
                }
            }
        }

        private async Task AgregarDirectorioAlZip(ZipArchive archive, string sourceDir, string entryPrefix, string[] excludeDirs)
        {
            var dirInfo = new DirectoryInfo(sourceDir);
            
            // Agregar archivos
            foreach (var file in dirInfo.GetFiles())
            {
                if (ShouldIncludeFile(file.Name))
                {
                    var entryName = Path.Combine(entryPrefix, file.Name).Replace('\\', '/');
                    var entry = archive.CreateEntry(entryName);
                    using var entryStream = entry.Open();
                    using var fileStream = file.OpenRead();
                    await fileStream.CopyToAsync(entryStream);
                }
            }

            // Agregar subdirectorios (recursivo)
            foreach (var subDir in dirInfo.GetDirectories())
            {
                if (!excludeDirs.Contains(subDir.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var newEntryPrefix = Path.Combine(entryPrefix, subDir.Name);
                    await AgregarDirectorioAlZip(archive, subDir.FullName, newEntryPrefix, excludeDirs);
                }
            }
        }

        private bool ShouldIncludeFile(string fileName)
        {
            var excludeExtensions = new[] { ".dll", ".exe", ".pdb", ".cache", ".log" };
            var extension = Path.GetExtension(fileName).ToLower();
            return !excludeExtensions.Contains(extension);
        }
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public string BackupPath { get; set; } = "";
        public string ZipPath { get; set; } = "";
        public string DatabaseName { get; set; } = "";
        public string Timestamp { get; set; } = "";
    }
}
