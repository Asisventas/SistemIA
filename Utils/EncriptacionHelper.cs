using System.Security.Cryptography;
using System.Text;

namespace SistemIA.Utils;

/// <summary>
/// Utilidad para encriptar y desencriptar datos sensibles (contraseñas de conexión, etc.)
/// Usa AES-256 con una clave derivada del nombre de la máquina para vincular al equipo.
/// </summary>
public static class EncriptacionHelper
{
    // Salt fijo para la derivación de clave (no es secreto, pero añade entropía)
    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("SistemIA_2024_v3");
    
    /// <summary>
    /// Genera una clave de encriptación basada en el identificador de la máquina.
    /// Esto vincula los datos encriptados a este equipo específico.
    /// </summary>
    private static byte[] GenerarClave()
    {
        // Usar combinación de datos del equipo para generar clave única
        var baseKey = $"SistemIA-{Environment.MachineName}-{Environment.UserName}";
        
        using var deriveBytes = new Rfc2898DeriveBytes(baseKey, Salt, 10000, HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(32); // 256 bits para AES-256
    }

    /// <summary>
    /// Encripta un texto usando AES-256-CBC
    /// </summary>
    /// <param name="textoPlano">Texto a encriptar</param>
    /// <returns>Texto encriptado en Base64 con IV incluido</returns>
    public static string Encriptar(string textoPlano)
    {
        if (string.IsNullOrEmpty(textoPlano))
            return textoPlano;

        try
        {
            using var aes = Aes.Create();
            aes.Key = GenerarClave();
            aes.GenerateIV(); // IV aleatorio para cada encriptación
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var textoBytes = Encoding.UTF8.GetBytes(textoPlano);
            var encriptado = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);

            // Combinar IV + datos encriptados
            var resultado = new byte[aes.IV.Length + encriptado.Length];
            Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
            Buffer.BlockCopy(encriptado, 0, resultado, aes.IV.Length, encriptado.Length);

            // Retornar con prefijo para identificar que está encriptado
            return "ENC:" + Convert.ToBase64String(resultado);
        }
        catch
        {
            // En caso de error, retornar texto original (mejor que fallar)
            return textoPlano;
        }
    }

    /// <summary>
    /// Desencripta un texto previamente encriptado con Encriptar()
    /// </summary>
    /// <param name="textoEncriptado">Texto encriptado en Base64 con prefijo ENC:</param>
    /// <returns>Texto original desencriptado</returns>
    public static string Desencriptar(string textoEncriptado)
    {
        if (string.IsNullOrEmpty(textoEncriptado))
            return textoEncriptado;

        // Si no tiene el prefijo, no está encriptado
        if (!textoEncriptado.StartsWith("ENC:"))
            return textoEncriptado;

        try
        {
            var datosBase64 = textoEncriptado.Substring(4); // Quitar "ENC:"
            var datos = Convert.FromBase64String(datosBase64);

            using var aes = Aes.Create();
            aes.Key = GenerarClave();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extraer IV (primeros 16 bytes)
            var iv = new byte[16];
            Buffer.BlockCopy(datos, 0, iv, 0, 16);
            aes.IV = iv;

            // Extraer datos encriptados
            var encriptado = new byte[datos.Length - 16];
            Buffer.BlockCopy(datos, 16, encriptado, 0, encriptado.Length);

            using var decryptor = aes.CreateDecryptor();
            var desencriptado = decryptor.TransformFinalBlock(encriptado, 0, encriptado.Length);

            return Encoding.UTF8.GetString(desencriptado);
        }
        catch
        {
            // Si falla la desencriptación, devolver vacío (contraseña inválida)
            return string.Empty;
        }
    }

    /// <summary>
    /// Verifica si un texto está encriptado
    /// </summary>
    public static bool EstaEncriptado(string texto)
    {
        return !string.IsNullOrEmpty(texto) && texto.StartsWith("ENC:");
    }
}
