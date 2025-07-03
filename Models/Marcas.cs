using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("Marcas")] // Sigue apuntando a la tabla 'Marcas'
    public class Marca
    {
        [Key]
        [Column("Id_Marca")] // Coincide con la columna SQL
        public int IdMarca { get; set; }

        [Required]
        [MaxLength(50)] // Coincide con nvarchar(50)
        [Column("Marca")] // Coincide con la columna SQL
        public string NombreMarca { get; set; } = null!;

        // Navegación inversa para la relación con Producto
        //public ICollection<Producto>? Productos { get; set; }
    }
}
