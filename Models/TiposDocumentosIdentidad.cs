using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Models
{
    public class TiposDocumentosIdentidad
    {
        [Key]
        [StringLength(2)]
        public string TipoDocumento { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; } = "";

        public ICollection<Cliente>? Clientes { get; set; }
    }
}