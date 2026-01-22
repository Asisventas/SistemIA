using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Representa una salida de stock (productos vencidos, mermas, dañados, etc.)
    /// </summary>
    [Table("SalidasStock")]
    public class SalidaStock
    {
        [Key]
        public int IdSalidaStock { get; set; }

        [Required]
        public int IdDeposito { get; set; }

        [Required]
        public DateTime FechaSalida { get; set; } = DateTime.Now;

        /// <summary>Motivo de la salida: Vencimiento, Merma, Rotura, Daño, Robo, Donación, Muestrario, Autoconsumo, Otro</summary>
        [Required]
        [StringLength(50)]
        public string MotivoSalida { get; set; } = "Vencimiento";

        /// <summary>Observación o descripción detallada de la salida</summary>
        [StringLength(1000)]
        public string? Observacion { get; set; }

        /// <summary>Número de documento interno o referencia</summary>
        [StringLength(50)]
        public string? NumeroDocumento { get; set; }

        /// <summary>Usuario que registró la salida</summary>
        [StringLength(100)]
        public string? UsuarioCreacion { get; set; }

        /// <summary>Estado del documento: Borrador, Confirmada, Anulada</summary>
        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // ========== AUDITORÍA ==========
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaConfirmacion { get; set; }
        [StringLength(100)]
        public string? UsuarioConfirmacion { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        [StringLength(100)]
        public string? UsuarioAnulacion { get; set; }
        [StringLength(500)]
        public string? MotivoAnulacion { get; set; }

        // ========== NAVEGACIÓN ==========
        [ForeignKey(nameof(IdDeposito))]
        public virtual Deposito? Deposito { get; set; }

        public virtual ICollection<SalidaStockDetalle> Detalles { get; set; } = new List<SalidaStockDetalle>();
    }

    /// <summary>
    /// Motivos predefinidos para salida de stock
    /// </summary>
    public static class MotivosSalidaStock
    {
        public const string Vencimiento = "Vencimiento";
        public const string Merma = "Merma";
        public const string Rotura = "Rotura";
        public const string Daño = "Daño";
        public const string Robo = "Robo";
        public const string Donacion = "Donación";
        public const string Muestrario = "Muestrario";
        public const string Autoconsumo = "Autoconsumo";
        public const string DevolucionProveedor = "Devolución Proveedor";
        public const string Otro = "Otro";

        public static List<string> ObtenerTodos() => new()
        {
            Vencimiento,
            Merma,
            Rotura,
            Daño,
            Robo,
            Donacion,
            Muestrario,
            Autoconsumo,
            DevolucionProveedor,
            Otro
        };
    }
}
