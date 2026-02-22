using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("Cajas")]
    public class Caja
    {
        [Key]
        [Column("id_caja")]
        public int IdCaja { get; set; }

        /// <summary>
        /// Nombre descriptivo de la caja (ej: "Caja 1", "Caja Principal")
        /// </summary>
        [StringLength(50)]
        public string? Nombre { get; set; }

        /// <summary>
        /// Sucursal a la que pertenece esta caja
        /// </summary>
        public int? IdSucursal { get; set; }

        public int? CantTurnos { get; set; }
        public int? TurnoActual { get; set; }

        [StringLength(3)] public string? Nivel1 { get; set; }
        [StringLength(3)] public string? Nivel2 { get; set; }
        [StringLength(7)] public string? FacturaInicial { get; set; }
        public int? Serie { get; set; }
        [StringLength(8)] public string? Timbrado { get; set; }
        public DateTime? VigenciaDel { get; set; }
        public DateTime? VigenciaAl { get; set; }
        [StringLength(40)] public string? NombreImpresora { get; set; }
        public bool? BloquearFactura { get; set; }
        public int? CajaActual { get; set; }
        public DateTime? FechaActualCaja { get; set; }
        public int? Imprimir_Factura { get; set; }

        // Remisión
        [StringLength(3)] public string? Nivel1R { get; set; }
        [StringLength(3)] public string? Nivel2R { get; set; }
        [StringLength(7)] public string? FacturaInicialR { get; set; }
        public int? SerieR { get; set; }
        [StringLength(8)] public string? TimbradoR { get; set; }
        public DateTime? VigenciaDelR { get; set; }
        public DateTime? VigenciaAlR { get; set; }
        [StringLength(40)] public string? NombreImpresoraR { get; set; }
        public bool? BloquearFacturaR { get; set; }
        public bool? Imprimir_remisionR { get; set; }

        // Nota de Crédito
        [StringLength(3)] public string? Nivel1NC { get; set; }
        [StringLength(3)] public string? Nivel2NC { get; set; }
        [StringLength(7)] public string? NumeroNC { get; set; }
        public int? SerieNC { get; set; }
        [StringLength(8)] public string? TimbradoNC { get; set; }
        public DateTime? VigenciaDelNC { get; set; }
        public DateTime? VigenciaAlNC { get; set; }
        [StringLength(40)] public string? NombreImpresoraNC { get; set; }
        public bool? BloquearFacturaNC { get; set; }
        public bool? Imprimir_remisionNC { get; set; }

        // Recibo
        [StringLength(3)] public string? Nivel1Recibo { get; set; }
        [StringLength(3)] public string? Nivel2Recibo { get; set; }
        [StringLength(7)] public string? NumeroRecibo { get; set; }
        public int? SerieRecibo { get; set; }
        [StringLength(8)] public string? TimbradoRecibo { get; set; }
        public DateTime? VigenciaDelRecibo { get; set; }
        public DateTime? VigenciaAlRecibo { get; set; }
        [StringLength(40)] public string? NombreImpresoraRecibo { get; set; }
        public bool? BloquearFacturaRecibo { get; set; }
        public bool? Imprimir_remisionRecibo { get; set; }

        [StringLength(10)] public string? modelo_factura { get; set; }
        [StringLength(13)] public string? anular_item { get; set; }
        public bool? bloquear_fechaCaja { get; set; }
        [StringLength(50)] public string? cierre_simultaneo { get; set; }
        public bool? numero_correlativo { get; set; }
        
        /// <summary>
        /// Tipo de facturación: "ELECTRONICA" o "AUTOIMPRESOR"
        /// </summary>
        [StringLength(20)]
        public string? TipoFacturacion { get; set; }
        
        /// <summary>
        /// Formato de impresión: "TICKET" (térmica 80mm), "MATRICIAL" (carta), "A4" (laser)
        /// </summary>
        [StringLength(20)]
        public string? FormatoImpresion { get; set; }
        
        /// <summary>
        /// Mostrar logo en la impresión (recomendado para térmica, no para matricial)
        /// </summary>
        public bool? MostrarLogo { get; set; }
        
        /// <summary>
        /// Ancho del papel en caracteres para ticket (default: 42 para 80mm)
        /// </summary>
        public int? AnchoTicket { get; set; }

        // ========== CONFIGURACIÓN RESTAURANTE/COMANDAS ==========
        
        /// <summary>
        /// Impresora de comandas para cocina (nombre local o IP de red).
        /// Formato para red: "\\192.168.1.100\NombreCompartido" o directo por IP/puerto.
        /// </summary>
        [StringLength(100)]
        public string? ImpresoraComandaCocina { get; set; }

        /// <summary>
        /// Impresora de comandas para barra (nombre local o IP de red).
        /// </summary>
        [StringLength(100)]
        public string? ImpresoraComandaBarra { get; set; }

        /// <summary>
        /// Puerto TCP para impresora de red cocina (default: 9100 para RAW printing).
        /// </summary>
        public int? PuertoImpresoraCocina { get; set; }

        /// <summary>
        /// Puerto TCP para impresora de red barra.
        /// </summary>
        public int? PuertoImpresoraBarra { get; set; }

        /// <summary>
        /// Si la impresora de cocina es de red (IP directo) vs compartida Windows.
        /// </summary>
        public bool? ComandaCocinaEsRed { get; set; }

        /// <summary>
        /// Si la impresora de barra es de red (IP directo) vs compartida Windows.
        /// </summary>
        public bool? ComandaBarraEsRed { get; set; }

        /// <summary>
        /// Imprimir comanda automáticamente al agregar items al pedido.
        /// </summary>
        public bool? ImprimirComandaAutomatica { get; set; }

        /// <summary>
        /// Número de copias de comanda para cocina.
        /// </summary>
        public int? CopiasComandaCocina { get; set; }

        /// <summary>
        /// Número de copias de comanda para barra.
        /// </summary>
        public int? CopiasComandaBarra { get; set; }

        // ========== IMPRESORA COMPROBANTE PEDIDO (CAJA) ==========

        /// <summary>
        /// Nombre o IP de la impresora para comprobante de pedido al cliente.
        /// Si está configurada, imprime ticket de pedido para el cliente.
        /// </summary>
        [MaxLength(100)]
        public string? ImpresoraComprobantePedido { get; set; }

        /// <summary>
        /// Puerto TCP para impresora de comprobante (default: 9100).
        /// </summary>
        public int? PuertoImpresoraComprobante { get; set; }

        /// <summary>
        /// Si la impresora de comprobante es de red (IP directo) vs compartida Windows.
        /// </summary>
        public bool? ComprobanteEsRed { get; set; }

        /// <summary>
        /// Imprimir comprobante de pedido automáticamente al confirmar pedido.
        /// </summary>
        public bool? ImprimirComprobantePedidoAuto { get; set; }
    }
}
