using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("TiposCambio")]
    public class TipoCambio
    {
        [Key]
        public int IdTipoCambio { get; set; }

        [Required]
        [Display(Name = "Moneda Origen")]
        public int IdMonedaOrigen { get; set; }

        [ForeignKey("IdMonedaOrigen")]
        public virtual Moneda MonedaOrigen { get; set; } = null!;

        [Required]
        [Display(Name = "Moneda Destino")]
        public int IdMonedaDestino { get; set; }

        [ForeignKey("IdMonedaDestino")]
        public virtual Moneda MonedaDestino { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Tasa de Cambio")]
        public decimal TasaCambio { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Tasa de Compra")]
        public decimal? TasaCompra { get; set; }

        [Required]
        [Display(Name = "Fecha del Tipo de Cambio")]
        public DateTime FechaTipoCambio { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Fuente")]
        public string? Fuente { get; set; } // API utilizada

        [Display(Name = "Es Automático")]
        public bool EsAutomatico { get; set; } = true; // Si se actualiza automáticamente

        [Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        // Propiedades de auditoría
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string DescripcionCompleta => $"1 {MonedaOrigen?.CodigoISO} = {TasaCambio:N6} {MonedaDestino?.CodigoISO}";

        [NotMapped]
        public bool EsActual => FechaTipoCambio.Date == DateTime.Today;

        [NotMapped]
        public TimeSpan TiempoDesdeActualizacion => DateTime.Now - (FechaModificacion ?? FechaCreacion);

        // Método para convertir montos
        public decimal ConvertirMonto(decimal monto)
        {
            return monto * TasaCambio;
        }

        // Método para convertir monto inverso
        public decimal ConvertirMontoInverso(decimal monto)
        {
            return TasaCambio != 0 ? monto / TasaCambio : 0;
        }
    }
}
