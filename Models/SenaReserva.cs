using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemIA.Models.Enums;

namespace SistemIA.Models
{
    /// <summary>
    /// Pago de seña/anticipo de una reserva.
    /// Permite registrar el cobro con todos los datos necesarios para el cierre de caja.
    /// </summary>
    [Table("SenasReservas")]
    public class SenaReserva
    {
        [Key]
        public int IdSenaReserva { get; set; }

        // ========== RESERVA ==========
        
        /// <summary>
        /// Reserva a la que corresponde esta seña
        /// </summary>
        [Required]
        public int IdReserva { get; set; }
        public Reserva? Reserva { get; set; }

        // ========== CLIENTE ==========
        
        /// <summary>
        /// Cliente que pagó la seña (puede ser diferente al de la reserva)
        /// </summary>
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Nombre del cliente (si no está registrado)
        /// </summary>
        [MaxLength(200)]
        public string? NombreCliente { get; set; }

        // ========== DATOS DEL PAGO ==========
        
        /// <summary>
        /// Fecha y hora del pago de la seña
        /// </summary>
        public DateTime FechaPago { get; set; } = DateTime.Now;

        /// <summary>
        /// Monto de la seña pagada
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Monto { get; set; }

        /// <summary>
        /// Moneda del pago
        /// </summary>
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }

        /// <summary>
        /// Tipo de cambio aplicado si no es PYG
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TipoCambio { get; set; }

        /// <summary>
        /// Monto convertido a Guaraníes
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoGs { get; set; }

        // ========== MEDIO DE PAGO ==========
        
        /// <summary>
        /// Medio de pago principal (Efectivo, Tarjeta, QR, etc.)
        /// </summary>
        public MedioPago MedioPago { get; set; } = MedioPago.Efectivo;

        // Datos específicos según medio de pago
        // Tarjeta
        public TipoTarjeta? TipoTarjeta { get; set; }
        [MaxLength(50)] public string? MarcaTarjeta { get; set; }
        [MaxLength(4)] public string? Ultimos4 { get; set; }
        [MaxLength(50)] public string? NumeroAutorizacion { get; set; }

        // Transferencia / QR
        [MaxLength(50)] public string? BancoTransferencia { get; set; }
        [MaxLength(60)] public string? NumeroComprobante { get; set; }

        // Cheque
        [MaxLength(40)] public string? BancoCheque { get; set; }
        [MaxLength(30)] public string? NumeroCheque { get; set; }
        public DateTime? FechaCobroCheque { get; set; }

        // ========== CAJA Y TURNO ==========
        
        /// <summary>
        /// Sucursal donde se registró el pago
        /// </summary>
        public int? IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        /// <summary>
        /// Caja donde se registró el pago
        /// </summary>
        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }

        /// <summary>
        /// Turno de caja al momento del pago
        /// </summary>
        public int? Turno { get; set; }

        /// <summary>
        /// Fecha de caja (puede diferir de FechaPago si se registra retroactivamente)
        /// </summary>
        public DateTime? FechaCaja { get; set; }

        // ========== USUARIO ==========
        
        /// <summary>
        /// Usuario que registró el cobro
        /// </summary>
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado: CONFIRMADO, ANULADO, DEVUELTO
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "CONFIRMADO";

        /// <summary>
        /// Si la seña fue devuelta al cliente (ej: cancelación de reserva)
        /// </summary>
        public bool Devuelta { get; set; } = false;

        /// <summary>
        /// Fecha de devolución si aplica
        /// </summary>
        public DateTime? FechaDevolucion { get; set; }

        /// <summary>
        /// Motivo de anulación o devolución
        /// </summary>
        [MaxLength(500)]
        public string? MotivoAnulacion { get; set; }

        // ========== OBSERVACIONES ==========
        
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Número de recibo o comprobante emitido
        /// </summary>
        [MaxLength(30)]
        public string? NumeroRecibo { get; set; }
    }
}
