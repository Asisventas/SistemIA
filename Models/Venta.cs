using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Models
{
    [Index(nameof(IdLote))]
    [Index(nameof(Establecimiento), nameof(PuntoExpedicion), nameof(NumeroFactura), nameof(Timbrado), nameof(Serie), IsUnique = true, Name = "IX_Ventas_Numeracion_Unica")]
    public class Venta
    {
        [Key]
        public int IdVenta { get; set; }

        // Sucursal (obligatorio) -> columna 'suc'
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // Cliente (receptor SIFEN)
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // Numeración SIFEN
        [MaxLength(8)]
        public string? Timbrado { get; set; }

        [MaxLength(3)]
        public string? Establecimiento { get; set; }

        [MaxLength(3)]
        public string? PuntoExpedicion { get; set; }

        [MaxLength(7)]
        public string? NumeroFactura { get; set; }

        // Serie del timbrado
        public int? Serie { get; set; }

        // Datos generales de la operación
        public DateTime Fecha { get; set; } = DateTime.Now;

    // Moneda y cambio
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        [MaxLength(4)] public string? SimboloMoneda { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? CambioDelDia { get; set; }
    public bool? EsMonedaExtranjera { get; set; }

        // Condición y totales
    [MaxLength(50)] public string? FormaPago { get; set; } // Contado/Credito/Otros
    public int? IdTipoPago { get; set; }
    public TipoPago? TipoPago { get; set; }
    [MaxLength(10)] public string? CodigoCondicion { get; set; } // p.ej. CONTADO/CREDITO
    [MaxLength(13)] public string MedioPago { get; set; } = "EFECTIVO"; // EFECTIVO, TARJETA, CHEQUE, TRANSFERENCIA, QR
        public int? Plazo { get; set; } // días, si es crédito
        public DateTime? FechaVencimiento { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Total { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal? CreditoSaldo { get; set; }
    [NotMapped] public decimal? CreditoUsado { get; set; } // Campo calculado para mostrar saldo disponible
        [MaxLength(280)] public string? TotalEnLetras { get; set; }

        // Varios
        [MaxLength(20)] public string? TipoRegistro { get; set; }
        public int? CodigoRegistro { get; set; }
        [MaxLength(280)] public string? Comentario { get; set; }
        [MaxLength(20)] public string? Estado { get; set; } // Borrador/Confirmado/Anulado

        // Caja / Turno
        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }
        public int? Turno { get; set; }

        // Usuario y vendedor
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        [MaxLength(250)] public string? Vendedor { get; set; }

        // SIFEN (facturación electrónica)
        [MaxLength(64)] public string? CDC { get; set; }
        [MaxLength(9)] public string? CodigoSeguridad { get; set; } // Código de seguridad de 9 dígitos para CDC
        [MaxLength(30)] public string? EstadoSifen { get; set; } // PENDIENTE/ENVIADO/ACEPTADO/RECHAZADO
        public DateTime? FechaEnvioSifen { get; set; }
        public string? MensajeSifen { get; set; }
        public string? XmlCDE { get; set; }
    [MaxLength(50)] public string? IdLote { get; set; }
    public string? UrlQrSifen { get; set; } // URL completa del QR con hash (dCarQR del XML firmado)

    // Referencias documentales
        [MaxLength(12)] public string? TipoDocumento { get; set; } // Factura, NotaCredito, etc.
        public int? IdTipoDocumentoOperacion { get; set; }
        public TipoDocumentoOperacion? TipoDocumentoOperacion { get; set; }
    [MaxLength(20)] public string? NroPedido { get; set; } // Número de pedido/presupuesto de referencia

    // Clasificación general
    [MaxLength(13)] public string TipoIngreso { get; set; } = "VENTAS";
    
    // Referencia al presupuesto origen (si fue convertido desde un presupuesto)
    public int? IdPresupuestoOrigen { get; set; }
    }
}
