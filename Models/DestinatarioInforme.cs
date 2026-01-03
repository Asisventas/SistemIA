using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Destinatarios de correo para envío automático de informes.
    /// Permite configurar múltiples correos por tipo de informe.
    /// </summary>
    [Table("DestinatariosInforme")]
    public class DestinatarioInforme
    {
        [Key]
        public int IdDestinatarioInforme { get; set; }

        // ========== INFORMACIÓN DEL DESTINATARIO ==========

        /// <summary>
        /// Nombre del destinatario (para identificación)
        /// </summary>
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Dirección de correo electrónico
        /// </summary>
        [Required(ErrorMessage = "El correo es requerido")]
        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el destinatario está activo
        /// </summary>
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // ========== TIPOS DE INFORMES ==========

        /// <summary>
        /// Recibir informe de ventas diario
        /// </summary>
        [Display(Name = "Ventas Diarias")]
        public bool RecibeVentasDiarias { get; set; } = false;

        /// <summary>
        /// Recibir resumen de ventas semanal
        /// </summary>
        [Display(Name = "Resumen Semanal")]
        public bool RecibeResumenSemanal { get; set; } = false;

        /// <summary>
        /// Recibir resumen de ventas mensual
        /// </summary>
        [Display(Name = "Resumen Mensual")]
        public bool RecibeResumenMensual { get; set; } = true;

        /// <summary>
        /// Recibir informe de stock bajo
        /// </summary>
        [Display(Name = "Alertas Stock Bajo")]
        public bool RecibeAlertaStock { get; set; } = false;

        /// <summary>
        /// Recibir informe de cierres de caja
        /// </summary>
        [Display(Name = "Cierres de Caja")]
        public bool RecibeCierreCaja { get; set; } = false;

        /// <summary>
        /// Recibir informe de compras
        /// </summary>
        [Display(Name = "Informe Compras")]
        public bool RecibeInformeCompras { get; set; } = false;

        /// <summary>
        /// Recibir informe de compras detallado
        /// </summary>
        [Display(Name = "Compras Detallado")]
        public bool RecibeComprasDetallado { get; set; } = false;

        /// <summary>
        /// Recibir informe de ventas detallado
        /// </summary>
        [Display(Name = "Ventas Detallado")]
        public bool RecibeVentasDetallado { get; set; } = false;

        /// <summary>
        /// Recibir informe de ventas agrupado
        /// </summary>
        [Display(Name = "Ventas Agrupado")]
        public bool RecibeVentasAgrupado { get; set; } = false;

        /// <summary>
        /// Recibir informe de ventas por clasificación
        /// </summary>
        [Display(Name = "Ventas Clasificación")]
        public bool RecibeVentasClasificacion { get; set; } = false;

        /// <summary>
        /// Recibir informe de notas de crédito
        /// </summary>
        [Display(Name = "Notas de Crédito")]
        public bool RecibeNotasCredito { get; set; } = false;

        /// <summary>
        /// Recibir informe de notas de crédito detallado
        /// </summary>
        [Display(Name = "NC Detallado")]
        public bool RecibeNCDetallado { get; set; } = false;

        /// <summary>
        /// Recibir informe de notas de crédito de compras
        /// </summary>
        [Display(Name = "NC Compras")]
        public bool RecibeNCCompras { get; set; } = false;

        /// <summary>
        /// Recibir informe de productos valorizado
        /// </summary>
        [Display(Name = "Productos Valorizado")]
        public bool RecibeProductosValorizado { get; set; } = false;

        /// <summary>
        /// Recibir informe de productos detallado
        /// </summary>
        [Display(Name = "Productos Detallado")]
        public bool RecibeProductosDetallado { get; set; } = false;

        /// <summary>
        /// Recibir informe de movimientos de productos
        /// </summary>
        [Display(Name = "Movimientos Stock")]
        public bool RecibeMovimientosStock { get; set; } = false;

        /// <summary>
        /// Recibir informe de ajustes de stock
        /// </summary>
        [Display(Name = "Ajustes Stock")]
        public bool RecibeAjustesStock { get; set; } = false;

        /// <summary>
        /// Recibir informe de cuentas por cobrar
        /// </summary>
        [Display(Name = "Cuentas por Cobrar")]
        public bool RecibeCuentasPorCobrar { get; set; } = false;

        /// <summary>
        /// Recibir informe de cuentas por pagar
        /// </summary>
        [Display(Name = "Cuentas por Pagar")]
        public bool RecibeCuentasPorPagar { get; set; } = false;

        /// <summary>
        /// Recibir resumen de caja diario
        /// </summary>
        [Display(Name = "Resumen Caja")]
        public bool RecibeResumenCaja { get; set; } = false;

        /// <summary>
        /// Recibir informe de asistencia de empleados
        /// </summary>
        [Display(Name = "Asistencia")]
        public bool RecibeAsistencia { get; set; } = false;

        /// <summary>
        /// Recibir informe de facturas electrónicas (SIFEN)
        /// </summary>
        [Display(Name = "Resumen SIFEN")]
        public bool RecibeResumenSifen { get; set; } = false;

        /// <summary>
        /// Recibir alertas de vencimientos (productos, documentos)
        /// </summary>
        [Display(Name = "Alertas Vencimientos")]
        public bool RecibeAlertaVencimientos { get; set; } = false;

        /// <summary>
        /// Recibir copia de todas las facturas emitidas
        /// </summary>
        [Display(Name = "Copia Facturas")]
        public bool RecibeCopiaFacturas { get; set; } = false;

        /// <summary>
        /// Recibir resumen al cierre del sistema
        /// </summary>
        [Display(Name = "Resumen al Cierre")]
        public bool RecibeResumenCierre { get; set; } = false;

        // ========== CONFIGURACIÓN DE ENVÍO ==========

        /// <summary>
        /// Tipo de copia: TO (principal), CC (copia), BCC (oculta)
        /// </summary>
        [StringLength(5)]
        [Display(Name = "Tipo de Copia")]
        public string TipoCopia { get; set; } = "TO";

        /// <summary>
        /// Notas o comentarios sobre este destinatario
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }

        // ========== RELACIONES ==========

        /// <summary>
        /// ID de la sucursal (si es específico de una sucursal)
        /// </summary>
        [Display(Name = "Sucursal")]
        public int? IdSucursal { get; set; }

        [ForeignKey("IdSucursal")]
        public virtual Sucursal? Sucursal { get; set; }

        /// <summary>
        /// ID de la configuración de correo asociada
        /// </summary>
        [Display(Name = "Configuración de Correo")]
        public int? IdConfiguracionCorreo { get; set; }

        [ForeignKey("IdConfiguracionCorreo")]
        public virtual ConfiguracionCorreo? ConfiguracionCorreo { get; set; }

        // ========== AUDITORÍA ==========

        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string? UsuarioCreacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========

        /// <summary>
        /// Resumen de los informes que recibe este destinatario
        /// </summary>
        [NotMapped]
        public string ResumenInformes
        {
            get
            {
                var informes = new List<string>();
                if (RecibeVentasDiarias) informes.Add("Ventas Diarias");
                if (RecibeResumenSemanal) informes.Add("Semanal");
                if (RecibeResumenMensual) informes.Add("Mensual");
                if (RecibeAlertaStock) informes.Add("Stock");
                if (RecibeCierreCaja) informes.Add("Cierres");
                if (RecibeInformeCompras) informes.Add("Compras");
                if (RecibeComprasDetallado) informes.Add("Compras Det.");
                if (RecibeVentasDetallado) informes.Add("Ventas Det.");
                if (RecibeVentasAgrupado) informes.Add("Ventas Agrup.");
                if (RecibeVentasClasificacion) informes.Add("Ventas Clasif.");
                if (RecibeNotasCredito) informes.Add("NC");
                if (RecibeNCDetallado) informes.Add("NC Det.");
                if (RecibeNCCompras) informes.Add("NC Compras");
                if (RecibeProductosValorizado) informes.Add("Prod. Val.");
                if (RecibeProductosDetallado) informes.Add("Prod. Det.");
                if (RecibeMovimientosStock) informes.Add("Mov. Stock");
                if (RecibeAjustesStock) informes.Add("Ajustes");
                if (RecibeCuentasPorCobrar) informes.Add("CxC");
                if (RecibeCuentasPorPagar) informes.Add("CxP");
                if (RecibeResumenCaja) informes.Add("Res. Caja");
                if (RecibeAsistencia) informes.Add("Asistencia");
                if (RecibeResumenSifen) informes.Add("SIFEN");
                if (RecibeAlertaVencimientos) informes.Add("Vencimientos");
                if (RecibeCopiaFacturas) informes.Add("Facturas");
                if (RecibeResumenCierre) informes.Add("Cierre Sistema");

                return informes.Count > 0 ? string.Join(", ", informes) : "Ninguno";
            }
        }
    }

    /// <summary>
    /// Tipos de copia para destinatarios
    /// </summary>
    public static class TiposCopiaCorreo
    {
        public const string Principal = "TO";
        public const string Copia = "CC";
        public const string CopiaOculta = "BCC";

        public static List<(string Valor, string Nombre)> Listado => new()
        {
            (Principal, "Principal (TO)"),
            (Copia, "Con Copia (CC)"),
            (CopiaOculta, "Copia Oculta (BCC)")
        };
    }
}
