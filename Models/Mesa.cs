using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Representa una mesa de restaurante o cancha/espacio alquilable.
    /// Se muestra visualmente como un cuadrado/rectángulo en el panel de gestión.
    /// </summary>
    [Table("Mesas")]
    public class Mesa
    {
        [Key]
        public int IdMesa { get; set; }

        // ========== SUCURSAL (igual que otros modelos) ==========
        
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // ========== IDENTIFICACIÓN ==========
        
        /// <summary>
        /// Número o código de la mesa (ej: "1", "2", "VIP-1", "Cancha A")
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Numero { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo opcional (ej: "Mesa VIP", "Cancha de Fútbol 5")
        /// </summary>
        [MaxLength(100)]
        public string? Nombre { get; set; }

        /// <summary>
        /// Descripción adicional o notas
        /// </summary>
        [MaxLength(500)]
        public string? Descripcion { get; set; }

        // ========== TIPO Y CATEGORÍA ==========
        
        /// <summary>
        /// Tipo de espacio: Mesa, Cancha, Sala, Terraza, etc.
        /// </summary>
        [MaxLength(50)]
        public string Tipo { get; set; } = "Mesa";

        /// <summary>
        /// Zona o sector donde está ubicada (ej: "Interior", "Terraza", "VIP", "Planta Alta")
        /// </summary>
        [MaxLength(50)]
        public string? Zona { get; set; }

        /// <summary>
        /// Capacidad de personas
        /// </summary>
        public int Capacidad { get; set; } = 4;

        // ========== VISUALIZACIÓN EN PANEL ==========
        
        /// <summary>
        /// Posición X en el panel visual (píxeles o porcentaje)
        /// </summary>
        public int PosicionX { get; set; } = 0;

        /// <summary>
        /// Posición Y en el panel visual
        /// </summary>
        public int PosicionY { get; set; } = 0;

        /// <summary>
        /// Ancho del elemento visual (píxeles)
        /// </summary>
        public int Ancho { get; set; } = 100;

        /// <summary>
        /// Alto del elemento visual (píxeles)
        /// </summary>
        public int Alto { get; set; } = 100;

        /// <summary>
        /// Forma visual: Cuadrado, Circulo, Rectangulo
        /// </summary>
        [MaxLength(20)]
        public string Forma { get; set; } = "Cuadrado";

        /// <summary>
        /// Color de fondo cuando está libre (formato hex: #RRGGBB)
        /// </summary>
        [MaxLength(10)]
        public string ColorLibre { get; set; } = "#28a745"; // Verde

        /// <summary>
        /// Color de fondo cuando está ocupada
        /// </summary>
        [MaxLength(10)]
        public string ColorOcupada { get; set; } = "#dc3545"; // Rojo

        /// <summary>
        /// Color de fondo cuando tiene reserva próxima
        /// </summary>
        [MaxLength(10)]
        public string ColorReservada { get; set; } = "#ffc107"; // Amarillo

        /// <summary>
        /// URL de imagen personalizada (ej: imagen de cancha, mesa especial)
        /// </summary>
        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Icono Bootstrap a mostrar (ej: "bi-table", "bi-dribbble" para cancha)
        /// </summary>
        [MaxLength(50)]
        public string? Icono { get; set; }

        // ========== CONFIGURACIÓN DE TARIFAS (para canchas/alquileres) ==========
        
        /// <summary>
        /// Si cobra por tiempo (true para canchas, false para mesas normales)
        /// </summary>
        public bool CobraPorTiempo { get; set; } = false;

        /// <summary>
        /// Tarifa por hora (si CobraPorTiempo = true)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TarifaPorHora { get; set; }

        /// <summary>
        /// Tiempo mínimo de reserva en minutos
        /// </summary>
        public int? TiempoMinimoMinutos { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado actual: Libre, Ocupada, Reservada, Mantenimiento, Inactiva
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Libre";

        /// <summary>
        /// Si la mesa está activa para uso
        /// </summary>
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Orden de visualización en el panel
        /// </summary>
        public int Orden { get; set; } = 0;

        // ========== MESAS UNIDAS ==========
        
        /// <summary>
        /// ID de la mesa principal a la que está unida (null si no está unida o es la principal)
        /// </summary>
        public int? IdMesaPrincipal { get; set; }
        
        /// <summary>
        /// Referencia a la mesa principal
        /// </summary>
        [ForeignKey("IdMesaPrincipal")]
        public Mesa? MesaPrincipal { get; set; }
        
        /// <summary>
        /// Mesas que están unidas a esta (si es principal)
        /// </summary>
        public ICollection<Mesa>? MesasUnidas { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        public ICollection<Pedido>? Pedidos { get; set; }
        public ICollection<Reserva>? Reservas { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Texto para mostrar en el panel visual
        /// </summary>
        [NotMapped]
        public string TextoMostrar => !string.IsNullOrEmpty(Nombre) ? Nombre : $"{Tipo} {Numero}";

        /// <summary>
        /// Estilo CSS para el color según estado
        /// </summary>
        [NotMapped]
        public string ColorActual => Estado switch
        {
            "Libre" => ColorLibre,
            "Ocupada" => ColorOcupada,
            "Reservada" => ColorReservada,
            "Mantenimiento" => "#6c757d", // Gris
            _ => ColorLibre
        };
        
        /// <summary>
        /// Indica si esta mesa está unida a otra (es secundaria)
        /// </summary>
        [NotMapped]
        public bool EstaUnida => IdMesaPrincipal.HasValue;
        
        /// <summary>
        /// Indica si esta mesa tiene otras mesas unidas (es principal)
        /// </summary>
        [NotMapped]
        public bool TieneMesasUnidas => MesasUnidas?.Any() == true;
        
        /// <summary>
        /// Texto que muestra las mesas unidas (ej: "1+2+3")
        /// </summary>
        [NotMapped]
        public string TextoMesasUnidas
        {
            get
            {
                if (MesasUnidas == null || !MesasUnidas.Any())
                    return Numero;
                
                var numeros = new List<string> { Numero };
                numeros.AddRange(MesasUnidas.Select(m => m.Numero));
                return string.Join("+", numeros.OrderBy(n => n));
            }
        }
    }
}
