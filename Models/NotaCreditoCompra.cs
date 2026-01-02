using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Models
{
    /// <summary>
    /// Nota de Crédito de Compra - Documento que reduce el saldo de una factura de compra
    /// Motivos: Devolución, Descuento, Bonificación, Ajuste de precio, etc.
    /// </summary>
    [Index(nameof(IdSucursal), nameof(NumeroNota), IsUnique = true, Name = "IX_NotaCreditoCompras_Numeracion")]
    [Index(nameof(IdCompraAsociada))]
    public class NotaCreditoCompra
    {
        [Key]
        public int IdNotaCreditoCompra { get; set; }

        // ========== NUMERACIÓN ==========
        [MaxLength(3)]
        public string? Establecimiento { get; set; }
        
        [MaxLength(3)]
        public string? PuntoExpedicion { get; set; }
        
        [MaxLength(7)]
        public string? NumeroNota { get; set; }

        // ========== RELACIONES PRINCIPALES ==========
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }

        public int IdProveedor { get; set; }
        public ProveedorSifenMejorado? Proveedor { get; set; }
        
        [MaxLength(200)]
        public string? NombreProveedor { get; set; }
        
        [MaxLength(20)]
        public string? RucProveedor { get; set; }

        // ========== COMPRA/FACTURA ASOCIADA ==========
        public int? IdCompraAsociada { get; set; }
        public Compra? CompraAsociada { get; set; }

        // Datos del documento asociado (para casos sin IdCompraAsociada)
        [MaxLength(3)]
        public string? EstablecimientoAsociado { get; set; }
        
        [MaxLength(3)]
        public string? PuntoExpedicionAsociado { get; set; }
        
        [MaxLength(7)]
        public string? NumeroFacturaAsociado { get; set; }
        
        [MaxLength(8)]
        public string? TimbradoAsociado { get; set; }

        // ========== FECHAS ==========
        public DateTime Fecha { get; set; } = DateTime.Now;
        public DateTime? FechaContable { get; set; }
        
        public int? Turno { get; set; }

        // ========== MOTIVO ==========
        [MaxLength(50)]
        public string Motivo { get; set; } = "Devolución";
        
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ========== MONEDA ==========
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        
        [MaxLength(4)]
        public string? SimboloMoneda { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal CambioDelDia { get; set; } = 1;
        
        public bool EsMonedaExtranjera { get; set; }

        // ========== TOTALES ==========
        [Column(TypeName = "decimal(18,4)")]
        public decimal Subtotal { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIVA10 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIVA5 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalExenta { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDescuento { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }
        
        [MaxLength(280)]
        public string? TotalEnLetras { get; set; }

        // ========== ESTADO ==========
        [MaxLength(20)]
        public string Estado { get; set; } = "Borrador"; // Borrador / Confirmada / Anulada

        // ========== IMPUTACIONES TRIBUTARIAS ==========
        public bool? ImputarIVA { get; set; }
        public bool? ImputarIRP { get; set; }
        public bool? ImputarIRE { get; set; }
        public bool? NoImputar { get; set; }

        // ========== DEPÓSITO (para devolución de stock) ==========
        public int? IdDeposito { get; set; }
        public Deposito? Deposito { get; set; }
        
        /// <summary>
        /// Indica cómo afecta el stock: -1 = Resta (devolución a proveedor), 1 = Suma (ingreso), 0 = No afecta
        /// </summary>
        public int AfectaStock { get; set; } = 0;

        // ========== USUARIO ==========
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // ========== AUDITORÍA ==========
        [MaxLength(100)]
        public string? CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // ========== DETALLES ==========
        public ICollection<NotaCreditoCompraDetalle>? Detalles { get; set; }
    }

    /// <summary>
    /// Catálogo de motivos de nota de crédito de compra
    /// </summary>
    public static class MotivosNotaCreditoCompra
    {
        public const string DevolucionAjustePrecios = "Devolución y Ajuste de precios";
        public const string Devolucion = "Devolución";
        public const string Descuento = "Descuento";
        public const string Bonificacion = "Bonificación";
        public const string AjustePrecio = "Ajuste de precio";
        public const string ProductoDefectuoso = "Producto defectuoso";
        public const string ErrorFacturacion = "Error de facturación";

        public static readonly string[] Todos = new[]
        {
            DevolucionAjustePrecios,
            Devolucion,
            Descuento,
            Bonificacion,
            AjustePrecio,
            ProductoDefectuoso,
            ErrorFacturacion
        };
    }
}
