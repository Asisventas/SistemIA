using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class Presupuesto
    {
        [Key]
        public int IdPresupuesto { get; set; }

        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

    // Numeración propia de presupuesto
    [MaxLength(20)] public string? NumeroPresupuesto { get; set; }

        // Moneda y cambio
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        [MaxLength(4)] public string? SimboloMoneda { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? CambioDelDia { get; set; }
        public bool? EsMonedaExtranjera { get; set; }

        // Totales
        [Column(TypeName = "decimal(18,4)")] public decimal Total { get; set; }
        [MaxLength(280)] public string? TotalEnLetras { get; set; }

        // Validez
        public int? ValidezDias { get; set; }
        public DateTime? ValidoHasta { get; set; }

        // Varios
        [MaxLength(280)] public string? Comentario { get; set; }
        [MaxLength(20)] public string? Estado { get; set; } // Presupuesto/Borrador/Anulado

    // Conversión
    public int? IdVentaConvertida { get; set; }
    }
}
