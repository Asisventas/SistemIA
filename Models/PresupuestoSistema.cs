using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Presupuesto comercial del sistema SistemIA para clientes potenciales
    /// </summary>
    public class PresupuestoSistema
    {
        [Key]
        public int IdPresupuesto { get; set; }

        // ========== CLIENTE POTENCIAL ==========
        [Required, MaxLength(200)]
        public string NombreCliente { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? RucCliente { get; set; }

        [MaxLength(300)]
        public string? DireccionCliente { get; set; }

        [MaxLength(50)]
        public string? TelefonoCliente { get; set; }

        [MaxLength(150)]
        public string? EmailCliente { get; set; }

        [MaxLength(100)]
        public string? ContactoCliente { get; set; }

        [MaxLength(100)]
        public string? CargoContacto { get; set; }

        // ========== NUMERACIÓN ==========
        public int NumeroPresupuesto { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;

        public DateTime FechaVigencia { get; set; } = DateTime.Now.AddDays(30);

        // ========== PRECIOS CONTADO ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioContadoUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal PrecioContadoGs { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TipoCambio { get; set; } = 7500m;

        // ========== PRECIOS LEASING/CUOTAS ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioLeasingMensualUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal PrecioLeasingMensualGs { get; set; }

        public int CantidadCuotasLeasing { get; set; } = 12;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EntradaLeasingUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal EntradaLeasingGs { get; set; }

        // ========== COSTOS ADICIONALES ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoImplementacionUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoImplementacionGs { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoCapacitacionUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoCapacitacionGs { get; set; }

        public int HorasCapacitacion { get; set; } = 8;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoMantenimientoMensualUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoMantenimientoMensualGs { get; set; }

        // ========== COSTOS OPCIONALES ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoSucursalAdicionalUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoSucursalAdicionalGs { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoUsuarioAdicionalUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoUsuarioAdicionalGs { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoHoraDesarrolloUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoHoraDesarrolloGs { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoVisitaTecnicaUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal CostoVisitaTecnicaGs { get; set; }

        // ========== MÓDULOS INCLUIDOS (JSON o flags) ==========
        public bool ModuloVentas { get; set; } = true;
        public bool ModuloCompras { get; set; } = true;
        public bool ModuloInventario { get; set; } = true;
        public bool ModuloClientes { get; set; } = true;
        public bool ModuloCaja { get; set; } = true;
        public bool ModuloSifen { get; set; } = true;
        public bool ModuloInformes { get; set; } = true;
        public bool ModuloUsuarios { get; set; } = true;
        public bool ModuloCorreo { get; set; } = true;
        public bool ModuloAsistenteIA { get; set; } = true;

        // ========== CANTIDADES INCLUIDAS ==========
        public int CantidadSucursalesIncluidas { get; set; } = 1;
        public int CantidadUsuariosIncluidos { get; set; } = 5;
        public int CantidadCajasIncluidas { get; set; } = 2;

        // ========== OBSERVACIONES ==========
        [MaxLength(2000)]
        public string? Observaciones { get; set; }

        [MaxLength(2000)]
        public string? CondicionesPago { get; set; } = "50% al inicio de implementación, 50% al finalizar.";

        [MaxLength(1000)]
        public string? Garantias { get; set; } = "30 días de garantía en implementación. Actualizaciones incluidas durante período de soporte.";

        // ========== ESTADO ==========
        [MaxLength(20)]
        public string Estado { get; set; } = "Borrador"; // Borrador, Enviado, Aceptado, Rechazado, Vencido

        public DateTime? FechaEnvio { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        // ========== AUDITORÍA ==========
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime FechaModificacion { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? UsuarioCreacion { get; set; }

        // ========== NAVEGACIÓN ==========
        public int IdSucursal { get; set; } = 1;
        public Sucursal? Sucursal { get; set; }

        // ========== ITEMS/DETALLES DEL PRESUPUESTO ==========
        public ICollection<PresupuestoSistemaDetalle> Detalles { get; set; } = new List<PresupuestoSistemaDetalle>();
    }
}
