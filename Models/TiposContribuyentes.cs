using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace SistemIA.Models
{
    public class TiposContribuyentes
    {
       
        public int IdTipoContribuyente { get; set; }
        [Required]
        [StringLength(100)]
        public string NombreTipo { get; set; } = "";
        public string? Descripcion { get; set; }

        public ICollection<Cliente>? Clientes { get; set; }
    }
}
