using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Representa un vehículo de cliente para el modo Taller.
    /// Almacena datos del vehículo que pueden reutilizarse en múltiples órdenes de trabajo.
    /// </summary>
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }

        // ========== CLIENTE PROPIETARIO ==========
        
        /// <summary>
        /// Cliente dueño del vehículo
        /// </summary>
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // ========== SUCURSAL ==========
        
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // ========== IDENTIFICACIÓN DEL VEHÍCULO ==========
        
        /// <summary>
        /// Número de matrícula/patente (ej: "ABC 123")
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Matricula { get; set; } = string.Empty;

        /// <summary>
        /// Marca del vehículo (Toyota, Ford, Chevrolet, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? Marca { get; set; }

        /// <summary>
        /// Modelo del vehículo (Corolla, Focus, Cruze, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? Modelo { get; set; }

        /// <summary>
        /// Año de fabricación
        /// </summary>
        public int? Anio { get; set; }

        /// <summary>
        /// Color del vehículo
        /// </summary>
        [MaxLength(30)]
        public string? Color { get; set; }

        /// <summary>
        /// Número de chasis/VIN (17 caracteres estándar)
        /// </summary>
        [MaxLength(20)]
        public string? NumeroChasis { get; set; }

        /// <summary>
        /// Número de motor
        /// </summary>
        [MaxLength(30)]
        public string? NumeroMotor { get; set; }

        /// <summary>
        /// Tipo de combustible: Nafta, Diesel, GNC, Híbrido, Eléctrico
        /// </summary>
        [MaxLength(20)]
        public string? TipoCombustible { get; set; }

        /// <summary>
        /// Tipo de vehículo: Auto, Camioneta, Moto, Camión, Ómnibus, etc.
        /// </summary>
        [MaxLength(30)]
        public string TipoVehiculo { get; set; } = "Auto";

        /// <summary>
        /// Kilometraje actual (se actualiza en cada visita)
        /// </summary>
        public int? KilometrajeActual { get; set; }

        // ========== DATOS ADICIONALES ==========
        
        /// <summary>
        /// Observaciones generales del vehículo
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// URL de foto del vehículo
        /// </summary>
        [MaxLength(500)]
        public string? FotoUrl { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Si el vehículo está activo en el sistema
        /// </summary>
        public bool Activo { get; set; } = true;

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Órdenes de trabajo realizadas a este vehículo
        /// </summary>
        public ICollection<OrdenTrabajo>? OrdenesTrabajo { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Descripción completa del vehículo para mostrar
        /// </summary>
        [NotMapped]
        public string DescripcionCompleta => $"{Marca} {Modelo} ({Anio}) - {Matricula}";

        /// <summary>
        /// Descripción corta del vehículo
        /// </summary>
        [NotMapped]
        public string DescripcionCorta => $"{Marca} {Modelo} - {Matricula}";
    }
}
