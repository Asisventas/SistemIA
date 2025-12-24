using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Remisión interna - documento que registra entrega de mercadería sin facturar
    /// </summary>
    public class RemisionInterna
    {
        [Key]
        public int IdRemision { get; set; }

        // Referencia a la venta/documento original
        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        // Cliente receptor
        [Required]
        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Sucursal de origen
        [Required]
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // Datos de la remisión
        public DateTime FechaRemision { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string NumeroRemision { get; set; } = string.Empty; // Número correlativo

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, FACTURADO, ANULADO

        // Referencia a la factura que liquidó esta remisión
        public int? IdVentaFactura { get; set; }
        public Venta? VentaFactura { get; set; }

        public DateTime? FechaFacturacion { get; set; }

        // Moneda
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        [MaxLength(280)]
        public string? Observaciones { get; set; }

        // Usuario que emitió
        public int? IdUsuarioEmitio { get; set; }
        public Usuario? UsuarioEmitio { get; set; }

        // Colección de detalles
        public ICollection<RemisionInternaDetalle>? Detalles { get; set; }
    }
}
