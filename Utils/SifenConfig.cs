using System;

namespace SistemIA.Utils
{
    /// <summary>
    /// Configuración centralizada para URLs y endpoints de SIFEN Paraguay
    /// </summary>
    public static class SifenConfig
    {
    // URLs base: usar endpoints de servicio con .wsdl para POST SOAP
        private const string SIFEN_TEST_BASE = "https://sifen-test.set.gov.py";
        private const string SIFEN_PROD_BASE = "https://sifen.set.gov.py";

        // Endpoints de servicio (CON .wsdl para evitar error 0160 "XML Mal Formado")
        // Documentado en SIFEN_SOLUCION_FINAL.md - URLs sin .wsdl causan rechazo
        private const string CONSULTA_RUC_ENDPOINT = "/de/ws/consultas/consulta-ruc.wsdl";
        private const string ENVIO_DE_ENDPOINT = "/de/ws/sync/recibe.wsdl";
        private const string ENVIO_LOTE_ENDPOINT = "/de/ws/async/recibe-lote.wsdl";
        private const string CONSULTA_DE_ENDPOINT = "/de/ws/consultas/consulta.wsdl";
        private const string CONSULTA_LOTE_ENDPOINT = "/de/ws/consultas/consulta-lote.wsdl";
        private const string ENVIO_EVENTO_ENDPOINT = "/de/ws/eventos/evento.wsdl";

        /// <summary>
        /// Obtiene la URL completa para consulta de RUC según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL completa con extensión .wsdl</returns>
        public static string GetConsultaRucUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod" 
                ? SIFEN_PROD_BASE + CONSULTA_RUC_ENDPOINT
                : SIFEN_TEST_BASE + CONSULTA_RUC_ENDPOINT;
        }

        /// <summary>
        /// Obtiene la URL completa para envío de DE según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL completa con extensión .wsdl</returns>
        public static string GetEnvioDeUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod"
                ? SIFEN_PROD_BASE + ENVIO_DE_ENDPOINT
                : SIFEN_TEST_BASE + ENVIO_DE_ENDPOINT;
        }

        /// <summary>
        /// Obtiene la URL completa para envío de Lote según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL completa con extensión .wsdl</returns>
        public static string GetEnvioLoteUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod"
                ? SIFEN_PROD_BASE + ENVIO_LOTE_ENDPOINT
                : SIFEN_TEST_BASE + ENVIO_LOTE_ENDPOINT;
        }

        /// <summary>
        /// Obtiene la URL completa para consulta de DE según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL completa con extensión .wsdl</returns>
        public static string GetConsultaDeUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod"
                ? SIFEN_PROD_BASE + CONSULTA_DE_ENDPOINT
                : SIFEN_TEST_BASE + CONSULTA_DE_ENDPOINT;
        }

        /// <summary>
        /// Obtiene la URL completa para consulta de lote según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL completa con extensión .wsdl</returns>
        public static string GetConsultaLoteUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod"
                ? SIFEN_PROD_BASE + CONSULTA_LOTE_ENDPOINT
                : SIFEN_TEST_BASE + CONSULTA_LOTE_ENDPOINT;
        }

        /// <summary>
        /// Obtiene la URL completa para envío de Eventos (anulaciones, inutilizaciones) según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL completa con extensión .wsdl</returns>
        public static string GetEnvioEventoUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod"
                ? SIFEN_PROD_BASE + ENVIO_EVENTO_ENDPOINT
                : SIFEN_TEST_BASE + ENVIO_EVENTO_ENDPOINT;
        }

        /// <summary>
        /// Obtiene la URL base según el ambiente
        /// </summary>
        /// <param name="ambiente">test o prod</param>
        /// <returns>URL base del ambiente</returns>
        public static string GetBaseUrl(string ambiente)
        {
            return ambiente?.ToLower() == "prod" ? SIFEN_PROD_BASE : SIFEN_TEST_BASE;
        }

        /// <summary>
        /// Lista de todos los endpoints disponibles con extensión .wsdl
        /// </summary>
        public static readonly string[] Endpoints = new[]
        {
            CONSULTA_RUC_ENDPOINT,
            ENVIO_DE_ENDPOINT,
            ENVIO_LOTE_ENDPOINT,
            CONSULTA_DE_ENDPOINT,
            CONSULTA_LOTE_ENDPOINT,
            ENVIO_EVENTO_ENDPOINT
        };
    }
}
