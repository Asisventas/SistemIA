using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Tabla de configuración del sistema.
    /// Solo debe existir un registro (singleton).
    /// </summary>
    [Table("ConfiguracionSistema")]
    public class ConfiguracionSistema
    {
        [Key]
        public int IdConfiguracion { get; set; }

        // ========== CONFIGURACIÓN FARMACIA ==========
        
        /// <summary>
        /// Indica si el sistema opera en modo farmacia con control de precios regulados
        /// </summary>
        [Display(Name = "Modo Farmacia Activo")]
        public bool FarmaciaModoActivo { get; set; } = false;

        /// <summary>
        /// Si está activo, valida que el precio de venta no supere el Precio Ministerio
        /// </summary>
        [Display(Name = "Validar Precio Ministerio")]
        public bool FarmaciaValidarPrecioMinisterio { get; set; } = false;

        /// <summary>
        /// Muestra el campo Precio Ministerio en el formulario de productos
        /// </summary>
        [Display(Name = "Mostrar Precio Ministerio en Productos")]
        public bool FarmaciaMostrarPrecioMinisterio { get; set; } = false;

        /// <summary>
        /// Muestra el campo Precio Ministerio en detalles de compras
        /// </summary>
        [Display(Name = "Mostrar Precio Ministerio en Compras")]
        public bool FarmaciaMostrarPrecioMinisterioEnCompras { get; set; } = false;

        /// <summary>
        /// Si está activo, el descuento automático se calcula como porcentaje sobre el Precio Ministerio.
        /// El descuento resultante es: ((PrecioMinisterio - PrecioVenta) / PrecioMinisterio) * 100
        /// </summary>
        [Display(Name = "Descuento basado en Precio Ministerio")]
        public bool FarmaciaDescuentoBasadoEnPrecioMinisterio { get; set; } = false;

        // ========== CONFIGURACIÓN DE DESCUENTOS ==========
        
        /// <summary>
        /// Permite aplicar descuentos en ventas
        /// </summary>
        [Display(Name = "Permitir Vender con Descuento")]
        public bool PermitirVenderConDescuento { get; set; } = false;

        /// <summary>
        /// Porcentaje máximo de descuento permitido (0-100)
        /// </summary>
        [Display(Name = "Descuento Máximo (%)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeDescuentoMaximo { get; set; }

        // ========== OTRAS CONFIGURACIONES FUTURAS ==========
        
        /// <summary>
        /// Nombre de la empresa/negocio
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Nombre del Negocio")]
        public string? NombreNegocio { get; set; }

        // ========== AUDITORÍA ==========
        
        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }
    }
}
