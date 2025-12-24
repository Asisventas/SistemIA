using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("TransferenciasDeposito")]
    public class TransferenciaDeposito
    {
        [Key]
        public int IdTransferencia { get; set; }

        [Required]
        public int IdDepositoOrigen { get; set; }

        [Required]
        public int IdDepositoDestino { get; set; }

        [Required]
        public DateTime FechaTransferencia { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Comentario { get; set; }

        [StringLength(100)]
        public string? UsuarioCreacion { get; set; }

        // Navegación
        [ForeignKey(nameof(IdDepositoOrigen))]
        public virtual Deposito? DepositoOrigen { get; set; }

        [ForeignKey(nameof(IdDepositoDestino))]
        public virtual Deposito? DepositoDestino { get; set; }

        public virtual ICollection<TransferenciaDepositoDetalle> Detalles { get; set; } = new List<TransferenciaDepositoDetalle>();
    }
}
