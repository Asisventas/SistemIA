using System;
using System.Threading.Tasks;
using SistemIA.Models;

namespace SistemIA.Utils
{
    /// <summary>
    /// Utilidad para probar conectividad con SIFEN Paraguay
    /// </summary>
    public static class SifenTester
    {
        /// <summary>
        /// Resultado de una prueba de conectividad
        /// </summary>
        public class TestResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public int StatusCode { get; set; }
            public string ResponsePreview { get; set; } = string.Empty;
        }

        /// <summary>
        /// Prueba conectividad básica sin certificado
        /// </summary>
        /// <param name="ambiente">1=test, 2=prod</param>
        /// <returns>Resultado de la prueba</returns>
        public static async Task<TestResult> TestBasicConnectivity(string ambiente)
        {
            try
            {
                var url = SifenConfig.GetConsultaRucUrl(ambiente == "2" ? "prod" : "test");
                
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var response = await client.GetAsync(url);
                
                return new TestResult
                {
                    Success = true,
                    Message = $"Conectividad OK a {url}",
                    Details = $"Status: {response.StatusCode}",
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Success = false,
                    Message = "Error de conectividad básica",
                    Details = ex.Message
                };
            }
        }

        /// <summary>
        /// Prueba conexión completa con certificado y consulta RUC
        /// </summary>
        /// <param name="ambiente">1=test, 2=prod</param>
        /// <param name="certificadoPath">Ruta del certificado .p12</param>
        /// <param name="certificadoPassword">Contraseña del certificado</param>
        /// <param name="rucPrueba">RUC para probar (por defecto 1319270)</param>
        /// <returns>Resultado de la prueba</returns>
        public static async Task<TestResult> TestConnection(string ambiente, string certificadoPath, string certificadoPassword, string rucPrueba = "1319270")
        {
            try
            {
                var url = SifenConfig.GetConsultaRucUrl(ambiente == "2" ? "prod" : "test");
                
                var sifen = new Sifen();
                var resultado = await sifen.Consulta(url, rucPrueba, "1", certificadoPath, certificadoPassword);
                
                if (resultado.Contains("\"error\""))
                {
                    return new TestResult
                    {
                        Success = false,
                        Message = "Error en consulta SIFEN",
                        Details = "La consulta retornó un error",
                        ResponsePreview = resultado.Substring(0, Math.Min(300, resultado.Length))
                    };
                }
                
                // Verificar si contiene datos válidos de SIFEN
                if (resultado.Contains("dRazCons") || resultado.Contains("0502") || resultado.Contains("exitoso"))
                {
                    return new TestResult
                    {
                        Success = true,
                        Message = "✅ Conexión SIFEN exitosa - RUC consultado correctamente",
                        Details = $"URL: {url}",
                        ResponsePreview = resultado.Substring(0, Math.Min(500, resultado.Length))
                    };
                }
                
                return new TestResult
                {
                    Success = false,
                    Message = "Respuesta inesperada de SIFEN",
                    Details = "La respuesta no contiene los datos esperados",
                    ResponsePreview = resultado.Substring(0, Math.Min(300, resultado.Length))
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    Success = false,
                    Message = "Error al probar conexión SIFEN",
                    Details = ex.Message
                };
            }
        }

        /// <summary>
        /// Obtiene información del ambiente según el código
        /// </summary>
        /// <param name="ambienteCodigo">1=test, 2=prod</param>
        /// <returns>Información del ambiente</returns>
        public static string GetAmbienteInfo(string ambienteCodigo)
        {
            return ambienteCodigo == "2" ? "Producción" : "Pruebas";
        }
    }
}
