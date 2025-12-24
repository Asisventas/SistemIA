using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cuota individual de una cuenta por cobrar
    /// </summary>
    public class CuentaPorCobrarCuota
    {
        [Key]
        public int IdCuota { get; set; }

        // Cuenta por cobrar a la que pertenece
        [Required]
        public int IdCuentaPorCobrar { get; set; }
        public CuentaPorCobrar? CuentaPorCobrar { get; set; }

        // Datos de la cuota
        public int NumeroCuota { get; set; } // 1, 2, 3, etc.

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoCuota { get; set; } // Monto original de la cuota

        [Column(TypeName = "decimal(18,4)")]
        public decimal SaldoCuota { get; set; } // Saldo pendiente de esta cuota

        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; } // Fecha en que se pagó completamente

        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, PAGADO, VENCIDO

        [MaxLength(280)]
        public string? Observaciones { get; set; }
    }
}
