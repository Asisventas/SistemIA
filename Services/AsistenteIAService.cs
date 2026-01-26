using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemIA.Models;
using SistemIA.Models.AsistenteIA;

namespace SistemIA.Services
{
    public interface IAsistenteIAService
    {
        Task<RespuestaAsistente> ProcesarConsultaAsync(string consulta, int? idUsuario, string? paginaActual = null, List<MensajeChat>? historialConversacion = null);
        Task<ConsejoContextual?> ObtenerConsejoContextualAsync(string modulo, string contexto);
        Task RegistrarErrorAsync(string modulo, string pagina, string mensaje, string? stackTrace, int? idUsuario);
        Task<List<RegistroError>> ObtenerErroresRecientesAsync(int cantidad = 50);
        Task ActualizarBaseConocimientoAsync();
        Task RecargarConocimientoAsync();
        Task RegistrarPreguntaSinRespuestaAsync(string pregunta);
        string ObtenerSaludoPersonalizado(string nombreUsuario);
        Task<List<ConversacionAsistente>> ObtenerHistorialAsync(int? idUsuario, int cantidad = 20);
        Task<bool> EsUsuarioAdminAsync(int? idUsuario);
        Task<(bool exito, string mensaje)> ProcesarComandoAprendizajeAsync(string comando, string contenido, int idUsuario, string nombreUsuario = "Admin");
    }

    /// <summary>
    /// Representa un mensaje en el historial de chat para contexto
    /// </summary>
    public class MensajeChat
    {
        public bool EsUsuario { get; set; }
        public string Texto { get; set; } = string.Empty;
        public DateTime Hora { get; set; }
    }

    public class AsistenteIAService : IAsistenteIAService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AsistenteIAService> _logger;
        private readonly RutasSistemaService _rutasService;
        private BaseConocimiento _conocimiento;
        private readonly string _rutaConocimiento;
        private readonly Random _random = new();

        // Patrones para detectar comandos de aprendizaje (solo admin)
        private readonly string[] _patronesAprendizaje = {
            @"^recuerda\s+(que|esto|lo siguiente)\s*[:\s]?\s*(.+)",
            @"^memoriza\s+(que|esto|lo siguiente)\s*[:\s]?\s*(.+)",
            @"^aprende\s+(que|esto|lo siguiente)\s*[:\s]?\s*(.+)",
            @"^actualiza\s+(tu conocimiento|tu base de datos|que)\s*[:\s]?\s*(.+)",
            @"^guarda\s+(esto|que|lo siguiente)\s*[:\s]?\s*(.+)",
            @"^agrega\s+a\s+tu\s+conocimiento\s*[:\s]?\s*(.+)",
            @"^cuando\s+(?:alguien\s+)?pregunte\s+(.+?)\s*[,:]?\s*(?:responde|di|dile)\s+(.+)",
            @"^modifica\s+(?:el\s+)?(?:articulo|conocimiento)\s*[:\s]?\s*(.+)",
            @"^corrige\s+(?:el\s+)?(?:articulo|conocimiento)\s*[:\s]?\s*(.+)",
            @"^cambia\s+(?:la\s+)?respuesta\s+(?:de|sobre)\s*(.+)"
        };

        // Patrones para detectar preguntas sobre el historial
        private readonly string[] _patronesHistorial = {
            @"que\s+(?:te\s+)?(?:pregunte|dije|hablamos)",
            @"de\s+que\s+(?:hablamos|charlamos)",
            @"recuerdas?\s+(?:lo\s+que|nuestra)",
            @"mi\s+(?:ultima|anterior)\s+(?:pregunta|consulta)",
            @"que\s+(?:hablamos|charlamos)\s+(?:antes|ayer|hoy)",
            @"nuestra\s+(?:conversacion|charla)",
            @"resumen\s+(?:de\s+)?(?:lo\s+que|nuestra)"
        };

        // Saludos personalizados
        private readonly string[] _saludos = {
            "¡Hola {0}! 👋 Soy tu asistente de SistemIA. ¿En qué puedo ayudarte?",
            "¡Buen día {0}! 🌟 Estoy aquí para asistirte con el sistema.",
            "¡Hola {0}! 😊 ¿Qué necesitas saber hoy?",
            "¡Qué tal {0}! 💼 Estoy listo para ayudarte."
        };

        // Respuestas cuando no entiende
        private readonly string[] _respuestasNoEntendido = {
            "Disculpa {0}, no estoy seguro de entender tu consulta. ¿Podrías reformularla?",
            "Hmm {0}, no encontré información sobre eso. ¿Podrías ser más específico?",
            "{0}, no tengo datos sobre ese tema. Intenta preguntar de otra forma.",
            "Lo siento {0}, no pude encontrar una respuesta. ¿Quizás puedo ayudarte con algo más?"
        };

        // Despedidas
        private readonly string[] _despedidas = {
            "¡Hasta luego {0}! Si necesitas algo más, aquí estaré. 👋",
            "¡Éxito con tu trabajo {0}! 🚀",
            "¡Que tengas un excelente día {0}! 😊"
        };

        private readonly ITrackingService? _trackingService;
        private readonly IHubIACentralService? _hubIAService;

        public AsistenteIAService(AppDbContext context, ILogger<AsistenteIAService> logger, RutasSistemaService rutasService, ITrackingService? trackingService = null, IHubIACentralService? hubIAService = null)
        {
            _context = context;
            _logger = logger;
            _rutasService = rutasService;
            _trackingService = trackingService;
            _hubIAService = hubIAService;
            _rutaConocimiento = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Conocimiento", "base_conocimiento.json");
            _conocimiento = CargarBaseConocimiento();
        }

        private BaseConocimiento CargarBaseConocimiento()
        {
            try
            {
                if (File.Exists(_rutaConocimiento))
                {
                    var json = File.ReadAllText(_rutaConocimiento);
                    return JsonSerializer.Deserialize<BaseConocimiento>(json) ?? CrearBaseConocimientoInicial();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al cargar base de conocimiento, usando inicial");
            }
            return CrearBaseConocimientoInicial();
        }

        public string ObtenerSaludoPersonalizado(string nombreUsuario)
        {
            var saludo = _saludos[_random.Next(_saludos.Length)];
            return string.Format(saludo, nombreUsuario);
        }

        public async Task<bool> EsUsuarioAdminAsync(int? idUsuario)
        {
            if (!idUsuario.HasValue) return false;
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id_Usu == idUsuario);
            return usuario?.Id_Rol == 1;
        }

        public async Task<RespuestaAsistente> ProcesarConsultaAsync(string consulta, int? idUsuario, string? paginaActual = null, List<MensajeChat>? historialConversacion = null)
        {
            var nombreUsuario = "Usuario";
            var esAdmin = false;
            if (idUsuario.HasValue)
            {
                var usuario = await _context.Usuarios.FindAsync(idUsuario.Value);
                if (usuario != null)
                {
                    nombreUsuario = usuario.Nombres.Split(' ')[0]; // Solo primer nombre
                    esAdmin = usuario.Id_Rol == 1;
                }
            }

            var consultaLimpia = LimpiarTexto(consulta);
            var respuesta = new RespuestaAsistente();

            // NUEVO: Analizar contexto del usuario desde tracking para personalizar respuestas
            var contextoTracking = AnalizarContextoUsuario(idUsuario);

            // 0. Verificar si es un comando de aprendizaje (solo admin)
            if (esAdmin)
            {
                var comandoAprendizaje = DetectarComandoAprendizaje(consulta);
                if (comandoAprendizaje.esComando)
                {
                    var resultado = await ProcesarComandoAprendizajeAsync(comandoAprendizaje.tipo, comandoAprendizaje.contenido, idUsuario!.Value, nombreUsuario);
                    respuesta.Mensaje = resultado.mensaje;
                    respuesta.Exito = resultado.exito;
                    respuesta.TipoRespuesta = "aprendizaje";
                    await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);
                    return respuesta;
                }
            }

            // 0.5 Verificar si pregunta por historial de conversación
            if (DetectarPreguntaHistorial(consultaLimpia))
            {
                respuesta = await GenerarRespuestaHistorialAsync(idUsuario, nombreUsuario, historialConversacion);
                await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);
                return respuesta;
            }

            // 0.6 NUEVO: Si el usuario dice "ayuda", "tengo problema", "error" etc. y tuvo errores recientes, ofrecer ayuda proactiva
            if (contextoTracking?.TuvoErroresRecientes == true && EsConsultaDeAyudaGeneral(consultaLimpia))
            {
                respuesta.Mensaje = $"Noté que tuviste un problema hace poco, {nombreUsuario}. ";
                if (!string.IsNullOrEmpty(contextoTracking.UltimoError))
                {
                    respuesta.Mensaje += $"El error fue: *\"{contextoTracking.UltimoError}\"*\n\n";
                }
                respuesta.Mensaje += "¿Quieres que te ayude a resolverlo o prefieres contactar al soporte técnico?";
                respuesta.TipoRespuesta = "ayuda_proactiva";
                respuesta.Sugerencias = new List<string> {
                    "Sí, ayúdame con ese error",
                    "Contactar soporte técnico",
                    "No, tengo otra pregunta"
                };
                await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);
                return respuesta;
            }

            // 1. NUEVO: Detectar si es una pregunta de "CÓMO HACER" algo
            // Estas tienen prioridad porque el usuario quiere una GUÍA, no solo navegación
            var guiaEncontrada = BuscarGuiaPasoAPaso(consultaLimpia);
            if (guiaEncontrada != null)
            {
                respuesta.Mensaje = $"¡Claro {nombreUsuario}! {guiaEncontrada.Introduccion}\n\n{guiaEncontrada.Pasos}\n\n💡 **Tip**: {guiaEncontrada.Tip}";
                respuesta.TipoRespuesta = "guia";
                respuesta.RutaNavegacion = guiaEncontrada.Ruta;
                respuesta.Icono = guiaEncontrada.Icono;
                respuesta.Sugerencias = guiaEncontrada.SugerenciasRelacionadas;
                await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);
                return respuesta;
            }

            // 2. Buscar ruta directa usando el escáner inteligente de rutas
            // Solo para navegación simple (sin preguntas de "cómo")
            var rutaEncontrada = _rutasService.BuscarMejorRuta(consultaLimpia);
            if (rutaEncontrada != null)
            {
                respuesta.Mensaje = $"¡Claro {nombreUsuario}! Te llevo a **{rutaEncontrada.Titulo}**.";
                respuesta.TipoRespuesta = "navegacion";
                respuesta.RutaNavegacion = rutaEncontrada.Ruta;
                respuesta.Icono = rutaEncontrada.Icono;
                respuesta.Sugerencias = _rutasService.ObtenerRutas()
                    .Where(r => r.Categoria == rutaEncontrada.Categoria && r.Ruta != rutaEncontrada.Ruta)
                    .Take(3)
                    .Select(r => r.Titulo)
                    .ToList();
                await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);
                return respuesta;
            }

            // 3. Detectar intención para preguntas explicativas (cómo hacer X, qué es Y)
            var intencion = DetectarIntencion(consultaLimpia);

            // 3. Procesar según intención
            if (intencion != null)
            {
                respuesta = await ProcesarIntencionAsync(intencion, consultaLimpia, nombreUsuario, paginaActual);
            }
            else
            {
                // 3. Buscar en artículos de conocimiento (incluyendo contexto de conversación)
                var resultados = BuscarArticulosConContexto(consultaLimpia, historialConversacion);
                
                if (resultados.Any())
                {
                    var mejor = resultados.First();
                    respuesta.Mensaje = $"{nombreUsuario}, {mejor.Articulo!.Contenido}";
                    respuesta.TipoRespuesta = string.IsNullOrEmpty(mejor.Articulo.RutaNavegacion) ? "texto" : "navegacion";
                    respuesta.RutaNavegacion = mejor.Articulo.RutaNavegacion;
                    respuesta.Icono = mejor.Articulo.Icono;
                    
                    // Agregar artículos relacionados
                    respuesta.ArticulosRelacionados = resultados.Skip(1).Take(3)
                        .Select(r => r.Articulo!)
                        .ToList();

                    // Sugerencias basadas en categoría
                    respuesta.Sugerencias = _conocimiento.Articulos
                        .Where(a => a.Categoria == mejor.Articulo.Categoria && a.Id != mejor.Articulo.Id)
                        .Take(3)
                        .Select(a => a.Titulo)
                        .ToList();
                    
                    // Incrementar contador de uso del artículo de BD
                    await IncrementarUsoArticuloAsync(mejor.Articulo.Id);
                }
                else
                {
                    // ========== HUB IA CENTRAL FALLBACK ==========
                    // Si no encontró respuesta local, intentar con el Hub IA Central
                    if (_hubIAService != null)
                    {
                        try
                        {
                            var conexionHub = await _hubIAService.VerificarConexionAsync();
                            if (conexionHub.Conectado && conexionHub.Habilitado)
                            {
                                _logger.LogInformation("[Hub IA] Consultando Hub para: {Consulta}", consulta);
                                
                                var hubRespuesta = await _hubIAService.ConsultarAsync(consulta, null, paginaActual);
                                
                                if (hubRespuesta != null && hubRespuesta.Success && !string.IsNullOrEmpty(hubRespuesta.Respuesta))
                                {
                                    respuesta.Mensaje = $"{nombreUsuario}, {hubRespuesta.Respuesta}";
                                    respuesta.TipoRespuesta = "hub_ia";
                                    respuesta.Exito = true;
                                    
                                    // Indicar que la respuesta vino del Hub
                                    if (hubRespuesta.Fuentes?.Any() == true)
                                    {
                                        respuesta.Mensaje += $"\n\n📡 *Respuesta del Hub IA Central*";
                                        var fuentesTitulos = hubRespuesta.Fuentes
                                            .Where(f => !string.IsNullOrEmpty(f.Titulo))
                                            .Select(f => f.Titulo);
                                        if (fuentesTitulos.Any())
                                        {
                                            respuesta.Mensaje += $"\n_Fuentes: {string.Join(", ", fuentesTitulos)}_";
                                        }
                                    }
                                    
                                    respuesta.Sugerencias = new List<string> {
                                        "¿Cómo crear una venta?",
                                        "¿Cómo funciona SIFEN?",
                                        "¿Cómo ver reportes?"
                                    };
                                    
                                    await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);
                                    return respuesta;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[Hub IA] Error consultando Hub, usando respuesta local");
                        }
                    }
                    // ========== FIN HUB IA CENTRAL FALLBACK ==========
                    
                    respuesta.Mensaje = string.Format(_respuestasNoEntendido[_random.Next(_respuestasNoEntendido.Length)], nombreUsuario);
                    respuesta.Exito = false;
                    
                    // Registrar pregunta sin respuesta para aprendizaje
                    await RegistrarPreguntaSinRespuestaAsync(consulta);
                    
                    // NUEVO: Usar sugerencias contextuales basadas en tracking
                    var sugerenciasContextuales = GenerarSugerenciasContextuales(contextoTracking);
                    if (sugerenciasContextuales.Any())
                    {
                        respuesta.Sugerencias = sugerenciasContextuales;
                    }
                    else
                    {
                        // Sugerencias generales como fallback
                        respuesta.Sugerencias = new List<string> {
                            "¿Cómo crear una venta?",
                            "¿Cómo funciona SIFEN?",
                            "¿Cómo ver reportes?",
                            "¿Cómo agregar un cliente?"
                        };
                    }
                }
            }

            // Guardar conversación
            await GuardarConversacionAsync(idUsuario, nombreUsuario, consulta, respuesta, paginaActual);

            return respuesta;
        }

        /// <summary>
        /// Detecta si la consulta es de tipo "ayuda general" o relacionada a problemas
        /// </summary>
        private bool EsConsultaDeAyudaGeneral(string consultaLimpia)
        {
            var patronesAyuda = new[] {
                "ayuda", "ayudame", "tengo problema", "no puedo", "no funciona",
                "error", "falla", "bug", "no anda", "no me deja", "que pasa",
                "que paso", "que hago", "que hice mal", "me equivoque"
            };
            return patronesAyuda.Any(p => consultaLimpia.Contains(p));
        }

        /// <summary>
        /// Incrementa el contador de uso de un artículo de BD
        /// </summary>
        private async Task IncrementarUsoArticuloAsync(string articuloId)
        {
            try
            {
                // Solo artículos de BD tienen formato "db_123"
                if (articuloId.StartsWith("db_") && int.TryParse(articuloId[3..], out var idArticulo))
                {
                    var articulo = await _context.ArticulosConocimiento.FindAsync(idArticulo);
                    if (articulo != null)
                    {
                        articulo.VecesUtilizado++;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Ignorar errores al incrementar
            }
        }

        private IntencionUsuario? DetectarIntencion(string texto)
        {
            foreach (var intencion in _conocimiento.Intenciones)
            {
                foreach (var patron in intencion.Patrones)
                {
                    if (Regex.IsMatch(texto, patron, RegexOptions.IgnoreCase))
                    {
                        return intencion;
                    }
                }
            }
            return null;
        }

        private async Task<RespuestaAsistente> ProcesarIntencionAsync(IntencionUsuario intencion, string consulta, string nombreUsuario, string? paginaActual)
        {
            var respuesta = new RespuestaAsistente();

            switch (intencion.TipoAccion)
            {
                case "saludo":
                    respuesta.Mensaje = ObtenerSaludoPersonalizado(nombreUsuario);
                    respuesta.TipoRespuesta = "texto";
                    respuesta.Sugerencias = new List<string> {
                        "¿Cómo crear una venta?",
                        "Ver reportes",
                        "¿Qué es SIFEN?",
                        "Agregar cliente"
                    };
                    break;

                case "despedida":
                    respuesta.Mensaje = string.Format(_despedidas[_random.Next(_despedidas.Length)], nombreUsuario);
                    respuesta.TipoRespuesta = "texto";
                    break;

                case "navegacion":
                    var ruta = intencion.AccionParametro ?? ObtenerRutaDeConsulta(consulta);
                    if (!string.IsNullOrEmpty(ruta))
                    {
                        var nombreModulo = ObtenerNombreModulo(ruta);
                        respuesta.Mensaje = $"¡Claro {nombreUsuario}! Te llevo a {nombreModulo}.";
                        respuesta.TipoRespuesta = "navegacion";
                        respuesta.RutaNavegacion = ruta;
                        respuesta.Icono = ObtenerIconoModulo(ruta);
                    }
                    else
                    {
                        respuesta.Mensaje = $"{nombreUsuario}, ¿a qué sección deseas ir?";
                        respuesta.Sugerencias = _conocimiento.RutasModulos.Keys.Take(6).ToList();
                    }
                    break;

                case "informe":
                    respuesta.Mensaje = $"{nombreUsuario}, te muestro los informes disponibles:";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/informes";
                    respuesta.Sugerencias = new List<string> {
                        "Informe de ventas",
                        "Informe de compras",
                        "Stock valorizado",
                        "Cuentas por cobrar"
                    };
                    break;

                case "consejo":
                    var consejo = await ObtenerConsejoContextualAsync(paginaActual ?? "general", "general");
                    if (consejo != null)
                    {
                        respuesta.Mensaje = $"💡 {nombreUsuario}, {consejo.Mensaje}";
                        respuesta.TipoRespuesta = "consejo";
                    }
                    else
                    {
                        respuesta.Mensaje = $"{nombreUsuario}, aquí tienes algunos consejos útiles:";
                        respuesta.Sugerencias = new List<string> {
                            "Usa F2 para buscar productos rápidamente",
                            "Confirma las ventas antes de cerrar caja",
                            "Revisa el stock mínimo regularmente"
                        };
                    }
                    break;

                case "ayuda":
                    respuesta.Mensaje = $"{nombreUsuario}, puedo ayudarte con:\n\n" +
                        "📦 **Inventario**: productos, stock, ajustes\n" +
                        "💰 **Ventas**: facturas, presupuestos, cobros\n" +
                        "🛒 **Compras**: registrar compras, proveedores\n" +
                        "📊 **Informes**: reportes, estadísticas\n" +
                        "⚙️ **Configuración**: sistema, SIFEN, correos\n\n" +
                        "¿Sobre qué tema necesitas información?";
                    respuesta.TipoRespuesta = "texto";
                    break;

                case "explicacion_correo":
                    respuesta.Mensaje = $"{nombreUsuario}, para configurar el **envío automático de correo** debes:\n\n" +
                        "1️⃣ Ir a **Configuración → Correo Electrónico**\n" +
                        "2️⃣ Configurar el servidor SMTP (ej: smtp.gmail.com)\n" +
                        "3️⃣ Ingresar usuario y contraseña del correo\n" +
                        "4️⃣ Agregar destinatarios y seleccionar qué informes enviar\n" +
                        "5️⃣ Activar el envío automático al cierre o programado\n\n" +
                        "💡 Para Gmail necesitas generar una **contraseña de aplicación** desde tu cuenta Google.";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/configuracion/correo";
                    respuesta.Icono = "bi-envelope";
                    respuesta.Sugerencias = new List<string> {
                        "Ir a configuración de correo",
                        "¿Cómo obtengo contraseña de aplicación?",
                        "¿Qué informes puedo enviar?"
                    };
                    break;

                case "explicacion_sifen":
                    respuesta.Mensaje = $"{nombreUsuario}, **SIFEN** es el Sistema de Facturación Electrónica de Paraguay (SET).\n\n" +
                        "Para configurarlo necesitas:\n" +
                        "1️⃣ **Certificado digital** (.pfx) del contribuyente\n" +
                        "2️⃣ **Timbrado electrónico** habilitado por SET\n" +
                        "3️⃣ Configurar ambiente (Test/Producción) en **Datos del Emisor**\n\n" +
                        "📌 El sistema genera el **CDC** automáticamente y firma los documentos XML.";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/configuracion/sociedad";
                    respuesta.Icono = "bi-file-earmark-check";
                    respuesta.Sugerencias = new List<string> {
                        "Ir a Datos del Emisor",
                        "¿Qué es el CDC?",
                        "¿Cómo consultar estado SIFEN?"
                    };
                    break;

                case "explicacion_backup":
                    respuesta.Mensaje = $"{nombreUsuario}, para hacer **backup** de la base de datos:\n\n" +
                        "**Opción 1 - SQL Server Management Studio:**\n" +
                        "1️⃣ Click derecho en BD `asiswebapp`\n" +
                        "2️⃣ Tareas → Copia de seguridad\n" +
                        "3️⃣ Selecciona destino (.bak)\n\n" +
                        "**Opción 2 - Comando SQL:**\n" +
                        "```sql\nBACKUP DATABASE asiswebapp TO DISK = 'C:\\Backups\\asiswebapp.bak'\n```\n\n" +
                        "💡 **Recomendación**: Backup diario y guardar copia externa (nube/disco)";
                    respuesta.TipoRespuesta = "informacion";
                    respuesta.Icono = "bi-hdd";
                    respuesta.Sugerencias = new List<string> { "¿Cómo restaurar backup?", "¿Cómo programar backup automático?" };
                    break;

                case "explicacion_cierre_caja":
                    respuesta.Mensaje = $"{nombreUsuario}, para hacer **cierre de caja**:\n\n" +
                        "1️⃣ Ve a **Ventas → Cierre de Caja**\n" +
                        "2️⃣ Verifica que todas las ventas estén confirmadas\n" +
                        "3️⃣ Revisa el resumen de operaciones\n" +
                        "4️⃣ Ingresa el **efectivo contado** físicamente\n" +
                        "5️⃣ El sistema calcula la diferencia\n" +
                        "6️⃣ **Confirma** el cierre\n\n" +
                        "💡 Cierra caja al final de cada turno";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/caja/cierre";
                    respuesta.Icono = "bi-cash-stack";
                    respuesta.Sugerencias = new List<string> { "Ir a Cierre de Caja", "Ver historial de cierres" };
                    break;

                case "explicacion_nota_credito":
                    respuesta.Mensaje = $"{nombreUsuario}, para crear una **Nota de Crédito** (devolución):\n\n" +
                        "1️⃣ Ve a **Ventas → Notas de Crédito**\n" +
                        "2️⃣ Click en **Nueva NC**\n" +
                        "3️⃣ Busca la **factura original**\n" +
                        "4️⃣ Selecciona productos a devolver\n" +
                        "5️⃣ Indica el motivo\n" +
                        "6️⃣ **Confirma** la NC\n\n" +
                        "💡 El stock se restaura automáticamente";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/notas-credito";
                    respuesta.Icono = "bi-receipt-cutoff";
                    respuesta.Sugerencias = new List<string> { "Ir a Notas de Crédito", "¿Cómo anular una venta?" };
                    break;

                case "explicacion_ajuste_stock":
                    respuesta.Mensaje = $"{nombreUsuario}, para **ajustar stock**:\n\n" +
                        "1️⃣ Ve a **Inventario → Ajustes de Stock**\n" +
                        "2️⃣ Selecciona el depósito\n" +
                        "3️⃣ Busca el producto\n" +
                        "4️⃣ Ingresa cantidad nueva o ajuste (+/-)\n" +
                        "5️⃣ Selecciona motivo (merma, inventario físico, etc.)\n" +
                        "6️⃣ **Confirma**\n\n" +
                        "📊 Ver historial en: Informes → Ajustes de Stock";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/inventario/ajustes";
                    respuesta.Icono = "bi-box-seam";
                    respuesta.Sugerencias = new List<string> { "Ir a Ajustes de Stock", "Ver movimientos de inventario" };
                    break;

                case "explicacion_cuentas_cobrar":
                    respuesta.Mensaje = $"{nombreUsuario}, para ver **cuentas por cobrar**:\n\n" +
                        "1️⃣ Ve a **Ventas → Cuentas por Cobrar** o\n" +
                        "2️⃣ **Informes → Cuentas por Cobrar**\n\n" +
                        "Verás:\n" +
                        "• Total adeudado por cliente\n" +
                        "• Cuotas pendientes con vencimientos\n" +
                        "• Días de atraso\n\n" +
                        "💰 Para cobrar: click en el cliente → Registrar Cobro";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/cobros";
                    respuesta.Icono = "bi-currency-dollar";
                    respuesta.Sugerencias = new List<string> { "Ir a Cobros", "Ver informe de morosos" };
                    break;

                case "explicacion_cuentas_pagar":
                    respuesta.Mensaje = $"{nombreUsuario}, para ver **cuentas por pagar**:\n\n" +
                        "1️⃣ Ve a **Compras → Pagos a Proveedores** o\n" +
                        "2️⃣ **Informes → Cuentas por Pagar**\n\n" +
                        "Para pagar:\n" +
                        "• Selecciona proveedor\n" +
                        "• Elige facturas a pagar\n" +
                        "• Ingresa monto y forma de pago\n" +
                        "• Confirma el pago";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/pagos-proveedores";
                    respuesta.Icono = "bi-cash-coin";
                    respuesta.Sugerencias = new List<string> { "Ir a Pagos a Proveedores", "Ver deudas pendientes" };
                    break;

                case "explicacion_usuario":
                    respuesta.Mensaje = $"{nombreUsuario}, para **gestionar usuarios**:\n\n" +
                        "**Crear usuario:**\n" +
                        "1️⃣ Ve a **Personal → Gestión de Usuarios**\n" +
                        "2️⃣ Click en Nuevo Usuario\n" +
                        "3️⃣ Completa datos y asigna Rol\n\n" +
                        "**Configurar permisos:**\n" +
                        "1️⃣ Ve a **Personal → Permisos de Usuarios**\n" +
                        "2️⃣ Selecciona usuario o rol\n" +
                        "3️⃣ Marca/desmarca permisos por módulo";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/menu-usuarios";
                    respuesta.Icono = "bi-person-plus";
                    respuesta.Sugerencias = new List<string> { "Ir a Usuarios", "Configurar permisos" };
                    break;

                case "explicacion_actualizacion":
                    respuesta.Mensaje = $"{nombreUsuario}, para **actualizar el sistema**:\n\n" +
                        "1️⃣ Ve a **Configuración → Actualización Sistema**\n" +
                        "2️⃣ Click en Buscar Actualizaciones\n" +
                        "3️⃣ Si hay versión nueva:\n" +
                        "   • Revisa notas de versión\n" +
                        "   • ⚠️ Haz backup primero\n" +
                        "   • Click en Descargar e Instalar\n" +
                        "4️⃣ Reinicia la aplicación\n\n" +
                        "💡 Actualiza fuera de horario pico";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/actualizacion-sistema";
                    respuesta.Icono = "bi-cloud-download";
                    respuesta.Sugerencias = new List<string> { "Ir a Actualización", "¿Cómo hacer backup?" };
                    break;

                case "explicacion_presupuesto":
                    respuesta.Mensaje = $"{nombreUsuario}, para crear un **presupuesto**:\n\n" +
                        "1️⃣ Ve a **Ventas → Presupuestos**\n" +
                        "2️⃣ Click en Nuevo Presupuesto\n" +
                        "3️⃣ Selecciona cliente\n" +
                        "4️⃣ Agrega productos con precios\n" +
                        "5️⃣ Define validez (días)\n" +
                        "6️⃣ Guarda\n\n" +
                        "**Opciones:** Convertir a Venta | Enviar por correo | Imprimir\n\n" +
                        "💡 Los presupuestos no afectan stock ni son fiscales";
                    respuesta.TipoRespuesta = "navegacion";
                    respuesta.RutaNavegacion = "/presupuestos/explorar";
                    respuesta.Icono = "bi-file-earmark-text";
                    respuesta.Sugerencias = new List<string> { "Ir a Presupuestos", "¿Cómo convertir a venta?" };
                    break;

                default:
                    if (intencion.RespuestasPosibles.Any())
                    {
                        var respuestaTexto = intencion.RespuestasPosibles[_random.Next(intencion.RespuestasPosibles.Count)];
                        respuesta.Mensaje = string.Format(respuestaTexto, nombreUsuario);
                    }
                    break;
            }

            return respuesta;
        }

        private List<ResultadoBusqueda> BuscarArticulos(string consulta)
        {
            var palabrasConsulta = consulta.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Length > 2)
                .ToList();

            var resultados = new List<ResultadoBusqueda>();

            foreach (var articulo in _conocimiento.Articulos)
            {
                double puntuacion = 0;
                var razones = new List<string>();

                // Coincidencia en título (peso alto)
                var tituloLimpio = LimpiarTexto(articulo.Titulo);
                foreach (var palabra in palabrasConsulta)
                {
                    if (tituloLimpio.Contains(palabra))
                    {
                        puntuacion += 10;
                        razones.Add($"título contiene '{palabra}'");
                    }
                }

                // Coincidencia en palabras clave (peso alto)
                foreach (var keyword in articulo.PalabrasClave)
                {
                    var keyLimpio = LimpiarTexto(keyword);
                    foreach (var palabra in palabrasConsulta)
                    {
                        if (keyLimpio.Contains(palabra) || palabra.Contains(keyLimpio))
                        {
                            puntuacion += 8;
                            razones.Add($"palabra clave '{keyword}'");
                        }
                    }
                }

                // Coincidencia en sinónimos
                foreach (var sinonimo in articulo.Sinonimos)
                {
                    var sinLimpio = LimpiarTexto(sinonimo);
                    foreach (var palabra in palabrasConsulta)
                    {
                        if (sinLimpio.Contains(palabra) || palabra.Contains(sinLimpio))
                        {
                            puntuacion += 6;
                            razones.Add($"sinónimo '{sinonimo}'");
                        }
                    }
                }

                // Coincidencia en contenido (peso bajo)
                var contenidoLimpio = LimpiarTexto(articulo.Contenido);
                foreach (var palabra in palabrasConsulta)
                {
                    if (contenidoLimpio.Contains(palabra))
                    {
                        puntuacion += 2;
                    }
                }

                // Bonificación por prioridad
                puntuacion += articulo.Prioridad * 0.5;

                if (puntuacion > 3)
                {
                    resultados.Add(new ResultadoBusqueda
                    {
                        Articulo = articulo,
                        Puntuacion = puntuacion,
                        RazonCoincidencia = string.Join(", ", razones.Distinct().Take(3))
                    });
                }
            }

            return resultados.OrderByDescending(r => r.Puntuacion).Take(5).ToList();
        }

        /// <summary>
        /// Búsqueda mejorada que considera el contexto de la conversación
        /// </summary>
        private List<ResultadoBusqueda> BuscarArticulosConContexto(string consulta, List<MensajeChat>? historialConversacion)
        {
            // Primero buscar normalmente
            var resultados = BuscarArticulos(consulta);
            
            // Si hay historial, usar contexto para mejorar búsqueda
            if (historialConversacion?.Any() == true && resultados.Count == 0)
            {
                // Extraer palabras clave del historial reciente (últimos 5 mensajes)
                var mensajesRecientes = historialConversacion
                    .Where(m => m.EsUsuario)
                    .TakeLast(5)
                    .Select(m => m.Texto)
                    .ToList();

                var palabrasContexto = mensajesRecientes
                    .SelectMany(m => LimpiarTexto(m).Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .Where(p => p.Length > 3)
                    .GroupBy(p => p)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList();

                // Buscar con palabras del contexto combinadas
                var consultaExtendida = $"{consulta} {string.Join(" ", palabrasContexto)}";
                resultados = BuscarArticulos(LimpiarTexto(consultaExtendida));
            }

            return resultados;
        }

        /// <summary>
        /// Detecta si el usuario está preguntando por su historial de conversación
        /// </summary>
        private bool DetectarPreguntaHistorial(string consultaLimpia)
        {
            foreach (var patron in _patronesHistorial)
            {
                if (Regex.IsMatch(consultaLimpia, patron, RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Genera respuesta basada en el historial de conversación
        /// </summary>
        private async Task<RespuestaAsistente> GenerarRespuestaHistorialAsync(int? idUsuario, string nombreUsuario, List<MensajeChat>? historialLocal)
        {
            var respuesta = new RespuestaAsistente();

            // Obtener historial de la BD
            var historialBD = await ObtenerHistorialAsync(idUsuario, 10);
            
            if (historialBD.Any() || (historialLocal?.Any() == true))
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"📜 {nombreUsuario}, aquí está un resumen de nuestra conversación reciente:\n");

                if (historialBD.Any())
                {
                    foreach (var conv in historialBD.Take(5))
                    {
                        sb.AppendLine($"**Tú ({conv.Fecha:dd/MM HH:mm})**: {TruncarTexto(conv.Pregunta, 50)}");
                    }
                }
                else if (historialLocal?.Any() == true)
                {
                    foreach (var msg in historialLocal.Where(m => m.EsUsuario).TakeLast(5))
                    {
                        sb.AppendLine($"**Tú ({msg.Hora:HH:mm})**: {TruncarTexto(msg.Texto, 50)}");
                    }
                }

                sb.AppendLine("\n¿Te gustaría que profundice en alguno de estos temas?");
                
                respuesta.Mensaje = sb.ToString();
                respuesta.TipoRespuesta = "historial";
                respuesta.Exito = true;
            }
            else
            {
                respuesta.Mensaje = $"{nombreUsuario}, esta es nuestra primera conversación. ¡Pregúntame lo que necesites! 😊";
                respuesta.TipoRespuesta = "texto";
                respuesta.Exito = true;
            }

            return respuesta;
        }

        private string TruncarTexto(string texto, int maxLength)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Length <= maxLength ? texto : texto[..maxLength] + "...";
        }

        /// <summary>
        /// Detecta si el mensaje es un comando de aprendizaje (solo para admin)
        /// </summary>
        private (bool esComando, string tipo, string contenido) DetectarComandoAprendizaje(string consulta)
        {
            foreach (var patron in _patronesAprendizaje)
            {
                var match = Regex.Match(consulta, patron, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var contenido = match.Groups.Count > 2 
                        ? $"{match.Groups[1].Value}: {match.Groups[2].Value}"
                        : match.Groups[match.Groups.Count - 1].Value;
                    
                    return (true, "memorizar", contenido.Trim());
                }
            }
            return (false, "", "");
        }

        /// <summary>
        /// Procesa un comando de aprendizaje del admin
        /// </summary>
        public async Task<(bool exito, string mensaje)> ProcesarComandoAprendizajeAsync(string comando, string contenido, int idUsuario, string nombreUsuario = "Admin")
        {
            try
            {
                // Verificar que es admin
                if (!await EsUsuarioAdminAsync(idUsuario))
                {
                    return (false, $"⚠️ {nombreUsuario}, solo los administradores pueden enseñarme cosas nuevas.");
                }

                // Extraer pregunta y respuesta si el formato es "cuando pregunten X responde Y"
                string titulo;
                string respuesta;
                var matchPreguntaRespuesta = Regex.Match(contenido, @"(.+?)\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
                
                if (matchPreguntaRespuesta.Success)
                {
                    titulo = matchPreguntaRespuesta.Groups[1].Value.Trim();
                    respuesta = matchPreguntaRespuesta.Groups[2].Value.Trim();
                }
                else
                {
                    // Si no tiene formato pregunta:respuesta, usar todo como contenido
                    titulo = $"Información aprendida - {DateTime.Now:dd/MM/yyyy}";
                    respuesta = contenido;
                }

                // Buscar si ya existe un artículo similar
                var articuloExistente = await BuscarArticuloSimilarAsync(titulo, respuesta);
                
                if (articuloExistente != null)
                {
                    // Actualizar artículo existente
                    articuloExistente.Contenido = respuesta;
                    articuloExistente.FechaActualizacion = DateTime.Now;
                    articuloExistente.VecesUtilizado++;
                    
                    // Agregar palabras clave nuevas
                    var palabrasClaveNuevas = ExtraerPalabrasClave(titulo + " " + respuesta);
                    if (!string.IsNullOrEmpty(articuloExistente.PalabrasClave))
                    {
                        var existentes = articuloExistente.PalabrasClave.Split(',').ToHashSet();
                        var nuevas = palabrasClaveNuevas.Split(',');
                        foreach (var n in nuevas)
                        {
                            existentes.Add(n);
                        }
                        articuloExistente.PalabrasClave = string.Join(",", existentes.Take(20));
                    }
                    else
                    {
                        articuloExistente.PalabrasClave = palabrasClaveNuevas;
                    }

                    await _context.SaveChangesAsync();
                    await RecargarConocimientoAsync();

                    _logger.LogInformation("Conocimiento actualizado: {Titulo} por usuario {IdUsuario}", articuloExistente.Titulo, idUsuario);

                    return (true, $"🔄 ¡Excelente {nombreUsuario}! He actualizado el artículo existente:\n\n" +
                                  $"**Título**: {articuloExistente.Titulo}\n" +
                                  $"**Nuevo contenido**: {TruncarTexto(respuesta, 100)}\n\n" +
                                  $"El conocimiento ha sido actualizado. 🧠");
                }

                // Crear artículo nuevo
                var nuevoArticulo = new ArticuloConocimientoDB
                {
                    Titulo = titulo,
                    Contenido = respuesta,
                    Categoria = "Aprendido",
                    Subcategoria = "Aprendizaje Directo",
                    PalabrasClave = ExtraerPalabrasClave(titulo + " " + respuesta),
                    Prioridad = 8, // Alta prioridad para conocimiento del admin
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                    IdUsuarioCreador = idUsuario
                };

                _context.ArticulosConocimiento.Add(nuevoArticulo);
                await _context.SaveChangesAsync();

                // Recargar conocimiento
                await RecargarConocimientoAsync();

                _logger.LogInformation("Nuevo conocimiento aprendido: {Titulo} por usuario {IdUsuario}", titulo, idUsuario);

                return (true, $"✅ ¡Perfecto {nombreUsuario}! He guardado:\n\n" +
                              $"**Título**: {titulo}\n" +
                              $"**Contenido**: {TruncarTexto(respuesta, 100)}\n\n" +
                              $"Ahora podré responder preguntas relacionadas. 🧠");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar comando de aprendizaje");
                return (false, $"❌ {nombreUsuario}, hubo un error al guardar el conocimiento. Por favor intenta de nuevo.");
            }
        }

        /// <summary>
        /// Extrae palabras clave de un texto
        /// </summary>
        private string ExtraerPalabrasClave(string texto)
        {
            var palabrasComunes = new HashSet<string> {
                "el", "la", "los", "las", "un", "una", "unos", "unas", "de", "del", "al",
                "en", "con", "por", "para", "que", "se", "es", "son", "como", "cuando",
                "donde", "quien", "cual", "esto", "eso", "aquello", "mi", "tu", "su",
                "este", "esta", "estos", "estas", "ese", "esa", "esos", "esas"
            };

            var palabras = LimpiarTexto(texto)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Length > 3 && !palabrasComunes.Contains(p))
                .Distinct()
                .Take(10);

            return string.Join(",", palabras);
        }

        /// <summary>
        /// Analiza el contexto del usuario basándose en su historial de tracking.
        /// Retorna información útil para personalizar respuestas.
        /// </summary>
        private ContextoUsuarioTracking? AnalizarContextoUsuario(int? idUsuario)
        {
            if (_trackingService == null || !idUsuario.HasValue)
                return null;

            try
            {
                var acciones = _trackingService.ObtenerAccionesRecientes(idUsuario: idUsuario, cantidad: 30);
                var errores = _trackingService.ObtenerErroresRecientes(idUsuario: idUsuario, cantidad: 10);

                if (!acciones.Any() && !errores.Any())
                    return null;

                var contexto = new ContextoUsuarioTracking();

                // Detectar última página visitada
                var ultimaNavegacion = acciones
                    .Where(a => a.TipoAccion == TipoAccionTracking.Navegacion && !string.IsNullOrEmpty(a.Ruta))
                    .OrderByDescending(a => a.FechaHora)
                    .FirstOrDefault();
                
                if (ultimaNavegacion != null)
                {
                    contexto.UltimaPagina = ultimaNavegacion.Ruta;
                    contexto.CategoriaActual = ultimaNavegacion.Categoria;
                }

                // Detectar si hubo errores recientes (posible frustración del usuario)
                contexto.TuvoErroresRecientes = errores.Any(e => e.FechaHora > DateTime.Now.AddMinutes(-10));
                if (contexto.TuvoErroresRecientes)
                {
                    contexto.UltimoError = errores
                        .Where(e => e.FechaHora > DateTime.Now.AddMinutes(-10))
                        .OrderByDescending(e => e.FechaHora)
                        .FirstOrDefault()?.MensajeError;
                }

                // Detectar patrón de navegación (dónde pasó más tiempo)
                var frecuenciaModulos = acciones
                    .Where(a => !string.IsNullOrEmpty(a.Categoria))
                    .GroupBy(a => a.Categoria)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key!)
                    .Take(3)
                    .ToList();
                
                contexto.ModulosMasUsados = frecuenciaModulos;

                // Detectar si está en un flujo específico (ej: creando venta, haciendo cierre)
                var accionesRecientes = acciones
                    .Where(a => a.FechaHora > DateTime.Now.AddMinutes(-5))
                    .ToList();
                
                if (accionesRecientes.Any())
                {
                    // Detectar flujo de ventas
                    if (accionesRecientes.Any(a => a.Ruta?.Contains("/ventas") == true))
                        contexto.FlujoActivo = "ventas";
                    else if (accionesRecientes.Any(a => a.Ruta?.Contains("/compras") == true))
                        contexto.FlujoActivo = "compras";
                    else if (accionesRecientes.Any(a => a.Ruta?.Contains("/cierre") == true))
                        contexto.FlujoActivo = "cierre_caja";
                    else if (accionesRecientes.Any(a => a.Ruta?.Contains("/notas-credito") == true))
                        contexto.FlujoActivo = "nota_credito";
                    else if (accionesRecientes.Any(a => a.Ruta?.Contains("/inventario") == true || a.Ruta?.Contains("/productos") == true))
                        contexto.FlujoActivo = "inventario";
                }

                // Detectar última operación realizada
                var ultimaOperacion = acciones
                    .Where(a => a.TipoAccion == TipoAccionTracking.Operacion || 
                               a.TipoAccion == TipoAccionTracking.Creacion ||
                               a.TipoAccion == TipoAccionTracking.Edicion)
                    .OrderByDescending(a => a.FechaHora)
                    .FirstOrDefault();
                
                if (ultimaOperacion != null)
                {
                    contexto.UltimaOperacion = ultimaOperacion.Descripcion;
                    contexto.UltimaOperacionTipo = ultimaOperacion.TipoAccion;
                }

                // Detectar consultas previas a la IA (qué estaba preguntando)
                var consultasIA = acciones
                    .Where(a => a.TipoAccion == TipoAccionTracking.ConsultaIA)
                    .OrderByDescending(a => a.FechaHora)
                    .Take(5)
                    .ToList();
                
                contexto.ConsultasIAPrevias = consultasIA.Select(c => c.Descripcion).ToList();

                return contexto;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al analizar contexto de tracking del usuario");
                return null;
            }
        }

        /// <summary>
        /// Genera sugerencias personalizadas basadas en el contexto del usuario
        /// </summary>
        private List<string> GenerarSugerenciasContextuales(ContextoUsuarioTracking? contexto)
        {
            var sugerencias = new List<string>();
            
            if (contexto == null)
                return sugerencias;

            // Si tuvo errores, sugerir ayuda relacionada
            if (contexto.TuvoErroresRecientes)
            {
                sugerencias.Add("¿Tuviste algún problema?");
                sugerencias.Add("Reportar problema al soporte");
            }

            // Sugerencias según el flujo activo
            switch (contexto.FlujoActivo)
            {
                case "ventas":
                    sugerencias.Add("¿Cómo agregar descuento?");
                    sugerencias.Add("¿Cómo imprimir la factura?");
                    sugerencias.Add("¿Cómo agregar cliente nuevo?");
                    break;
                case "compras":
                    sugerencias.Add("¿Cómo registrar un pago?");
                    sugerencias.Add("¿Cómo ver mis cuentas por pagar?");
                    break;
                case "cierre_caja":
                    sugerencias.Add("¿Cómo corregir un arqueo?");
                    sugerencias.Add("¿Qué hacer si falta efectivo?");
                    break;
                case "nota_credito":
                    sugerencias.Add("¿Cómo vincular a una factura?");
                    sugerencias.Add("¿Qué pasa con el stock?");
                    break;
                case "inventario":
                    sugerencias.Add("¿Cómo hacer ajuste de stock?");
                    sugerencias.Add("¿Cómo transferir entre depósitos?");
                    break;
            }

            return sugerencias.Take(4).ToList();
        }

        /// <summary>
        /// Genera respuesta contextual cuando el usuario parece tener problemas
        /// </summary>
        private string? GenerarMensajeContextual(ContextoUsuarioTracking? contexto, string nombreUsuario)
        {
            if (contexto == null)
                return null;

            // Si tuvo errores recientes, ofrecer ayuda proactiva
            if (contexto.TuvoErroresRecientes && !string.IsNullOrEmpty(contexto.UltimoError))
            {
                return $"Noté que tuviste un problema hace poco. ¿Puedo ayudarte con eso, {nombreUsuario}?";
            }

            // Si está en un módulo específico, ofrecer ayuda contextual
            if (!string.IsNullOrEmpty(contexto.FlujoActivo))
            {
                return contexto.FlujoActivo switch
                {
                    "ventas" => $"Veo que estás trabajando en ventas, {nombreUsuario}. ¿En qué te puedo ayudar?",
                    "compras" => $"Veo que estás en compras, {nombreUsuario}. ¿Necesitas ayuda con algo?",
                    "cierre_caja" => $"Veo que estás haciendo un cierre de caja, {nombreUsuario}. ¿Alguna duda?",
                    _ => null
                };
            }

            return null;
        }

        /// <summary>
        /// Clase para almacenar el contexto analizado del usuario
        /// </summary>
        private class ContextoUsuarioTracking
        {
            public string? UltimaPagina { get; set; }
            public string? CategoriaActual { get; set; }
            public bool TuvoErroresRecientes { get; set; }
            public string? UltimoError { get; set; }
            public List<string> ModulosMasUsados { get; set; } = new();
            public string? FlujoActivo { get; set; }
            public string? UltimaOperacion { get; set; }
            public string? UltimaOperacionTipo { get; set; }
            public List<string> ConsultasIAPrevias { get; set; } = new();
        }

        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            
            texto = texto.ToLowerInvariant();
            texto = Regex.Replace(texto, @"[áàäâ]", "a");
            texto = Regex.Replace(texto, @"[éèëê]", "e");
            texto = Regex.Replace(texto, @"[íìïî]", "i");
            texto = Regex.Replace(texto, @"[óòöô]", "o");
            texto = Regex.Replace(texto, @"[úùüû]", "u");
            texto = Regex.Replace(texto, @"[ñ]", "n");
            texto = Regex.Replace(texto, @"[^\w\s]", " ");
            texto = Regex.Replace(texto, @"\s+", " ");
            
            return texto.Trim();
        }

        /// <summary>
        /// Busca un artículo similar en la BD para actualizarlo en vez de crear uno nuevo
        /// </summary>
        private async Task<ArticuloConocimientoDB?> BuscarArticuloSimilarAsync(string titulo, string contenido)
        {
            var tituloLimpio = LimpiarTexto(titulo);
            var palabrasClave = tituloLimpio.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Length > 3)
                .Take(5)
                .ToList();

            if (!palabrasClave.Any())
                return null;

            // Buscar artículos con título similar
            var articulos = await _context.ArticulosConocimiento
                .Where(a => a.Activo)
                .ToListAsync();

            // Calcular similitud
            foreach (var articulo in articulos)
            {
                var tituloArticulo = LimpiarTexto(articulo.Titulo);
                var coincidencias = palabrasClave.Count(p => tituloArticulo.Contains(p));
                
                // Si coinciden más del 60% de las palabras clave
                if (coincidencias >= Math.Ceiling(palabrasClave.Count * 0.6))
                {
                    return articulo;
                }
            }

            return null;
        }

        private string? ObtenerRutaDeConsulta(string consulta)
        {
            foreach (var (nombre, ruta) in _conocimiento.RutasModulos)
            {
                if (consulta.Contains(LimpiarTexto(nombre)))
                {
                    return ruta;
                }
            }
            return null;
        }

        private string ObtenerNombreModulo(string ruta)
        {
            // Buscar coincidencia exacta primero
            var nombre = _conocimiento.RutasModulos
                .FirstOrDefault(r => r.Value == ruta).Key;
            
            if (!string.IsNullOrEmpty(nombre))
                return nombre;
            
            // Mapeo directo para rutas comunes
            var nombresAmigables = new Dictionary<string, string>
            {
                { "/configuracion/correo", "Configuración de Correo" },
                { "/configuracion/sociedad", "Datos de la Empresa" },
                { "/configuracion/cajas", "Configuración de Cajas" },
                { "/configuracion/tipos-pago", "Tipos de Pago" },
                { "/menu-usuarios", "Gestión de Usuarios" },
                { "/personal/permisos-usuarios", "Permisos de Usuarios" },
                { "/informes", "Centro de Informes" },
                { "/inventario/ajustes", "Ajustes de Stock" },
                { "/inventario/depositos", "Depósitos" },
                { "/caja/cierre", "Cierre de Caja" },
                { "/actualizacion-sistema", "Actualización del Sistema" },
                { "/ventas", "Ventas" },
                { "/compras", "Compras" },
                { "/productos", "Productos" },
                { "/clientes", "Clientes" },
                { "/notas-credito", "Notas de Crédito" },
                { "/cobros", "Cobros" },
                { "/pagos-proveedores", "Pagos a Proveedores" },
                { "/presupuestos/explorar", "Presupuestos" }
            };
            
            return nombresAmigables.GetValueOrDefault(ruta) ?? ruta.TrimStart('/').Replace("/", " → ");
        }

        private string ObtenerIconoModulo(string ruta)
        {
            var iconos = new Dictionary<string, string>
            {
                { "/ventas", "bi-cart" },
                { "/compras", "bi-bag" },
                { "/productos", "bi-box" },
                { "/clientes", "bi-people" },
                { "/proveedores", "bi-truck" },
                { "/caja", "bi-cash-stack" },
                { "/informes", "bi-graph-up" },
                { "/configuracion", "bi-gear" }
            };

            return iconos.FirstOrDefault(i => ruta.StartsWith(i.Key)).Value ?? "bi-arrow-right";
        }

        public async Task<ConsejoContextual?> ObtenerConsejoContextualAsync(string modulo, string contexto)
        {
            var consejos = _conocimiento.Consejos
                .Where(c => c.Activo && 
                           (c.Modulo == modulo || c.Modulo == "general") &&
                           (c.Contexto == contexto || c.Contexto == "general"))
                .ToList();

            if (!consejos.Any()) return null;

            return consejos[_random.Next(consejos.Count)];
        }

        public async Task RegistrarErrorAsync(string modulo, string pagina, string mensaje, string? stackTrace, int? idUsuario)
        {
            try
            {
                var nombreUsuario = "Sistema";
                if (idUsuario.HasValue)
                {
                    var usuario = await _context.Usuarios.FindAsync(idUsuario.Value);
                    nombreUsuario = usuario?.UsuarioNombre ?? "Desconocido";
                }

                var error = new RegistroError
                {
                    Fecha = DateTime.Now,
                    Modulo = modulo,
                    Pagina = pagina,
                    MensajeError = mensaje,
                    StackTrace = stackTrace,
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario,
                    TipoError = ClasificarError(mensaje)
                };

                _context.Set<RegistroError>().Add(error);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar error en base de datos");
            }
        }

        private string ClasificarError(string mensaje)
        {
            mensaje = mensaje.ToLowerInvariant();
            
            if (mensaje.Contains("null") || mensaje.Contains("reference"))
                return "NullReference";
            if (mensaje.Contains("database") || mensaje.Contains("sql") || mensaje.Contains("connection"))
                return "Database";
            if (mensaje.Contains("timeout"))
                return "Timeout";
            if (mensaje.Contains("permission") || mensaje.Contains("unauthorized"))
                return "Permiso";
            if (mensaje.Contains("validation") || mensaje.Contains("invalid"))
                return "Validacion";
            if (mensaje.Contains("network") || mensaje.Contains("http"))
                return "Red";
            
            return "General";
        }

        public async Task<List<RegistroError>> ObtenerErroresRecientesAsync(int cantidad = 50)
        {
            return await _context.Set<RegistroError>()
                .OrderByDescending(e => e.Fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task ActualizarBaseConocimientoAsync()
        {
            _conocimiento = CargarBaseConocimiento();
            await CargarConocimientoDesdeBDAsync();
        }

        /// <summary>
        /// Recarga el conocimiento desde JSON y BD (para cuando el admin hace cambios)
        /// </summary>
        public async Task RecargarConocimientoAsync()
        {
            _conocimiento = CargarBaseConocimiento();
            await CargarConocimientoDesdeBDAsync();
            _logger.LogInformation("Base de conocimiento recargada: {Count} artículos totales", _conocimiento.Articulos.Count);
        }

        /// <summary>
        /// Carga artículos de conocimiento desde la BD y los combina con el JSON
        /// </summary>
        private async Task CargarConocimientoDesdeBDAsync()
        {
            try
            {
                var articulosDB = await _context.ArticulosConocimiento
                    .Where(a => a.Activo)
                    .ToListAsync();

                // Convertir artículos de BD a formato de conocimiento
                foreach (var artDB in articulosDB)
                {
                    var articuloConv = artDB.ToArticuloConocimiento();
                    
                    // Verificar si ya existe uno similar (evitar duplicados)
                    var existente = _conocimiento.Articulos.FirstOrDefault(a => 
                        a.Titulo.Equals(artDB.Titulo, StringComparison.OrdinalIgnoreCase));
                    
                    if (existente == null)
                    {
                        _conocimiento.Articulos.Add(articuloConv);
                    }
                    else
                    {
                        // Actualizar el existente si el de BD tiene mayor prioridad
                        if (artDB.Prioridad > existente.Prioridad)
                        {
                            existente.Contenido = artDB.Contenido;
                            existente.PalabrasClave = artDB.ObtenerPalabrasClave();
                            existente.Sinonimos = artDB.ObtenerSinonimos();
                            existente.RutaNavegacion = artDB.RutaNavegacion;
                            existente.Icono = artDB.Icono;
                            existente.Prioridad = artDB.Prioridad;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al cargar conocimiento desde BD, usando solo JSON");
            }
        }

        /// <summary>
        /// Registra una pregunta que no tuvo buena respuesta para que el admin pueda crear conocimiento
        /// </summary>
        public async Task RegistrarPreguntaSinRespuestaAsync(string pregunta)
        {
            try
            {
                var preguntaLimpia = pregunta.Trim();
                if (string.IsNullOrWhiteSpace(preguntaLimpia) || preguntaLimpia.Length < 5)
                    return;

                // Buscar si ya existe una similar
                var existente = await _context.PreguntasSinRespuesta
                    .FirstOrDefaultAsync(p => p.Pregunta.ToLower().Contains(preguntaLimpia.ToLower().Substring(0, Math.Min(20, preguntaLimpia.Length))));

                if (existente != null)
                {
                    existente.CantidadVeces++;
                    existente.UltimaVez = DateTime.Now;
                }
                else
                {
                    _context.PreguntasSinRespuesta.Add(new PreguntaSinRespuesta
                    {
                        Pregunta = preguntaLimpia,
                        CantidadVeces = 1,
                        PrimeraVez = DateTime.Now,
                        UltimaVez = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar pregunta sin respuesta");
            }
        }

        private async Task GuardarConversacionAsync(int? idUsuario, string nombreUsuario, string pregunta, RespuestaAsistente respuesta, string? paginaOrigen)
        {
            try
            {
                var conversacion = new ConversacionAsistente
                {
                    Fecha = DateTime.Now,
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario,
                    Pregunta = pregunta,
                    Respuesta = respuesta.Mensaje,
                    TipoIntencion = respuesta.TipoRespuesta,
                    Util = respuesta.Exito,
                    PaginaOrigen = paginaOrigen
                };

                _context.Set<ConversacionAsistente>().Add(conversacion);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al guardar conversación del asistente");
            }
        }

        /// <summary>
        /// Obtiene el historial de conversaciones del asistente.
        /// IMPORTANTE: Las conversaciones son PRIVADAS por usuario.
        /// Si idUsuario es null, devuelve lista vacía por seguridad.
        /// </summary>
        public async Task<List<ConversacionAsistente>> ObtenerHistorialAsync(int? idUsuario, int cantidad = 20)
        {
            // SEGURIDAD: Si no hay usuario identificado, no mostrar historial de nadie
            if (!idUsuario.HasValue)
            {
                _logger.LogDebug("ObtenerHistorialAsync llamado sin idUsuario - devolviendo lista vacía por seguridad");
                return new List<ConversacionAsistente>();
            }
            
            return await _context.Set<ConversacionAsistente>()
                .Where(c => c.IdUsuario == idUsuario.Value)  // Solo conversaciones del usuario actual
                .OrderByDescending(c => c.Fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        private BaseConocimiento CrearBaseConocimientoInicial()
        {
            return new BaseConocimiento
            {
                Version = "1.0.0",
                FechaActualizacion = DateTime.Now,
                Intenciones = CrearIntencionesIniciales(),
                RutasModulos = CrearRutasModulos()
            };
        }

        private List<IntencionUsuario> CrearIntencionesIniciales()
        {
            return new List<IntencionUsuario>
            {
                new() {
                    Nombre = "saludo",
                    TipoAccion = "saludo",
                    Patrones = new() { @"^hola", @"^buen(os|as)?", @"^hey", @"^que tal", @"^saludos" }
                },
                new() {
                    Nombre = "despedida",
                    TipoAccion = "despedida",
                    Patrones = new() { @"^adios", @"^chau", @"^hasta luego", @"^nos vemos", @"^bye" }
                },
                new() {
                    Nombre = "ayuda",
                    TipoAccion = "ayuda",
                    Patrones = new() { @"ayuda", @"help", @"que puedes hacer", @"como funciona" }
                },
                new() {
                    Nombre = "navegacion_ventas",
                    TipoAccion = "navegacion",
                    AccionParametro = "/ventas",
                    Patrones = new() { @"ir a ventas", @"abrir ventas", @"mostrar ventas", @"crear venta", @"nueva venta" }
                },
                new() {
                    Nombre = "navegacion_compras",
                    TipoAccion = "navegacion",
                    AccionParametro = "/compras",
                    Patrones = new() { @"ir a compras", @"abrir compras", @"mostrar compras", @"registrar compra" }
                },
                new() {
                    Nombre = "navegacion_productos",
                    TipoAccion = "navegacion",
                    AccionParametro = "/productos",
                    Patrones = new() { @"ir a productos", @"ver productos", @"buscar producto", @"inventario" }
                },
                new() {
                    Nombre = "navegacion_clientes",
                    TipoAccion = "navegacion",
                    AccionParametro = "/clientes",
                    Patrones = new() { @"ir a clientes", @"ver clientes", @"buscar cliente", @"agregar cliente" }
                },
                new() {
                    Nombre = "navegacion_caja",
                    TipoAccion = "navegacion",
                    AccionParametro = "/caja",
                    Patrones = new() { @"ir a caja", @"abrir caja", @"cerrar caja", @"cierre de caja" }
                },
                new() {
                    Nombre = "ver_informes",
                    TipoAccion = "informe",
                    Patrones = new() { @"ver informe", @"generar reporte", @"mostrar estadistica", @"reportes?" }
                },
                new() {
                    Nombre = "pedir_consejo",
                    TipoAccion = "consejo",
                    Patrones = new() { @"dame un consejo", @"tienes alguna sugerencia", @"recomendacion", @"tip" }
                },
                // === CONFIGURACIONES ESPECÍFICAS (solo preguntas explicativas: cómo, qué) ===
                new() {
                    Nombre = "configurar_correo",
                    TipoAccion = "explicacion_correo",
                    Patrones = new() { @"como.+configur.+correo", @"como.+smtp", @"como.+envio.+automatico", @"que.+es.+smtp", @"pasos.+correo" }
                },
                new() {
                    Nombre = "configurar_sifen",
                    TipoAccion = "explicacion_sifen",
                    Patrones = new() { @"como.+sifen", @"como.+factura.+electronica", @"que.+es.+sifen", @"como.+timbrado", @"como.+certificado", @"pasos.+sifen" }
                },
                // NOTA: Las navegaciones directas ahora las maneja RutasSistemaService automáticamente
                // === FUNCIONALIDADES DEL SISTEMA ===
                new() {
                    Nombre = "backup",
                    TipoAccion = "explicacion_backup",
                    Patrones = new() { @"backup", @"respaldo", @"copia.+seguridad", @"respaldar", @"guardar.+datos", @"restaurar" }
                },
                new() {
                    Nombre = "cierre_caja",
                    TipoAccion = "explicacion_cierre_caja",
                    Patrones = new() { @"cierre.+caja", @"cerrar.+caja", @"arqueo", @"cuadrar.+caja", @"diferencia.+caja" }
                },
                new() {
                    Nombre = "nota_credito",
                    TipoAccion = "explicacion_nota_credito",
                    Patrones = new() { @"nota.+credito", @"devolucion", @"devolver.+producto", @"nc" }
                },
                new() {
                    Nombre = "ajuste_stock",
                    TipoAccion = "explicacion_ajuste_stock",
                    Patrones = new() { @"ajust.+stock", @"ajust.+inventario", @"modific.+stock", @"merma", @"inventario.+fisico" }
                },
                new() {
                    Nombre = "cuentas_cobrar",
                    TipoAccion = "explicacion_cuentas_cobrar",
                    Patrones = new() { @"cuentas?.+cobrar", @"deuda.+cliente", @"credito.+cliente", @"moroso", @"cobr.+cuota" }
                },
                new() {
                    Nombre = "cuentas_pagar",
                    TipoAccion = "explicacion_cuentas_pagar",
                    Patrones = new() { @"cuentas?.+pagar", @"deuda.+proveedor", @"pag.+proveedor" }
                },
                new() {
                    Nombre = "crear_usuario",
                    TipoAccion = "explicacion_usuario",
                    Patrones = new() { @"crear.+usuario", @"nuevo.+usuario", @"agregar.+usuario", @"permiso.+usuario" }
                },
                new() {
                    Nombre = "actualizacion",
                    TipoAccion = "explicacion_actualizacion",
                    Patrones = new() { @"actualizar.+sistema", @"nueva.+version", @"update", @"actualizacion" }
                },
                new() {
                    Nombre = "presupuesto",
                    TipoAccion = "explicacion_presupuesto",
                    Patrones = new() { @"presupuesto", @"cotizacion", @"proforma" }
                }
            };
        }

        private Dictionary<string, string> CrearRutasModulos()
        {
            return new Dictionary<string, string>
            {
                // === INICIO ===
                { "Inicio", "/" },
                { "Panel de Control", "/" },
                { "Dashboard", "/" },
                
                // === PRODUCTOS ===
                { "Productos", "/productos" },
                { "Administrar Productos", "/productos" },
                { "Nuevo Producto", "/productos" },
                
                // === VENTAS ===
                { "Ventas", "/ventas" },
                { "Nueva Venta", "/ventas" },
                { "Realizar Venta", "/ventas" },
                { "Explorar Ventas", "/ventas/explorar" },
                { "Explorador de Ventas", "/ventas/explorar" },
                { "Presupuestos", "/presupuestos/explorar" },
                { "Explorar Presupuestos", "/presupuestos/explorar" },
                { "Nota de Crédito", "/notas-credito" },
                { "Notas de Crédito", "/notas-credito" },
                { "Explorar NC", "/notas-credito/explorar" },
                { "Explorar Notas de Crédito", "/notas-credito/explorar" },
                { "Cierre de Caja", "/caja/cierre" },
                { "Cerrar Caja", "/caja/cierre" },
                { "Historial Cierres", "/caja/historial-cierres" },
                { "Historial de Cierres", "/caja/historial-cierres" },
                
                // === COMPRAS ===
                { "Compras", "/compras" },
                { "Nueva Compra", "/compras" },
                { "Registrar Compra", "/compras" },
                { "Explorar Compras", "/compras/explorar" },
                { "Explorador de Compras", "/compras/explorar" },
                { "NC Compra", "/notas-credito-compra" },
                { "Nueva NC Compra", "/notas-credito-compra" },
                { "Nota de Crédito Compra", "/notas-credito-compra" },
                { "Explorar NC Compras", "/notas-credito-compra/explorar" },
                
                // === CLIENTES ===
                { "Clientes", "/clientes/explorar" },
                { "Explorar Clientes", "/clientes/explorar" },
                { "Cuentas por Cobrar", "/cobros" },
                { "Cobros", "/cobros" },
                { "Historial de Cobros", "/cobros/listado" },
                
                // === PROVEEDORES ===
                { "Proveedores", "/proveedores/explorar" },
                { "Explorar Proveedores", "/proveedores/explorar" },
                { "Pagos a Proveedores", "/pagos-proveedores" },
                { "Historial de Pagos", "/pagos-proveedores/historial" },
                { "Cuentas por Pagar", "/informes/cuentas-por-pagar" },
                
                // === INFORMES DE VENTAS ===
                { "Informes", "/informes/ventas-agrupado" },
                { "Ventas Agrupado", "/informes/ventas-agrupado" },
                { "Informe Ventas Agrupado", "/informes/ventas-agrupado" },
                { "Ventas Detallado", "/informes/ventas-detallado" },
                { "Informe Ventas Detallado", "/informes/ventas-detallado" },
                { "Ventas por Clasificación", "/informes/ventas-clasificacion" },
                { "Resumen de Caja", "/informes/resumen-caja" },
                
                // === INFORMES NC ===
                { "Informe NC Agrupado", "/informes/nc-agrupado" },
                { "Informe NC Detallado", "/informes/nc-detallado" },
                
                // === INFORMES DE COMPRAS ===
                { "Compras Agrupado", "/informes/compras-general" },
                { "Informe Compras", "/informes/compras-general" },
                { "Compras Detallado", "/informes/compras-detallado" },
                { "NC Compras Agrupado", "/informes/nc-compras-agrupado" },
                { "NC Compras Detallado", "/informes/nc-compras-detallado" },
                
                // === INFORMES DE PRODUCTOS ===
                { "Productos Detallado", "/informes/productos-detallado" },
                { "Stock Valorizado", "/informes/productos-valorizado" },
                { "Listado Valorizado", "/informes/productos-valorizado" },
                { "Movimientos Inventario", "/informes/movimientos-productos" },
                { "Movimientos de Stock", "/informes/movimientos-productos" },
                { "Ajustes de Stock", "/informes/ajustes-stock" },
                { "Informe Ajustes Stock", "/informes/ajustes-stock" },
                
                // === INFORMES DE CLIENTES ===
                { "Listado de Clientes", "/informes/clientes" },
                { "Listado Cobro Clientes", "/informes/cuentas-por-cobrar" },
                
                // === INFORMES DE PROVEEDORES ===
                { "Listado de Proveedores", "/informes/proveedores" },
                { "Listado Pago Proveedores", "/informes/cuentas-por-pagar" },
                
                // === INFORMES DE PERSONAL ===
                { "Informe de Asistencia", "/informes-asistencia" },
                { "Listado de Asistencia", "/listado-asistencia" },
                
                // === GESTIÓN DE PERSONAL ===
                { "Usuarios", "/menu-usuarios" },
                { "Gestión de Usuarios", "/menu-usuarios" },
                { "Asistencia", "/registro-asistencia" },
                { "Registro Asistencia", "/registro-asistencia" },
                { "Registro Directo", "/registro-directo" },
                { "Horarios", "/horarios" },
                { "Asignar Horarios", "/asignacionhorarios" },
                { "Permisos de Usuarios", "/personal/permisos-usuarios" },
                
                // === INVENTARIO ===
                { "Depósitos", "/inventario/depositos" },
                { "Inventario Depósitos", "/inventario/depositos" },
                { "Ajustes Stock", "/inventario/ajustes" },
                { "Nuevo Ajuste Stock", "/inventario/ajustes" },
                { "Transferencias", "/inventario/transferencias" },
                { "Transferencia Stock", "/inventario/transferencias" },
                { "Cambiar Sucursal", "/seleccionar-sucursal" },
                { "Explorar Ajustes", "/inventario/ajustes/explorar" },
                
                // === CONFIGURACIÓN - EMPRESA ===
                { "Sucursales", "/sucursales" },
                { "Gestión Sucursales", "/sucursales" },
                { "Sociedad", "/configuracion/sociedad" },
                { "Datos Emisor", "/configuracion/sociedad" },
                { "Cajas", "/configuracion/cajas" },
                { "Configuración Cajas", "/configuracion/cajas" },
                
                // === CONFIGURACIÓN - CATÁLOGOS ===
                { "Tipos de Pago", "/configuracion/tipos-pago" },
                { "Tipos de Documento", "/configuracion/tipos-documento" },
                { "Tipos de IVA", "/configuracion/tipos-iva" },
                { "Marcas y Clasificaciones", "/configuracion/marcas-clasificaciones" },
                { "Precios y Descuentos", "/configuracion/precios-descuentos" },
                
                // === CONFIGURACIÓN - SISTEMA ===
                { "Configuración General", "/configuracion-sistema" },
                { "Configuración Sistema", "/configuracion-sistema" },
                { "Correo Electrónico", "/configuracion/correo" },
                { "Configuración Correo", "/configuracion/correo" },
                { "Tema", "/configuracion/tema" },
                { "Configuración Tema", "/configuracion/tema" },
                { "Auditoría", "/configuracion/auditoria" },
                { "Actualización", "/actualizacion-sistema" },
                { "Actualización Sistema", "/actualizacion-sistema" },
                { "Manual del Sistema", "/manual-sistema" },
                { "Ayuda", "/manual-sistema" },
                
                // === ASISTENTE IA ===
                { "Asistente IA", "/asistente-ia" },
                { "Admin Asistente IA", "/admin/asistente-ia" },
                { "Administrar Asistente", "/admin/asistente-ia" },
                
                // === DESARROLLO ===
                { "Pruebas XML SIFEN", "/pruebas-xml" },
                { "Generar Instalador", "/admin/instalador" }
            };
        }

        // ========== SISTEMA DE GUÍAS PASO A PASO ==========
        
        /// <summary>
        /// Busca si la consulta es una pregunta de "cómo hacer" y devuelve la guía correspondiente
        /// </summary>
        private GuiaPasoAPaso? BuscarGuiaPasoAPaso(string consulta)
        {
            var consultaLower = consulta.ToLower()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n");
            
            // ========== PASO 1: Detectar INTENCIÓN ==========
            // ¿El usuario quiere una GUÍA/TUTORIAL o solo NAVEGAR?
            var indicadoresGuia = new[] { 
                "como ", "cómo ", "pasos", "paso a paso", "tutorial", "proceso", 
                "manera de", "forma de", "puedo ", "hago ", "hacer ", "realizo", "realizar",
                "creo ", "crear ", "guia", "guía", "necesito", "ayuda con", "ayuda para",
                "explicar", "explica", "enseña", "instrucciones", "indicame", "dime como",
                "quiero ", "quisiera ", "cuales son", "cuáles son", "contactar", "contacto con",
                "se hace", "se realiza", "se crea", "se registra", "se genera"
            };
            
            bool esConsultaDeGuia = indicadoresGuia.Any(ind => consultaLower.Contains(ind));
            
            if (!esConsultaDeGuia)
            {
                _logger.LogInformation("[BuscarGuia] No es consulta de guía: '{Consulta}'", consulta);
                return null;
            }
            
            // ========== PASO 2: Extraer TEMA/ENTIDAD ==========
            // Identificar DE QUÉ está hablando el usuario
            var temasDetectados = new List<(string tema, int peso)>();
            
            // Diccionario de temas y sus palabras clave asociadas
            var diccionarioTemas = new Dictionary<string, string[]>
            {
                // Módulos principales
                ["venta"] = new[] { "venta", "ventas", "factura", "facturar", "facturacion", "vender" },
                ["producto"] = new[] { "producto", "productos", "articulo", "item", "mercaderia" },
                ["cliente"] = new[] { "cliente", "clientes", "consumidor" },
                ["compra"] = new[] { "compra", "compras", "comprar", "adquisicion" },
                ["proveedor"] = new[] { "proveedor", "proveedores", "vendedor" },
                ["presupuesto"] = new[] { "presupuesto", "presupuestos", "cotizacion", "cotizar" },
                ["nota_credito"] = new[] { "nota de credito", "nota credito", "nc", "devolucion", "devolver", "anular venta" },
                ["cobro"] = new[] { "cobro", "cobros", "cobrar", "cobranza", "cuenta por cobrar", "credito cliente" },
                ["pago"] = new[] { "pago", "pagos", "pagar", "cuenta por pagar", "deuda proveedor" },
                
                // Inventario
                ["stock"] = new[] { "stock", "inventario", "existencia", "existencias" },
                ["ajuste_stock"] = new[] { "ajuste", "ajustar stock", "corregir stock", "ajuste inventario" },
                ["transferencia"] = new[] { "transferencia", "transferir", "mover stock", "trasladar" },
                ["deposito"] = new[] { "deposito", "depositos", "almacen", "bodega" },
                
                // Caja
                ["cierre_caja"] = new[] { "cierre", "cerrar caja", "arqueo", "cierre de caja", "cuadre" },
                ["apertura_caja"] = new[] { "apertura", "abrir caja", "iniciar turno" },
                ["caja"] = new[] { "caja", "resumen caja", "movimiento caja", "estado caja" },
                
                // Configuración
                ["usuario"] = new[] { "usuario", "usuarios", "empleado", "personal", "operador" },
                ["permiso"] = new[] { "permiso", "permisos", "acceso", "rol", "roles" },
                ["sociedad"] = new[] { "sociedad", "empresa", "datos empresa", "configuracion empresa" },
                ["sucursal"] = new[] { "sucursal", "sucursales", "local", "tienda" },
                ["caja_config"] = new[] { "configurar caja", "timbrado", "numeracion" },
                ["sifen"] = new[] { "sifen", "factura electronica", "set", "hacienda" },
                ["correo"] = new[] { "correo", "email", "smtp", "configurar correo" },
                ["tema_visual"] = new[] { "tema visual", "modo oscuro", "modo claro", "apariencia", "color sistema" },
                ["configuracion"] = new[] { "tipo pago", "tipo iva", "configurar sistema" },
                
                // Descuentos y precios - TEMA ESPECÍFICO
                ["descuento"] = new[] { "descuento", "descuentos", "aplicar descuento", "descuento producto", "descuento cliente" },
                
                // Categorías y precios
                ["categoria"] = new[] { "categoria", "categorias", "marca", "marcas", "clasificacion", "familia" },
                ["precio"] = new[] { "lista precio", "precio especial", "precio diferenciado", "mayorista" },
                
                // Reportes
                ["reporte"] = new[] { "reporte", "reportes", "informe", "informes", "listado" },
                
                // Cuentas por cobrar/pagar
                ["cuentas_cobrar"] = new[] { "cuentas por cobrar", "cuenta por cobrar", "deuda cliente", "credito vencido" },
                ["cuentas_pagar"] = new[] { "cuentas por pagar", "cuenta por pagar", "deuda proveedor", "pagar proveedor" },
                
                // Asistencia
                ["asistencia"] = new[] { "asistencia", "horario", "entrada", "salida", "turno", "falta", "tardanza" },
                
                // Auditoría
                ["auditoria"] = new[] { "auditoria", "log", "historial cambios", "quien modifico" },
                
                // Manual
                ["manual"] = new[] { "manual", "documentacion", "ayuda sistema" },
                
                // Soporte
                ["soporte"] = new[] { "soporte", "ayuda tecnica", "problema", "error", "contactar", "contacto" },
                
                // NC Compras
                ["nota_credito_compra"] = new[] { "nota credito compra", "nc compra", "devolucion compra", "credito proveedor" },
                
                // Sistema
                ["backup"] = new[] { "backup", "respaldo", "copia seguridad", "respaldar" },
                ["actualizacion"] = new[] { "actualizar", "actualizacion", "nueva version", "update" }
            };
            
            // Buscar qué temas aparecen en la consulta
            foreach (var (tema, palabrasClave) in diccionarioTemas)
            {
                foreach (var palabra in palabrasClave)
                {
                    if (consultaLower.Contains(palabra))
                    {
                        // Peso basado en longitud de la palabra (más específica = más peso)
                        int peso = palabra.Length;
                        // Bonus si es coincidencia de frase completa
                        if (palabra.Contains(" ")) peso += 10;
                        temasDetectados.Add((tema, peso));
                        break; // Solo contar una vez por tema
                    }
                }
            }
            
            if (!temasDetectados.Any())
            {
                _logger.LogInformation("[BuscarGuia] No se detectó tema en: '{Consulta}'", consulta);
                return null;
            }
            
            // Ordenar por peso y tomar el tema más relevante
            var temaPrincipal = temasDetectados.OrderByDescending(t => t.peso).First().tema;
            _logger.LogInformation("[BuscarGuia] Tema detectado: '{Tema}' en consulta: '{Consulta}'", temaPrincipal, consulta);
            
            // ========== PASO 3: Buscar GUÍA por tema ==========
            var guia = _guiasPasoAPaso.FirstOrDefault(g => g.Tema == temaPrincipal);
            
            if (guia != null)
            {
                _logger.LogInformation("[BuscarGuia] ✓ Guía encontrada para tema '{Tema}'", temaPrincipal);
            }
            else
            {
                _logger.LogInformation("[BuscarGuia] ✗ No hay guía para tema '{Tema}'", temaPrincipal);
            }
            
            return guia;
        }

        // Lista de guías paso a paso para las funciones principales
        private readonly List<GuiaPasoAPaso> _guiasPasoAPaso = new()
        {
            // ========== SOPORTE Y AYUDA ==========
            new GuiaPasoAPaso
            {
                Tema = "soporte",
                Patrones = new[] { "contactar soporte", "soporte tecnico", "ayuda tecnica", "contacto soporte", "hablar soporte", "comunicar soporte", "soporte", "problema sistema", "reportar error", "error sistema" },
                Introduccion = "Para contactar con soporte técnico:",
                Pasos = @"1️⃣ **Opción 1 - Desde el Sistema**:
   • Ve a **Configuración** → **Manual del Sistema**
   • Busca la sección **Contacto de Soporte**
   • Encontrarás email y teléfono de soporte

2️⃣ **Opción 2 - Enviar Solicitud de Soporte**:
   • Usa el botón **📎** en este chat
   • Puedes adjuntar capturas de pantalla
   • Describe tu problema detalladamente
   • El equipo de soporte recibirá tu mensaje

3️⃣ **Información útil para soporte**:
   • Describe el error exacto
   • Indica qué estabas haciendo
   • Menciona si el error es recurrente
   • Adjunta capturas si es posible",
                Tip = "Mientras más detalles proporciones, más rápido podremos ayudarte.",
                Ruta = "/manual-sistema",
                Icono = "bi-headset",
                SugerenciasRelacionadas = new List<string> { "Manual del sistema", "Reportar error", "Actualización" }
            },

            // ========== PRODUCTOS ==========
            new GuiaPasoAPaso
            {
                Tema = "producto",
                Patrones = new[] { "crear producto", "nuevo producto", "agregar producto", "registrar producto", "cargar producto", "producto", "pasos producto" },
                Introduccion = "Para crear un nuevo producto, sigue estos pasos:",
                Pasos = @"1️⃣ Ve a **Productos** → Click en **➕ Nuevo**
2️⃣ Completa los datos básicos:
   • **Código**: Código único del producto (o genera automático)
   • **Código de barras**: Escanea o ingresa manualmente
   • **Descripción**: Nombre del producto
3️⃣ Configura precios:
   • **Costo**: Precio de compra
   • **Precio de venta**: Precio al público
   • **Tipo de IVA**: 10%, 5% o Exento
4️⃣ Asigna categoría y marca (opcional)
5️⃣ Configura stock inicial si es necesario
6️⃣ Click en **💾 Guardar**",
                Tip = "Puedes usar el lector de código de barras para agilizar el proceso.",
                Ruta = "/productos",
                Icono = "bi-box-seam",
                SugerenciasRelacionadas = new List<string> { "Ajustar stock", "Ver productos", "Crear categoría" }
            },
            
            new GuiaPasoAPaso
            {
                Tema = "producto",
                Patrones = new[] { "editar producto", "modificar producto", "cambiar producto", "actualizar producto" },
                Introduccion = "Para editar un producto existente:",
                Pasos = @"1️⃣ Ve a **Productos** y busca el producto
2️⃣ Usa los filtros o escribe en el buscador
3️⃣ Click en el ícono **✏️ Editar** (lápiz azul)
4️⃣ Modifica los campos necesarios
5️⃣ Click en **💾 Guardar cambios**",
                Tip = "Los cambios de precio no afectan ventas ya realizadas.",
                Ruta = "/productos",
                Icono = "bi-pencil",
                SugerenciasRelacionadas = new List<string> { "Ver historial de precios", "Ajustar stock" }
            },

            // ========== VENTAS ==========
            new GuiaPasoAPaso
            {
                Tema = "venta",
                Patrones = new[] { "crear venta", "nueva venta", "hacer venta", "registrar venta", "facturar", "hacer factura", "realizar venta", "venta", "pasos venta", "proceso venta" },
                Introduccion = "Para realizar una nueva venta:",
                Pasos = @"1️⃣ Ve a **Ventas** (o presiona F2)
2️⃣ Selecciona o busca el **Cliente**
   • Escribe nombre, RUC o CI para buscar
   • Para ventas al contado sin datos: usa cliente genérico
3️⃣ Agrega productos:
   • Escanea código de barras, o
   • Escribe nombre/código y selecciona de la lista
4️⃣ Ajusta cantidades si es necesario
5️⃣ Selecciona **forma de pago**: Contado o Crédito
6️⃣ Si es crédito, configura las cuotas
7️⃣ Click en **✅ Confirmar Venta**
8️⃣ Imprime el comprobante",
                Tip = "Usa F3 para buscar productos rápidamente y F4 para buscar clientes.",
                Ruta = "/ventas",
                Icono = "bi-cart-check",
                SugerenciasRelacionadas = new List<string> { "Anular venta", "Ver historial ventas", "Crear cliente" }
            },

            new GuiaPasoAPaso
            {
                Tema = "venta",
                Patrones = new[] { "anular venta", "cancelar venta", "eliminar venta" },
                Introduccion = "Para anular una venta:",
                Pasos = @"1️⃣ Ve a **Ventas** → **Explorar**
2️⃣ Busca la venta por número o fecha
3️⃣ Click en **👁️ Ver** para abrir la venta
4️⃣ Click en **🗑️ Anular**
5️⃣ Ingresa el motivo de anulación
6️⃣ Confirma la anulación

⚠️ **Importante**: Si la factura ya fue enviada a SIFEN, debes crear una Nota de Crédito en lugar de anular.",
                Tip = "Las ventas anuladas se mantienen en el historial para auditoría.",
                Ruta = "/ventas/explorar",
                Icono = "bi-x-circle",
                SugerenciasRelacionadas = new List<string> { "Crear nota de crédito", "Reimprimir factura" }
            },

            // ========== CLIENTES ==========
            new GuiaPasoAPaso
            {
                Tema = "cliente",
                Patrones = new[] { "crear cliente", "nuevo cliente", "agregar cliente", "registrar cliente" },
                Introduccion = "Para crear un nuevo cliente:",
                Pasos = @"1️⃣ Ve a **Clientes** → Click en **➕ Nuevo**
2️⃣ Ingresa el **RUC o CI** del cliente
   • El sistema buscará automáticamente en el RUC
3️⃣ Completa los datos:
   • **Razón Social / Nombre**
   • **Dirección**
   • **Teléfono** y **Email**
4️⃣ Configura opciones:
   • **Lista de Precios** (si tiene precio especial)
   • **Límite de crédito** (si aplica)
5️⃣ Click en **💾 Guardar**",
                Tip = "Si el cliente tiene RUC, sus datos se cargan automáticamente desde la base del SET.",
                Ruta = "/clientes",
                Icono = "bi-person-plus",
                SugerenciasRelacionadas = new List<string> { "Ver clientes", "Asignar precio especial" }
            },

            // ========== COMPRAS ==========
            new GuiaPasoAPaso
            {
                Tema = "compra",
                Patrones = new[] { "crear compra", "nueva compra", "registrar compra", "agregar compra", "cargar compra" },
                Introduccion = "Para registrar una compra:",
                Pasos = @"1️⃣ Ve a **Compras** → Click en **➕ Nueva**
2️⃣ Selecciona el **Proveedor**
3️⃣ Ingresa datos de la factura del proveedor:
   • **Timbrado**, **Número de factura**
   • **Fecha** de la factura
4️⃣ Agrega los productos comprados:
   • Busca por nombre o código
   • Ingresa cantidad y costo unitario
5️⃣ Verifica los totales
6️⃣ Selecciona si es **Contado** o **Crédito**
7️⃣ Click en **✅ Confirmar Compra**",
                Tip = "Al confirmar, el stock se actualiza automáticamente.",
                Ruta = "/compras",
                Icono = "bi-bag-plus",
                SugerenciasRelacionadas = new List<string> { "Ver compras", "Crear proveedor", "Pagar proveedor" }
            },

            // ========== CAJA ==========
            new GuiaPasoAPaso
            {
                Tema = "cierre_caja",
                Patrones = new[] { "cerrar caja", "cierre caja", "cierre de caja", "hacer cierre", "cuadrar caja", "arqueo" },
                Introduccion = "Para realizar el cierre de caja:",
                Pasos = @"1️⃣ Ve a **Caja** → **Cierre de Caja**
2️⃣ Verifica que estás en la caja y turno correctos
3️⃣ Revisa el resumen de movimientos:
   • Ventas del día
   • Cobros realizados
   • Pagos efectuados
4️⃣ Cuenta el efectivo físico en caja
5️⃣ Ingresa el **monto contado**
6️⃣ El sistema calcula la diferencia (faltante/sobrante)
7️⃣ Si hay diferencia, ingresa una observación
8️⃣ Click en **✅ Confirmar Cierre**
9️⃣ Imprime el comprobante de cierre",
                Tip = "Realiza el cierre al final de cada turno para mantener control del efectivo.",
                Ruta = "/caja/cierre",
                Icono = "bi-cash-stack",
                SugerenciasRelacionadas = new List<string> { "Ver historial cierres", "Resumen de caja" }
            },

            // ========== NOTAS DE CRÉDITO ==========
            new GuiaPasoAPaso
            {
                Tema = "nota_credito",
                Patrones = new[] { "crear nota credito", "nueva nota credito", "hacer nota credito", "devolucion", "nota de credito" },
                Introduccion = "Para crear una Nota de Crédito (devolución):",
                Pasos = @"1️⃣ Ve a **Notas de Crédito** → **➕ Nueva**
2️⃣ Busca la **venta original** por número
3️⃣ Selecciona el motivo:
   • Devolución de mercadería
   • Descuento posterior
   • Anulación de factura
4️⃣ Selecciona los productos a devolver
5️⃣ Ajusta cantidades si es devolución parcial
6️⃣ Verifica el monto total de la NC
7️⃣ Click en **✅ Confirmar**
8️⃣ El sistema generará la NC con numeración automática",
                Tip = "La NC reduce automáticamente el saldo si era venta a crédito.",
                Ruta = "/notas-credito",
                Icono = "bi-receipt-cutoff",
                SugerenciasRelacionadas = new List<string> { "Ver notas de crédito", "Anular venta" }
            },

            // ========== COBROS ==========
            new GuiaPasoAPaso
            {
                Tema = "cobro",
                Patrones = new[] { "cobrar", "registrar cobro", "recibir pago", "cobrar cuota", "cobrar credito", "cobrar cliente" },
                Introduccion = "Para registrar un cobro a cliente:",
                Pasos = @"1️⃣ Ve a **Cobros** 
2️⃣ Busca el cliente con saldo pendiente
3️⃣ Verás las cuotas pendientes de pago
4️⃣ Selecciona las cuotas a cobrar
5️⃣ Ingresa el monto recibido
6️⃣ Selecciona la forma de pago:
   • Efectivo
   • Transferencia
   • Cheque
7️⃣ Click en **✅ Registrar Cobro**
8️⃣ Imprime el recibo",
                Tip = "Puedes hacer cobros parciales de una cuota.",
                Ruta = "/cobros",
                Icono = "bi-currency-dollar",
                SugerenciasRelacionadas = new List<string> { "Ver cuentas por cobrar", "Historial de cobros" }
            },

            // ========== PAGOS A PROVEEDORES ==========
            new GuiaPasoAPaso
            {
                Tema = "pago",
                Patrones = new[] { "pagar proveedor", "pago proveedor", "pagar compra", "abonar proveedor" },
                Introduccion = "Para registrar un pago a proveedor:",
                Pasos = @"1️⃣ Ve a **Pagos a Proveedores**
2️⃣ Busca el proveedor con saldo pendiente
3️⃣ Verás las facturas pendientes de pago
4️⃣ Selecciona las facturas a pagar
5️⃣ Ingresa el monto a pagar
6️⃣ Selecciona la forma de pago y la caja
7️⃣ Click en **✅ Registrar Pago**",
                Tip = "Los pagos se descuentan automáticamente del efectivo en caja.",
                Ruta = "/pagos-proveedores",
                Icono = "bi-credit-card",
                SugerenciasRelacionadas = new List<string> { "Ver cuentas por pagar", "Historial de pagos" }
            },

            // ========== STOCK / INVENTARIO ==========
            new GuiaPasoAPaso
            {
                Tema = "ajuste_stock",
                Patrones = new[] { "ajustar stock", "ajuste stock", "corregir stock", "modificar stock", "ajuste inventario" },
                Introduccion = "Para realizar un ajuste de stock:",
                Pasos = @"1️⃣ Ve a **Inventario** → **Ajustes de Stock**
2️⃣ Click en **➕ Nuevo Ajuste**
3️⃣ Selecciona el tipo:
   • **Entrada**: Aumenta stock (encontrado, bonificación)
   • **Salida**: Reduce stock (pérdida, daño, vencimiento)
4️⃣ Selecciona el depósito
5️⃣ Agrega los productos a ajustar
6️⃣ Ingresa las cantidades
7️⃣ Escribe el motivo del ajuste
8️⃣ Click en **✅ Confirmar Ajuste**",
                Tip = "Los ajustes quedan registrados en el historial para auditoría.",
                Ruta = "/inventario/ajustes",
                Icono = "bi-boxes",
                SugerenciasRelacionadas = new List<string> { "Ver movimientos stock", "Transferir stock" }
            },

            new GuiaPasoAPaso
            {
                Tema = "transferencia",
                Patrones = new[] { "transferir stock", "transferencia stock", "mover productos", "trasladar mercaderia" },
                Introduccion = "Para transferir stock entre depósitos:",
                Pasos = @"1️⃣ Ve a **Inventario** → **Transferencias**
2️⃣ Click en **➕ Nueva Transferencia**
3️⃣ Selecciona:
   • **Depósito origen**: De dónde sale
   • **Depósito destino**: A dónde va
4️⃣ Agrega los productos a transferir
5️⃣ Ingresa las cantidades
6️⃣ Click en **✅ Confirmar Transferencia**",
                Tip = "La transferencia descuenta del origen y suma al destino automáticamente.",
                Ruta = "/inventario/transferencias",
                Icono = "bi-arrow-left-right",
                SugerenciasRelacionadas = new List<string> { "Ver depósitos", "Ajustar stock" }
            },

            // ========== PRESUPUESTOS ==========
            new GuiaPasoAPaso
            {
                Tema = "presupuesto",
                Patrones = new[] { "crear presupuesto", "nuevo presupuesto", "hacer presupuesto", "cotizacion", "cotizar" },
                Introduccion = "Para crear un presupuesto/cotización:",
                Pasos = @"1️⃣ Ve a **Presupuestos** → **➕ Nuevo**
2️⃣ Selecciona el cliente (o déjalo genérico)
3️⃣ Agrega los productos cotizados
4️⃣ Ajusta precios si es necesario
5️⃣ Configura:
   • **Validez**: Días de vigencia
   • **Condiciones**: Forma de pago, entrega
6️⃣ Click en **💾 Guardar**
7️⃣ Imprime o envía por email al cliente

Para **convertir en venta**: Abre el presupuesto y click en **Convertir a Venta**",
                Tip = "Los presupuestos no afectan stock ni generan obligaciones fiscales.",
                Ruta = "/presupuestos",
                Icono = "bi-file-text",
                SugerenciasRelacionadas = new List<string> { "Ver presupuestos", "Crear venta" }
            },

            // ========== USUARIOS Y PERMISOS ==========
            new GuiaPasoAPaso
            {
                Tema = "usuario",
                Patrones = new[] { "crear usuario", "nuevo usuario", "agregar usuario" },
                Introduccion = "Para crear un nuevo usuario del sistema:",
                Pasos = @"1️⃣ Ve a **Usuarios** → **➕ Nuevo**
2️⃣ Completa datos personales:
   • Nombre, Apellido, CI
   • Email, Teléfono
3️⃣ Configura acceso:
   • **Nombre de usuario**: Para iniciar sesión
   • **Contraseña**: Mínimo 6 caracteres
4️⃣ Asigna el **Rol**:
   • Administrador: Acceso total
   • Vendedor: Solo ventas y cobros
   • Cajero: Caja y ventas
5️⃣ Click en **💾 Guardar**",
                Tip = "Los permisos se configuran por rol, no por usuario individual.",
                Ruta = "/usuarios",
                Icono = "bi-person-plus",
                SugerenciasRelacionadas = new List<string> { "Configurar permisos", "Ver usuarios" }
            },

            new GuiaPasoAPaso
            {
                Tema = "permiso",
                Patrones = new[] { "configurar permiso", "asignar permiso", "dar permiso", "quitar permiso", "permisos rol" },
                Introduccion = "Para configurar permisos de un rol:",
                Pasos = @"1️⃣ Ve a **Personal** → **Permisos de Usuarios**
2️⃣ Selecciona el **Rol** a configurar
3️⃣ Verás la lista de módulos del sistema
4️⃣ Para cada módulo, activa/desactiva:
   • ✅ **Ver**: Puede acceder al módulo
   • ✅ **Crear**: Puede crear registros
   • ✅ **Editar**: Puede modificar
   • ✅ **Eliminar**: Puede eliminar
5️⃣ Los cambios se guardan automáticamente",
                Tip = "El rol Administrador siempre tiene todos los permisos.",
                Ruta = "/personal/permisos-usuarios",
                Icono = "bi-shield-lock",
                SugerenciasRelacionadas = new List<string> { "Crear rol", "Ver usuarios" }
            },

            // ========== CONFIGURACIÓN ==========
            new GuiaPasoAPaso
            {
                Tema = "tema_visual",
                Patrones = new[] { "cambiar tema", "modo oscuro", "modo claro", "cambiar color", "apariencia" },
                Introduccion = "Para cambiar el tema visual del sistema:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Tema**
2️⃣ Elige entre:
   • 🌙 **Oscuro**: Fondo negro, ideal para poca luz
   • ☀️ **Claro**: Fondo blanco, alto contraste
   • 🌤️ **Tenue**: Fondo gris suave (recomendado)
3️⃣ Click en **Aplicar**
4️⃣ El cambio es inmediato",
                Tip = "El tema se guarda por navegador, cada usuario puede tener su preferencia.",
                Ruta = "/configuracion/tema",
                Icono = "bi-palette",
                SugerenciasRelacionadas = new List<string> { "Configuración sistema" }
            },

            new GuiaPasoAPaso
            {
                Tema = "correo",
                Patrones = new[] { "configurar correo", "smtp", "email sistema", "envio correo" },
                Introduccion = "Para configurar el envío de correos:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Correo Electrónico**
2️⃣ Configura el servidor SMTP:
   • **Servidor**: smtp.gmail.com (para Gmail)
   • **Puerto**: 587
   • **SSL**: Activado
3️⃣ Credenciales:
   • **Usuario**: tu correo
   • **Contraseña**: contraseña de aplicación
4️⃣ Configura remitente:
   • **Correo**: desde donde se envían
   • **Nombre**: nombre que aparece
5️⃣ Click en **Probar Conexión**
6️⃣ Si funciona, **Guardar**",
                Tip = "Para Gmail, necesitas crear una 'Contraseña de aplicación' desde la seguridad de tu cuenta Google.",
                Ruta = "/configuracion/correo",
                Icono = "bi-envelope-at",
                SugerenciasRelacionadas = new List<string> { "Configurar destinatarios", "Probar envío" }
            },

            // ========== SIFEN ==========
            new GuiaPasoAPaso
            {
                Tema = "sifen",
                Patrones = new[] { "configurar sifen", "factura electronica", "activar sifen", "habilitar sifen" },
                Introduccion = "Para configurar Facturación Electrónica (SIFEN):",
                Pasos = @"1️⃣ Ve a **Configuración** → **SIFEN**
2️⃣ Carga el **Certificado Digital** (.pfx)
3️⃣ Ingresa la contraseña del certificado
4️⃣ Configura:
   • **Ambiente**: Test o Producción
   • **Versión SIFEN**: Normalmente 150
5️⃣ Click en **Guardar**
6️⃣ Click en **Probar Conexión** para verificar

⚠️ **Requisitos previos**:
• Tener certificado digital del SET
• RUC habilitado para facturación electrónica
• Timbrado electrónico vigente",
                Tip = "Siempre prueba primero en ambiente de Test antes de pasar a Producción.",
                Ruta = "/admin/sifen",
                Icono = "bi-patch-check",
                SugerenciasRelacionadas = new List<string> { "Ver diagnóstico SIFEN", "Configurar timbrado" }
            },

            // ========== PROVEEDORES ==========
            new GuiaPasoAPaso
            {
                Tema = "proveedor",
                Patrones = new[] { "crear proveedor", "nuevo proveedor", "agregar proveedor", "registrar proveedor" },
                Introduccion = "Para crear un nuevo proveedor:",
                Pasos = @"1️⃣ Ve a **Proveedores** → **Explorar Proveedores**
2️⃣ Click en **➕ Nuevo Proveedor**
3️⃣ Ingresa el **RUC** del proveedor
   • El sistema buscará datos automáticamente
4️⃣ Completa los datos:
   • **Razón Social / Nombre**
   • **Dirección**
   • **Teléfono** y **Email**
5️⃣ Configura condiciones de pago (si aplica):
   • Días de crédito
   • Límite de crédito
6️⃣ Click en **💾 Guardar**",
                Tip = "Mantén actualizado el RUC para validar comprobantes de compra.",
                Ruta = "/proveedores/explorar",
                Icono = "bi-building-fill-gear",
                SugerenciasRelacionadas = new List<string> { "Ver proveedores", "Registrar compra" }
            },

            new GuiaPasoAPaso
            {
                Tema = "proveedor",
                Patrones = new[] { "buscar proveedor", "explorar proveedor", "ver proveedor", "listar proveedor" },
                Introduccion = "Para buscar y explorar proveedores:",
                Pasos = @"1️⃣ Ve a **Proveedores** → **Explorar Proveedores**
2️⃣ Usa los filtros disponibles:
   • **Búsqueda**: Por nombre o RUC
   • **Estado**: Activos, Inactivos, Todos
3️⃣ Los resultados se muestran en la tabla
4️⃣ Click en **👁️ Ver** para ver detalles
5️⃣ Click en **✏️ Editar** para modificar",
                Tip = "Puedes exportar la lista a Excel desde el botón de descarga.",
                Ruta = "/proveedores/explorar",
                Icono = "bi-search",
                SugerenciasRelacionadas = new List<string> { "Crear proveedor", "Cuentas por pagar" }
            },

            // ========== NOTAS DE CRÉDITO COMPRAS ==========
            new GuiaPasoAPaso
            {
                Tema = "nota_credito_compra",
                Patrones = new[] { "nota credito compra", "nc compra", "devolucion compra", "credito proveedor" },
                Introduccion = "Para crear una Nota de Crédito de Compra (devolución a proveedor):",
                Pasos = @"1️⃣ Ve a **Compras** → **Nueva NC Compra**
2️⃣ Busca la **compra original** por número
3️⃣ Selecciona el motivo:
   • Devolución de mercadería
   • Error en factura
   • Crédito del proveedor
4️⃣ Selecciona los productos a devolver
5️⃣ Ajusta cantidades si es parcial
6️⃣ Ingresa datos de la NC del proveedor:
   • Número de la NC
   • Timbrado
   • Fecha
7️⃣ Click en **✅ Confirmar**",
                Tip = "La NC reduce el saldo a pagar al proveedor y devuelve el stock al depósito.",
                Ruta = "/notas-credito-compra",
                Icono = "bi-file-earmark-minus",
                SugerenciasRelacionadas = new List<string> { "Explorar NC Compras", "Ver compras" }
            },

            // ========== INFORMES DE VENTAS ==========
            new GuiaPasoAPaso
            {
                Tema = "reporte",
                Patrones = new[] { "informe venta", "reporte venta", "ver ventas dia", "ventas del dia" },
                Introduccion = "Para generar informes de ventas:",
                Pasos = @"1️⃣ Ve a **Informes** → **Ventas Agrupado** o **Ventas Detallado**
2️⃣ **Ventas Agrupado**: Resumen por fecha, vendedor, etc.
   • Útil para ver totales y tendencias
3️⃣ **Ventas Detallado**: Lista cada producto vendido
   • Útil para análisis detallado
4️⃣ Configura filtros:
   • **Rango de fechas**
   • **Sucursal** (si hay varias)
   • **Vendedor** (opcional)
5️⃣ Click en **Generar Informe**
6️⃣ Puedes **Imprimir** o **Exportar a Excel**",
                Tip = "El informe agrupado es más rápido para ver totales; el detallado muestra cada línea de venta.",
                Ruta = "/informes/ventas-agrupado",
                Icono = "bi-graph-up-arrow",
                SugerenciasRelacionadas = new List<string> { "Informe NC", "Resumen de caja" }
            },

            // ========== INFORMES DE COMPRAS ==========
            new GuiaPasoAPaso
            {
                Tema = "reporte",
                Patrones = new[] { "informe compra", "reporte compra", "ver compras", "compras periodo" },
                Introduccion = "Para generar informes de compras:",
                Pasos = @"1️⃣ Ve a **Informes** → **Compras Agrupado** o **Compras Detallado**
2️⃣ **Compras Agrupado**: Resumen por proveedor, fecha
3️⃣ **Compras Detallado**: Lista cada producto comprado
4️⃣ Configura filtros:
   • **Rango de fechas**
   • **Proveedor** (opcional)
   • **Sucursal**
5️⃣ Click en **Generar Informe**
6️⃣ Exporta a Excel si necesitas análisis adicional",
                Tip = "Compara compras vs ventas para analizar rotación de inventario.",
                Ruta = "/informes/compras-general",
                Icono = "bi-cart3",
                SugerenciasRelacionadas = new List<string> { "Informe NC Compras", "Stock valorizado" }
            },

            // ========== INFORMES DE PRODUCTOS ==========
            new GuiaPasoAPaso
            {
                Tema = "stock",
                Patrones = new[] { "informe producto", "stock valorizado", "valor inventario", "costo inventario" },
                Introduccion = "Para ver el inventario valorizado:",
                Pasos = @"1️⃣ Ve a **Informes** → **Listado Valorizado**
2️⃣ Selecciona el **Depósito** o todos
3️⃣ Filtros opcionales:
   • **Categoría**: Por familia de productos
   • **Marca**: Por marca específica
   • **Stock mínimo**: Solo productos con bajo stock
4️⃣ Click en **Generar Informe**
5️⃣ Verás:
   • Stock actual de cada producto
   • Costo unitario
   • Valor total del inventario",
                Tip = "Usa este informe para contabilidad y control de activos.",
                Ruta = "/informes/productos-valorizado",
                Icono = "bi-currency-exchange",
                SugerenciasRelacionadas = new List<string> { "Movimientos inventario", "Ajustes stock" }
            },

            new GuiaPasoAPaso
            {
                Tema = "stock",
                Patrones = new[] { "movimiento stock", "movimiento inventario", "historial stock", "kardex" },
                Introduccion = "Para ver movimientos de inventario (Kardex):",
                Pasos = @"1️⃣ Ve a **Informes** → **Movimientos Inventario**
2️⃣ Selecciona el **Producto** a consultar
3️⃣ Define el **Rango de fechas**
4️⃣ Opcionalmente filtra por **Depósito**
5️⃣ Click en **Buscar**
6️⃣ Verás cada movimiento:
   • Entradas (compras, ajustes+, transferencias)
   • Salidas (ventas, ajustes-, transferencias)
   • Stock resultante después de cada movimiento",
                Tip = "El Kardex es fundamental para auditorías y control de faltantes.",
                Ruta = "/informes/movimientos-productos",
                Icono = "bi-arrow-left-right",
                SugerenciasRelacionadas = new List<string> { "Ajustar stock", "Stock valorizado" }
            },

            // ========== INFORMES DE CLIENTES ==========
            new GuiaPasoAPaso
            {
                Tema = "reporte",
                Patrones = new[] { "informe cliente", "listado cliente", "reporte cliente" },
                Introduccion = "Para generar informes de clientes:",
                Pasos = @"1️⃣ Ve a **Informes** → **Listado de Clientes**
2️⃣ Configura filtros opcionales:
   • **Estado**: Activos/Inactivos
   • **Con saldo pendiente**: Solo deudores
3️⃣ Click en **Generar**
4️⃣ Verás datos de cada cliente:
   • Nombre, RUC, Contacto
   • Total de compras
   • Saldo pendiente",
                Tip = "Exporta a Excel para hacer mailing o análisis de cartera.",
                Ruta = "/informes/clientes",
                Icono = "bi-people",
                SugerenciasRelacionadas = new List<string> { "Cuentas por cobrar", "Historial cobros" }
            },

            new GuiaPasoAPaso
            {
                Tema = "cuentas_cobrar",
                Patrones = new[] { "cuenta por cobrar", "deuda cliente", "saldo cliente", "credito vencido" },
                Introduccion = "Para ver cuentas por cobrar:",
                Pasos = @"1️⃣ Ve a **Informes** → **Listado de Cobro a Clientes**
2️⃣ Configura filtros:
   • **Fecha de corte**: Para ver vencimientos
   • **Cliente específico** (opcional)
   • **Solo vencidos**: Muestra morosos
3️⃣ Click en **Generar**
4️⃣ Verás por cada cliente:
   • Facturas pendientes
   • Monto total adeudado
   • Días de mora
   • Antigüedad de la deuda",
                Tip = "Usa la columna de días de mora para priorizar gestión de cobro.",
                Ruta = "/informes/cuentas-por-cobrar",
                Icono = "bi-cash-stack",
                SugerenciasRelacionadas = new List<string> { "Registrar cobro", "Historial cobros" }
            },

            // ========== INFORMES DE PROVEEDORES ==========
            new GuiaPasoAPaso
            {
                Tema = "cuentas_pagar",
                Patrones = new[] { "cuenta por pagar", "deuda proveedor", "saldo proveedor" },
                Introduccion = "Para ver cuentas por pagar a proveedores:",
                Pasos = @"1️⃣ Ve a **Informes** → **Listado de Pago a Proveedores**
2️⃣ Configura filtros:
   • **Fecha de corte**: Para ver vencimientos
   • **Proveedor específico** (opcional)
   • **Solo vencidos**: Muestra deudas vencidas
3️⃣ Click en **Generar**
4️⃣ Verás por cada proveedor:
   • Facturas pendientes de pago
   • Monto total a pagar
   • Fecha de vencimiento
   • Días de mora",
                Tip = "Planifica los pagos según vencimiento para mantener buena relación con proveedores.",
                Ruta = "/informes/cuentas-por-pagar",
                Icono = "bi-cash-coin",
                SugerenciasRelacionadas = new List<string> { "Registrar pago", "Historial pagos" }
            },

            // ========== RESUMEN DE CAJA ==========
            new GuiaPasoAPaso
            {
                Tema = "caja",
                Patrones = new[] { "resumen caja", "estado caja", "ver caja", "movimiento caja" },
                Introduccion = "Para ver el resumen de caja:",
                Pasos = @"1️⃣ Ve a **Informes** → **Resumen de Caja**
2️⃣ Selecciona:
   • **Caja**: La caja a consultar
   • **Fecha**: Día específico o rango
   • **Turno**: Si manejas turnos
3️⃣ Click en **Generar**
4️⃣ Verás:
   • **Ingresos**: Ventas contado, cobros
   • **Egresos**: Pagos, devoluciones
   • **Saldo**: Efectivo esperado en caja",
                Tip = "Compara el saldo esperado con el físico antes del cierre.",
                Ruta = "/informes/resumen-caja",
                Icono = "bi-journal-check",
                SugerenciasRelacionadas = new List<string> { "Cierre de caja", "Historial cierres" }
            },

            // ========== HISTORIAL DE CIERRES ==========
            new GuiaPasoAPaso
            {
                Tema = "cierre_caja",
                Patrones = new[] { "historial cierre", "cierres anteriores", "ver cierres", "arqueos anteriores" },
                Introduccion = "Para ver el historial de cierres de caja:",
                Pasos = @"1️⃣ Ve a **Ventas** → **Historial Cierres**
2️⃣ Filtra por:
   • **Rango de fechas**
   • **Caja específica**
   • **Usuario que cerró**
3️⃣ Click en un cierre para ver detalle:
   • Ventas del turno
   • Cobros realizados
   • Diferencias encontradas
   • Composición del efectivo",
                Tip = "Revisa los cierres con diferencias para identificar patrones de error.",
                Ruta = "/caja/historial-cierres",
                Icono = "bi-clock-history",
                SugerenciasRelacionadas = new List<string> { "Cierre de caja", "Resumen caja" }
            },

            // ========== GESTIÓN DE PERSONAL ==========
            new GuiaPasoAPaso
            {
                Tema = "asistencia",
                Patrones = new[] { "registrar asistencia", "marcar entrada", "marcar salida", "control asistencia" },
                Introduccion = "Para registrar asistencia del personal:",
                Pasos = @"1️⃣ Ve a **Gestión de Personal** → **Asistencia**
2️⃣ Opciones de registro:
   • **Con cámara**: Reconocimiento facial
   • **Manual**: Seleccionar empleado
3️⃣ El sistema registra:
   • Hora de entrada
   • Hora de salida
   • Ubicación (si está configurado)
4️⃣ Los registros quedan en el historial

**Para registro rápido**: Usa **Registro Directo** con la cámara",
                Tip = "Configura los horarios primero para que el sistema calcule horas extras y tardanzas.",
                Ruta = "/registro-asistencia",
                Icono = "bi-calendar-check",
                SugerenciasRelacionadas = new List<string> { "Ver asistencia", "Configurar horarios" }
            },

            new GuiaPasoAPaso
            {
                Tema = "asistencia",
                Patrones = new[] { "configurar horario", "crear horario", "horario trabajo", "turno trabajo" },
                Introduccion = "Para configurar horarios de trabajo:",
                Pasos = @"1️⃣ Ve a **Gestión de Personal** → **Horarios**
2️⃣ Click en **➕ Nuevo Horario**
3️⃣ Define:
   • **Nombre**: Ej: 'Turno Mañana'
   • **Hora entrada**: Hora normal de llegada
   • **Hora salida**: Hora normal de salida
   • **Tolerancia**: Minutos de gracia
4️⃣ Configura días de la semana
5️⃣ Click en **💾 Guardar**

Luego **asigna** el horario a los empleados",
                Tip = "Puedes crear múltiples horarios para diferentes turnos o departamentos.",
                Ruta = "/horarios",
                Icono = "bi-clock",
                SugerenciasRelacionadas = new List<string> { "Asignar horario", "Ver asistencia" }
            },

            new GuiaPasoAPaso
            {
                Tema = "asistencia",
                Patrones = new[] { "asignar horario", "horario empleado", "asignar turno" },
                Introduccion = "Para asignar horarios a empleados:",
                Pasos = @"1️⃣ Ve a **Gestión de Personal** → **Asignar**
2️⃣ Selecciona el **Empleado**
3️⃣ Selecciona el **Horario** a asignar
4️⃣ Define el **período de vigencia**:
   • Fecha desde
   • Fecha hasta (o indefinido)
5️⃣ Click en **Asignar**

El sistema usará este horario para calcular:
• Tardanzas
• Horas extras
• Faltas",
                Tip = "Un empleado puede tener horarios diferentes según el período.",
                Ruta = "/asignacionhorarios",
                Icono = "bi-calendar2-week",
                SugerenciasRelacionadas = new List<string> { "Ver horarios", "Informe asistencia" }
            },

            new GuiaPasoAPaso
            {
                Tema = "asistencia",
                Patrones = new[] { "informe asistencia", "reporte asistencia", "ver faltas", "ver tardanzas" },
                Introduccion = "Para generar informes de asistencia:",
                Pasos = @"1️⃣ Ve a **Informes** → **Informe de Asistencia**
2️⃣ Configura filtros:
   • **Rango de fechas**
   • **Empleado** (o todos)
   • **Departamento** (si aplica)
3️⃣ Click en **Generar**
4️⃣ Verás por cada empleado:
   • Días trabajados
   • Tardanzas (minutos)
   • Faltas
   • Horas extras",
                Tip = "Exporta a Excel para liquidación de sueldos.",
                Ruta = "/informes-asistencia",
                Icono = "bi-person-badge",
                SugerenciasRelacionadas = new List<string> { "Listado asistencia", "Configurar horarios" }
            },

            // ========== INVENTARIO AVANZADO ==========
            new GuiaPasoAPaso
            {
                Tema = "inventario",
                Patrones = new[] { "crear deposito", "nuevo deposito", "agregar deposito", "almacen" },
                Introduccion = "Para crear un nuevo depósito/almacén:",
                Pasos = @"1️⃣ Ve a **Inventario** → **Depósitos**
2️⃣ Click en **➕ Nuevo Depósito**
3️⃣ Completa los datos:
   • **Nombre**: Identificación del depósito
   • **Descripción**: Ubicación o uso
   • **Sucursal**: A qué sucursal pertenece
4️⃣ Click en **💾 Guardar**

Los productos podrán tener stock en este depósito",
                Tip = "Crea depósitos separados para: Tienda, Bodega, Dañados, etc.",
                Ruta = "/inventario/depositos",
                Icono = "bi-building-down",
                SugerenciasRelacionadas = new List<string> { "Transferir stock", "Ajustar stock" }
            },

            new GuiaPasoAPaso
            {
                Tema = "ajuste_stock",
                Patrones = new[] { "explorar ajuste", "ver ajustes", "historial ajuste", "buscar ajuste" },
                Introduccion = "Para explorar ajustes de stock realizados:",
                Pasos = @"1️⃣ Ve a **Inventario** → **Explorar Ajustes**
2️⃣ Filtra por:
   • **Rango de fechas**
   • **Tipo**: Entrada o Salida
   • **Usuario**: Quién hizo el ajuste
   • **Depósito**
3️⃣ Click en un ajuste para ver detalle:
   • Productos ajustados
   • Cantidades
   • Motivo registrado",
                Tip = "Los ajustes son auditoría importante; no se pueden eliminar.",
                Ruta = "/inventario/ajustes/explorar",
                Icono = "bi-search",
                SugerenciasRelacionadas = new List<string> { "Nuevo ajuste", "Movimientos stock" }
            },

            // ========== CONFIGURACIÓN EMPRESA ==========
            new GuiaPasoAPaso
            {
                Tema = "sucursal",
                Patrones = new[] { "crear sucursal", "nueva sucursal", "agregar sucursal", "configurar sucursal" },
                Introduccion = "Para crear o configurar una sucursal:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Sucursales**
2️⃣ Click en **➕ Nueva Sucursal** o edita existente
3️⃣ Completa datos:
   • **Nombre**: Identificación
   • **Dirección**: Dirección fiscal
   • **Teléfono**
   • **Código establecimiento**: Para SIFEN (3 dígitos)
4️⃣ Carga el **Logo** (opcional)
5️⃣ Click en **💾 Guardar**",
                Tip = "El código de establecimiento debe coincidir con el registrado en el SET.",
                Ruta = "/sucursales",
                Icono = "bi-building",
                SugerenciasRelacionadas = new List<string> { "Configurar sociedad", "Configurar cajas" }
            },

            new GuiaPasoAPaso
            {
                Tema = "configuracion",
                Patrones = new[] { "configurar sociedad", "datos empresa", "datos emisor", "ruc empresa" },
                Introduccion = "Para configurar los datos de la empresa (Sociedad/Emisor):",
                Pasos = @"1️⃣ Ve a **Configuración** → **Sociedad (Emisor)**
2️⃣ Completa los datos fiscales:
   • **RUC** y **DV** (dígito verificador)
   • **Razón Social**
   • **Nombre de Fantasía**
   • **Tipo de Contribuyente**
3️⃣ Datos de contacto:
   • **Dirección fiscal**
   • **Teléfono**, **Email**
4️⃣ Actividad económica principal
5️⃣ Click en **💾 Guardar**

⚠️ Estos datos aparecen en todas las facturas",
                Tip = "Verifica que el RUC coincida exactamente con el registrado en el SET.",
                Ruta = "/configuracion/sociedad",
                Icono = "bi-shop",
                SugerenciasRelacionadas = new List<string> { "Configurar sucursales", "Configurar SIFEN" }
            },

            new GuiaPasoAPaso
            {
                Tema = "caja",
                Patrones = new[] { "configurar caja", "crear caja", "punto expedicion", "punto de venta" },
                Introduccion = "Para configurar cajas (puntos de expedición):",
                Pasos = @"1️⃣ Ve a **Configuración** → **Cajas**
2️⃣ Click en **➕ Nueva Caja** o edita existente
3️⃣ Configura:
   • **Nombre**: Identificación (Ej: Caja 1)
   • **Código punto expedición**: 3 dígitos para SIFEN
   • **Sucursal**: A cuál pertenece
   • **Tipo de facturación**: Electrónica o Autoimpresor
4️⃣ Si es electrónica:
   • **Timbrado electrónico**
5️⃣ Si es autoimpresor:
   • **Timbrado**, Número desde/hasta
6️⃣ Click en **💾 Guardar**",
                Tip = "Cada caja debe tener un timbrado configurado para poder facturar.",
                Ruta = "/configuracion/cajas",
                Icono = "bi-upc-scan",
                SugerenciasRelacionadas = new List<string> { "Configurar timbrado", "Cierre de caja" }
            },

            // ========== CATÁLOGOS ==========
            new GuiaPasoAPaso
            {
                Tema = "configuracion",
                Patrones = new[] { "tipo pago", "forma pago", "metodo pago", "crear tipo pago" },
                Introduccion = "Para configurar tipos/formas de pago:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Tipos de Pago**
2️⃣ Verás los tipos predefinidos:
   • Efectivo
   • Tarjeta de Crédito
   • Tarjeta de Débito
   • Transferencia
   • Cheque
3️⃣ Para agregar nuevo, click en **➕ Nuevo**
4️⃣ Configura:
   • **Nombre**
   • **Código SIFEN** (obligatorio para facturación electrónica)
   • **Activo**: Si/No",
                Tip = "Los códigos SIFEN deben coincidir con el catálogo del SET.",
                Ruta = "/configuracion/tipos-pago",
                Icono = "bi-credit-card",
                SugerenciasRelacionadas = new List<string> { "Tipos de documento", "Tipos de IVA" }
            },

            new GuiaPasoAPaso
            {
                Tema = "configuracion",
                Patrones = new[] { "tipo iva", "tasa iva", "impuesto", "exento", "gravado" },
                Introduccion = "Para configurar tipos de IVA:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Tipos de IVA**
2️⃣ Tipos estándar en Paraguay:
   • **IVA 10%**: Tasa general
   • **IVA 5%**: Tasa reducida
   • **Exento**: Sin IVA
3️⃣ Cada tipo tiene:
   • **Porcentaje**: 10, 5, o 0
   • **Código SIFEN**: Para facturación electrónica
4️⃣ Normalmente no necesitas modificar estos",
                Tip = "Al crear productos, asigna el tipo de IVA correcto según la legislación.",
                Ruta = "/configuracion/tipos-iva",
                Icono = "bi-percent",
                SugerenciasRelacionadas = new List<string> { "Crear producto", "Tipos de pago" }
            },

            new GuiaPasoAPaso
            {
                Tema = "categoria",
                Patrones = new[] { "crear categoria", "categoria producto", "familia producto", "clasificacion producto" },
                Introduccion = "Para crear categorías de productos:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Marcas y Clasificaciones**
2️⃣ En la sección **Categorías**:
3️⃣ Click en **➕ Nueva Categoría**
4️⃣ Ingresa:
   • **Nombre**: Ej: Bebidas, Lácteos, Limpieza
   • **Descripción** (opcional)
5️⃣ Click en **💾 Guardar**

Las categorías ayudan a organizar y filtrar productos",
                Tip = "Usa categorías para informes de ventas por familia de productos.",
                Ruta = "/configuracion/marcas-clasificaciones",
                Icono = "bi-tags-fill",
                SugerenciasRelacionadas = new List<string> { "Crear marca", "Crear producto" }
            },

            new GuiaPasoAPaso
            {
                Tema = "categoria",
                Patrones = new[] { "crear marca", "marca producto", "nueva marca" },
                Introduccion = "Para crear marcas de productos:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Marcas y Clasificaciones**
2️⃣ En la sección **Marcas**:
3️⃣ Click en **➕ Nueva Marca**
4️⃣ Ingresa:
   • **Nombre**: Ej: Coca-Cola, Samsung, etc.
5️⃣ Click en **💾 Guardar**

Las marcas ayudan a identificar y filtrar productos",
                Tip = "Combina categorías + marcas para mejor organización del catálogo.",
                Ruta = "/configuracion/marcas-clasificaciones",
                Icono = "bi-tag",
                SugerenciasRelacionadas = new List<string> { "Crear categoría", "Crear producto" }
            },

            new GuiaPasoAPaso
            {
                Tema = "precio",
                Patrones = new[] { "lista precio", "precio diferenciado", "precio especial", "precio mayorista" },
                Introduccion = "Para configurar listas de precios:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Precios y Descuentos**
2️⃣ Click en **➕ Nueva Lista de Precios**
3️⃣ Configura:
   • **Nombre**: Ej: Mayorista, VIP, Empleados
   • **Factor**: Multiplicador sobre precio base
     - 1.0 = mismo precio
     - 0.9 = 10% descuento
     - 0.85 = 15% descuento
4️⃣ Click en **💾 Guardar**
5️⃣ **Asigna la lista a clientes** específicos

Cuando vendas a ese cliente, se aplica el precio especial",
                Tip = "También puedes configurar precios específicos por producto para cada lista.",
                Ruta = "/configuracion/precios-descuentos",
                Icono = "bi-currency-exchange",
                SugerenciasRelacionadas = new List<string> { "Asignar a cliente", "Crear producto", "Aplicar descuento" }
            },

            // ========== DESCUENTOS ==========
            new GuiaPasoAPaso
            {
                Tema = "descuento",
                Patrones = new[] { "descuento", "descuentos", "aplicar descuento", "como funcionan descuentos" },
                Introduccion = "Los descuentos en el sistema funcionan así:",
                Pasos = @"**📍 Ubicación:** Ve a **Configuración** → **Precios y Descuentos**

1️⃣ **Descuentos por Lista de Precios:**
   • Crea listas con factor de descuento (0.9 = 10% desc.)
   • Asigna la lista al cliente
   • El descuento se aplica automáticamente

2️⃣ **Descuento Manual en Venta:**
   • En la línea del producto, ingresa el **% de descuento**
   • El sistema recalcula el total automáticamente

3️⃣ **Precios Especiales por Cliente:**
   • En **Precios y Descuentos**, crea precios específicos
   • Por producto + cliente = precio fijo

4️⃣ **Descuento por Cantidad:**
   • Configura reglas de descuento por volumen
   • Se aplica automáticamente al superar la cantidad",
                Tip = "Los descuentos se calculan sobre el precio base con IVA incluido.",
                Ruta = "/configuracion/precios-descuentos",
                Icono = "bi-percent",
                SugerenciasRelacionadas = new List<string> { "Lista de precios", "Precio por cliente", "Configurar descuento" }
            },

            // ========== LISTA DE PRECIOS ==========
            new GuiaPasoAPaso
            {
                Tema = "precio",
                Patrones = new[] { "lista precios", "crear lista precios", "configurar precios", "precio mayorista" },
                Introduccion = "Para configurar listas de precios:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Precios y Descuentos**
2️⃣ Click en **➕ Nueva Lista de Precios**
3️⃣ Configura:
   • **Nombre**: Ej: Mayorista, VIP, Empleados
   • **Factor**: Multiplicador sobre precio base
     - 1.0 = mismo precio
     - 0.9 = 10% descuento
     - 0.85 = 15% descuento
4️⃣ Click en **💾 Guardar**
5️⃣ **Asigna la lista a clientes** específicos

Cuando vendas a ese cliente, se aplica el precio especial",
                Tip = "También puedes configurar precios específicos por producto para cada lista.",
                Ruta = "/configuracion/precios-descuentos",
                Icono = "bi-currency-exchange",
                SugerenciasRelacionadas = new List<string> { "Asignar a cliente", "Crear producto" }
            },

            // ========== SISTEMA ==========
            new GuiaPasoAPaso
            {
                Tema = "auditoria",
                Patrones = new[] { "auditoria", "log sistema", "historial cambios", "quien modifico" },
                Introduccion = "Para ver la auditoría del sistema:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Auditoría**
2️⃣ Filtra por:
   • **Rango de fechas**
   • **Usuario**: Quién hizo la acción
   • **Módulo**: Ventas, Productos, etc.
   • **Acción**: Crear, Editar, Eliminar
3️⃣ Click en **Buscar**
4️⃣ Verás cada registro de auditoría:
   • Fecha y hora
   • Usuario
   • Acción realizada
   • Datos anteriores/nuevos",
                Tip = "La auditoría es fundamental para detectar errores y accesos no autorizados.",
                Ruta = "/configuracion/auditoria",
                Icono = "bi-journal-text",
                SugerenciasRelacionadas = new List<string> { "Ver usuarios", "Configurar permisos" }
            },

            new GuiaPasoAPaso
            {
                Tema = "actualizacion",
                Patrones = new[] { "actualizar sistema", "nueva version", "update sistema", "actualizacion" },
                Introduccion = "Para actualizar el sistema:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Actualización**
2️⃣ El sistema verifica si hay nuevas versiones
3️⃣ Si hay actualización disponible:
   • Verás las novedades de la versión
   • Click en **Descargar Actualización**
4️⃣ Una vez descargada:
   • Click en **Aplicar Actualización**
5️⃣ El sistema se reiniciará automáticamente

⚠️ **Importante**: Haz respaldo antes de actualizar",
                Tip = "Las actualizaciones incluyen mejoras de seguridad y nuevas funciones.",
                Ruta = "/actualizacion-sistema",
                Icono = "bi-arrow-repeat",
                SugerenciasRelacionadas = new List<string> { "Ver manual", "Hacer respaldo" }
            },

            new GuiaPasoAPaso
            {
                Tema = "manual",
                Patrones = new[] { "manual sistema", "ayuda sistema", "documentacion", "como usar" },
                Introduccion = "Para acceder al manual del sistema:",
                Pasos = @"1️⃣ Ve a **Configuración** → **Manual del Sistema**
2️⃣ Encontrarás:
   • **Guías por módulo**: Ventas, Compras, Inventario, etc.
   • **Videos tutoriales** (si están disponibles)
   • **Preguntas frecuentes**
   • **Contacto de soporte**
3️⃣ Usa el buscador para encontrar temas específicos

También puedes usar el **Asistente IA** para preguntas rápidas",
                Tip = "El Asistente IA está disponible desde cualquier página del sistema.",
                Ruta = "/manual-sistema",
                Icono = "bi-book",
                SugerenciasRelacionadas = new List<string> { "Asistente IA", "Contactar soporte" }
            },

            // ========== VENTAS ADICIONALES ==========
            new GuiaPasoAPaso
            {
                Tema = "venta",
                Patrones = new[] { "reimprimir factura", "imprimir factura", "duplicado factura" },
                Introduccion = "Para reimprimir una factura:",
                Pasos = @"1️⃣ Ve a **Ventas** → **Explorador de Ventas**
2️⃣ Busca la venta por:
   • Número de factura
   • Fecha
   • Cliente
3️⃣ Click en **👁️ Ver** para abrir la venta
4️⃣ Click en **🖨️ Imprimir**
5️⃣ Selecciona formato:
   • **Ticket**: Para impresora térmica
   • **A4/Carta**: Para impresora normal
   • **PDF**: Para guardar o enviar",
                Tip = "Las reimpresiones quedan registradas en auditoría.",
                Ruta = "/ventas/explorar",
                Icono = "bi-printer",
                SugerenciasRelacionadas = new List<string> { "Enviar por email", "Ver historial" }
            },

            new GuiaPasoAPaso
            {
                Tema = "presupuesto",
                Patrones = new[] { "convertir presupuesto", "presupuesto a venta", "facturar presupuesto" },
                Introduccion = "Para convertir un presupuesto en venta:",
                Pasos = @"1️⃣ Ve a **Ventas** → **Presupuestos**
2️⃣ Busca el presupuesto por número o cliente
3️⃣ Click en **👁️ Ver** para abrir
4️⃣ Verifica que el cliente aceptó
5️⃣ Click en **🔄 Convertir a Venta**
6️⃣ El sistema creará la venta con los mismos items
7️⃣ Confirma y procesa el pago
8️⃣ El presupuesto queda marcado como 'Facturado'",
                Tip = "Puedes ajustar productos y cantidades antes de confirmar la venta.",
                Ruta = "/presupuestos/explorar",
                Icono = "bi-arrow-right-circle",
                SugerenciasRelacionadas = new List<string> { "Crear presupuesto", "Nueva venta" }
            },

            new GuiaPasoAPaso
            {
                Tema = "venta",
                Patrones = new[] { "explorar venta", "buscar venta", "historial venta", "ver ventas" },
                Introduccion = "Para buscar y explorar ventas:",
                Pasos = @"1️⃣ Ve a **Ventas** → **Explorador de Ventas**
2️⃣ Usa los filtros:
   • **Rango de fechas**
   • **Cliente**: Busca por nombre o RUC
   • **Número**: Busca factura específica
   • **Estado**: Confirmadas, Anuladas, etc.
3️⃣ Los resultados se muestran en la tabla
4️⃣ Click en **👁️ Ver** para detalle completo
5️⃣ Desde el detalle puedes:
   • Reimprimir
   • Crear NC
   • Ver pagos",
                Tip = "Usa el filtro de fechas para acotar la búsqueda y mejorar rendimiento.",
                Ruta = "/ventas/explorar",
                Icono = "bi-search",
                SugerenciasRelacionadas = new List<string> { "Nueva venta", "Informe ventas" }
            },

            // ========== COMPRAS ADICIONALES ==========
            new GuiaPasoAPaso
            {
                Tema = "compra",
                Patrones = new[] { "explorar compra", "buscar compra", "historial compra", "ver compras" },
                Introduccion = "Para buscar y explorar compras:",
                Pasos = @"1️⃣ Ve a **Compras** → **Explorador de Compras**
2️⃣ Usa los filtros:
   • **Rango de fechas**
   • **Proveedor**: Busca por nombre o RUC
   • **Número factura**: Busca compra específica
   • **Estado**: Confirmadas, Anuladas
3️⃣ Los resultados se muestran en la tabla
4️⃣ Click en **👁️ Ver** para detalle completo
5️⃣ Desde el detalle puedes:
   • Ver productos comprados
   • Registrar pago
   • Crear NC",
                Tip = "Registra siempre el número exacto de la factura del proveedor.",
                Ruta = "/compras/explorar",
                Icono = "bi-search",
                SugerenciasRelacionadas = new List<string> { "Nueva compra", "Pagar proveedor" }
            },

            // ========== COBROS ADICIONALES ==========
            new GuiaPasoAPaso
            {
                Tema = "cobro",
                Patrones = new[] { "historial cobro", "ver cobros", "cobros realizados" },
                Introduccion = "Para ver el historial de cobros:",
                Pasos = @"1️⃣ Ve a **Informes** → **Historial de Cobros**
2️⃣ Filtra por:
   • **Rango de fechas**
   • **Cliente**
   • **Usuario que cobró**
3️⃣ Verás cada cobro con:
   • Fecha y hora
   • Cliente
   • Monto cobrado
   • Forma de pago
   • Recibo generado
4️⃣ Click en un cobro para ver detalle",
                Tip = "Exporta a Excel para conciliación bancaria.",
                Ruta = "/cobros/listado",
                Icono = "bi-receipt-cutoff",
                SugerenciasRelacionadas = new List<string> { "Registrar cobro", "Cuentas por cobrar" }
            },

            // ========== PAGOS ADICIONALES ==========
            new GuiaPasoAPaso
            {
                Tema = "pago",
                Patrones = new[] { "historial pago", "ver pagos", "pagos realizados", "pago proveedor historial" },
                Introduccion = "Para ver el historial de pagos a proveedores:",
                Pasos = @"1️⃣ Ve a **Informes** → **Historial de Pagos**
2️⃣ Filtra por:
   • **Rango de fechas**
   • **Proveedor**
   • **Forma de pago**
3️⃣ Verás cada pago con:
   • Fecha
   • Proveedor
   • Monto pagado
   • Forma de pago
   • Referencia/comprobante
4️⃣ Click en un pago para ver detalle",
                Tip = "Guarda los comprobantes de transferencia como respaldo.",
                Ruta = "/pagos-proveedores/historial",
                Icono = "bi-clock-history",
                SugerenciasRelacionadas = new List<string> { "Registrar pago", "Cuentas por pagar" }
            },

            // ========== CAMBIO DE SUCURSAL ==========
            new GuiaPasoAPaso
            {
                Tema = "sucursal",
                Patrones = new[] { "cambiar sucursal", "seleccionar sucursal", "otra sucursal" },
                Introduccion = "Para cambiar de sucursal activa:",
                Pasos = @"1️⃣ Ve a **Inventario** → **Cambiar Sucursal**
2️⃣ Verás las sucursales disponibles
3️⃣ Selecciona la sucursal deseada
4️⃣ El sistema cambiará el contexto

Todas las operaciones se realizarán en la nueva sucursal:
• Ventas
• Compras
• Inventario
• Informes",
                Tip = "Verifica siempre la sucursal activa antes de realizar operaciones.",
                Ruta = "/seleccionar-sucursal",
                Icono = "bi-arrow-left-right",
                SugerenciasRelacionadas = new List<string> { "Ver sucursales", "Configurar sucursal" }
            },

            // ========== INFORMES POR CLASIFICACIÓN ==========
            new GuiaPasoAPaso
            {
                Tema = "reporte",
                Patrones = new[] { "venta clasificacion", "venta categoria", "venta por familia", "analisis categoria" },
                Introduccion = "Para ver ventas por clasificación/categoría:",
                Pasos = @"1️⃣ Ve a **Informes** → **Por Clasificación**
2️⃣ Selecciona:
   • **Rango de fechas**
   • **Tipo de agrupación**: Categoría, Marca, o ambos
3️⃣ Click en **Generar**
4️⃣ Verás:
   • Gráfico de participación
   • Tabla con totales por categoría
   • Porcentaje del total
   • Comparativa con período anterior",
                Tip = "Ideal para identificar qué familias de productos son más rentables.",
                Ruta = "/informes/ventas-clasificacion",
                Icono = "bi-pie-chart",
                SugerenciasRelacionadas = new List<string> { "Informe ventas", "Stock valorizado" }
            },

            // ========== ELIMINAR PRODUCTOS ==========
            new GuiaPasoAPaso
            {
                Patrones = new[] { "eliminar producto", "borrar producto", "dar baja producto", "desactivar producto" },
                Introduccion = "Para eliminar o desactivar un producto:",
                Pasos = @"1️⃣ Ve a **Productos** → **Administrar**
2️⃣ Busca el producto
3️⃣ Click en **✏️ Editar**
4️⃣ Opciones:
   • **Desactivar**: Cambia estado a 'Inactivo'
     - No aparece en ventas
     - Se mantiene historial
   • **Eliminar**: Solo si no tiene movimientos
     - Click en **🗑️ Eliminar**
     - Confirma la acción

⚠️ Si el producto tiene ventas/compras, solo puede desactivarse",
                Tip = "Desactivar es mejor que eliminar para mantener historial de reportes.",
                Ruta = "/productos",
                Icono = "bi-trash",
                SugerenciasRelacionadas = new List<string> { "Editar producto", "Ajustar stock" }
            },

            // ========== VENTA A CREDITO ==========
            new GuiaPasoAPaso
            {
                Patrones = new[] { "venta credito", "vender credito", "venta cuota", "venta fiado", "financiar venta" },
                Introduccion = "Para realizar una venta a crédito:",
                Pasos = @"1️⃣ Inicia la venta normalmente en **Ventas**
2️⃣ Agrega el cliente (obligatorio para crédito)
3️⃣ Agrega los productos
4️⃣ En **Forma de Pago**, selecciona **Crédito**
5️⃣ Configura las cuotas:
   • **Cantidad de cuotas**
   • **Fecha primera cuota**
   • **Frecuencia**: Semanal, Quincenal, Mensual
6️⃣ El sistema calcula el valor de cada cuota
7️⃣ Click en **✅ Confirmar Venta**
8️⃣ Las cuotas se generan automáticamente

El cliente aparecerá en **Cuentas por Cobrar**",
                Tip = "Verifica el límite de crédito del cliente antes de confirmar.",
                Ruta = "/ventas",
                Icono = "bi-calendar2-check",
                SugerenciasRelacionadas = new List<string> { "Cobrar cuota", "Cuentas por cobrar" }
            },

            // ========== DESCUENTOS EN VENTA ==========
            new GuiaPasoAPaso
            {
                Tema = "descuento",
                Patrones = new[] { "aplicar descuento", "descuento venta", "hacer descuento", "dar descuento" },
                Introduccion = "Para aplicar descuentos en una venta:",
                Pasos = @"1️⃣ En la pantalla de **Ventas**, agrega productos
2️⃣ Opciones de descuento:
   
   **Por producto**:
   • Click en el producto agregado
   • Modifica el precio o aplica % descuento
   
   **Global (toda la venta)**:
   • Busca el campo **Descuento %**
   • Ingresa el porcentaje
   • Se aplica al subtotal
   
3️⃣ El total se recalcula automáticamente
4️⃣ El descuento aparece en la factura",
                Tip = "Los descuentos quedan registrados para análisis de rentabilidad.",
                Ruta = "/ventas",
                Icono = "bi-percent",
                SugerenciasRelacionadas = new List<string> { "Lista de precios", "Nueva venta" }
            },
        };
    } // fin clase AsistenteIAService

    /// <summary>
    /// Modelo para guías paso a paso
    /// </summary>
    public class GuiaPasoAPaso
    {
        /// <summary>
        /// Tema principal de la guía (venta, producto, cliente, etc.) - usado para matching
        /// </summary>
        public string Tema { get; set; } = "";
        public string[] Patrones { get; set; } = Array.Empty<string>(); // Obsoleto, usar Tema
        public string Introduccion { get; set; } = "";
        public string Pasos { get; set; } = "";
        public string Tip { get; set; } = "";
        public string Ruta { get; set; } = "";
        public string Icono { get; set; } = "";
        public List<string> SugerenciasRelacionadas { get; set; } = new();
    }
}
