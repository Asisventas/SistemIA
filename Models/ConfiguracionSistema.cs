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

        // ========== CONFIGURACIÓN CÓDIGO DE BARRAS ==========
        
        /// <summary>
        /// Si está activo, al escanear un código de barras el producto se agrega directamente al detalle con cantidad 1.
        /// Si está desactivado, el producto se selecciona en el formulario permitiendo modificar cantidad, descuento, etc. antes de agregar.
        /// </summary>
        [Display(Name = "Agregar producto automáticamente al escanear código de barras")]
        public bool AgregarProductoAlEscanear { get; set; } = true;

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

        // ========== CONFIGURACIÓN SIFEN ==========
        
        /// <summary>
        /// Intervalo en minutos para verificar documentos pendientes SIFEN (default: 2)
        /// </summary>
        [Display(Name = "Intervalo Cola SIFEN (minutos)")]
        public int SifenIntervaloMinutos { get; set; } = 2;

        /// <summary>
        /// Habilita/deshabilita el servicio de cola SIFEN
        /// </summary>
        [Display(Name = "Servicio Cola SIFEN Activo")]
        public bool SifenColaActiva { get; set; } = true;

        /// <summary>
        /// Máximo de documentos a procesar por ciclo
        /// </summary>
        [Display(Name = "Máx. Documentos por Ciclo")]
        public int SifenMaxDocumentosPorCiclo { get; set; } = 10;

        /// <summary>
        /// Máximo de reintentos por documento
        /// </summary>
        [Display(Name = "Máx. Reintentos por Documento")]
        public int SifenMaxReintentos { get; set; } = 3;

        // ========== CONFIGURACIÓN RESTAURANTE/CANCHAS ==========
        
        /// <summary>
        /// Habilita el modo restaurante (panel de mesas, pedidos, comandas)
        /// </summary>
        [Display(Name = "Modo Restaurante Activo")]
        public bool RestauranteModoActivo { get; set; } = false;

        /// <summary>
        /// Tipo de negocio para el módulo de mesas: Restaurante, Complejo, Taller
        /// </summary>
        [Display(Name = "Tipo de Negocio")]
        [StringLength(30)]
        public string TipoNegocioMesas { get; set; } = "Restaurante";

        /// <summary>
        /// Habilita el modo canchas/espacios alquilables
        /// </summary>
        [Display(Name = "Modo Canchas/Alquileres Activo")]
        public bool CanchasModoActivo { get; set; } = false;

        /// <summary>
        /// Minutos de advertencia antes de finalizar la cancha
        /// </summary>
        [Display(Name = "Alerta Cancha (minutos antes)")]
        public int CanchasMinutosAlerta { get; set; } = 5;

        /// <summary>
        /// Sonido de alerta cuando queda poco tiempo
        /// </summary>
        [Display(Name = "Sonido Alerta Tiempo")]
        [StringLength(50)]
        public string? CanchasSonidoAlerta { get; set; } = "beep";

        /// <summary>
        /// Sonido de alerta cuando finaliza el tiempo
        /// </summary>
        [Display(Name = "Sonido Finalización")]
        [StringLength(50)]
        public string? CanchasSonidoFin { get; set; } = "gong";

        /// <summary>
        /// Repetir sonido de finalización cada X segundos (0 = no repetir)
        /// </summary>
        [Display(Name = "Repetir Sonido Fin (segundos)")]
        public int CanchasRepetirSonidoSegundos { get; set; } = 30;

        /// <summary>
        /// Mostrar temporizador de tiempo en mesas ocupadas
        /// </summary>
        [Display(Name = "Mostrar Tiempo en Mesas")]
        public bool RestauranteMostrarTiempo { get; set; } = true;

        /// <summary>
        /// Aplicar cargo por servicio automático (%)
        /// </summary>
        [Display(Name = "Cargo por Servicio (%)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? RestauranteCargoServicio { get; set; }

        /// <summary>
        /// Permitir pagos parciales en pedidos
        /// </summary>
        [Display(Name = "Permitir Pagos Parciales")]
        public bool RestaurantePermitirPagosParciales { get; set; } = true;

        /// <summary>
        /// Permitir división de cuenta entre comensales
        /// </summary>
        [Display(Name = "Permitir División de Cuenta")]
        public bool RestaurantePermitirDivisionCuenta { get; set; } = true;

        /// <summary>
        /// Imprimir comanda automáticamente al agregar items
        /// </summary>
        [Display(Name = "Imprimir Comanda Automática")]
        public bool RestauranteImprimirComandaAuto { get; set; } = false;

        /// <summary>
        /// Nombre de impresora para comandas de cocina
        /// </summary>
        [Display(Name = "Impresora Cocina")]
        [StringLength(100)]
        public string? RestauranteImpresoraCocina { get; set; }

        /// <summary>
        /// Nombre de impresora para comandas de barra
        /// </summary>
        [Display(Name = "Impresora Barra")]
        [StringLength(100)]
        public string? RestauranteImpresoraBarra { get; set; }

        /// <summary>
        /// Enviar recordatorio de reservas (minutos antes)
        /// </summary>
        [Display(Name = "Recordatorio Reservas (min)")]
        public int? ReservasRecordatorioMinutos { get; set; } = 60;

        /// <summary>
        /// Permitir reservas desde el sistema
        /// </summary>
        [Display(Name = "Habilitar Reservas")]
        public bool ReservasHabilitadas { get; set; } = true;

        // ========== GIMNASIO ==========

        /// <summary>
        /// Activa el módulo de gimnasio con control de acceso
        /// </summary>
        [Display(Name = "Modo Gimnasio Activo")]
        public bool GimnasioModoActivo { get; set; } = false;

        /// <summary>
        /// Mensaje de bienvenida al registrar entrada
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Mensaje Bienvenida Gimnasio")]
        public string? GimnasioMensajeBienvenida { get; set; } = "¡Bienvenido {nombre}! Tu membresía vence en {dias} días.";

        /// <summary>
        /// Mensaje de despedida al registrar salida
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Mensaje Despedida Gimnasio")]
        public string? GimnasioMensajeDespedida { get; set; } = "¡Hasta pronto {nombre}! Hoy estuviste {duracion}.";

        /// <summary>
        /// Mensaje cuando la membresía está por vencer (7 días o menos)
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Mensaje Vencimiento Próximo")]
        public string? GimnasioMensajeVencimientoProximo { get; set; } = "⚠️ {nombre}, tu membresía vence en {dias} días. ¡Renueva ahora!";

        /// <summary>
        /// Mensaje cuando la membresía está vencida
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Mensaje Membresía Vencida")]
        public string? GimnasioMensajeVencido { get; set; } = "❌ {nombre}, tu membresía venció hace {dias} días. Por favor renueva.";

        /// <summary>
        /// Habilitar reconocimiento facial para acceso
        /// </summary>
        [Display(Name = "Reconocimiento Facial Activo")]
        public bool GimnasioReconocimientoFacialActivo { get; set; } = true;

        /// <summary>
        /// Porcentaje mínimo de coincidencia para reconocimiento facial (0-100)
        /// </summary>
        [Display(Name = "Umbral Coincidencia Facial (%)")]
        public int GimnasioUmbralCoincidenciaFacial { get; set; } = 75;

        /// <summary>
        /// Días de gracia después del vencimiento (configuración global)
        /// </summary>
        [Display(Name = "Días de Gracia")]
        public int GimnasioDiasGracia { get; set; } = 3;

        /// <summary>
        /// Permitir acceso en días de gracia
        /// </summary>
        [Display(Name = "Permitir Acceso en Días de Gracia")]
        public bool GimnasioPermitirAccesoDiasGracia { get; set; } = true;

        /// <summary>
        /// Habilitar síntesis de voz para mensajes
        /// </summary>
        [Display(Name = "Habilitar Mensajes de Voz")]
        public bool GimnasioHabilitarVoz { get; set; } = false;

        /// <summary>
        /// Registrar automáticamente la salida después de X horas (0 = no registrar)
        /// </summary>
        [Display(Name = "Auto-Salida Después de (horas)")]
        public int GimnasioHorasAutoSalida { get; set; } = 4;

        // ========== AGENDA ==========

        /// <summary>
        /// Activa el módulo de agenda con gestión de citas y recordatorios
        /// </summary>
        [Display(Name = "Modo Agenda Activo")]
        public bool AgendaModoActivo { get; set; } = false;

        /// <summary>
        /// Minutos de anticipación por defecto para recordatorios
        /// </summary>
        [Display(Name = "Recordatorio (minutos antes)")]
        public int AgendaRecordatorioMinutos { get; set; } = 30;

        /// <summary>
        /// Enviar recordatorios por correo electrónico
        /// </summary>
        [Display(Name = "Enviar Recordatorios por Email")]
        public bool AgendaEnviarRecordatoriosEmail { get; set; } = true;

        /// <summary>
        /// Habilitar colores personalizados por cliente
        /// </summary>
        [Display(Name = "Colores por Cliente")]
        public bool AgendaColoresPorCliente { get; set; } = true;

        /// <summary>
        /// Mostrar ubicación GPS en citas
        /// </summary>
        [Display(Name = "Mostrar Ubicación GPS")]
        public bool AgendaMostrarUbicacionGPS { get; set; } = false;

        // ========== OTRAS CONFIGURACIONES ==========
        
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
