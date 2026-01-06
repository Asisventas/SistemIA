using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models;

/// <summary>
/// Almacena el historial de cambios, mejoras y actualizaciones implementadas en el sistema.
/// Sirve como referencia para usuarios y para que la IA pueda consultar el contexto de trabajos realizados.
/// </summary>
public class HistorialCambioSistema
{
    [Key]
    public int IdHistorialCambio { get; set; }

    // ========== INFORMACIÓN DE LA VERSIÓN ==========
    [MaxLength(20)]
    public string? Version { get; set; }  // Ej: "1.5.2"

    [Required]
    public DateTime FechaCambio { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(100)]
    public string TituloCambio { get; set; } = "";

    // ========== CATEGORIZACIÓN ==========
    /// <summary>
    /// Tema principal del cambio para agrupación y búsqueda
    /// </summary>
    [MaxLength(100)]
    public string? Tema { get; set; }

    /// <summary>
    /// Tipo de cambio: Mejora, Corrección, Nueva Funcionalidad, Optimización, Seguridad, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TipoCambio { get; set; } = "Mejora";

    /// <summary>
    /// Módulo afectado: Ventas, Compras, Inventario, SIFEN, Sistema, etc.
    /// </summary>
    [MaxLength(50)]
    public string? ModuloAfectado { get; set; }

    /// <summary>
    /// Prioridad/Impacto: Alta, Media, Baja
    /// </summary>
    [MaxLength(20)]
    public string Prioridad { get; set; } = "Media";

    /// <summary>
    /// Tags/etiquetas para búsqueda (separados por coma)
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Referencias externas o relacionadas (URLs, tickets, IDs)
    /// </summary>
    [MaxLength(500)]
    public string? Referencias { get; set; }

    // ========== DESCRIPCIÓN DETALLADA ==========
    /// <summary>
    /// Descripción breve del cambio (para listados)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string DescripcionBreve { get; set; } = "";

    /// <summary>
    /// Descripción técnica detallada del cambio (para documentación y contexto IA)
    /// </summary>
    public string? DescripcionTecnica { get; set; }

    /// <summary>
    /// Archivos modificados (separados por coma o JSON)
    /// </summary>
    public string? ArchivosModificados { get; set; }

    /// <summary>
    /// Notas adicionales o instrucciones especiales
    /// </summary>
    public string? Notas { get; set; }

    // ========== TRAZABILIDAD ==========
    /// <summary>
    /// Usuario o desarrollador que implementó el cambio
    /// </summary>
    [MaxLength(100)]
    public string? ImplementadoPor { get; set; }

    /// <summary>
    /// Referencia a ticket, issue o solicitud
    /// </summary>
    [MaxLength(50)]
    public string? ReferenciaTicket { get; set; }

    /// <summary>
    /// ID de la conversación IA relacionada (si aplica)
    /// </summary>
    public int? IdConversacionIA { get; set; }

    // ========== ESTADO ==========
    /// <summary>
    /// Estado: Implementado, En Pruebas, Pendiente, Revertido
    /// </summary>
    [MaxLength(30)]
    public string Estado { get; set; } = "Implementado";

    /// <summary>
    /// Si el cambio requirió migración de BD
    /// </summary>
    public bool RequiereMigracion { get; set; } = false;

    /// <summary>
    /// Nombre de la migración EF Core (si aplica)
    /// </summary>
    [MaxLength(100)]
    public string? NombreMigracion { get; set; }

    // ========== AUDITORÍA ==========
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }

    // ========== NAVEGACIÓN ==========
    [ForeignKey(nameof(IdConversacionIA))]
    public virtual ConversacionIAHistorial? ConversacionIA { get; set; }
}

/// <summary>
/// Almacena el historial de conversaciones con IA (Claude Opus 4.5, etc.)
/// para mantener contexto de los trabajos realizados en el sistema.
/// </summary>
public class ConversacionIAHistorial
{
    [Key]
    public int IdConversacionIA { get; set; }

    // ========== INFORMACIÓN DE LA SESIÓN ==========
    [Required]
    public DateTime FechaInicio { get; set; } = DateTime.Now;

    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Modelo de IA utilizado: Claude Opus 4.5, GPT-4, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ModeloIA { get; set; } = "Claude Opus 4.5";

    /// <summary>
    /// Título descriptivo de la sesión/conversación
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = "";

    // ========== RESUMEN Y CONTEXTO ==========
    /// <summary>
    /// Resumen ejecutivo de lo trabajado en la sesión (para consulta rápida de la IA)
    /// </summary>
    [Required]
    public string ResumenEjecutivo { get; set; } = "";

    /// <summary>
    /// Objetivos planteados al inicio de la sesión
    /// </summary>
    public string? ObjetivosSesion { get; set; }

    /// <summary>
    /// Resultados obtenidos al finalizar la sesión
    /// </summary>
    public string? ResultadosObtenidos { get; set; }

    /// <summary>
    /// Tareas pendientes o próximos pasos identificados
    /// </summary>
    public string? TareasPendientes { get; set; }

    // ========== DETALLES TÉCNICOS ==========
    /// <summary>
    /// Lista de módulos/áreas trabajadas (JSON o separado por comas)
    /// </summary>
    public string? ModulosTrabajados { get; set; }

    /// <summary>
    /// Archivos creados (JSON array)
    /// </summary>
    public string? ArchivosCreados { get; set; }

    /// <summary>
    /// Archivos modificados (JSON array)
    /// </summary>
    public string? ArchivosModificados { get; set; }

    /// <summary>
    /// Migraciones de BD generadas
    /// </summary>
    public string? MigracionesGeneradas { get; set; }

    /// <summary>
    /// Problemas encontrados y cómo se resolvieron
    /// </summary>
    public string? ProblemasResoluciones { get; set; }

    /// <summary>
    /// Decisiones técnicas importantes tomadas
    /// </summary>
    public string? DecisionesTecnicas { get; set; }

    // ========== CATEGORIZACIÓN ==========
    /// <summary>
    /// Etiquetas para búsqueda: SIFEN, Ventas, UI, Bug Fix, etc.
    /// </summary>
    [MaxLength(500)]
    public string? Etiquetas { get; set; }

    /// <summary>
    /// Nivel de complejidad: Simple, Moderado, Complejo
    /// </summary>
    [MaxLength(20)]
    public string Complejidad { get; set; } = "Moderado";

    // ========== MÉTRICAS ==========
    /// <summary>
    /// Duración estimada de la sesión en minutos
    /// </summary>
    public int? DuracionMinutos { get; set; }

    /// <summary>
    /// Cantidad de cambios/commits realizados
    /// </summary>
    public int CantidadCambios { get; set; } = 0;

    // ========== AUDITORÍA ==========
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }

    // ========== NAVEGACIÓN ==========
    public virtual ICollection<HistorialCambioSistema> CambiosRelacionados { get; set; } = new List<HistorialCambioSistema>();
}
