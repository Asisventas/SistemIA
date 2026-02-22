using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Registro de progreso físico del cliente en el gimnasio.
    /// Permite trackear evolución de peso, medidas corporales y fotos.
    /// </summary>
    [Table("ProgresosCliente")]
    public class ProgresoCliente
    {
        [Key]
        public int IdProgreso { get; set; }

        // ========== RELACIONES ==========
        
        /// <summary>
        /// Cliente al que pertenece el registro
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Rutina activa al momento del registro (opcional)
        /// </summary>
        public int? IdRutina { get; set; }
        [ForeignKey("IdRutina")]
        public RutinaGimnasio? Rutina { get; set; }

        /// <summary>
        /// Personal Trainer que tomó las medidas (opcional)
        /// </summary>
        public int? IdInstructor { get; set; }
        [ForeignKey("IdInstructor")]
        public Instructor? Instructor { get; set; }

        // ========== FECHA DEL REGISTRO ==========
        
        /// <summary>
        /// Fecha del registro de progreso
        /// </summary>
        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // ========== MEDIDAS BÁSICAS ==========
        
        /// <summary>
        /// Peso corporal en kg
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? PesoKg { get; set; }

        /// <summary>
        /// Altura en cm
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? AlturaCm { get; set; }

        /// <summary>
        /// Porcentaje de grasa corporal
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeGrasa { get; set; }

        /// <summary>
        /// Porcentaje de masa muscular
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeMusculo { get; set; }

        /// <summary>
        /// Índice de Masa Corporal (calculado)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? IMC { get; set; }

        // ========== MEDIDAS CORPORALES (cm) ==========
        
        /// <summary>
        /// Circunferencia del pecho
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaPecho { get; set; }

        /// <summary>
        /// Circunferencia de la cintura
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaCintura { get; set; }

        /// <summary>
        /// Circunferencia de la cadera
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaCadera { get; set; }

        /// <summary>
        /// Circunferencia del brazo derecho
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaBrazoDerecho { get; set; }

        /// <summary>
        /// Circunferencia del brazo izquierdo
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaBrazoIzquierdo { get; set; }

        /// <summary>
        /// Circunferencia del muslo derecho
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaMusloDerecho { get; set; }

        /// <summary>
        /// Circunferencia del muslo izquierdo
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaMusloIzquierdo { get; set; }

        /// <summary>
        /// Circunferencia de la pantorrilla
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? MedidaPantorrilla { get; set; }

        // ========== FOTOS DE PROGRESO ==========
        
        /// <summary>
        /// Foto frontal
        /// </summary>
        public byte[]? FotoFrontal { get; set; }

        /// <summary>
        /// Foto lateral
        /// </summary>
        public byte[]? FotoLateral { get; set; }

        /// <summary>
        /// Foto posterior
        /// </summary>
        public byte[]? FotoPosterior { get; set; }

        // ========== OBSERVACIONES ==========
        
        /// <summary>
        /// Notas u observaciones del registro
        /// </summary>
        [StringLength(500)]
        public string? Notas { get; set; }

        /// <summary>
        /// Objetivos actuales del cliente
        /// </summary>
        [StringLength(300)]
        public string? Objetivos { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Calcula el IMC si tiene peso y altura
        /// </summary>
        [NotMapped]
        public decimal? IMCCalculado
        {
            get
            {
                if (PesoKg.HasValue && AlturaCm.HasValue && AlturaCm > 0)
                {
                    var alturaM = AlturaCm.Value / 100m;
                    return Math.Round(PesoKg.Value / (alturaM * alturaM), 2);
                }
                return null;
            }
        }

        /// <summary>
        /// Clasificación del IMC
        /// </summary>
        [NotMapped]
        public string ClasificacionIMC
        {
            get
            {
                var imc = IMC ?? IMCCalculado;
                if (!imc.HasValue) return "-";
                return imc.Value switch
                {
                    < 18.5m => "Bajo peso",
                    < 25m => "Normal",
                    < 30m => "Sobrepeso",
                    < 35m => "Obesidad I",
                    < 40m => "Obesidad II",
                    _ => "Obesidad III"
                };
            }
        }

        /// <summary>
        /// Indica si tiene fotos de progreso
        /// </summary>
        [NotMapped]
        public bool TieneFotos => FotoFrontal != null || FotoLateral != null || FotoPosterior != null;
    }
}
