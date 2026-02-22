using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    /// <summary>
    /// Enumeración de tipos de informes disponibles para envío automático por correo.
    /// </summary>
    public enum TipoInformeEnum
    {
        // ========== INFORMES DE VENTAS ==========
        [Display(Name = "Ventas Diarias")]
        VentasDiarias = 1,

        [Display(Name = "Ventas Detallado")]
        VentasDetallado = 2,

        [Display(Name = "Ventas Agrupado")]
        VentasAgrupado = 3,

        [Display(Name = "Ventas por Clasificación")]
        VentasClasificacion = 4,

        // ========== INFORMES DE COMPRAS ==========
        [Display(Name = "Compras General")]
        ComprasGeneral = 10,

        [Display(Name = "Compras Detallado")]
        ComprasDetallado = 11,

        // ========== INFORMES DE NOTAS DE CRÉDITO ==========
        [Display(Name = "Notas de Crédito Agrupado")]
        NotasCreditoAgrupado = 20,

        [Display(Name = "Notas de Crédito Detallado")]
        NotasCreditoDetallado = 21,

        [Display(Name = "NC Compras Agrupado")]
        NCComprasAgrupado = 22,

        [Display(Name = "NC Compras Detallado")]
        NCComprasDetallado = 23,

        // ========== INFORMES DE STOCK ==========
        [Display(Name = "Productos Valorizado")]
        ProductosValorizado = 30,

        [Display(Name = "Productos Detallado")]
        ProductosDetallado = 31,

        [Display(Name = "Movimientos de Productos")]
        MovimientosProductos = 32,

        [Display(Name = "Ajustes de Stock")]
        AjustesStock = 33,

        [Display(Name = "Alerta Stock Bajo")]
        AlertaStockBajo = 34,

        // ========== INFORMES DE CAJA ==========
        [Display(Name = "Cierre de Caja")]
        CierreCaja = 40,

        [Display(Name = "Resumen de Caja")]
        ResumenCaja = 41,

        [Display(Name = "Cierre de Complejos")]
        CierreComplejos = 42,

        // ========== INFORMES FINANCIEROS ==========
        [Display(Name = "Cuentas por Cobrar")]
        CuentasPorCobrar = 50,

        [Display(Name = "Cuentas por Pagar")]
        CuentasPorPagar = 51,

        // ========== INFORMES DE PERSONAL ==========
        [Display(Name = "Asistencia")]
        Asistencia = 60,

        // ========== INFORMES SIFEN ==========
        [Display(Name = "Resumen SIFEN")]
        ResumenSifen = 70,

        // ========== INFORMES PERIÓDICOS ==========
        [Display(Name = "Resumen Semanal")]
        ResumenSemanal = 80,

        [Display(Name = "Resumen Mensual")]
        ResumenMensual = 81,

        [Display(Name = "Resumen al Cierre del Sistema")]
        ResumenCierreSistema = 82,

        // ========== ALERTAS ==========
        [Display(Name = "Alerta Vencimientos")]
        AlertaVencimientos = 90,

        // ========== INFORMES DE GIMNASIO ==========
        [Display(Name = "Asistencia Clases Gimnasio")]
        GimnasioAsistenciaClases = 100,

        [Display(Name = "Resumen Membresías")]
        GimnasioMembresias = 101,

        [Display(Name = "Clases Más Populares")]
        GimnasioClasesPopulares = 102,

        [Display(Name = "Rendimiento Instructores")]
        GimnasioInstructores = 103,

        [Display(Name = "Membresías por Vencer")]
        GimnasioMembresiasPorVencer = 104
    }

    /// <summary>
    /// Clase estática con métodos de utilidad para informes.
    /// </summary>
    public static class TipoInformeHelper
    {
        /// <summary>
        /// Obtiene la lista de todos los informes disponibles agrupados por categoría.
        /// </summary>
        public static List<InformeCategorizado> ObtenerInformesCategorizados()
        {
            return new List<InformeCategorizado>
            {
                // Ventas
                new("Ventas", new List<InformeInfo>
                {
                    new(TipoInformeEnum.VentasDiarias, "Ventas Diarias", "Informe diario de ventas realizadas", "RecibeVentasDiarias"),
                    new(TipoInformeEnum.VentasDetallado, "Ventas Detallado", "Informe detallado de ventas con desglose de productos", "RecibeVentasDetallado"),
                    new(TipoInformeEnum.VentasAgrupado, "Ventas Agrupado", "Ventas agrupadas por período/vendedor", "RecibeVentasAgrupado"),
                    new(TipoInformeEnum.VentasClasificacion, "Ventas por Clasificación", "Ventas clasificadas por categoría de producto", "RecibeVentasClasificacion")
                }),

                // Compras
                new("Compras", new List<InformeInfo>
                {
                    new(TipoInformeEnum.ComprasGeneral, "Compras General", "Resumen general de compras", "RecibeInformeCompras"),
                    new(TipoInformeEnum.ComprasDetallado, "Compras Detallado", "Informe detallado de compras por proveedor", "RecibeComprasDetallado")
                }),

                // Notas de Crédito
                new("Notas de Crédito", new List<InformeInfo>
                {
                    new(TipoInformeEnum.NotasCreditoAgrupado, "NC Agrupado", "Notas de crédito agrupadas", "RecibeNotasCredito"),
                    new(TipoInformeEnum.NotasCreditoDetallado, "NC Detallado", "Notas de crédito con detalle", "RecibeNCDetallado"),
                    new(TipoInformeEnum.NCComprasAgrupado, "NC Compras Agrupado", "NC de compras agrupadas", "RecibeNCCompras"),
                    new(TipoInformeEnum.NCComprasDetallado, "NC Compras Detallado", "NC de compras detallado", "RecibeNCCompras")
                }),

                // Stock
                new("Stock / Inventario", new List<InformeInfo>
                {
                    new(TipoInformeEnum.ProductosValorizado, "Productos Valorizado", "Inventario valorizado", "RecibeProductosValorizado"),
                    new(TipoInformeEnum.ProductosDetallado, "Productos Detallado", "Lista detallada de productos", "RecibeProductosDetallado"),
                    new(TipoInformeEnum.MovimientosProductos, "Movimientos", "Movimientos de entrada/salida de stock", "RecibeMovimientosStock"),
                    new(TipoInformeEnum.AjustesStock, "Ajustes de Stock", "Ajustes manuales de inventario", "RecibeAjustesStock"),
                    new(TipoInformeEnum.AlertaStockBajo, "Alerta Stock Bajo", "Productos con stock bajo mínimo", "RecibeAlertaStock")
                }),

                // Caja
                new("Caja", new List<InformeInfo>
                {
                    new(TipoInformeEnum.CierreCaja, "Cierre de Caja", "Detalle del cierre de caja", "RecibeCierreCaja"),
                    new(TipoInformeEnum.ResumenCaja, "Resumen de Caja", "Resumen diario de caja", "RecibeResumenCaja"),
                    new(TipoInformeEnum.CierreComplejos, "Cierre de Complejos", "Informe de cierre de complejos/restaurante", "RecibeInformeComplejos")
                }),

                // Financieros
                new("Financieros", new List<InformeInfo>
                {
                    new(TipoInformeEnum.CuentasPorCobrar, "Cuentas por Cobrar", "Estado de cuentas a cobrar", "RecibeCuentasPorCobrar"),
                    new(TipoInformeEnum.CuentasPorPagar, "Cuentas por Pagar", "Estado de cuentas a pagar", "RecibeCuentasPorPagar")
                }),

                // Personal
                new("Personal", new List<InformeInfo>
                {
                    new(TipoInformeEnum.Asistencia, "Asistencia", "Registro de asistencia de empleados", "RecibeAsistencia")
                }),

                // SIFEN
                new("Facturación Electrónica", new List<InformeInfo>
                {
                    new(TipoInformeEnum.ResumenSifen, "Resumen SIFEN", "Estado de documentos electrónicos", "RecibeResumenSifen")
                }),

                // Periódicos
                new("Resúmenes Periódicos", new List<InformeInfo>
                {
                    new(TipoInformeEnum.ResumenSemanal, "Resumen Semanal", "Resumen consolidado semanal", "RecibeResumenSemanal"),
                    new(TipoInformeEnum.ResumenMensual, "Resumen Mensual", "Resumen consolidado mensual", "RecibeResumenMensual"),
                    new(TipoInformeEnum.ResumenCierreSistema, "Resumen al Cierre", "Resumen al cerrar el sistema", "RecibeResumenCierre")
                }),

                // Alertas
                new("Alertas", new List<InformeInfo>
                {
                    new(TipoInformeEnum.AlertaVencimientos, "Vencimientos", "Alertas de productos/documentos próximos a vencer", "RecibeAlertaVencimientos")
                }),

                // Gimnasio
                new("Gimnasio", new List<InformeInfo>
                {
                    new(TipoInformeEnum.GimnasioAsistenciaClases, "Asistencia a Clases", "Registro de asistencia diaria a clases grupales", "RecibeInformeGimnasio"),
                    new(TipoInformeEnum.GimnasioMembresias, "Resumen Membresías", "Estado actual de membresías activas", "RecibeInformeGimnasio"),
                    new(TipoInformeEnum.GimnasioClasesPopulares, "Clases Populares", "Ranking de clases por popularidad", "RecibeInformeGimnasio"),
                    new(TipoInformeEnum.GimnasioInstructores, "Informe Instructores", "Rendimiento y horas de instructores", "RecibeInformeGimnasio"),
                    new(TipoInformeEnum.GimnasioMembresiasPorVencer, "Membresías por Vencer", "Membresías próximas a expirar", "RecibeInformeGimnasio")
                })
            };
        }

        /// <summary>
        /// Obtiene la lista plana de todos los informes disponibles.
        /// </summary>
        public static List<InformeInfo> ObtenerTodosLosInformes()
        {
            return ObtenerInformesCategorizados()
                .SelectMany(c => c.Informes)
                .ToList();
        }

        /// <summary>
        /// Obtiene el nombre del campo del DestinatarioInforme que controla si recibe este tipo de informe.
        /// </summary>
        public static string ObtenerCampoDestinatario(TipoInformeEnum tipo)
        {
            return ObtenerTodosLosInformes()
                .FirstOrDefault(i => i.Tipo == tipo)?
                .CampoDestinatario ?? string.Empty;
        }
    }

    /// <summary>
    /// Información de un informe específico.
    /// </summary>
    public class InformeInfo
    {
        public TipoInformeEnum Tipo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string CampoDestinatario { get; set; }

        public InformeInfo(TipoInformeEnum tipo, string nombre, string descripcion, string campoDestinatario)
        {
            Tipo = tipo;
            Nombre = nombre;
            Descripcion = descripcion;
            CampoDestinatario = campoDestinatario;
        }
    }

    /// <summary>
    /// Informes agrupados por categoría.
    /// </summary>
    public class InformeCategorizado
    {
        public string Categoria { get; set; }
        public List<InformeInfo> Informes { get; set; }

        public InformeCategorizado(string categoria, List<InformeInfo> informes)
        {
            Categoria = categoria;
            Informes = informes;
        }
    }
}
