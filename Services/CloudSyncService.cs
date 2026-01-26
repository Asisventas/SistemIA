using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using SistemIA.Data;
using SistemIA.Models;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace SistemIA.Services;

/// <summary>
/// Clase para reportar el progreso de subida
/// </summary>
public class CloudSyncUploadProgress
{
    public string NombreArchivo { get; set; } = "";
    public long BytesEnviados { get; set; }
    public long TotalBytes { get; set; }
    public int PorcentajeCompletado => TotalBytes > 0 ? (int)(BytesEnviados * 100 / TotalBytes) : 0;
    public string Estado { get; set; } = "Preparando";
    public string? MensajeError { get; set; }
    public DateTime Inicio { get; set; } = DateTime.Now;
    public double VelocidadMBps => BytesEnviados > 0 && (DateTime.Now - Inicio).TotalSeconds > 0 
        ? Math.Round(BytesEnviados / 1024.0 / 1024.0 / (DateTime.Now - Inicio).TotalSeconds, 2) 
        : 0;
}

/// <summary>
/// StreamContent que reporta progreso de lectura
/// </summary>
public class ProgressStreamContent : HttpContent
{
    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly Action<long, long>? _onProgress;
    private readonly long _totalSize;

    public ProgressStreamContent(Stream stream, long totalSize, Action<long, long>? onProgress = null, int bufferSize = 81920)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _totalSize = totalSize;
        _onProgress = onProgress;
        _bufferSize = bufferSize;
        Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var buffer = new byte[_bufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await stream.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
            _onProgress?.Invoke(totalBytesRead, _totalSize);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _totalSize;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();
        base.Dispose(disposing);
    }
}

/// <summary>
/// HashSet thread-safe para tracking de chunks en subida paralela
/// </summary>
public class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dict = new();

    public bool Add(T item) => _dict.TryAdd(item, 0);
    public bool Contains(T item) => _dict.ContainsKey(item);
    public int Count => _dict.Count;
    
    public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Resultado de subida de un chunk individual
/// </summary>
public class ChunkResult
{
    public bool Success { get; set; }
    public int ChunkIndex { get; set; }
    public int BytesEnviados { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Interfaz del servicio de sincronización con CloudSync
/// </summary>
public interface ICloudSyncService
{
    /// <summary>
    /// Verifica la conexión con CloudSync
    /// </summary>
    Task<CloudSyncEstadoConexion> VerificarConexionAsync();

    /// <summary>
    /// Sube un archivo a CloudSync
    /// </summary>
    Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoAsync(
        string rutaArchivo, 
        bool esAutomatico = false, 
        int? idUsuario = null,
        Action<CloudSyncUploadProgress>? onProgress = null);

    /// <summary>
    /// Sube todos los archivos de backup pendientes
    /// </summary>
    Task<(int exitosos, int fallidos, List<string> errores)> SincronizarBackupsAsync(
        int? idUsuario = null);

    /// <summary>
    /// Lista los archivos en CloudSync
    /// </summary>
    Task<List<CloudSyncFileInfo>> ListarArchivosRemotosAsync(string? carpeta = null);

    /// <summary>
    /// Elimina un archivo de CloudSync
    /// </summary>
    Task<bool> EliminarArchivoRemotoAsync(string idArchivo);

    /// <summary>
    /// Descarga un archivo de CloudSync
    /// </summary>
    Task<(bool exito, string? rutaLocal)> DescargarArchivoAsync(string idArchivo, string carpetaDestino);

    /// <summary>
    /// Obtiene la configuración actual
    /// </summary>
    Task<ConfiguracionCloudSync?> ObtenerConfiguracionAsync();

    /// <summary>
    /// Guarda la configuración
    /// </summary>
    Task<bool> GuardarConfiguracionAsync(ConfiguracionCloudSync config);

    /// <summary>
    /// Obtiene el historial de backups
    /// </summary>
    Task<List<HistorialBackupCloud>> ObtenerHistorialAsync(int cantidad = 50);

    /// <summary>
    /// Obtiene los archivos locales pendientes de subir
    /// </summary>
    Task<List<(string ruta, long tamano, DateTime fechaModificacion)>> ObtenerArchivosPendientesAsync();

    /// <summary>
    /// Diagnostica la API para detectar el formato de subida esperado
    /// </summary>
    Task<CloudSyncDiagnostico> DiagnosticarApiAsync();

    /// <summary>
    /// Sube un archivo usando chunks (fragmentos) para archivos grandes > 50 MB
    /// </summary>
    Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoChunksAsync(
        string rutaArchivo,
        Action<CloudSyncUploadProgress>? onProgress = null,
        int chunkSizeMB = 10);

    /// <summary>
    /// Sube un archivo usando chunks PARALELOS (4 conexiones simultáneas) para máxima velocidad
    /// </summary>
    Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoParaleloAsync(
        string rutaArchivo,
        Action<CloudSyncUploadProgress>? onProgress = null,
        int chunkSizeMB = 5,
        int maxParalelo = 4);

    /// <summary>
    /// Verifica si un archivo local ya está sincronizado en la nube
    /// </summary>
    /// <returns>Tupla con: existe en nube, es idéntico (mismo tamaño), info del archivo remoto</returns>
    Task<(bool existeEnNube, bool esIdentico, CloudSyncFileInfo? archivoRemoto)> VerificarSincronizacionAsync(
        string rutaArchivoLocal);

    /// <summary>
    /// Obtiene el estado de sincronización de múltiples archivos
    /// </summary>
    Task<List<EstadoSincronizacionArchivo>> ObtenerEstadoSincronizacionAsync(
        List<string> rutasArchivos);

    /// <summary>
    /// Elimina todo el historial de backups
    /// </summary>
    Task<(bool exito, int registrosEliminados)> EliminarHistorialAsync();
}

/// <summary>
/// Resultado del diagnóstico de la API
/// </summary>
public class CloudSyncDiagnostico
{
    public bool Conexion { get; set; }
    public string? EndpointUpload { get; set; }
    public string? CampoArchivo { get; set; }
    public List<string> EndpointsEncontrados { get; set; } = new();
    public List<string> Errores { get; set; } = new();
    public string? RespuestaServidor { get; set; }
}

/// <summary>
/// Estado de sincronización de un archivo local vs remoto
/// </summary>
public class EstadoSincronizacionArchivo
{
    public string RutaLocal { get; set; } = "";
    public string NombreArchivo { get; set; } = "";
    public long TamanoLocal { get; set; }
    public DateTime FechaLocal { get; set; }
    
    // Estado de sincronización
    public bool ExisteEnNube { get; set; }
    public bool EstaSincronizado { get; set; }  // Mismo nombre Y tamaño
    public bool NecesitaActualizar { get; set; } // Mismo nombre pero diferente tamaño
    
    // Info del archivo remoto (si existe)
    public string? IdRemoto { get; set; }
    public long? TamanoRemoto { get; set; }
    public DateTime? FechaRemota { get; set; }
    
    // Descripción del estado
    public string DescripcionEstado => (ExisteEnNube, EstaSincronizado, NecesitaActualizar) switch
    {
        (false, _, _) => "No sincronizado",
        (true, true, _) => "Sincronizado ✓",
        (true, false, true) => "Actualización disponible",
        _ => "Diferente en la nube"
    };
    
    public string ClaseCss => (ExisteEnNube, EstaSincronizado, NecesitaActualizar) switch
    {
        (false, _, _) => "text-warning",
        (true, true, _) => "text-success",
        (true, false, true) => "text-info",
        _ => "text-secondary"
    };
    
    public string Icono => (ExisteEnNube, EstaSincronizado, NecesitaActualizar) switch
    {
        (false, _, _) => "bi-cloud-slash",
        (true, true, _) => "bi-cloud-check",
        (true, false, true) => "bi-cloud-arrow-up",
        _ => "bi-cloud-minus"
    };
}

/// <summary>
/// Servicio de sincronización con CloudSync
/// </summary>
public class CloudSyncService : ICloudSyncService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<CloudSyncService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private ConfiguracionCloudSync? _configCache;
    private DateTime _configCacheTime = DateTime.MinValue;

    public CloudSyncService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<CloudSyncService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<HttpClient> CrearClienteAsync()
    {
        var config = await ObtenerConfiguracionAsync();
        if (config == null || string.IsNullOrEmpty(config.ApiKey))
            throw new InvalidOperationException("CloudSync no está configurado. Configure la API Key primero.");

        var handler = new HttpClientHandler
        {
            // Aceptar certificados SSL autofirmados (servidores locales/desarrollo)
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        // Asegurar que la URL base termine en /
        var baseUrl = config.UrlApi.TrimEnd('/') + "/";
        
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(30) // Timeout largo para archivos grandes
        };

        client.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    public async Task<CloudSyncEstadoConexion> VerificarConexionAsync()
    {
        try
        {
            var config = await ObtenerConfiguracionAsync();
            if (config == null || string.IsNullOrEmpty(config.ApiKey))
            {
                return new CloudSyncEstadoConexion
                {
                    Conectado = false,
                    Mensaje = "CloudSync no está configurado. Configure la API Key."
                };
            }

            _logger.LogInformation("[CloudSync] Verificando conexión a: {Url}", config.UrlApi);

            using var client = await CrearClienteAsync();
            
            // Intentar obtener información del usuario/espacio
            _logger.LogInformation("[CloudSync] Enviando GET a: {Url}", client.BaseAddress + "files");
            var response = await client.GetAsync("files");
            
            _logger.LogInformation("[CloudSync] Respuesta: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("[CloudSync] Respuesta JSON: {Json}", json.Length > 500 ? json.Substring(0, 500) + "..." : json);
                
                var listResponse = JsonSerializer.Deserialize<CloudSyncListResponse>(json);

                return new CloudSyncEstadoConexion
                {
                    Conectado = true,
                    Mensaje = "Conexión exitosa",
                    ArchivosEnNube = listResponse?.Total ?? listResponse?.Files?.Count ?? 0
                };
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("[CloudSync] Error HTTP {StatusCode}: {Body}", response.StatusCode, errorBody);
            
            return new CloudSyncEstadoConexion
            {
                Conectado = false,
                Mensaje = $"Error de conexión: {response.StatusCode} - {errorBody}"
            };
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "[CloudSync] Error HTTP: {Message}", httpEx.Message);
            
            // Dar sugerencia si es error SSL
            var mensaje = httpEx.Message;
            if (mensaje.Contains("SSL") || mensaje.Contains("certificate"))
            {
                mensaje += " (Pruebe cambiar https:// por http:// en la URL)";
            }
            
            return new CloudSyncEstadoConexion
            {
                Conectado = false,
                Mensaje = $"Error: {mensaje}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CloudSync] Error verificando conexión: {Message}", ex.Message);
            return new CloudSyncEstadoConexion
            {
                Conectado = false,
                Mensaje = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoAsync(
        string rutaArchivo,
        bool esAutomatico = false,
        int? idUsuario = null,
        Action<CloudSyncUploadProgress>? onProgress = null)
    {
        var progress = new CloudSyncUploadProgress { NombreArchivo = Path.GetFileName(rutaArchivo) };
        var historial = new HistorialBackupCloud
        {
            NombreArchivo = Path.GetFileName(rutaArchivo),
            RutaLocal = rutaArchivo,
            FechaInicio = DateTime.Now,
            Estado = "Subiendo",
            FueAutomatico = esAutomatico,
            IdUsuario = idUsuario
        };

        void ReportProgress(string estado, long bytesEnviados = 0, string? error = null)
        {
            progress.Estado = estado;
            progress.BytesEnviados = bytesEnviados;
            progress.MensajeError = error;
            onProgress?.Invoke(progress);
        }

        try
        {
            ReportProgress("Verificando archivo");

            if (!File.Exists(rutaArchivo))
            {
                historial.Estado = "Error";
                historial.MensajeError = "[LOCAL] Archivo no encontrado en disco";
                await GuardarHistorialAsync(historial);
                ReportProgress("Error", 0, historial.MensajeError);
                return (false, historial.MensajeError, null);
            }

            var fileInfo = new FileInfo(rutaArchivo);
            historial.TamanoBytes = fileInfo.Length;
            progress.TotalBytes = fileInfo.Length;

            // Verificar que el archivo no está en uso
            try
            {
                using var testStream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.Read);
                testStream.Close();
            }
            catch (IOException ioEx)
            {
                var errorMsg = $"[LOCAL] Archivo en uso por otro proceso: {ioEx.Message}";
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                ReportProgress("Error", 0, errorMsg);
                return (false, errorMsg, null);
            }

            ReportProgress("Calculando hash");
            historial.HashMD5 = await CalcularMD5Async(rutaArchivo);

            // Guardar historial con estado "Subiendo"
            await GuardarHistorialAsync(historial);

            ReportProgress("Conectando");
            _logger.LogInformation("[CloudSync] Iniciando subida: {Archivo} ({Tamano} bytes)", 
                historial.NombreArchivo, historial.TamanoBytes);

            using var client = await CrearClienteAsync();
            
            // Aumentar timeout basado en tamaño del archivo (mínimo 5 min, +1 min por cada 100 MB)
            var timeoutMinutes = Math.Max(5, 5 + (int)(fileInfo.Length / (100 * 1024 * 1024)));
            client.Timeout = TimeSpan.FromMinutes(timeoutMinutes);
            _logger.LogInformation("[CloudSync] Timeout configurado: {Minutos} minutos", timeoutMinutes);

            using var content = new MultipartFormDataContent();
            
            // Abrir stream con buffer grande para mejor rendimiento
            await using var fileStream = new FileStream(
                rutaArchivo, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                bufferSize: 1024 * 1024); // 1 MB buffer

            // Usar ProgressStreamContent para reportar progreso
            var progressContent = new ProgressStreamContent(
                fileStream, 
                fileInfo.Length, 
                (bytesEnviados, total) => ReportProgress("Subiendo", bytesEnviados));

            // NOTA: El servidor CloudSync espera el campo "files" (plural)
            // Confirmado por diagnóstico API - NO usar "file" ni "archivo"
            content.Add(progressContent, "files", Path.GetFileName(rutaArchivo));

            ReportProgress("Subiendo", 0);
            _logger.LogInformation("[CloudSync] Enviando POST a: {Url}", client.BaseAddress + "files/upload");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("files/upload", content);
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is TimeoutException || !tcEx.CancellationToken.IsCancellationRequested)
            {
                var errorMsg = $"[TIMEOUT] La subida tardó más de {timeoutMinutes} minutos. El archivo es muy grande o la conexión muy lenta.";
                throw new Exception(errorMsg, tcEx);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("[CloudSync] Respuesta: Status={Status}, Body={Body}", 
                response.StatusCode, responseJson.Length > 500 ? responseJson[..500] + "..." : responseJson);

            historial.FechaFin = DateTime.Now;
            historial.DuracionSegundos = (int)(historial.FechaFin.Value - historial.FechaInicio).TotalSeconds;

            if (response.IsSuccessStatusCode)
            {
                var uploadResponse = JsonSerializer.Deserialize<CloudSyncUploadResponse>(responseJson);
                var fileInfo2 = uploadResponse?.GetFile(); // Helper que busca en File o Data.File
                
                historial.Estado = "Completado";
                historial.IdArchivoRemoto = fileInfo2?.Id;
                historial.RutaRemota = fileInfo2?.Path;
                
                await GuardarHistorialAsync(historial);
                await ActualizarUltimaSincronizacionAsync(true, "Completado");

                ReportProgress("Completado", fileInfo.Length);
                _logger.LogInformation("[CloudSync] ✓ Archivo {Archivo} subido exitosamente en {Seg} segundos", 
                    historial.NombreArchivo, historial.DuracionSegundos);
                return (true, "Archivo subido exitosamente", historial.IdArchivoRemoto);
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<CloudSyncUploadResponse>(responseJson);
                var serverError = errorResponse?.Error ?? errorResponse?.Message ?? responseJson;
                var errorMsg = $"[SERVIDOR] HTTP {(int)response.StatusCode}: {serverError}";
                
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                
                await GuardarHistorialAsync(historial);
                await ActualizarUltimaSincronizacionAsync(false, errorMsg);

                ReportProgress("Error", progress.BytesEnviados, errorMsg);
                _logger.LogError("[CloudSync] ✗ Error del servidor: {Error}", errorMsg);
                return (false, errorMsg, null);
            }
        }
        catch (HttpRequestException httpEx)
        {
            // Error de conexión/red
            var errorMsg = DiagnosticarErrorHttp(httpEx);
            historial.FechaFin = DateTime.Now;
            historial.Estado = "Error";
            historial.MensajeError = errorMsg;
            
            await GuardarHistorialAsync(historial);
            await ActualizarUltimaSincronizacionAsync(false, errorMsg);
            ReportProgress("Error", progress.BytesEnviados, errorMsg);

            _logger.LogError(httpEx, "[CloudSync] ✗ Error HTTP: {Error}", errorMsg);
            return (false, errorMsg, null);
        }
        catch (IOException ioEx)
        {
            // Error de lectura de archivo
            var errorMsg = $"[LOCAL] Error leyendo archivo: {ioEx.Message}";
            historial.FechaFin = DateTime.Now;
            historial.Estado = "Error";
            historial.MensajeError = errorMsg;
            
            await GuardarHistorialAsync(historial);
            await ActualizarUltimaSincronizacionAsync(false, errorMsg);
            ReportProgress("Error", progress.BytesEnviados, errorMsg);

            _logger.LogError(ioEx, "[CloudSync] ✗ Error IO: {Error}", errorMsg);
            return (false, errorMsg, null);
        }
        catch (Exception ex)
        {
            var errorMsg = $"[ERROR] {ex.Message}";
            historial.FechaFin = DateTime.Now;
            historial.Estado = "Error";
            historial.MensajeError = errorMsg;
            
            await GuardarHistorialAsync(historial);
            await ActualizarUltimaSincronizacionAsync(false, errorMsg);

            ReportProgress("Error", progress.BytesEnviados, errorMsg);
            _logger.LogError(ex, "[CloudSync] ✗ Error: {Error}", errorMsg);
            return (false, errorMsg, null);
        }
    }

    /// <summary>
    /// Sube un archivo usando chunks (fragmentos) para archivos grandes.
    /// Esto evita timeouts del servidor en archivos > 50 MB.
    /// </summary>
    public async Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoChunksAsync(
        string rutaArchivo,
        Action<CloudSyncUploadProgress>? onProgress = null,
        int chunkSizeMB = 10)
    {
        var progress = new CloudSyncUploadProgress
        {
            NombreArchivo = Path.GetFileName(rutaArchivo),
            Estado = "Iniciando"
        };

        var historial = new HistorialBackupCloud
        {
            NombreArchivo = progress.NombreArchivo,
            RutaLocal = rutaArchivo,
            Estado = "En progreso",
            FechaInicio = DateTime.Now
        };

        void ReportProgress(string estado, long bytesEnviados = 0, string? error = null)
        {
            progress.Estado = estado;
            progress.BytesEnviados = bytesEnviados;
            progress.MensajeError = error;
            onProgress?.Invoke(progress);
        }

        try
        {
            ReportProgress("Verificando archivo");

            if (!File.Exists(rutaArchivo))
            {
                historial.Estado = "Error";
                historial.MensajeError = "[LOCAL] Archivo no encontrado en disco";
                await GuardarHistorialAsync(historial);
                ReportProgress("Error", 0, historial.MensajeError);
                return (false, historial.MensajeError, null);
            }

            var fileInfo = new FileInfo(rutaArchivo);
            historial.TamanoBytes = fileInfo.Length;
            progress.TotalBytes = fileInfo.Length;

            // Calcular chunks
            int chunkSize = chunkSizeMB * 1024 * 1024; // Convertir a bytes
            int totalChunks = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);

            _logger.LogInformation("[CloudSync-Chunks] Archivo: {Nombre}, Tamaño: {Tamano} bytes, Chunks: {Total} x {Size}MB",
                fileInfo.Name, fileInfo.Length, totalChunks, chunkSizeMB);

            ReportProgress("Calculando hash");
            historial.HashMD5 = await CalcularMD5Async(rutaArchivo);
            await GuardarHistorialAsync(historial);

            using var client = await CrearClienteAsync();
            client.Timeout = TimeSpan.FromMinutes(5); // Timeout por chunk, no por archivo completo

            // ========== PASO 1: Iniciar subida ==========
            ReportProgress("Iniciando subida");
            _logger.LogInformation("[CloudSync-Chunks] Iniciando upload...");

            var initPayload = new
            {
                filename = fileInfo.Name,
                fileSize = fileInfo.Length,
                totalChunks = totalChunks,
                mimeType = "application/octet-stream"
            };

            var initResponse = await client.PostAsJsonAsync("files/upload/init", initPayload);
            var initJson = await initResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("[CloudSync-Chunks] Init response: {Status} - {Body}",
                initResponse.StatusCode, initJson.Length > 200 ? initJson[..200] : initJson);

            if (!initResponse.IsSuccessStatusCode)
            {
                var errorMsg = $"[SERVIDOR] Error iniciando subida: HTTP {(int)initResponse.StatusCode} - {initJson}";
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                ReportProgress("Error", 0, errorMsg);
                return (false, errorMsg, null);
            }

            var initData = JsonSerializer.Deserialize<ChunkUploadInitResponse>(initJson);
            var uploadId = initData?.Data?.UploadId;

            if (string.IsNullOrEmpty(uploadId))
            {
                var errorMsg = "[SERVIDOR] No se recibió uploadId del servidor";
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                ReportProgress("Error", 0, errorMsg);
                return (false, errorMsg, null);
            }

            _logger.LogInformation("[CloudSync-Chunks] Upload ID: {UploadId}", uploadId);

            // ========== PASO 2: Subir cada chunk ==========
            await using var fileStream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[chunkSize];
            long totalBytesEnviados = 0;

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                int bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSize);
                if (bytesRead == 0) break;

                ReportProgress($"Subiendo chunk {chunkIndex + 1}/{totalChunks}", totalBytesEnviados);
                _logger.LogDebug("[CloudSync-Chunks] Subiendo chunk {Index}/{Total} ({Bytes} bytes)",
                    chunkIndex + 1, totalChunks, bytesRead);

                using var chunkContent = new MultipartFormDataContent();
                var chunkBytes = new byte[bytesRead];
                Array.Copy(buffer, chunkBytes, bytesRead);

                chunkContent.Add(new ByteArrayContent(chunkBytes), "chunk", "chunk");
                chunkContent.Add(new StringContent(chunkIndex.ToString()), "chunkIndex");

                var chunkResponse = await client.PostAsync($"files/upload/chunk/{uploadId}", chunkContent);
                var chunkJson = await chunkResponse.Content.ReadAsStringAsync();

                if (!chunkResponse.IsSuccessStatusCode)
                {
                    var errorMsg = $"[SERVIDOR] Error en chunk {chunkIndex + 1}: HTTP {(int)chunkResponse.StatusCode} - {chunkJson}";
                    _logger.LogError("[CloudSync-Chunks] {Error}", errorMsg);
                    historial.Estado = "Error";
                    historial.MensajeError = errorMsg;
                    await GuardarHistorialAsync(historial);
                    ReportProgress("Error", totalBytesEnviados, errorMsg);
                    return (false, errorMsg, null);
                }

                totalBytesEnviados += bytesRead;
                ReportProgress($"Subiendo chunk {chunkIndex + 1}/{totalChunks}", totalBytesEnviados);
            }

            // ========== PASO 3: Completar subida ==========
            ReportProgress("Finalizando", totalBytesEnviados);
            _logger.LogInformation("[CloudSync-Chunks] Completando upload...");

            var completeResponse = await client.PostAsync($"files/upload/complete/{uploadId}", null);
            var completeJson = await completeResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("[CloudSync-Chunks] Complete response: {Status} - {Body}",
                completeResponse.StatusCode, completeJson.Length > 200 ? completeJson[..200] : completeJson);

            historial.FechaFin = DateTime.Now;
            historial.DuracionSegundos = (int)(historial.FechaFin.Value - historial.FechaInicio).TotalSeconds;

            if (completeResponse.IsSuccessStatusCode)
            {
                var completeData = JsonSerializer.Deserialize<ChunkUploadCompleteResponse>(completeJson);
                var fileId = completeData?.Data?.File?.Id ?? completeData?.File?.Id;
                var filePath = completeData?.Data?.File?.Path ?? completeData?.File?.Path;

                historial.Estado = "Completado";
                historial.IdArchivoRemoto = fileId;
                historial.RutaRemota = filePath;

                await GuardarHistorialAsync(historial);
                await ActualizarUltimaSincronizacionAsync(true, "Completado");

                ReportProgress("Completado", fileInfo.Length);
                _logger.LogInformation("[CloudSync-Chunks] ✓ Archivo subido exitosamente en {Seg} segundos ({Chunks} chunks)",
                    historial.DuracionSegundos, totalChunks);
                return (true, "Archivo subido exitosamente", fileId);
            }
            else
            {
                var errorMsg = $"[SERVIDOR] Error completando subida: HTTP {(int)completeResponse.StatusCode} - {completeJson}";
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                ReportProgress("Error", totalBytesEnviados, errorMsg);
                return (false, errorMsg, null);
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"[ERROR] {ex.Message}";
            historial.FechaFin = DateTime.Now;
            historial.Estado = "Error";
            historial.MensajeError = errorMsg;

            await GuardarHistorialAsync(historial);
            await ActualizarUltimaSincronizacionAsync(false, errorMsg);

            ReportProgress("Error", progress.BytesEnviados, errorMsg);
            _logger.LogError(ex, "[CloudSync-Chunks] ✗ Error: {Error}", errorMsg);
            return (false, errorMsg, null);
        }
    }

    /// <summary>
    /// Sube un archivo usando chunks PARALELOS para máxima velocidad.
    /// Usa 4 conexiones simultáneas por defecto y chunks de 5 MB.
    /// </summary>
    public async Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoParaleloAsync(
        string rutaArchivo,
        Action<CloudSyncUploadProgress>? onProgress = null,
        int chunkSizeMB = 5,
        int maxParalelo = 4)
    {
        const int MAX_RETRIES = 3;

        var progress = new CloudSyncUploadProgress
        {
            NombreArchivo = Path.GetFileName(rutaArchivo),
            Estado = "Iniciando subida paralela"
        };

        var historial = new HistorialBackupCloud
        {
            NombreArchivo = progress.NombreArchivo,
            RutaLocal = rutaArchivo,
            Estado = "En progreso",
            FechaInicio = DateTime.Now
        };

        void ReportProgress(string estado, long bytesEnviados = 0, string? error = null)
        {
            progress.Estado = estado;
            progress.BytesEnviados = bytesEnviados;
            progress.MensajeError = error;
            onProgress?.Invoke(progress);
        }

        try
        {
            ReportProgress("Verificando archivo");

            if (!File.Exists(rutaArchivo))
            {
                historial.Estado = "Error";
                historial.MensajeError = "[LOCAL] Archivo no encontrado";
                await GuardarHistorialAsync(historial);
                return (false, historial.MensajeError, null);
            }

            var fileInfo = new FileInfo(rutaArchivo);
            historial.TamanoBytes = fileInfo.Length;
            progress.TotalBytes = fileInfo.Length;

            int chunkSize = chunkSizeMB * 1024 * 1024;
            int totalChunks = (int)Math.Ceiling((double)fileInfo.Length / chunkSize);

            _logger.LogInformation(
                "[CloudSync-Paralelo] Archivo: {Nombre} ({Tamano:F2} MB), {Total} chunks x {Size}MB, {Paralelo} conexiones",
                fileInfo.Name, fileInfo.Length / 1024.0 / 1024.0, totalChunks, chunkSizeMB, maxParalelo);

            ReportProgress("Calculando hash");
            historial.HashMD5 = await CalcularMD5Async(rutaArchivo);
            await GuardarHistorialAsync(historial);

            // ========== PASO 1: Iniciar subida ==========
            ReportProgress("Iniciando upload en servidor");
            using var clientInit = await CrearClienteAsync();
            clientInit.Timeout = TimeSpan.FromMinutes(2);

            var initPayload = new
            {
                filename = fileInfo.Name,
                fileSize = fileInfo.Length,
                totalChunks = totalChunks,
                mimeType = "application/octet-stream"
            };

            var initResponse = await clientInit.PostAsJsonAsync("files/upload/init", initPayload);
            var initJson = await initResponse.Content.ReadAsStringAsync();

            if (!initResponse.IsSuccessStatusCode)
            {
                var errorMsg = $"[SERVIDOR] Error iniciando: HTTP {(int)initResponse.StatusCode}";
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                return (false, errorMsg, null);
            }

            var initData = JsonSerializer.Deserialize<ChunkUploadInitResponse>(initJson);
            var uploadId = initData?.Data?.UploadId;

            if (string.IsNullOrEmpty(uploadId))
            {
                historial.Estado = "Error";
                historial.MensajeError = "[SERVIDOR] No se recibió uploadId";
                await GuardarHistorialAsync(historial);
                return (false, historial.MensajeError, null);
            }

            _logger.LogInformation("[CloudSync-Paralelo] Upload ID: {Id}", uploadId);

            // ========== PASO 2: Subir chunks en PARALELO ==========
            var chunksCompletados = new ConcurrentHashSet<int>();
            var errores = new ConcurrentBag<string>();
            long bytesSubidos = 0;

            var semaphore = new SemaphoreSlim(maxParalelo);
            var tasks = new List<Task>();

            for (int i = 0; i < totalChunks; i++)
            {
                int chunkIndex = i;
                var task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var resultado = await SubirChunkConReintentosAsync(
                            uploadId, rutaArchivo, chunkIndex, chunkSize, fileInfo.Length, MAX_RETRIES);

                        if (resultado.Success)
                        {
                            chunksCompletados.Add(chunkIndex);
                            Interlocked.Add(ref bytesSubidos, resultado.BytesEnviados);

                            ReportProgress(
                                $"Subiendo... {chunksCompletados.Count}/{totalChunks} chunks ({bytesSubidos * 100 / fileInfo.Length}%)",
                                bytesSubidos);
                        }
                        else
                        {
                            errores.Add($"Chunk {chunkIndex}: {resultado.Error}");
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Verificar que todos los chunks se subieron
            if (chunksCompletados.Count != totalChunks)
            {
                var faltantes = totalChunks - chunksCompletados.Count;
                var errorMsg = $"[ERROR] Faltan {faltantes} chunks. Errores: {string.Join("; ", errores.Take(3))}";
                _logger.LogError("[CloudSync-Paralelo] {Error}", errorMsg);
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                return (false, errorMsg, null);
            }

            // ========== PASO 3: Completar subida ==========
            ReportProgress("Ensamblando archivo en servidor", fileInfo.Length);
            _logger.LogInformation("[CloudSync-Paralelo] Completando upload...");

            using var clientComplete = await CrearClienteAsync();
            clientComplete.Timeout = TimeSpan.FromMinutes(5);

            var completeResponse = await clientComplete.PostAsync($"files/upload/complete/{uploadId}", null);
            var completeJson = await completeResponse.Content.ReadAsStringAsync();

            historial.FechaFin = DateTime.Now;
            historial.DuracionSegundos = (int)(historial.FechaFin.Value - historial.FechaInicio).TotalSeconds;

            if (completeResponse.IsSuccessStatusCode)
            {
                var completeData = JsonSerializer.Deserialize<ChunkUploadCompleteResponse>(completeJson);
                var fileId = completeData?.Data?.File?.Id ?? completeData?.File?.Id;
                var filePath = completeData?.Data?.File?.Path ?? completeData?.File?.Path;

                historial.Estado = "Completado";
                historial.IdArchivoRemoto = fileId;
                historial.RutaRemota = filePath;

                await GuardarHistorialAsync(historial);
                await ActualizarUltimaSincronizacionAsync(true, "Completado");

                var velocidad = historial.DuracionSegundos > 0 
                    ? fileInfo.Length / 1024.0 / 1024.0 / historial.DuracionSegundos 
                    : 0;

                ReportProgress("Completado", fileInfo.Length);
                _logger.LogInformation(
                    "[CloudSync-Paralelo] ✓ Subido en {Seg}s ({Vel:F2} MB/s) usando {Paralelo} conexiones",
                    historial.DuracionSegundos, velocidad, maxParalelo);

                return (true, $"Subido en {historial.DuracionSegundos}s ({velocidad:F2} MB/s)", fileId);
            }
            else
            {
                var errorMsg = $"[SERVIDOR] Error completando: HTTP {(int)completeResponse.StatusCode}";
                historial.Estado = "Error";
                historial.MensajeError = errorMsg;
                await GuardarHistorialAsync(historial);
                return (false, errorMsg, null);
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"[ERROR] {ex.Message}";
            historial.FechaFin = DateTime.Now;
            historial.Estado = "Error";
            historial.MensajeError = errorMsg;
            await GuardarHistorialAsync(historial);
            _logger.LogError(ex, "[CloudSync-Paralelo] Error: {Error}", errorMsg);
            return (false, errorMsg, null);
        }
    }

    /// <summary>
    /// Sube un chunk individual con reintentos automáticos
    /// </summary>
    private async Task<ChunkResult> SubirChunkConReintentosAsync(
        string uploadId,
        string rutaArchivo,
        int chunkIndex,
        int chunkSize,
        long totalSize,
        int maxRetries)
    {
        for (int intento = 1; intento <= maxRetries; intento++)
        {
            try
            {
                var offset = (long)chunkIndex * chunkSize;
                var size = (int)Math.Min(chunkSize, totalSize - offset);

                // Leer chunk del archivo
                var buffer = new byte[size];
                await using (var fs = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    await fs.ReadAsync(buffer.AsMemory(0, size));
                }

                // Crear cliente HTTP para este chunk
                using var client = await CrearClienteAsync();
                client.Timeout = TimeSpan.FromMinutes(3);

                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(buffer), "chunk", "chunk");
                content.Add(new StringContent(chunkIndex.ToString()), "chunkIndex");

                var response = await client.PostAsync($"files/upload/chunk/{uploadId}", content);

                if (response.IsSuccessStatusCode)
                {
                    return new ChunkResult { Success = true, ChunkIndex = chunkIndex, BytesEnviados = size };
                }

                var errorJson = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "[CloudSync-Paralelo] Chunk {Index} intento {Intento}/{Max} falló: HTTP {Status}",
                    chunkIndex, intento, maxRetries, (int)response.StatusCode);

                if (intento < maxRetries)
                    await Task.Delay(1000 * intento); // Backoff exponencial
            }
            catch (Exception ex) when (intento < maxRetries)
            {
                _logger.LogWarning(ex, "[CloudSync-Paralelo] Chunk {Index} intento {Intento} excepción", chunkIndex, intento);
                await Task.Delay(1000 * intento);
            }
        }

        return new ChunkResult { Success = false, ChunkIndex = chunkIndex, Error = "Max reintentos alcanzados" };
    }

    /// <summary>
    /// Diagnostica errores HTTP para dar mensajes más claros
    /// </summary>
    private string DiagnosticarErrorHttp(HttpRequestException ex)
    {
        var inner = ex.InnerException;

        // Error de conexión rechazada
        if (inner is SocketException socketEx)
        {
            return socketEx.SocketErrorCode switch
            {
                SocketError.ConnectionRefused => "[RED] Conexión rechazada. El servidor no está disponible o el puerto está cerrado.",
                SocketError.TimedOut => "[RED] Timeout de conexión. El servidor no responde.",
                SocketError.HostNotFound => "[RED] Servidor no encontrado. Verifique la URL.",
                SocketError.NetworkUnreachable => "[RED] Red inalcanzable. Verifique su conexión a internet.",
                _ => $"[RED] Error de socket: {socketEx.Message}"
            };
        }

        // Error SSL
        if (ex.Message.Contains("SSL") || ex.Message.Contains("certificate") || ex.Message.Contains("TLS"))
        {
            return "[SSL] Error de conexión segura. Pruebe cambiar https:// por http:// en la URL.";
        }

        // Error de streaming
        if (ex.Message.Contains("copying content to a stream") || ex.Message.Contains("content stream"))
        {
            return "[RED] Error de transmisión. La conexión se interrumpió durante la subida. Posibles causas: conexión inestable, servidor reiniciado, o firewall.";
        }

        return $"[RED] Error de conexión: {ex.Message}";
    }

    public async Task<(int exitosos, int fallidos, List<string> errores)> SincronizarBackupsAsync(int? idUsuario = null)
    {
        var archivos = await ObtenerArchivosPendientesAsync();
        int exitosos = 0;
        int fallidos = 0;
        var errores = new List<string>();

        foreach (var (ruta, _, _) in archivos)
        {
            var (exito, mensaje, _) = await SubirArchivoAsync(ruta, esAutomatico: idUsuario == null, idUsuario: idUsuario);
            
            if (exito)
                exitosos++;
            else
            {
                fallidos++;
                errores.Add($"{Path.GetFileName(ruta)}: {mensaje}");
            }
        }

        return (exitosos, fallidos, errores);
    }

    public async Task<List<CloudSyncFileInfo>> ListarArchivosRemotosAsync(string? carpeta = null)
    {
        try
        {
            using var client = await CrearClienteAsync();
            var url = string.IsNullOrEmpty(carpeta) ? "files" : $"files?path={Uri.EscapeDataString(carpeta)}";
            
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var listResponse = JsonSerializer.Deserialize<CloudSyncListResponse>(json);
                return listResponse?.Files ?? new List<CloudSyncFileInfo>();
            }

            return new List<CloudSyncFileInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listando archivos remotos");
            return new List<CloudSyncFileInfo>();
        }
    }

    /// <summary>
    /// Verifica si un archivo local ya está sincronizado en la nube
    /// </summary>
    public async Task<(bool existeEnNube, bool esIdentico, CloudSyncFileInfo? archivoRemoto)> VerificarSincronizacionAsync(
        string rutaArchivoLocal)
    {
        try
        {
            if (!File.Exists(rutaArchivoLocal))
                return (false, false, null);

            var nombreArchivo = Path.GetFileName(rutaArchivoLocal);
            var infoLocal = new FileInfo(rutaArchivoLocal);
            
            // Obtener lista de archivos remotos
            var archivosRemotos = await ListarArchivosRemotosAsync();
            
            // Buscar archivo con el mismo nombre
            var archivoRemoto = archivosRemotos.FirstOrDefault(f => 
                string.Equals(f.Name, nombreArchivo, StringComparison.OrdinalIgnoreCase));
            
            if (archivoRemoto == null)
                return (false, false, null);

            // Comparar tamaños para determinar si son idénticos
            bool esIdentico = archivoRemoto.Size == infoLocal.Length;
            
            return (true, esIdentico, archivoRemoto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando sincronización del archivo {Ruta}", rutaArchivoLocal);
            return (false, false, null);
        }
    }

    /// <summary>
    /// Obtiene el estado de sincronización de múltiples archivos
    /// </summary>
    public async Task<List<EstadoSincronizacionArchivo>> ObtenerEstadoSincronizacionAsync(
        List<string> rutasArchivos)
    {
        var resultado = new List<EstadoSincronizacionArchivo>();
        
        try
        {
            // Obtener todos los archivos remotos una sola vez
            var archivosRemotos = await ListarArchivosRemotosAsync();
            var remotosDict = archivosRemotos
                .Where(f => !string.IsNullOrEmpty(f.Name))
                .ToDictionary(f => f.Name!.ToLowerInvariant(), f => f);

            foreach (var ruta in rutasArchivos)
            {
                var estado = new EstadoSincronizacionArchivo
                {
                    RutaLocal = ruta,
                    NombreArchivo = Path.GetFileName(ruta)
                };

                if (File.Exists(ruta))
                {
                    var info = new FileInfo(ruta);
                    estado.TamanoLocal = info.Length;
                    estado.FechaLocal = info.LastWriteTime;

                    // Buscar en remotos
                    var nombreKey = estado.NombreArchivo.ToLowerInvariant();
                    if (remotosDict.TryGetValue(nombreKey, out var remoto))
                    {
                        estado.ExisteEnNube = true;
                        estado.IdRemoto = remoto.Id;
                        estado.TamanoRemoto = remoto.Size;
                        estado.FechaRemota = remoto.UpdatedAt ?? remoto.CreatedAt;
                        
                        // Determinar estado
                        estado.EstaSincronizado = remoto.Size == info.Length;
                        estado.NecesitaActualizar = !estado.EstaSincronizado;
                    }
                }

                resultado.Add(estado);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo estado de sincronización");
        }

        return resultado;
    }

    public async Task<bool> EliminarArchivoRemotoAsync(string idArchivo)
    {
        try
        {
            using var client = await CrearClienteAsync();
            var response = await client.DeleteAsync($"files/{idArchivo}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando archivo remoto {Id}", idArchivo);
            return false;
        }
    }

    public async Task<(bool exito, string? rutaLocal)> DescargarArchivoAsync(string idArchivo, string carpetaDestino)
    {
        try
        {
            using var client = await CrearClienteAsync();
            var response = await client.GetAsync($"files/{idArchivo}/download");
            
            if (response.IsSuccessStatusCode)
            {
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') 
                    ?? $"backup_{idArchivo}";
                var rutaDestino = Path.Combine(carpetaDestino, fileName);

                await using var fileStream = File.Create(rutaDestino);
                await response.Content.CopyToAsync(fileStream);

                return (true, rutaDestino);
            }

            return (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error descargando archivo {Id}", idArchivo);
            return (false, null);
        }
    }

    public async Task<ConfiguracionCloudSync?> ObtenerConfiguracionAsync()
    {
        // Cache de 5 minutos
        if (_configCache != null && (DateTime.Now - _configCacheTime).TotalMinutes < 5)
            return _configCache;

        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            _configCache = await ctx.Set<ConfiguracionCloudSync>()
                .FirstOrDefaultAsync(c => c.Activo);
            _configCacheTime = DateTime.Now;
            return _configCache;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo configuración de CloudSync");
            return null;
        }
    }

    public async Task<bool> GuardarConfiguracionAsync(ConfiguracionCloudSync config)
    {
        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            
            var existente = await ctx.Set<ConfiguracionCloudSync>()
                .FirstOrDefaultAsync(c => c.IdConfiguracion == config.IdConfiguracion);

            if (existente != null)
            {
                existente.ApiKey = config.ApiKey;
                existente.UrlApi = config.UrlApi;
                existente.CarpetaBackup = config.CarpetaBackup;
                existente.ExtensionesIncluir = config.ExtensionesIncluir;
                existente.BackupAutomaticoHabilitado = config.BackupAutomaticoHabilitado;
                existente.HoraBackup = config.HoraBackup;
                existente.DiasBackup = config.DiasBackup;
                existente.RetenerUltimosBackups = config.RetenerUltimosBackups;
                existente.ComprimirAntesDeSubir = config.ComprimirAntesDeSubir;
                existente.FechaModificacion = DateTime.Now;
            }
            else
            {
                config.FechaCreacion = DateTime.Now;
                ctx.Set<ConfiguracionCloudSync>().Add(config);
            }

            await ctx.SaveChangesAsync();
            _configCache = null; // Invalidar cache
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando configuración de CloudSync");
            return false;
        }
    }

    public async Task<List<HistorialBackupCloud>> ObtenerHistorialAsync(int cantidad = 50)
    {
        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            return await ctx.Set<HistorialBackupCloud>()
                .Include(h => h.Usuario)
                .OrderByDescending(h => h.FechaInicio)
                .Take(cantidad)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo historial de backups");
            return new List<HistorialBackupCloud>();
        }
    }

    public async Task<(bool exito, int registrosEliminados)> EliminarHistorialAsync()
    {
        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var registros = await ctx.Set<HistorialBackupCloud>().ToListAsync();
            var cantidad = registros.Count;
            
            if (cantidad > 0)
            {
                ctx.Set<HistorialBackupCloud>().RemoveRange(registros);
                await ctx.SaveChangesAsync();
                _logger.LogInformation("Se eliminaron {Cantidad} registros del historial de backups", cantidad);
            }
            
            return (true, cantidad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando historial de backups");
            return (false, 0);
        }
    }

    public async Task<List<(string ruta, long tamano, DateTime fechaModificacion)>> ObtenerArchivosPendientesAsync()
    {
        var config = await ObtenerConfiguracionAsync();
        if (config == null || string.IsNullOrEmpty(config.CarpetaBackup))
            return new List<(string ruta, long tamano, DateTime fechaModificacion)>();

        if (!Directory.Exists(config.CarpetaBackup))
            return new List<(string ruta, long tamano, DateTime fechaModificacion)>();

        var extensiones = (config.ExtensionesIncluir ?? ".bak,.zip,.sql")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .ToHashSet();

        // Obtener archivos ya subidos
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var archivosSubidos = await ctx.Set<HistorialBackupCloud>()
            .Where(h => h.Estado == "Completado")
            .Select(h => new { h.NombreArchivo, h.HashMD5 })
            .ToListAsync();

        var hashesSubidos = archivosSubidos
            .Where(a => !string.IsNullOrEmpty(a.HashMD5))
            .Select(a => a.HashMD5!)
            .ToHashSet();

        var resultado = new List<(string ruta, long tamano, DateTime fechaModificacion)>();

        foreach (var archivo in Directory.GetFiles(config.CarpetaBackup, "*.*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(archivo).ToLowerInvariant();
            if (!extensiones.Contains(ext))
                continue;

            var info = new FileInfo(archivo);
            
            // Verificar si ya fue subido (por hash si es posible)
            var hash = await CalcularMD5Async(archivo);
            if (hashesSubidos.Contains(hash))
                continue;

            resultado.Add((ruta: archivo, tamano: info.Length, fechaModificacion: info.LastWriteTime));
        }

        return resultado.OrderByDescending(x => x.fechaModificacion).ToList();
    }

    // ========== Métodos privados ==========

    private async Task GuardarHistorialAsync(HistorialBackupCloud historial)
    {
        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            
            if (historial.IdHistorial == 0)
                ctx.Set<HistorialBackupCloud>().Add(historial);
            else
                ctx.Set<HistorialBackupCloud>().Update(historial);

            await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando historial de backup");
        }
    }

    private async Task ActualizarUltimaSincronizacionAsync(bool exito, string mensaje)
    {
        try
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var config = await ctx.Set<ConfiguracionCloudSync>().FirstOrDefaultAsync(c => c.Activo);
            
            if (config != null)
            {
                config.UltimaSincronizacion = DateTime.Now;
                config.EstadoUltimaSincronizacion = exito ? "Exitoso" : "Error";
                config.MensajeUltimaSincronizacion = mensaje;
                await ctx.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando última sincronización");
        }
    }

    private static async Task<string> CalcularMD5Async(string rutaArchivo)
    {
        using var md5 = MD5.Create();
        await using var stream = File.OpenRead(rutaArchivo);
        var hash = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Diagnostica la API CloudSync para detectar endpoints y formato esperado
    /// </summary>
    public async Task<CloudSyncDiagnostico> DiagnosticarApiAsync()
    {
        var diagnostico = new CloudSyncDiagnostico();

        try
        {
            // 1. Verificar conexión básica
            using var client = await CrearClienteAsync();
            
            // Probar endpoint raíz
            try
            {
                var rootResponse = await client.GetAsync("");
                diagnostico.EndpointsEncontrados.Add($"GET / → {(int)rootResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                diagnostico.Errores.Add($"GET / falló: {ex.Message}");
            }

            // 2. Probar diferentes endpoints de upload
            var endpoints = new[] { "files/upload", "upload", "files", "api/upload", "api/files/upload" };
            
            foreach (var endpoint in endpoints)
            {
                try
                {
                    // Hacer un OPTIONS o HEAD para ver si existe
                    var request = new HttpRequestMessage(HttpMethod.Options, endpoint);
                    var optResponse = await client.SendAsync(request);
                    diagnostico.EndpointsEncontrados.Add($"OPTIONS {endpoint} → {(int)optResponse.StatusCode}");
                }
                catch { }
            }

            // 3. Crear archivo de prueba temporal
            var tempFile = Path.Combine(Path.GetTempPath(), $"cloudsync_test_{Guid.NewGuid():N}.txt");
            await File.WriteAllTextAsync(tempFile, "CloudSync diagnostic test file");

            try
            {
                // 4. Probar diferentes nombres de campo para el archivo
                var fieldNames = new[] { "file", "archivo", "files", "backup", "data", "uploadFile" };
                var endpointTests = new[] { "files/upload", "upload" };

                foreach (var endpoint in endpointTests)
                {
                    foreach (var fieldName in fieldNames)
                    {
                        try
                        {
                            using var content = new MultipartFormDataContent();
                            var fileBytes = await File.ReadAllBytesAsync(tempFile);
                            var fileContent = new ByteArrayContent(fileBytes);
                            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            content.Add(fileContent, fieldName, "test.txt");

                            var response = await client.PostAsync(endpoint, content);
                            var responseText = await response.Content.ReadAsStringAsync();
                            
                            var resultado = $"POST {endpoint} [campo={fieldName}] → {(int)response.StatusCode}";
                            
                            if (response.IsSuccessStatusCode)
                            {
                                diagnostico.Conexion = true;
                                diagnostico.EndpointUpload = endpoint;
                                diagnostico.CampoArchivo = fieldName;
                                diagnostico.RespuestaServidor = responseText;
                                resultado += " ✓ FUNCIONA!";
                                
                                // Intentar eliminar el archivo de prueba subido
                                try
                                {
                                    var uploadResponse = JsonSerializer.Deserialize<CloudSyncUploadResponse>(responseText);
                                    if (!string.IsNullOrEmpty(uploadResponse?.File?.Id))
                                    {
                                        await client.DeleteAsync($"files/{uploadResponse.File.Id}");
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                resultado += $": {(responseText.Length > 200 ? responseText[..200] : responseText)}";
                            }

                            diagnostico.EndpointsEncontrados.Add(resultado);

                            // Si encontramos uno que funciona, terminamos
                            if (response.IsSuccessStatusCode)
                                goto EndDiagnostic;
                        }
                        catch (Exception ex)
                        {
                            diagnostico.Errores.Add($"POST {endpoint} [campo={fieldName}]: {ex.Message}");
                        }
                    }
                }

                EndDiagnostic:;
            }
            finally
            {
                // Limpiar archivo temporal
                try { File.Delete(tempFile); } catch { }
            }

            diagnostico.Conexion = !string.IsNullOrEmpty(diagnostico.CampoArchivo);
        }
        catch (Exception ex)
        {
            diagnostico.Errores.Add($"Error general: {ex.Message}");
        }

        return diagnostico;
    }
}

/// <summary>
/// Servicio en segundo plano para backups automáticos programados
/// Soporta: hora fija diaria o intervalo en horas
/// </summary>
public class CloudSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CloudSyncBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public CloudSyncBackgroundService(IServiceProvider services, ILogger<CloudSyncBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("☁️ CloudSync Background Service iniciado");

        // Esperar un poco al inicio para que la aplicación cargue
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        // Actualizar estado inicial del cache
        await ActualizarCacheEstadoAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarYEjecutarBackupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CloudSync Background Service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ActualizarCacheEstadoAsync()
    {
        try
        {
            using var scope = _services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var config = await ctx.ConfiguracionesCloudSync.FirstOrDefaultAsync(c => c.Activo);

            if (config != null)
            {
                BackupStatusCache.Actualizar(
                    config.UltimoBackupExitoso,
                    config.EstadoUltimaSincronizacion,
                    config.EstadoUltimaSincronizacion == "Exitoso"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error actualizando cache de estado de backup");
        }
    }

    private async Task VerificarYEjecutarBackupAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var config = await ctx.ConfiguracionesCloudSync.FirstOrDefaultAsync(c => c.Activo, stoppingToken);
        if (config == null || !config.BackupAutomaticoHabilitado)
            return;

        var ahora = DateTime.Now;
        bool debeEjecutar = false;

        // Tipo de programación: Intervalo (cada X horas) o Horario (hora fija)
        if (config.TipoProgramacion == "Intervalo")
        {
            if (config.UltimoBackupExitoso == null)
            {
                debeEjecutar = true;
            }
            else
            {
                var tiempoTranscurrido = ahora - config.UltimoBackupExitoso.Value;
                debeEjecutar = tiempoTranscurrido.TotalHours >= config.IntervaloHoras;
            }
        }
        else // Horario fijo
        {
            if (config.HoraBackup.HasValue)
            {
                var horaActual = ahora.TimeOfDay;
                var diferencia = Math.Abs((horaActual - config.HoraBackup.Value).TotalMinutes);

                if (diferencia <= 1)
                {
                    // Verificar que no se haya ejecutado hoy
                    if (config.UltimoBackupExitoso == null || config.UltimoBackupExitoso.Value.Date < ahora.Date)
                    {
                        // Verificar día de la semana
                        var diasPermitidos = (config.DiasBackup ?? "1,2,3,4,5")
                            .Split(',')
                            .Select(d => int.TryParse(d.Trim(), out var n) ? n : -1)
                            .Where(n => n >= 0 && n <= 7)
                            .ToList();

                        var diaActual = (int)ahora.DayOfWeek;
                        if (diaActual == 0) diaActual = 7;

                        debeEjecutar = diasPermitidos.Contains(diaActual);
                    }
                }
            }
        }

        if (debeEjecutar)
        {
            _logger.LogInformation("⏰ Iniciando backup programado automático...");
            await EjecutarBackupYSincronizarAsync(scope.ServiceProvider, config, stoppingToken);
        }

        // Actualizar próxima ejecución
        await ActualizarProximaEjecucion(ctx, config, ahora, stoppingToken);
    }

    private async Task EjecutarBackupYSincronizarAsync(
        IServiceProvider services,
        ConfiguracionCloudSync config,
        CancellationToken stoppingToken)
    {
        var backupService = services.GetRequiredService<BackupService>();
        var cloudSyncService = services.GetRequiredService<ICloudSyncService>();
        var ctx = services.GetRequiredService<AppDbContext>();

        try
        {
            // 1. Crear backup
            _logger.LogInformation("📦 Creando backup de base de datos...");
            var resultado = await backupService.CrearBackupParaCloudSync();

            if (!resultado.Success)
            {
                _logger.LogError("❌ Error creando backup: {Error}", resultado.Error);
                await ActualizarEstadoConfig(ctx, config, "Error", $"Error creando backup: {resultado.Error}");
                return;
            }

            _logger.LogInformation("✅ Backup creado: {Path}", resultado.BackupPath);

            // 2. Buscar y eliminar backup existente en la nube
            var archivosRemotos = await cloudSyncService.ListarArchivosRemotosAsync();
            var nombreArchivo = Path.GetFileName(resultado.BackupPath);
            var backupExistente = archivosRemotos?.FirstOrDefault(a =>
                a.Name?.Equals(nombreArchivo, StringComparison.OrdinalIgnoreCase) == true);

            if (backupExistente != null && !string.IsNullOrEmpty(backupExistente.Id))
            {
                _logger.LogInformation("🗑️ Eliminando backup anterior de la nube: {Name}", backupExistente.Name);
                await cloudSyncService.EliminarArchivoRemotoAsync(backupExistente.Id);
            }

            // 3. Subir nuevo backup
            _logger.LogInformation("☁️ Subiendo backup a la nube...");
            var (exito, mensaje, idRemoto) = await cloudSyncService.SubirArchivoParaleloAsync(
                resultado.BackupPath!,
                null  // Sin callback de progreso para backup automático
            );

            if (exito)
            {
                _logger.LogInformation("✅ Backup programado sincronizado exitosamente");

                config.UltimoBackupExitoso = DateTime.Now;
                config.UltimaSincronizacion = DateTime.Now;
                config.EstadoUltimaSincronizacion = "Exitoso";
                config.MensajeUltimaSincronizacion = $"Backup automático: {nombreArchivo}";
                config.FechaModificacion = DateTime.Now;

                await ctx.SaveChangesAsync(stoppingToken);

                // Actualizar cache para el indicador del menú
                BackupStatusCache.Actualizar(config.UltimoBackupExitoso, "Exitoso", true);

                // Registrar en historial
                var fileInfo = new FileInfo(resultado.BackupPath!);
                ctx.HistorialBackupsCloud.Add(new HistorialBackupCloud
                {
                    NombreArchivo = fileInfo.Name,
                    RutaLocal = resultado.BackupPath,
                    RutaRemota = idRemoto,
                    TamanoBytes = fileInfo.Length,
                    FechaInicio = DateTime.Now.AddMinutes(-2),
                    FechaFin = DateTime.Now,
                    DuracionSegundos = 120,
                    Estado = "Completado"
                });
                await ctx.SaveChangesAsync(stoppingToken);
            }
            else
            {
                _logger.LogError("❌ Error subiendo backup: {Error}", mensaje);
                await ActualizarEstadoConfig(ctx, config, "Error", $"Error subiendo: {mensaje}");
                BackupStatusCache.Actualizar(config.UltimoBackupExitoso, "Error", false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en backup programado");
            await ActualizarEstadoConfig(ctx, config, "Error", ex.Message);
            BackupStatusCache.Actualizar(null, "Error", false);
        }
    }

    private async Task ActualizarEstadoConfig(
        AppDbContext ctx,
        ConfiguracionCloudSync config,
        string estado,
        string mensaje)
    {
        config.EstadoUltimaSincronizacion = estado;
        config.MensajeUltimaSincronizacion = mensaje;
        config.FechaModificacion = DateTime.Now;
        await ctx.SaveChangesAsync();
    }

    private async Task ActualizarProximaEjecucion(
        AppDbContext ctx,
        ConfiguracionCloudSync config,
        DateTime ahora,
        CancellationToken stoppingToken)
    {
        DateTime? proxima = null;

        if (config.TipoProgramacion == "Intervalo")
        {
            proxima = config.UltimoBackupExitoso?.AddHours(config.IntervaloHoras) 
                      ?? ahora.AddHours(config.IntervaloHoras);
        }
        else if (config.HoraBackup.HasValue)
        {
            var hoy = ahora.Date.Add(config.HoraBackup.Value);

            if (hoy <= ahora)
            {
                var diasPermitidos = (config.DiasBackup ?? "1,2,3,4,5")
                    .Split(',')
                    .Select(d => int.TryParse(d.Trim(), out var n) ? n : -1)
                    .Where(n => n >= 0 && n <= 7)
                    .OrderBy(n => n)
                    .ToList();

                for (int i = 1; i <= 7; i++)
                {
                    var fecha = ahora.Date.AddDays(i).Add(config.HoraBackup.Value);
                    var dia = (int)fecha.DayOfWeek;
                    if (dia == 0) dia = 7;

                    if (diasPermitidos.Contains(dia))
                    {
                        proxima = fecha;
                        break;
                    }
                }
            }
            else
            {
                proxima = hoy;
            }
        }

        if (proxima != config.ProximaEjecucion)
        {
            config.ProximaEjecucion = proxima;
            await ctx.SaveChangesAsync(stoppingToken);
        }
    }
}

/// <summary>
/// Cache estático para consultar estado del último backup (usado por NavMenu)
/// </summary>
public static class BackupStatusCache
{
    private static DateTime? _ultimoBackup;
    private static string? _estado;
    private static bool _sincronizado;
    private static readonly object _lock = new();

    public static void Actualizar(DateTime? ultimoBackup, string? estado, bool sincronizado)
    {
        lock (_lock)
        {
            _ultimoBackup = ultimoBackup;
            _estado = estado;
            _sincronizado = sincronizado;
        }
    }

    public static (DateTime? UltimoBackup, string? Estado, bool Sincronizado) ObtenerEstado()
    {
        lock (_lock)
        {
            return (_ultimoBackup, _estado, _sincronizado);
        }
    }

    public static bool EstaActualizado(int horasLimite = 24)
    {
        lock (_lock)
        {
            if (!_ultimoBackup.HasValue || !_sincronizado) return false;
            return (DateTime.Now - _ultimoBackup.Value).TotalHours < horasLimite;
        }
    }
}
