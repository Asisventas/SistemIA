using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class CompraDetalle
    {
        [Key]
        public int IdCompraDetalle { get; set; }

        public int IdCompra { get; set; }
        public int IdProducto { get; set; }

        [Column(TypeName = "decimal(18,4)")] public decimal PrecioUnitario { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Cantidad { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Importe { get; set; }

        // IVA desglosado
        [Column(TypeName = "decimal(18,4)")] public decimal IVA10 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal IVA5 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Exenta { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Grabado10 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Grabado5 { get; set; }

    // Precio de venta de referencia al momento de la compra
    [Column(TypeName = "decimal(18,4)")]
    public decimal PrecioVentaRef { get; set; }

    // Factor de cambio usado (cotización aplicada) origen→destino en el momento de la compra
    [Column(TypeName = "decimal(18,4)")]
    public decimal? CambioDelDia { get; set; }

    // Depósito específico para este item (permite distribuir en diferentes depósitos)
    [Display(Name = "Depósito")]
    public int? IdDepositoItem { get; set; }

    [ForeignKey(nameof(IdDepositoItem))]
    public virtual Deposito? DepositoItem { get; set; }

    // Fecha de vencimiento específica para este item
    [Display(Name = "Fecha de Vencimiento")]
    [DataType(DataType.Date)]
    public DateTime? FechaVencimientoItem { get; set; }

    // Factor multiplicador aplicado para calcular precio de venta
    [Column(TypeName = "decimal(18,4)")]
    [Display(Name = "Factor Multiplicador")]
    public decimal? FactorMultiplicador { get; set; }

    // Porcentaje de margen calculado (solo para referencia)
    [Column(TypeName = "decimal(18,4)")]
    [Display(Name = "% Margen")]
    public decimal? PorcentajeMargen { get; set; }

    // Precio Ministerio (para farmacias) - precio máximo regulado al momento de la compra
    [Column(TypeName = "decimal(18,4)")]
    [Display(Name = "Precio Ministerio")]
    public decimal? PrecioMinisterio { get; set; }

        // Auditoría básica
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Navegación
        public Compra? Compra { get; set; }
        public Producto? Producto { get; set; }
        
        // ========== CAMPOS AUXILIARES (NO MAPEADOS) ==========
        
        // Propiedad auxiliar para validación (no se persiste en BD)
        [NotMapped]
        public bool PermiteDecimal { get; set; }
        
        // Para productos vendidos por paquete/caja
        [NotMapped]
        public decimal? CantidadPorPaquete { get; set; }
        
        [NotMapped]
        public bool PermiteVentaPorUnidad { get; set; } = true;
        
        // Modo de ingreso: "paquete" o "unidad"
        [NotMapped]
        public string ModoIngreso { get; set; } = "unidad";
        
        // Cantidad ingresada en el modo seleccionado (antes de convertir a unidades)
        [NotMapped]
        public decimal CantidadIngresada { get; set; } = 1;
    }
}
