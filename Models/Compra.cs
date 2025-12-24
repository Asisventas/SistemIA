using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class Compra
    {
        [Key]
        public int IdCompra { get; set; }

        // Documento fiscal (opcional) – alineado a SIFEN
        [StringLength(3)] public string? Establecimiento { get; set; } // dEst
        [StringLength(3)] public string? PuntoExpedicion { get; set; } // dPunExp
        [StringLength(7)] public string? NumeroFactura { get; set; }   // dNumDoc
        [StringLength(8)] public string? Timbrado { get; set; }        // dNumTim

        // Relaciones principales
        public int IdSucursal { get; set; } // columna "suc"
        public int IdProveedor { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdMoneda { get; set; }
        public int? IdDeposito { get; set; } // Depósito destino de la mercadería (opcional)
    public int? IdTipoPago { get; set; } // Catálogo configurable de tipos de pago
    public int? IdTipoDocumentoOperacion { get; set; } // Catálogo configurable del tipo de documento

        // Datos de operación
        public DateTime Fecha { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Total { get; set; }
    [StringLength(50)] public string? FormaPago { get; set; } // Texto libre, deprecated a futuro por IdTipoPago
        [StringLength(13)] public string MedioPago { get; set; } = "EFECTIVO"; // EFECTIVO, TARJETA, CHEQUE, TRANSFERENCIA, QR
        public int? PlazoDias { get; set; }
        [StringLength(20)] public string? Estado { get; set; } = "Borrador"; // Borrador/Confirmado/Anulado
        [StringLength(12)] public string TipoDocumento { get; set; } = "FACTURA"; // FACTURA, N/C, etc.
        [StringLength(13)] public string TipoIngreso { get; set; } = "COMPRAS";
        public int? CodigoDocumento { get; set; }
        [StringLength(280)] public string? TotalEnLetras { get; set; }
        public int? Turno { get; set; }
        public int? IdCaja { get; set; }

        // Imputaciones tributarias
        public bool? ImputarIVA { get; set; }
        public bool? ImputarIRP { get; set; }
        public bool? ImputarIRE { get; set; }
        public bool? NoImputar { get; set; }

        [StringLength(280)] public string? Comentario { get; set; }
        public bool? EsFacturaElectronica { get; set; }
        [StringLength(20)] public string? TipoRegistro { get; set; }
        public int? CodigoRegistro { get; set; }
        [StringLength(10)] public string? CodigoCondicion { get; set; } // p.ej. CONTADO/CREDITO

        // Moneda/FX
        public bool? EsMonedaExtranjera { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? CreditoSaldo { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal? CambioDelDia { get; set; }
        [StringLength(4)] public string? SimboloMoneda { get; set; }

        public int? IdCajaChica { get; set; }
        [StringLength(20)] public string? Vendedor { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

    // Navigation
    public ProveedorSifenMejorado? Proveedor { get; set; }
        public Sucursal? Sucursal { get; set; }
        public Usuario? Usuario { get; set; }
        public Moneda? Moneda { get; set; }
        public Deposito? Deposito { get; set; }
    public Caja? Caja { get; set; }
    public TipoPago? TipoPago { get; set; }
    public TipoDocumentoOperacion? TipoDocumentoOperacion { get; set; }
        public ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
    }
}
