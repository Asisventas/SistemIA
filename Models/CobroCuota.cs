using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cobro realizado sobre una o más cuotas
    /// </summary>
    public class CobroCuota
    {
        [Key]
        public int IdCobro { get; set; }

        // Cuenta por cobrar
        [Required]
        public int IdCuentaPorCobrar { get; set; }
        public CuentaPorCobrar? CuentaPorCobrar { get; set; }

        // Cliente
        [Required]
        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Datos del cobro
        public DateTime FechaCobro { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; } // Monto total cobrado en esta transacción

        // Moneda
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? CambioDelDia { get; set; } // Tipo de cambio usado para conversión si aplica

        [MaxLength(20)]
        public string Estado { get; set; } = "CONFIRMADO"; // CONFIRMADO, ANULADO

        [MaxLength(280)]
        public string? Observaciones { get; set; }

        // Usuario que registró el cobro
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // Caja donde se realizó el cobro
        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }
        
        // Sucursal donde se realizó el cobro
        public int? IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        
        // Turno de caja al momento del cobro
        public int? Turno { get; set; }

        // Número de recibo o comprobante
        [MaxLength(20)]
        public string? NumeroRecibo { get; set; }

        // Detalles de medios de pago
        public ICollection<CobroDetalle>? Detalles { get; set; }
    }
}
