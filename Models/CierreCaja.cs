using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemIA.Models.Enums;

namespace SistemIA.Models
{
    /// <summary>
    /// Registro de cierre de caja por turno. Almacena totales y entregas realizadas.
    /// </summary>
    [Table("CierresCaja")]
    public class CierreCaja
    {
        [Key]
        public int IdCierreCaja { get; set; }

        public int IdCaja { get; set; }
        public Caja? Caja { get; set; }
        
        /// <summary>
        /// Sucursal donde se realizó el cierre
        /// </summary>
        public int? IdSucursal { get; set; }

        public DateTime FechaCierre { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Fecha operativa del cierre (fecha de caja)
        /// </summary>
        public DateTime FechaCaja { get; set; }
        
        /// <summary>
        /// Turno que se está cerrando (1, 2, 3...)
        /// </summary>
        public int Turno { get; set; }

        /// <summary>
        /// Usuario que realiza el cierre
        /// </summary>
        [StringLength(100)]
        public string? UsuarioCierre { get; set; }

        // Totales calculados del turno
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVentasContado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVentasCredito { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCobrosCredito { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAnulaciones { get; set; }

        /// <summary>
        /// Total de Notas de Crédito emitidas en el turno (devoluciones/descuentos)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNotasCredito { get; set; }

        /// <summary>
        /// Cantidad de Notas de Crédito de Ventas emitidas en el turno
        /// </summary>
        public int CantNotasCredito { get; set; }

        /// <summary>
        /// Total de Notas de Crédito de Compras recibidas en el turno (ingresos de caja si afectan caja)
        /// Representan créditos del proveedor que pueden generar ingreso de efectivo
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNotasCreditoCompras { get; set; }

        /// <summary>
        /// Cantidad de Notas de Crédito de Compras recibidas en el turno
        /// </summary>
        public int CantNotasCreditoCompras { get; set; }

        /// <summary>
        /// Total de Compras en efectivo realizadas en el turno (egresos de caja)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalComprasEfectivo { get; set; }

        /// <summary>
        /// Cantidad de Compras en efectivo realizadas en el turno
        /// </summary>
        public int CantComprasEfectivo { get; set; }

        // Totales por medio de pago
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEfectivo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTarjetas { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCheques { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTransferencias { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalQR { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOtros { get; set; }

        // Total entregado vs esperado
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEsperado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEntregado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Diferencia { get; set; } // TotalEntregado - TotalEsperado

        [StringLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Estado del cierre: Pendiente, Cerrado, Revisado
        /// </summary>
        [StringLength(20)]
        public string Estado { get; set; } = "Cerrado";

        public ICollection<EntregaCaja>? Entregas { get; set; }
    }

    /// <summary>
    /// Detalle de entregas realizadas en el cierre de caja.
    /// Cada medio de pago se entrega por separado (efectivo, tarjetas, cheques, etc.)
    /// </summary>
    [Table("EntregasCaja")]
    public class EntregaCaja
    {
        [Key]
        public int IdEntregaCaja { get; set; }

        public int IdCierreCaja { get; set; }
        public CierreCaja? CierreCaja { get; set; }

        /// <summary>
        /// Medio de pago que se entrega
        /// </summary>
        public MedioPago Medio { get; set; } = MedioPago.Efectivo;

        /// <summary>
        /// Moneda de la entrega
        /// </summary>
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        /// <summary>
        /// Monto esperado según sistema
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoEsperado { get; set; }

        /// <summary>
        /// Monto realmente entregado
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoEntregado { get; set; }

        /// <summary>
        /// Diferencia (entregado - esperado)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Diferencia { get; set; }

        /// <summary>
        /// Persona que recibe la entrega
        /// </summary>
        [StringLength(100)]
        public string? ReceptorEntrega { get; set; }

        [StringLength(300)]
        public string? Observaciones { get; set; }

        // Para cheques: detalle adicional
        [StringLength(500)]
        public string? DetalleCheques { get; set; }

        // Para tarjetas: número de vouchers
        public int? CantidadVouchers { get; set; }
    }
}
