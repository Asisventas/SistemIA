using Microsoft.AspNetCore.Mvc;

namespace SistemIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private static readonly Dictionary<string, (string FilePath, DateTime Created)> _pendingDownloads = new();
    private static readonly object _lock = new();
    
    public DownloadController(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    /// <summary>
    /// Registra un archivo para descarga y devuelve un token temporal
    /// </summary>
    public static string RegisterDownload(string filePath)
    {
        var token = Guid.NewGuid().ToString("N");
        lock (_lock)
        {
            // Limpiar tokens expirados (más de 10 minutos)
            var expiredTokens = _pendingDownloads
                .Where(x => (DateTime.Now - x.Value.Created).TotalMinutes > 10)
                .Select(x => x.Key)
                .ToList();
            
            foreach (var expired in expiredTokens)
            {
                // Intentar eliminar archivo temporal
                try
                {
                    if (System.IO.File.Exists(_pendingDownloads[expired].FilePath))
                    {
                        System.IO.File.Delete(_pendingDownloads[expired].FilePath);
                    }
                }
                catch { }
                _pendingDownloads.Remove(expired);
            }
            
            _pendingDownloads[token] = (filePath, DateTime.Now);
        }
        return token;
    }
    
    [HttpGet("{token}")]
    public IActionResult Download(string token)
    {
        string? filePath = null;
        
        lock (_lock)
        {
            if (!_pendingDownloads.TryGetValue(token, out var download))
            {
                return NotFound(new { error = "El enlace de descarga ha expirado o no es válido" });
            }
            
            filePath = download.FilePath;
            _pendingDownloads.Remove(token);
        }
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "El archivo no existe" });
        }
        
        var fileName = Path.GetFileName(filePath);
        var contentType = "application/zip";
        
        // Leer el archivo y devolverlo
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        
        // Eliminar archivo temporal después de leerlo
        try
        {
            System.IO.File.Delete(filePath);
        }
        catch { }
        
        return File(fileBytes, contentType, fileName);
    }
    
    /// <summary>
    /// Endpoint alternativo para streaming de archivos grandes
    /// </summary>
    [HttpGet("stream/{token}")]
    public IActionResult DownloadStream(string token)
    {
        string? filePath = null;
        
        lock (_lock)
        {
            if (!_pendingDownloads.TryGetValue(token, out var download))
            {
                return NotFound(new { error = "El enlace de descarga ha expirado o no es válido" });
            }
            
            filePath = download.FilePath;
            // No eliminar el token inmediatamente para permitir reintentos
            // Se eliminará por timeout (10 minutos) o en la próxima limpieza
        }
        
        if (!System.IO.File.Exists(filePath))
        {
            // Eliminar token si el archivo ya no existe
            lock (_lock)
            {
                _pendingDownloads.Remove(token);
            }
            return NotFound(new { error = "El archivo no existe" });
        }
        
        var fileName = Path.GetFileName(filePath);
        
        // Usar FileStream sin DeleteOnClose para permitir múltiples descargas
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096);
        
        return File(stream, "application/zip", fileName);
    }
}
