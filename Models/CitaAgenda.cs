using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cita del calendario/agenda del sistema.
    /// Permite agendar servicios, recordatorios, visitas a clientes, etc.
    /// </summary>
    [Table("CitasAgenda")]
    public class CitaAgenda
    {
        [Key]
        public int IdCita { get; set; }

        // ========== SUCURSAL ==========
        
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // ========== TIPO DE CITA ==========
        
        /// <summary>
        /// Tipo de cita: Servicio, Recordatorio, Visita, Reunión, Entrega, Llamada, Otro
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TipoCita { get; set; } = "Servicio";

        /// <summary>
        /// Título o asunto de la cita
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de la cita
        /// </summary>
        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        // ========== CLIENTE ==========
        
        /// <summary>
        /// ID del cliente asociado (opcional)
        /// </summary>
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Nombre del cliente (para mostrar en calendario sin consultar BD)
        /// </summary>
        [MaxLength(200)]
        public string? NombreCliente { get; set; }

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        [MaxLength(30)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Email de contacto
        /// </summary>
        [MaxLength(150)]
        public string? Email { get; set; }

        // ========== FECHA Y HORA ==========
        
        /// <summary>
        /// Fecha y hora de inicio de la cita
        /// </summary>
        public DateTime FechaHoraInicio { get; set; }

        /// <summary>
        /// Fecha y hora de fin de la cita
        /// </summary>
        public DateTime FechaHoraFin { get; set; }

        /// <summary>
        /// Si es un evento de todo el día
        /// </summary>
        public bool TodoElDia { get; set; } = false;

        /// <summary>
        /// Duración en minutos
        /// </summary>
        public int DuracionMinutos { get; set; } = 60;

        // ========== UBICACIÓN ==========
        
        /// <summary>
        /// Dirección del lugar de la cita
        /// </summary>
        [MaxLength(300)]
        public string? Direccion { get; set; }

        /// <summary>
        /// Latitud para Google Maps
        /// </summary>
        [Column(TypeName = "decimal(10,7)")]
        public decimal? Latitud { get; set; }

        /// <summary>
        /// Longitud para Google Maps
        /// </summary>
        [Column(TypeName = "decimal(10,7)")]
        public decimal? Longitud { get; set; }

        /// <summary>
        /// URL de Google Maps (se genera automáticamente)
        /// </summary>
        [MaxLength(500)]
        public string? UrlMaps { get; set; }

        // ========== VISUAL ==========
        
        /// <summary>
        /// Color de fondo para mostrar en el calendario (hexadecimal)
        /// </summary>
        [MaxLength(20)]
        public string ColorFondo { get; set; } = "#3788d8";

        /// <summary>
        /// Color del texto
        /// </summary>
        [MaxLength(20)]
        public string ColorTexto { get; set; } = "#ffffff";

        /// <summary>
        /// Icono Bootstrap a mostrar (ej: bi-calendar-check)
        /// </summary>
        [MaxLength(50)]
        public string? Icono { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado de la cita: Programada, Confirmada, EnProgreso, Completada, Cancelada, NoAsistio
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = "Programada";

        /// <summary>
        /// Prioridad: Baja, Normal, Alta, Urgente
        /// </summary>
        [MaxLength(20)]
        public string Prioridad { get; set; } = "Normal";

        // ========== RECORDATORIOS ==========
        
        /// <summary>
        /// Si tiene recordatorio activo
        /// </summary>
        public bool TieneRecordatorio { get; set; } = false;

        /// <summary>
        /// Minutos antes para el primer recordatorio
        /// </summary>
        public int? MinutosRecordatorio1 { get; set; }

        /// <summary>
        /// Minutos antes para el segundo recordatorio
        /// </summary>
        public int? MinutosRecordatorio2 { get; set; }

        /// <summary>
        /// Enviar recordatorio por correo
        /// </summary>
        public bool EnviarCorreo { get; set; } = false;

        /// <summary>
        /// Fecha/hora en que se envió el recordatorio por correo
        /// </summary>
        public DateTime? FechaEnvioCorreo { get; set; }

        /// <summary>
        /// Mostrar notificación en el sistema
        /// </summary>
        public bool MostrarNotificacion { get; set; } = true;

        /// <summary>
        /// Si ya se mostró la notificación
        /// </summary>
        public bool NotificacionMostrada { get; set; } = false;

        // ========== RECURRENCIA ==========
        
        /// <summary>
        /// Si es un evento recurrente
        /// </summary>
        public bool EsRecurrente { get; set; } = false;

        /// <summary>
        /// Tipo de recurrencia: Diario, Semanal, Mensual, Anual
        /// </summary>
        [MaxLength(20)]
        public string? TipoRecurrencia { get; set; }

        /// <summary>
        /// Intervalo de recurrencia (cada X días, semanas, etc.)
        /// </summary>
        public int? IntervaloRecurrencia { get; set; }

        /// <summary>
        /// Fecha hasta la cual se repite
        /// </summary>
        public DateTime? FechaFinRecurrencia { get; set; }

        /// <summary>
        /// ID de la cita padre (si es una instancia de recurrencia)
        /// </summary>
        public int? IdCitaPadre { get; set; }

        // ========== ASIGNACIÓN ==========
        
        /// <summary>
        /// ID del usuario responsable/asignado
        /// </summary>
        public int? IdUsuarioAsignado { get; set; }
        public Usuario? UsuarioAsignado { get; set; }

        /// <summary>
        /// Nombre del responsable asignado
        /// </summary>
        [MaxLength(100)]
        public string? NombreAsignado { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        
        public int? IdUsuarioCreador { get; set; }
        public Usuario? UsuarioCreador { get; set; }

        // ========== NOTAS ==========
        
        /// <summary>
        /// Notas internas adicionales
        /// </summary>
        [MaxLength(2000)]
        public string? Notas { get; set; }

        /// <summary>
        /// Resultado/conclusión de la cita (después de completarla)
        /// </summary>
        [MaxLength(1000)]
        public string? Resultado { get; set; }

        // ========== RELACIONES ==========
        
        /// <summary>
        /// Recordatorios de esta cita
        /// </summary>
        public ICollection<RecordatorioCita>? Recordatorios { get; set; }

        // ========== MÉTODOS AUXILIARES ==========
        
        /// <summary>
        /// Genera la URL de Google Maps basada en las coordenadas o dirección
        /// </summary>
        public string GenerarUrlMaps()
        {
            if (Latitud.HasValue && Longitud.HasValue)
            {
                return $"https://www.google.com/maps?q={Latitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Longitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            else if (!string.IsNullOrEmpty(Direccion))
            {
                return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(Direccion)}";
            }
            return string.Empty;
        }

        /// <summary>
        /// Obtiene el icono según el tipo de cita
        /// </summary>
        public string ObtenerIcono()
        {
            if (!string.IsNullOrEmpty(Icono)) return Icono;

            return TipoCita switch
            {
                "Servicio" => "bi-tools",
                "Recordatorio" => "bi-bell",
                "Visita" => "bi-geo-alt",
                "Reunión" => "bi-people",
                "Entrega" => "bi-truck",
                "Llamada" => "bi-telephone",
                "Cita Médica" => "bi-heart-pulse",
                "Mantenimiento" => "bi-wrench",
                _ => "bi-calendar-event"
            };
        }

        /// <summary>
        /// Obtiene la clase CSS del badge según el estado
        /// </summary>
        public string ObtenerClaseBadgeEstado()
        {
            return Estado switch
            {
                "Programada" => "bg-secondary",
                "Confirmada" => "bg-primary",
                "EnProgreso" => "bg-info",
                "Completada" => "bg-success",
                "Cancelada" => "bg-danger",
                "NoAsistio" => "bg-warning text-dark",
                _ => "bg-secondary"
            };
        }
    }

    /// <summary>
    /// Recordatorio de una cita (para historial y alarmas)
    /// </summary>
    [Table("RecordatoriosCitas")]
    public class RecordatorioCita
    {
        [Key]
        public int IdRecordatorio { get; set; }

        public int IdCita { get; set; }
        public CitaAgenda? Cita { get; set; }

        /// <summary>
        /// Fecha y hora programada del recordatorio
        /// </summary>
        public DateTime FechaHoraProgramada { get; set; }

        /// <summary>
        /// Tipo: Notificacion, Correo, Ambos
        /// </summary>
        [MaxLength(20)]
        public string Tipo { get; set; } = "Notificacion";

        /// <summary>
        /// Si ya fue enviado/mostrado
        /// </summary>
        public bool Enviado { get; set; } = false;

        /// <summary>
        /// Fecha y hora en que se envió
        /// </summary>
        public DateTime? FechaEnvio { get; set; }

        /// <summary>
        /// Mensaje del recordatorio
        /// </summary>
        [MaxLength(500)]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Minutos de anticipación para el recordatorio (15, 30, 60, 1440, etc.)
        /// </summary>
        public int MinutosAntes { get; set; } = 30;

        /// <summary>
        /// Resultado del envío del recordatorio
        /// </summary>
        [MaxLength(200)]
        public string? Resultado { get; set; }
    }

    /// <summary>
    /// Colores predefinidos para clientes en el calendario
    /// </summary>
    [Table("ColoresClientesAgenda")]
    public class ColorClienteAgenda
    {
        [Key]
        public int IdColorCliente { get; set; }

        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        /// <summary>
        /// Color de fondo hexadecimal
        /// </summary>
        [MaxLength(20)]
        public string ColorFondo { get; set; } = "#3788d8";

        /// <summary>
        /// Color de texto hexadecimal
        /// </summary>
        [MaxLength(20)]
        public string ColorTexto { get; set; } = "#ffffff";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
