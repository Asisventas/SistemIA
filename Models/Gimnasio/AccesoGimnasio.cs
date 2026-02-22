using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Registro de acceso al gimnasio (entradas y salidas).
    /// Incluye validación de membresía y reconocimiento facial.
    /// </summary>
    [Table("AccesosGimnasio")]
    public class AccesoGimnasio
    {
        [Key]
        public int IdAcceso { get; set; }

        // ========== SUCURSAL / CAJA / TURNO ==========
        
        /// <summary>
        /// Sucursal donde se registra el acceso
        /// </summary>
        [Required]
        public int IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        /// <summary>
        /// Caja/Terminal donde se registró el acceso
        /// </summary>
        public int? IdCaja { get; set; }
        [ForeignKey("IdCaja")]
        public Caja? Caja { get; set; }

        /// <summary>
        /// Turno en que se registró el acceso
        /// </summary>
        public int? Turno { get; set; }

        /// <summary>
        /// Fecha de caja operativa
        /// </summary>
        public DateTime? FechaCaja { get; set; }

        // ========== CLIENTE ==========
        
        /// <summary>
        /// Cliente que ingresa/egresa
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Membresía activa del cliente al momento del acceso
        /// </summary>
        public int? IdMembresia { get; set; }
        [ForeignKey("IdMembresia")]
        public MembresiaCliente? Membresia { get; set; }

        // ========== TIPO DE ACCESO ==========
        
        /// <summary>
        /// Tipo de registro: Entrada, Salida
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TipoAcceso { get; set; } = "Entrada";

        /// <summary>
        /// Fecha y hora del registro
        /// </summary>
        [Required]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        /// <summary>
        /// ID del registro de entrada relacionado (solo para salidas)
        /// </summary>
        public int? IdAccesoEntrada { get; set; }

        // ========== MÉTODO DE VERIFICACIÓN ==========
        
        /// <summary>
        /// Método de identificación: ReconocimientoFacial, Codigo, Documento, Manual
        /// </summary>
        [Required]
        [StringLength(30)]
        public string MetodoVerificacion { get; set; } = "ReconocimientoFacial";

        /// <summary>
        /// Porcentaje de coincidencia del reconocimiento facial (0-100)
        /// </summary>
        public float? PorcentajeCoincidencia { get; set; }

        /// <summary>
        /// Foto capturada al momento del acceso (opcional)
        /// </summary>
        public byte[]? FotoCaptura { get; set; }

        // ========== ESTADO DE MEMBRESÍA AL ACCESO ==========
        
        /// <summary>
        /// Estado de la membresía al momento del acceso: Vigente, Vencida, DiasGracia, SinMembresia
        /// </summary>
        [Required]
        [StringLength(30)]
        public string EstadoMembresiaAlAcceso { get; set; } = "Vigente";

        /// <summary>
        /// Días restantes de la membresía al momento del acceso
        /// </summary>
        public int? DiasRestantes { get; set; }

        /// <summary>
        /// Indica si el acceso fue permitido o denegado
        /// </summary>
        public bool AccesoPermitido { get; set; } = true;

        /// <summary>
        /// Motivo si el acceso fue denegado
        /// </summary>
        [StringLength(200)]
        public string? MotivoDenegacion { get; set; }

        /// <summary>
        /// Indica si se permitió acceso por excepción (días de gracia, cortesía, etc.)
        /// </summary>
        public bool AccesoPorExcepcion { get; set; } = false;

        /// <summary>
        /// Tipo de excepción: DiasGracia, Cortesia, AutorizacionManual
        /// </summary>
        [StringLength(30)]
        public string? TipoExcepcion { get; set; }

        // ========== ÁREA DE ACCESO ==========
        
        /// <summary>
        /// Área del gimnasio a la que se ingresa (General, Piscina, Sauna, etc.)
        /// </summary>
        [StringLength(50)]
        public string? AreaAcceso { get; set; } = "General";

        /// <summary>
        /// Terminal/Torniquete de acceso
        /// </summary>
        [StringLength(50)]
        public string? PuntoAcceso { get; set; }

        // ========== MENSAJES ==========
        
        /// <summary>
        /// Mensaje de bienvenida o despedida mostrado
        /// </summary>
        [StringLength(300)]
        public string? MensajeMostrado { get; set; }

        /// <summary>
        /// Indica si se reprodujo mensaje de voz
        /// </summary>
        public bool MensajeVozReproducido { get; set; } = false;

        // ========== DURACIÓN (solo para salidas) ==========
        
        /// <summary>
        /// Duración de la visita en minutos (calculado al registrar salida)
        /// </summary>
        public int? DuracionVisitaMinutos { get; set; }

        // ========== OBSERVACIONES ==========
        
        [StringLength(500)]
        public string? Observaciones { get; set; }

        // ========== AUDITORÍA ==========
        
        /// <summary>
        /// Usuario que registró el acceso (puede ser automático o manual)
        /// </summary>
        public int? IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Indica si el registro fue automático (facial) o manual
        /// </summary>
        public bool RegistroAutomatico { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Hora formateada del acceso
        /// </summary>
        [NotMapped]
        public string HoraFormateada => FechaHora.ToString("HH:mm:ss");

        /// <summary>
        /// Duración formateada (ej: "1h 30m")
        /// </summary>
        [NotMapped]
        public string DuracionFormateada
        {
            get
            {
                if (DuracionVisitaMinutos == null) return "-";
                var horas = DuracionVisitaMinutos.Value / 60;
                var minutos = DuracionVisitaMinutos.Value % 60;
                if (horas > 0)
                    return $"{horas}h {minutos}m";
                return $"{minutos}m";
            }
        }

        /// <summary>
        /// Descripción del estado de acceso para mostrar
        /// </summary>
        [NotMapped]
        public string EstadoDescripcion
        {
            get
            {
                if (!AccesoPermitido)
                    return $"❌ Denegado: {MotivoDenegacion}";
                if (AccesoPorExcepcion)
                    return $"⚠️ Por excepción ({TipoExcepcion})";
                return "✅ Permitido";
            }
        }

        /// <summary>
        /// Color CSS según el estado
        /// </summary>
        [NotMapped]
        public string ColorEstado
        {
            get
            {
                if (!AccesoPermitido) return "danger";
                if (AccesoPorExcepcion) return "warning";
                return "success";
            }
        }

        /// <summary>
        /// Icono según tipo de acceso
        /// </summary>
        [NotMapped]
        public string IconoTipoAcceso => TipoAcceso == "Entrada" ? "bi-box-arrow-in-right" : "bi-box-arrow-right";
    }
}
