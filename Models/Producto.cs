using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int IdProducto { get; set; }

        // Identificación básica
        [Required]
        [StringLength(50)]
        [Display(Name = "Código Interno (dCodInt)")]
        public string CodigoInterno { get; set; } = string.Empty; // SIFEN dCodInt

        [Required]
        [StringLength(200)]
        [Display(Name = "Descripción (dDesProSer)")]
        public string Descripcion { get; set; } = string.Empty; // SIFEN dDesProSer

    [StringLength(14)]
    [Display(Name = "Código de barras", Description = "GTIN-8, UPC-12, EAN-13 o DUN-14. Opcional: puede dejarse vacío.")]
    [RegularExpression(@"^(\d{8}|\d{12}|\d{13}|\d{14})$", ErrorMessage = "Código de barras inválido. Use 8, 12, 13 o 14 dígitos, o deje vacío.")]
    public string? CodigoBarras { get; set; } // SIFEN dCodGTIN (8/12/13/14 dígitos)

    // Campos adicionales solicitados
    [StringLength(180)]
    [Column(TypeName = "varchar(180)")]
    [Display(Name = "Foto", Description = "Ruta o URL de la imagen del producto (opcional)")]
    public string? Foto { get; set; }

    [Required]
    [StringLength(10)]
    [Column(TypeName = "char(10)")]
    [Display(Name = "UndMedida", Description = "Descripción corta de la unidad (ej: UNIDAD, CAJA)")]
    public string UndMedida { get; set; } = "UNIDAD"; // requerido

    [StringLength(50)]
    [Display(Name = "IP (dirección IP)")]
    public string? IP { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Unidad (cUniMed)")]
        public string UnidadMedidaCodigo { get; set; } = "77"; // 77 = Unidad (SIFEN)

        [Display(Name = "Tipo Ítem (1=Prod,2=Serv)")]
        public int TipoItem { get; set; } = 1; // 1 Producto, 2 Servicio

    [Display(Name = "Es Combo")]
    public bool EsCombo { get; set; } = false; // Si es un producto compuesto que descuenta componentes

        // Relacionales
        [Display(Name = "Marca")]
        public int? IdMarca { get; set; }

        [ForeignKey(nameof(IdMarca))]
        public virtual Marca? Marca { get; set; }

    // Clasificación (opcional)
    [Display(Name = "Clasificación")]
    public int? IdClasificacion { get; set; }

    [ForeignKey(nameof(IdClasificacion))]
    public virtual Clasificacion? Clasificacion { get; set; }

        [Required]
        [Display(Name = "Tipo de IVA")]
        public int IdTipoIva { get; set; }

        [ForeignKey(nameof(IdTipoIva))]
        public virtual TiposIva TipoIva { get; set; } = null!;

    // Moneda predeterminada del precio/costo del producto
    [Display(Name = "Moneda predeterminada del precio")]
    public int? IdMonedaPrecio { get; set; }

    [ForeignKey(nameof(IdMonedaPrecio))]
    public virtual Moneda? MonedaPrecio { get; set; }

    // Relación obligatoria con Sucursal (columna 'suc')
    [Column("suc")]
    [Display(Name = "Sucursal")]
    public int IdSucursal { get; set; }

    [ForeignKey(nameof(IdSucursal))]
    public virtual Sucursal Sucursal { get; set; } = null!;

        // Precios y costos (base en Gs)
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Costo Unitario Gs")]
        [Range(0, 999999999999.9999)]
        public decimal? CostoUnitarioGs { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Precio Unitario Gs (dPUniProSer)")]
        [Range(0, 999999999999.9999)]
        public decimal PrecioUnitarioGs { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Precio Unitario USD")]
        [Range(0, 999999999999.9999)]
        public decimal? PrecioUnitarioUsd { get; set; }

        // ========== PRECIO MINISTERIO (FARMACIAS) ==========
        
        /// <summary>
        /// Precio máximo de venta regulado por el Ministerio de Salud (solo farmacias).
        /// El precio de venta no puede superar este valor.
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Precio Ministerio (Precio Máximo)")]
        [Range(0, 999999999999.9999)]
        public decimal? PrecioMinisterio { get; set; }

        // ========== CONFIGURACIÓN DE DESCUENTOS POR PRODUCTO ==========
        
        /// <summary>
        /// Indica si este producto permite aplicar descuentos en ventas
        /// </summary>
        [Display(Name = "Permitir Descuento")]
        public bool PermiteDescuento { get; set; } = true;

        /// <summary>
        /// Indica si este producto puede venderse a un precio menor al costo.
        /// Por defecto es false (no permite venta bajo costo).
        /// </summary>
        [Display(Name = "Permite Venta Bajo Costo")]
        public bool PermiteVentaBajoCosto { get; set; } = false;

        /// <summary>
        /// Si es true, usa el descuento específico del producto (DescuentoAutomaticoProducto).
        /// Si es false, usa el descuento de la configuración general (por marca/clasificación/todos).
        /// </summary>
        [Display(Name = "Usar Descuento Específico")]
        public bool UsaDescuentoEspecifico { get; set; } = false;

        /// <summary>
        /// Porcentaje de descuento automático específico para este producto.
        /// Solo se aplica si UsaDescuentoEspecifico = true.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Descuento Automático (%)")]
        [Range(0, 100)]
        public decimal? DescuentoAutomaticoProducto { get; set; }

        /// <summary>
        /// Porcentaje máximo de descuento permitido para este producto específico.
        /// Si es null, usa el máximo del sistema.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Descuento Máximo (%)")]
        [Range(0, 100)]
        public decimal? DescuentoMaximoProducto { get; set; }

        /// <summary>
        /// Porcentaje adicional que el cajero puede modificar sobre el descuento base del producto.
        /// Solo aplica si UsaDescuentoEspecifico = true.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Margen adicional cajero (%)")]
        [Range(0, 100)]
        public decimal? MargenAdicionalCajeroProducto { get; set; }

        // Inventario básico
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Stock")]
        public decimal Stock { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Stock Mínimo")]
        public decimal StockMinimo { get; set; } = 0;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    // Control de vencimiento
    [Display(Name = "Controlar Vencimiento")]
    public bool ControlarVencimiento { get; set; } = false;

    [Display(Name = "Fecha de Vencimiento")]
    [DataType(DataType.Date)]
    public DateTime? FechaVencimiento { get; set; }

    // Control de productos con receta (medicamentos controlados)
    [Display(Name = "Controlado con Receta")]
    public bool ControladoReceta { get; set; } = false;

    // Control de venta decimal (permite vender fracciones ej: 0.5 kg)
    [Display(Name = "Permitir Venta Decimal")]
    public bool PermiteDecimal { get; set; } = false;

    // Factor multiplicador para calcular precio de venta desde costo
    [Column(TypeName = "decimal(18,4)")]
    [Display(Name = "Factor Multiplicador")]
    [Range(0, 9999.9999)]
    public decimal? FactorMultiplicador { get; set; }

    // Depósito predeterminado para operaciones
    [Display(Name = "Depósito predeterminado")]
    public int? IdDepositoPredeterminado { get; set; }    [ForeignKey(nameof(IdDepositoPredeterminado))]
    public virtual Deposito? DepositoPredeterminado { get; set; }

    // Navegación inventario por depósito
    public ICollection<ProductoDeposito>? StocksPorDeposito { get; set; }

    // Componentes si es combo (líneas que se descuentan al vender el combo)
    public ICollection<ProductoComponente>? Componentes { get; set; }

        // Auditoría
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string? UsuarioCreacion { get; set; }

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }
    }
}
