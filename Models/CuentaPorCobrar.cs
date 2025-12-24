using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cuenta por cobrar (cabecera) - representa un crédito otorgado a un cliente
    /// </summary>
    public class CuentaPorCobrar
    {
        [Key]
        public int IdCuentaPorCobrar { get; set; }

        // Referencia a la venta original que generó el crédito
        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        // Cliente deudor
        [Required]
        public int IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Sucursal donde se originó
        [Required]
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // Datos del crédito
        public DateTime FechaCredito { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; } // Monto total del crédito

        [Column(TypeName = "decimal(18,4)")]
        public decimal SaldoPendiente { get; set; } // Saldo que falta pagar

        public int NumeroCuotas { get; set; } = 1; // Cantidad de cuotas
        public int PlazoDias { get; set; } = 30; // Plazo en días

        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, PAGADO, VENCIDO, CANCELADO

        // Moneda
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        [MaxLength(280)]
        public string? Observaciones { get; set; }

        // Usuario que autorizó el crédito
        public int? IdUsuarioAutorizo { get; set; }
        public Usuario? UsuarioAutorizo { get; set; }

        // Colección de cuotas
        public ICollection<CuentaPorCobrarCuota>? Cuotas { get; set; }

        // Colección de cobros realizados
        public ICollection<CobroCuota>? Cobros { get; set; }
    }
}
