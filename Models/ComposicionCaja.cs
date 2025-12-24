using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemIA.Models.Enums;

namespace SistemIA.Models
{
    /// <summary>
    /// Cabecera de la composición de caja por venta. Suma de detalles debe igualar total cobrado (contado) o anticipo (crédito).
    /// </summary>
    public class ComposicionCaja
    {
        [Key]
        public int IdComposicionCaja { get; set; }

        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Moneda principal del cobro y tipo de cambio aplicado
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? TipoCambioAplicado { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; } // Suma de detalles en moneda base (PYG)

        [Column(TypeName = "decimal(18,4)")]
        public decimal Vuelto { get; set; } // Vuelto entregado al cliente (si MontoTotal > TotalVenta)

        public ICollection<ComposicionCajaDetalle>? Detalles { get; set; }
    }

    /// <summary>
    /// Detalle de la composición de caja. Permite múltiples medios y monedas.
    /// </summary>
    public class ComposicionCajaDetalle
    {
        [Key]
        public int IdComposicionCajaDetalle { get; set; }

        public int IdComposicionCaja { get; set; }
        public ComposicionCaja? ComposicionCaja { get; set; }

        // Medio de pago
        public MedioPago Medio { get; set; } = MedioPago.Efectivo;

        // Moneda del pago y su tipo de cambio al momento
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? TipoCambio { get; set; }

        // Factores y montos
        [Column(TypeName = "decimal(18,4)")]
        public decimal Factor { get; set; } = 1m; // para equivalencias/commission si aplica

        [Column(TypeName = "decimal(18,4)")]
        public decimal Monto { get; set; } // en moneda del detalle

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoGs { get; set; } // convertido a PYG

        // Datos específicos según medio
        // Cheque
        [StringLength(40)] public string? BancoCheque { get; set; }
        [StringLength(30)] public string? NumeroCheque { get; set; }
        public DateTime? FechaCobroCheque { get; set; }

        // Tarjeta
        public Enums.TipoTarjeta? TipoTarjeta { get; set; }
        [StringLength(50)] public string? MarcaTarjeta { get; set; } // Visa, MasterCard
        [StringLength(4)] public string? Ultimos4 { get; set; }
        [StringLength(50)] public string? NumeroAutorizacion { get; set; }
        [StringLength(80)] public string? NombreEmisorTarjeta { get; set; }

        // Transferencia
        [StringLength(50)] public string? BancoTransferencia { get; set; }
        [StringLength(60)] public string? NumeroComprobante { get; set; }

        // Otros
        [StringLength(200)] public string? Observacion { get; set; }
    }
}
