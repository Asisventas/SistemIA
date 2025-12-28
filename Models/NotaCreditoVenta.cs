using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Models
{
    /// <summary>
    /// Nota de Crédito de Venta - Documento que reduce el saldo de una factura
    /// Motivos: Devolución, Descuento, Bonificación, Crédito incobrable, Ajuste de precio, etc.
    /// </summary>
    [Index(nameof(IdSucursal), nameof(NumeroNota), IsUnique = true, Name = "IX_NotaCreditoVentas_Numeracion")]
    [Index(nameof(IdVentaAsociada))]
    public class NotaCreditoVenta
    {
        [Key]
        public int IdNotaCredito { get; set; }
        
        // Alias para compatibilidad
        [NotMapped]
        public int IdNotaCreditoVenta => IdNotaCredito;

        // Numeración
        [MaxLength(3)]
        public string? Establecimiento { get; set; }
        
        [MaxLength(3)]
        public string? PuntoExpedicion { get; set; }
        
        public int NumeroNota { get; set; }

        // Relaciones principales
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int IdCaja { get; set; }
        public Caja? Caja { get; set; }

        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }
        
        [MaxLength(200)]
        public string? NombreCliente { get; set; }

        // Venta/Factura asociada
        public int? IdVentaAsociada { get; set; }
        public Venta? VentaAsociada { get; set; }

        // Tipo de documento asociado: Electrónico / Impreso
        [MaxLength(20)]
        public string? TipoDocumentoAsociado { get; set; }

        // Fechas
        public DateTime Fecha { get; set; } = DateTime.Now;
        public DateTime? FechaContable { get; set; }
        
        [MaxLength(3)]
        public string? Turno { get; set; }

        // Motivo de la nota de crédito
        [MaxLength(50)]
        public string Motivo { get; set; } = "Devolución";
        
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Moneda y cambio
        public int IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        
        [MaxLength(4)]
        public string? SimboloMoneda { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal CambioDelDia { get; set; } = 1;
        
        public bool EsMonedaExtranjera { get; set; }

        // Totales
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

        // Estado
        [MaxLength(20)]
        public string Estado { get; set; } = "Borrador"; // Borrador / Confirmada / Anulada

        // SIFEN (facturación electrónica)
        [MaxLength(8)]
        public string? Timbrado { get; set; }
        
        public int? Serie { get; set; }
        
        [MaxLength(64)]
        public string? CDC { get; set; }
        
        [MaxLength(9)]
        public string? CodigoSeguridad { get; set; }
        
        [MaxLength(30)]
        public string? EstadoSifen { get; set; }
        
        public DateTime? FechaEnvioSifen { get; set; }
        
        public string? MensajeSifen { get; set; }
        
        public string? XmlCDE { get; set; }
        
        [MaxLength(50)]
        public string? IdLote { get; set; }

        // Usuario
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        
        /// <summary>
        /// Indica cómo afecta el stock: 1 = Suma (devolución), -1 = Resta (salida), 0 = No afecta
        /// </summary>
        public int AfectaStock { get; set; } = 0;
        
        // Auditoría
        [MaxLength(100)]
        public string? CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Detalles
        public ICollection<NotaCreditoVentaDetalle>? Detalles { get; set; }
    }

    /// <summary>
    /// Catálogo de motivos de nota de crédito según SIFEN Paraguay
    /// </summary>
    public static class MotivosNotaCredito
    {
        public const string DevolucionAjustePrecios = "Devolución y Ajuste de precios";
        public const string Devolucion = "Devolución";
        public const string Descuento = "Descuento";
        public const string Bonificacion = "Bonificación";
        public const string CreditoIncobrable = "Crédito incobrable";
        public const string RecuperoCosto = "Recupero de costo";
        public const string RecuperoGasto = "Recupero de gasto";
        public const string AjustePrecio = "Ajuste de precio";

        public static readonly string[] Todos = new[]
        {
            DevolucionAjustePrecios,
            Devolucion,
            Descuento,
            Bonificacion,
            CreditoIncobrable,
            RecuperoCosto,
            RecuperoGasto,
            AjustePrecio
        };
    }
}
