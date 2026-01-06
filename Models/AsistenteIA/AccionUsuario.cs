using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.AsistenteIA
{
    /// <summary>
    /// Registro de acción del usuario para auditoría y contexto de soporte
    /// </summary>
    public class AccionUsuario
    {
        [Key]
        public long IdAccion { get; set; }

        // ========== USUARIO ==========
        public int? IdUsuario { get; set; }
        
        [MaxLength(100)]
        public string? NombreUsuario { get; set; }

        // ========== SESIÓN ==========
        /// <summary>
        /// Identificador único de la sesión del navegador
        /// </summary>
        [MaxLength(64)]
        public string? SessionId { get; set; }

        // ========== ACCIÓN ==========
        /// <summary>
        /// Tipo de acción: Navegacion, Click, Operacion, Error, Login, Logout
        /// </summary>
        [MaxLength(30)]
        public string TipoAccion { get; set; } = "Navegacion";

        /// <summary>
        /// Categoría: Ventas, Compras, Productos, Sistema, etc.
        /// </summary>
        [MaxLength(50)]
        public string? Categoria { get; set; }

        /// <summary>
        /// Descripción breve de la acción
        /// </summary>
        [MaxLength(200)]
        public string Descripcion { get; set; } = "";

        /// <summary>
        /// URL o ruta donde ocurrió la acción
        /// </summary>
        [MaxLength(300)]
        public string? Ruta { get; set; }

        /// <summary>
        /// Datos adicionales en JSON (parámetros, IDs, valores)
        /// </summary>
        public string? DatosJson { get; set; }

        // ========== ERROR (si aplica) ==========
        /// <summary>
        /// True si es un registro de error
        /// </summary>
        public bool EsError { get; set; } = false;

        /// <summary>
        /// Mensaje de error si aplica
        /// </summary>
        [MaxLength(1000)]
        public string? MensajeError { get; set; }

        /// <summary>
        /// Stack trace del error (opcional)
        /// </summary>
        public string? StackTrace { get; set; }

        // ========== CONTEXTO ==========
        /// <summary>
        /// Nombre del componente o página que registró la acción
        /// </summary>
        [MaxLength(100)]
        public string? Componente { get; set; }

        /// <summary>
        /// ID de entidad relacionada (IdVenta, IdProducto, etc.)
        /// </summary>
        public int? EntidadId { get; set; }

        /// <summary>
        /// Tipo de entidad (Venta, Producto, Cliente, etc.)
        /// </summary>
        [MaxLength(50)]
        public string? EntidadTipo { get; set; }

        // ========== TIMESTAMPS ==========
        public DateTime FechaHora { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha y hora del equipo cliente (puede diferir del servidor)
        /// </summary>
        public DateTime? FechaEquipo { get; set; }

        /// <summary>
        /// Duración de la acción en milisegundos (para operaciones)
        /// </summary>
        public int? DuracionMs { get; set; }

        // ========== CONTEXTO DE CAJA/SUCURSAL ==========
        /// <summary>
        /// ID de la sucursal donde se realizó la operación
        /// </summary>
        public int? IdSucursal { get; set; }

        /// <summary>
        /// Nombre de la sucursal
        /// </summary>
        [MaxLength(100)]
        public string? NombreSucursal { get; set; }

        /// <summary>
        /// ID de la caja donde se realizó la operación
        /// </summary>
        public int? IdCaja { get; set; }

        /// <summary>
        /// Nombre de la caja
        /// </summary>
        [MaxLength(100)]
        public string? NombreCaja { get; set; }

        /// <summary>
        /// Turno de caja al momento de la operación
        /// </summary>
        public int? Turno { get; set; }

        /// <summary>
        /// Fecha de caja (fecha operativa del sistema)
        /// </summary>
        public DateTime? FechaCaja { get; set; }
    }

    /// <summary>
    /// Tipos de acción para el tracking
    /// </summary>
    public static class TipoAccionTracking
    {
        public const string Navegacion = "Navegacion";
        public const string Click = "Click";
        public const string Operacion = "Operacion";
        public const string Error = "Error";
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string Busqueda = "Busqueda";
        public const string Creacion = "Creacion";
        public const string Edicion = "Edicion";
        public const string Eliminacion = "Eliminacion";
        public const string Impresion = "Impresion";
        public const string Exportacion = "Exportacion";
        public const string ConsultaIA = "ConsultaIA";
        public const string CierreCaja = "CierreCaja";
    }

    /// <summary>
    /// Categorías de módulos para clasificar acciones
    /// </summary>
    public static class CategoriaAccion
    {
        public const string Ventas = "Ventas";
        public const string Compras = "Compras";
        public const string Productos = "Productos";
        public const string Clientes = "Clientes";
        public const string Proveedores = "Proveedores";
        public const string Caja = "Caja";
        public const string Inventario = "Inventario";
        public const string Reportes = "Reportes";
        public const string Configuracion = "Configuracion";
        public const string Usuarios = "Usuarios";
        public const string Sistema = "Sistema";
        public const string AsistenteIA = "AsistenteIA";
        public const string SIFEN = "SIFEN";
        public const string NotasCredito = "NotasCredito";
        public const string Cobros = "Cobros";
        public const string Pagos = "Pagos";
    }
}
