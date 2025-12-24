using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("ListasPreciosDetalles")]
    public class ListaPrecioDetalle
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        [Display(Name = "Lista de Precio")]
        public int IdListaPrecio { get; set; }

        [ForeignKey("IdListaPrecio")]
        public virtual ListaPrecio ListaPrecio { get; set; } = null!;

        // TODO: Cambiar por IdProducto cuando se cree la tabla Productos
        [Required]
        [StringLength(50)]
        [Display(Name = "Código Producto")]
        public string CodigoProducto { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Anterior")]
        public decimal? PrecioAnterior { get; set; }

        [Display(Name = "Fecha Última Actualización")]
        public DateTime FechaUltimaActualizacion { get; set; } = DateTime.Now;

        [Display(Name = "Aplicar Descuento")]
        public bool AplicarDescuento { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Descuento Especial")]
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100%")]
        public decimal DescuentoEspecial { get; set; } = 0;

        [Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        // Propiedades de auditoría
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public decimal PrecioConDescuento
        {
            get
            {
                decimal descuentoTotal = 0;
                
                // Descuento especial del producto
                if (AplicarDescuento && DescuentoEspecial > 0)
                    descuentoTotal += DescuentoEspecial;
                
                // Descuento global de la lista (si aplica)
                if (ListaPrecio?.AplicarDescuentoGlobal == true)
                    descuentoTotal += ListaPrecio.PorcentajeDescuento;
                
                // Aplicar descuento
                return Precio * (1 - descuentoTotal / 100);
            }
        }

        [NotMapped]
        public decimal PorcentajeVariacion
        {
            get
            {
                if (!PrecioAnterior.HasValue || PrecioAnterior.Value == 0) return 0;
                return ((Precio - PrecioAnterior.Value) / PrecioAnterior.Value) * 100;
            }
        }

        [NotMapped]
        public bool TuvoCambio => PrecioAnterior.HasValue && PrecioAnterior.Value != Precio;

        [NotMapped]
        public string DescripcionCambio
        {
            get
            {
                if (!TuvoCambio) return "Sin cambios";
                
                var variacion = PorcentajeVariacion;
                var signo = variacion > 0 ? "+" : "";
                return $"{signo}{variacion:F2}%";
            }
        }

        // Método para convertir precio a otra moneda
        public decimal ConvertirPrecio(TipoCambio tipoCambio)
        {
            return tipoCambio.ConvertirMonto(PrecioConDescuento);
        }
    }
}
