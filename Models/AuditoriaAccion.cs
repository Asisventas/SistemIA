using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Registra todas las acciones realizadas por usuarios en el sistema para auditoría
    /// </summary>
    [Table("AuditoriasAcciones")]
    public class AuditoriaAccion
    {
        [Key]
        public int IdAuditoria { get; set; }

        /// <summary>
        /// ID del usuario que realizó la acción (null si es sistema)
        /// </summary>
        public int? IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Nombre del usuario en el momento de la acción (para histórico)
        /// </summary>
        [StringLength(200)]
        public string? NombreUsuario { get; set; }

        /// <summary>
        /// Rol del usuario en el momento de la acción
        /// </summary>
        [StringLength(100)]
        public string? RolUsuario { get; set; }

        /// <summary>
        /// Fecha y hora de la acción
        /// </summary>
        public DateTime FechaHora { get; set; } = DateTime.Now;

        /// <summary>
        /// Módulo donde se realizó la acción
        /// </summary>
        [StringLength(100)]
        public string? Modulo { get; set; }

        /// <summary>
        /// Acción realizada (ej: "Crear Venta", "Editar Cliente", "Eliminar Producto")
        /// </summary>
        [Required, StringLength(200)]
        public string Accion { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de acción (CREATE, READ, UPDATE, DELETE, EXPORT, PRINT, LOGIN, LOGOUT)
        /// </summary>
        [StringLength(50)]
        public string? TipoAccion { get; set; }

        /// <summary>
        /// Entidad afectada (ej: "Venta", "Cliente", "Producto")
        /// </summary>
        [StringLength(100)]
        public string? Entidad { get; set; }

        /// <summary>
        /// ID del registro afectado (si aplica)
        /// </summary>
        public int? IdRegistroAfectado { get; set; }

        /// <summary>
        /// Descripción detallada de la acción
        /// </summary>
        [StringLength(2000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Datos antes del cambio (JSON) - para UPDATE/DELETE
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? DatosAntes { get; set; }

        /// <summary>
        /// Datos después del cambio (JSON) - para CREATE/UPDATE
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? DatosDespues { get; set; }

        /// <summary>
        /// Dirección IP del usuario
        /// </summary>
        [StringLength(50)]
        public string? DireccionIP { get; set; }

        /// <summary>
        /// Navegador del usuario
        /// </summary>
        [StringLength(500)]
        public string? Navegador { get; set; }

        // ========== CONTEXTO DE OPERACIÓN ==========
        
        /// <summary>
        /// Fecha/hora del equipo del usuario
        /// </summary>
        public DateTime? FechaHoraEquipo { get; set; }
        
        /// <summary>
        /// Fecha de caja activa al momento de la acción
        /// </summary>
        public DateTime? FechaCaja { get; set; }
        
        /// <summary>
        /// Turno activo al momento de la acción
        /// </summary>
        public int? Turno { get; set; }
        
        /// <summary>
        /// ID de la sucursal donde se realizó la acción
        /// </summary>
        public int? IdSucursal { get; set; }
        
        /// <summary>
        /// Nombre de la sucursal (para histórico)
        /// </summary>
        [StringLength(200)]
        public string? NombreSucursal { get; set; }
        
        /// <summary>
        /// ID de la caja donde se realizó la acción
        /// </summary>
        public int? IdCaja { get; set; }
        
        /// <summary>
        /// Nombre de la caja (para histórico)
        /// </summary>
        [StringLength(100)]
        public string? NombreCaja { get; set; }

        /// <summary>
        /// Si la acción fue exitosa
        /// </summary>
        public bool Exitosa { get; set; } = true;

        /// <summary>
        /// Mensaje de error si la acción falló
        /// </summary>
        [StringLength(2000)]
        public string? MensajeError { get; set; }

        /// <summary>
        /// Nivel de severidad (INFO, WARNING, ERROR, CRITICAL)
        /// </summary>
        [StringLength(20)]
        public string? Severidad { get; set; } = "INFO";
    }
}
