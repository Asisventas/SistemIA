using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Tarjeta de pago registrada por el cliente para cobros automáticos.
    /// Almacena datos tokenizados (NUNCA el número completo de la tarjeta).
    /// </summary>
    [Table("TarjetasPagoCliente")]
    public class TarjetaPagoCliente
    {
        [Key]
        public int IdTarjeta { get; set; }

        // ========== RELACIONES ==========
        
        /// <summary>
        /// Cliente propietario de la tarjeta
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        // ========== DATOS DE LA TARJETA (TOKENIZADOS) ==========
        
        /// <summary>
        /// Alias de la tarjeta (ej: "Mi Visa Personal")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de tarjeta: Credito, Debito
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TipoTarjeta { get; set; } = "Credito";

        /// <summary>
        /// Marca de la tarjeta: Visa, Mastercard, etc.
        /// </summary>
        [Required]
        [StringLength(30)]
        public string MarcaTarjeta { get; set; } = string.Empty;

        /// <summary>
        /// Últimos 4 dígitos de la tarjeta (para mostrar al usuario)
        /// </summary>
        [Required]
        [StringLength(4)]
        public string Ultimos4Digitos { get; set; } = string.Empty;

        /// <summary>
        /// Mes de expiración (1-12)
        /// </summary>
        [Required]
        public int MesExpiracion { get; set; }

        /// <summary>
        /// Año de expiración (4 dígitos)
        /// </summary>
        [Required]
        public int AnioExpiracion { get; set; }

        /// <summary>
        /// Nombre del titular como aparece en la tarjeta
        /// </summary>
        [Required]
        [StringLength(100)]
        public string NombreTitular { get; set; } = string.Empty;

        /// <summary>
        /// Token de la tarjeta proporcionado por el procesador de pagos
        /// (ej: Bancard, PagoParaguay, etc.)
        /// </summary>
        [StringLength(500)]
        public string? TokenProcesador { get; set; }

        /// <summary>
        /// ID del procesador de pagos (Bancard, etc.)
        /// </summary>
        [StringLength(50)]
        public string? IdProcesadorPago { get; set; }

        // ========== CONFIGURACIÓN ==========
        
        /// <summary>
        /// Es la tarjeta predeterminada para cobros automáticos
        /// </summary>
        public bool EsPredeterminada { get; set; } = false;

        /// <summary>
        /// Permite cobros automáticos con esta tarjeta
        /// </summary>
        public bool PermiteCobrosAutomaticos { get; set; } = true;

        /// <summary>
        /// Límite de monto por transacción (0 = sin límite)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal LimiteMontoTransaccion { get; set; } = 0;

        // ========== ESTADO ==========
        
        /// <summary>
        /// Tarjeta activa
        /// </summary>
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Fecha de registro
        /// </summary>
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        /// <summary>
        /// Última fecha de uso exitoso
        /// </summary>
        public DateTime? UltimoUso { get; set; }

        /// <summary>
        /// Número de transacciones realizadas
        /// </summary>
        public int TransaccionesRealizadas { get; set; } = 0;

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Descripción corta: "Visa ****1234"
        /// </summary>
        [NotMapped]
        public string DescripcionCorta => $"{MarcaTarjeta} ****{Ultimos4Digitos}";

        /// <summary>
        /// Verifica si la tarjeta está expirada
        /// </summary>
        [NotMapped]
        public bool EstaExpirada
        {
            get
            {
                var fechaExpiracion = new DateTime(AnioExpiracion, MesExpiracion, 1).AddMonths(1).AddDays(-1);
                return DateTime.Now > fechaExpiracion;
            }
        }

        /// <summary>
        /// Fecha de expiración formateada (MM/YY)
        /// </summary>
        [NotMapped]
        public string FechaExpiracionFormateada => $"{MesExpiracion:D2}/{AnioExpiracion % 100:D2}";

        /// <summary>
        /// Icono de la marca (para UI)
        /// </summary>
        [NotMapped]
        public string IconoMarca => MarcaTarjeta?.ToLower() switch
        {
            "visa" => "bi-credit-card-2-front",
            "mastercard" => "bi-credit-card",
            "american express" => "bi-credit-card-fill",
            "amex" => "bi-credit-card-fill",
            _ => "bi-credit-card"
        };
    }
}
