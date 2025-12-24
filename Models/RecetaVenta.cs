using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Almacena los datos de receta médica requeridos para productos controlados en ventas.
    /// </summary>
    public class RecetaVenta
    {
        [Key]
        public int IdRecetaVenta { get; set; }

        [Required]
        [Display(Name = "Venta")]
        public int IdVenta { get; set; }

        [ForeignKey(nameof(IdVenta))]
        public virtual Venta? Venta { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public int IdProducto { get; set; }

        [ForeignKey(nameof(IdProducto))]
        public virtual Producto? Producto { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Número de Registro")]
        public string NumeroRegistro { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de la Receta")]
        [DataType(DataType.Date)]
        public DateTime FechaReceta { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre del Médico")]
        public string NombreMedico { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre del Paciente")]
        public string NombrePaciente { get; set; } = string.Empty;

        // Auditoría
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario Registro")]
        public string? UsuarioRegistro { get; set; }
    }
}
