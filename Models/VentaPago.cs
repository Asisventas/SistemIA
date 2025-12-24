using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemIA.Models.Enums;

namespace SistemIA.Models
{
    /// <summary>
    /// Información de pago/condición de la venta según SIFEN E7.
    /// </summary>
    public class VentaPago
    {
        [Key]
        public int IdVentaPago { get; set; }

        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        /// <summary>
        /// iCondOpe - 1=Contado, 2=Crédito
        /// </summary>
        public int CondicionOperacion { get; set; } = 1;

        // Moneda
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? TipoCambio { get; set; }

        // Totales
        [Column(TypeName = "decimal(18,4)")] public decimal ImporteTotal { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? Anticipo { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? DescuentoTotal { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? RecargoTotal { get; set; }

        // E7.1 - Pagos
        public ICollection<VentaPagoDetalle>? Detalles { get; set; }

        // E7.2 - Crédito
        public ICollection<VentaCuota>? Cuotas { get; set; }
    }

    public class VentaPagoDetalle
    {
        [Key]
        public int IdVentaPagoDetalle { get; set; }

        public int IdVentaPago { get; set; }
        public VentaPago? VentaPago { get; set; }

        public MedioPago Medio { get; set; } = MedioPago.Efectivo; // iTiPago

        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? TipoCambio { get; set; }

        [Column(TypeName = "decimal(18,4)")] public decimal Monto { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal MontoGs { get; set; }

        // Tarjeta
        public TipoTarjeta? TipoTarjeta { get; set; }
        [StringLength(50)] public string? MarcaTarjeta { get; set; }
        [StringLength(80)] public string? NombreEmisorTarjeta { get; set; }
        [StringLength(4)] public string? Ultimos4 { get; set; }
        [StringLength(50)] public string? NumeroAutorizacion { get; set; }

        // Cheque
        [StringLength(40)] public string? BancoCheque { get; set; }
        [StringLength(30)] public string? NumeroCheque { get; set; }
        public DateTime? FechaCobroCheque { get; set; }

        // Transferencia
        [StringLength(50)] public string? BancoTransferencia { get; set; }
        [StringLength(60)] public string? NumeroComprobante { get; set; }

        [StringLength(200)] public string? Observacion { get; set; }
    }

    /// <summary>
    /// Detalle de cuotas (E7.2). Si CondicionOperacion=2.
    /// </summary>
    public class VentaCuota
    {
        [Key]
        public int IdVentaCuota { get; set; }

        public int IdVentaPago { get; set; }
        public VentaPago? VentaPago { get; set; }

        public int NumeroCuota { get; set; }
        public DateTime FechaVencimiento { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal MontoCuota { get; set; }
        public bool Pagada { get; set; } = false;
        public DateTime? FechaPago { get; set; }
    }
}
