using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Data
{
    public static class SeedPermisos
    {
        public static async Task ActualizarModulosAsync(IDbContextFactory<AppDbContext> dbFactory)
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();

            // Obtener módulos padres existentes
            var ventas = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/ventas" && m.IdModuloPadre == null);
            var inventario = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/inventario" && m.IdModuloPadre == null);
            var clientes = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/clientes" && m.IdModuloPadre == null);
            var proveedores = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/proveedores" && m.IdModuloPadre == null);
            var compras = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/compras" && m.IdModuloPadre == null);
            var reportes = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/reportes" && m.IdModuloPadre == null);
            var personal = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/personal" && m.IdModuloPadre == null);
            var configuracion = await ctx.Modulos.FirstOrDefaultAsync(m => m.RutaPagina == "/configuracion" && m.IdModuloPadre == null);

            var nuevosModulos = new List<Modulo>();

            // Agregar submódulos de Ventas si no existen
            if (ventas != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/ventas/presupuestos"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Presupuestos",
                        Descripcion = "Gestión de presupuestos",
                        Icono = "bi-receipt",
                        Orden = 2,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/ventas/presupuestos"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/ventas/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Historial de Ventas",
                        Descripcion = "Consulta de ventas realizadas",
                        Icono = "bi-clock-history",
                        Orden = 3,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/ventas/explorar"
                    });
                }
            }

            // Agregar submódulos de Inventario si no existen
            if (inventario != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/inventario/depositos"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Depósitos",
                        Descripcion = "Gestión de depósitos",
                        Icono = "bi-building",
                        Orden = 2,
                        IdModuloPadre = inventario.IdModulo,
                        Activo = true,
                        RutaPagina = "/inventario/depositos"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/inventario/ajustes/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Historial de Ajustes",
                        Descripcion = "Consulta de ajustes realizados",
                        Icono = "bi-clock-history",
                        Orden = 4,
                        IdModuloPadre = inventario.IdModulo,
                        Activo = true,
                        RutaPagina = "/inventario/ajustes/explorar"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/reportes/productos-detallado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Informe de Productos",
                        Descripcion = "Informe detallado de productos",
                        Icono = "bi-file-earmark-text",
                        Orden = 5,
                        IdModuloPadre = inventario.IdModulo,
                        Activo = true,
                        RutaPagina = "/reportes/productos-detallado"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/inventario/alertas-vencimiento"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Alertas de Vencimiento",
                        Descripcion = "Control de productos próximos a vencer",
                        Icono = "bi-exclamation-triangle",
                        Orden = 6,
                        IdModuloPadre = inventario.IdModulo,
                        Activo = true,
                        RutaPagina = "/inventario/alertas-vencimiento"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/inventario/lotes"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Gestión de Lotes",
                        Descripcion = "Administración de lotes de productos",
                        Icono = "bi-box-seam",
                        Orden = 7,
                        IdModuloPadre = inventario.IdModulo,
                        Activo = true,
                        RutaPagina = "/inventario/lotes"
                    });
                }
            }

            // Agregar submódulos de Clientes si no existen
            if (clientes != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/clientes/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Historial de Clientes",
                        Descripcion = "Consulta de clientes",
                        Icono = "bi-people",
                        Orden = 2,
                        IdModuloPadre = clientes.IdModulo,
                        Activo = true,
                        RutaPagina = "/clientes/explorar"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/clientes/cobros"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Cobros",
                        Descripcion = "Gestión de cobros a clientes",
                        Icono = "bi-cash-coin",
                        Orden = 3,
                        IdModuloPadre = clientes.IdModulo,
                        Activo = true,
                        RutaPagina = "/clientes/cobros"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/cuentas-por-cobrar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Pendientes de Cobro",
                        Descripcion = "Informe de cuentas por cobrar pendientes",
                        Icono = "bi-file-earmark-text",
                        Orden = 4,
                        IdModuloPadre = clientes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/cuentas-por-cobrar"
                    });
                }
            }

            // Agregar submódulos de Proveedores si no existen
            if (proveedores != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/proveedores/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Historial de Proveedores",
                        Descripcion = "Consulta de proveedores",
                        Icono = "bi-list-ul",
                        Orden = 2,
                        IdModuloPadre = proveedores.IdModulo,
                        Activo = true,
                        RutaPagina = "/proveedores/explorar"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/proveedores/sifen"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Proveedores SIFEN",
                        Descripcion = "Catálogo de proveedores SIFEN",
                        Icono = "bi-database",
                        Orden = 3,
                        IdModuloPadre = proveedores.IdModulo,
                        Activo = true,
                        RutaPagina = "/proveedores/sifen"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/pagos"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Pagos a Proveedores",
                        Descripcion = "Gestión de pagos",
                        Icono = "bi-cash-stack",
                        Orden = 4,
                        IdModuloPadre = proveedores.IdModulo,
                        Activo = true,
                        RutaPagina = "/pagos"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/cuentas-por-pagar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Pendientes de Pago",
                        Descripcion = "Informe de cuentas por pagar pendientes",
                        Icono = "bi-file-earmark-text",
                        Orden = 5,
                        IdModuloPadre = proveedores.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/cuentas-por-pagar"
                    });
                }
            }

            // Agregar submódulos de Reportes si no existen
            if (reportes != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/reportes/ventas-detallado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Informe de Ventas",
                        Descripcion = "Informe detallado de ventas",
                        Icono = "bi-file-bar-graph",
                        Orden = 1,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/reportes/ventas-detallado"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/reportes/compras-general"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Informe de Compras General",
                        Descripcion = "Informe general de compras",
                        Icono = "bi-file-earmark-bar-graph",
                        Orden = 2,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/reportes/compras-general"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/reportes/compras-detallado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Informe de Compras Detallado",
                        Descripcion = "Informe detallado de compras",
                        Icono = "bi-file-earmark-spreadsheet",
                        Orden = 3,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/reportes/compras-detallado"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/reportes/asistencias"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Informe de Asistencias",
                        Descripcion = "Informe de asistencias del personal",
                        Icono = "bi-calendar2-check",
                        Orden = 4,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/reportes/asistencias"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/reportes/listado-asistencias"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Listado de Asistencias",
                        Descripcion = "Listado de asistencias",
                        Icono = "bi-list-check",
                        Orden = 5,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/reportes/listado-asistencias"
                    });
                }
            }

            // Agregar submódulos de Personal si no existen
            if (personal != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/personal/usuarios"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Menú de Usuarios",
                        Descripcion = "Gestión de usuarios del sistema",
                        Icono = "bi-person-circle",
                        Orden = 2,
                        IdModuloPadre = personal.IdModulo,
                        Activo = true,
                        RutaPagina = "/personal/usuarios"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/personal/asistencias/registro"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Registro de Asistencias",
                        Descripcion = "Registro de entrada/salida",
                        Icono = "bi-clock",
                        Orden = 3,
                        IdModuloPadre = personal.IdModulo,
                        Activo = true,
                        RutaPagina = "/personal/asistencias/registro"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/personal/horarios"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Horarios",
                        Descripcion = "Gestión de horarios",
                        Icono = "bi-calendar3",
                        Orden = 4,
                        IdModuloPadre = personal.IdModulo,
                        Activo = true,
                        RutaPagina = "/personal/horarios"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/personal/asignacion-horarios"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Asignación de Horarios",
                        Descripcion = "Asignación de horarios a empleados",
                        Icono = "bi-calendar-plus",
                        Orden = 5,
                        IdModuloPadre = personal.IdModulo,
                        Activo = true,
                        RutaPagina = "/personal/asignacion-horarios"
                    });
                }
            }

            // Agregar submódulos de Configuración si no existen
            if (configuracion != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/configuracion/cajas"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Cajas",
                        Descripcion = "Configuración de cajas",
                        Icono = "bi-cash-register",
                        Orden = 3,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/configuracion/cajas"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/configuracion/tipos-pago"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Tipos de Pago",
                        Descripcion = "Configuración de tipos de pago",
                        Icono = "bi-credit-card",
                        Orden = 4,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/configuracion/tipos-pago"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/configuracion/tipos-documento"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Tipos de Documento",
                        Descripcion = "Configuración de tipos de documento",
                        Icono = "bi-file-text",
                        Orden = 5,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/configuracion/tipos-documento"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/configuracion/tipos-iva"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Tipos de IVA",
                        Descripcion = "Configuración de tipos de IVA",
                        Icono = "bi-percent",
                        Orden = 6,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/configuracion/tipos-iva"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/configuracion/listas-precios"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Listas de Precios",
                        Descripcion = "Configuración de listas de precios",
                        Icono = "bi-tag",
                        Orden = 7,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/configuracion/listas-precios"
                    });
                }

                // Módulo Configuración de Correo Electrónico
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/configuracion/correo"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Correo Electrónico",
                        Descripcion = "Configuración de correo para envío de informes y notificaciones",
                        Icono = "bi-envelope-at",
                        Orden = 8,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/configuracion/correo"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/generar-paquete-actualizacion"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Generar Paquete",
                        Descripcion = "Generar paquete de actualización del sistema",
                        Icono = "bi-box-seam",
                        Orden = 9,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/generar-paquete-actualizacion"
                    });
                }

                // Módulo Manual del Sistema
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/manual-sistema"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Manual del Sistema",
                        Descripcion = "Documentación y ayuda del sistema",
                        Icono = "bi-book",
                        Orden = 10,
                        IdModuloPadre = configuracion.IdModulo,
                        Activo = true,
                        RutaPagina = "/manual-sistema"
                    });
                }
            }

            // Agregar submódulos de Notas de Crédito (bajo Ventas) si no existen
            if (ventas != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/notas-credito"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Nota de Crédito",
                        Descripcion = "Emisión de notas de crédito sobre ventas",
                        Icono = "bi-file-earmark-minus",
                        Orden = 4,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/notas-credito"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/notas-credito/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Explorar Notas de Crédito",
                        Descripcion = "Consulta de notas de crédito emitidas",
                        Icono = "bi-clock-history",
                        Orden = 5,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/notas-credito/explorar"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/notas-credito/imprimir"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Imprimir NC Ventas",
                        Descripcion = "Impresión de notas de crédito de ventas",
                        Icono = "bi-printer",
                        Orden = 6,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/notas-credito/imprimir"
                    });
                }
            }

            // Agregar submódulos de Notas de Crédito Compra (bajo Compras) si no existen
            if (compras != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/notas-credito-compra"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Nota de Crédito Compra",
                        Descripcion = "Emisión de notas de crédito sobre compras",
                        Icono = "bi-file-earmark-minus",
                        Orden = 3,
                        IdModuloPadre = compras.IdModulo,
                        Activo = true,
                        RutaPagina = "/notas-credito-compra"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/notas-credito-compra/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Explorar NC Compras",
                        Descripcion = "Consulta de notas de crédito de compras",
                        Icono = "bi-clock-history",
                        Orden = 4,
                        IdModuloPadre = compras.IdModulo,
                        Activo = true,
                        RutaPagina = "/notas-credito-compra/explorar"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/notas-credito-compra/imprimir"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Imprimir NC Compras",
                        Descripcion = "Impresión de notas de crédito de compras",
                        Icono = "bi-printer",
                        Orden = 5,
                        IdModuloPadre = compras.IdModulo,
                        Activo = true,
                        RutaPagina = "/notas-credito-compra/imprimir"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/compras/explorar"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Historial de Compras",
                        Descripcion = "Consulta de compras realizadas",
                        Icono = "bi-clock-history",
                        Orden = 2,
                        IdModuloPadre = compras.IdModulo,
                        Activo = true,
                        RutaPagina = "/compras/explorar"
                    });
                }
            }

            // Agregar submódulos de Caja (bajo Ventas) si no existen
            if (ventas != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/caja/cierre"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Cierre de Caja",
                        Descripcion = "Cierre y arqueo de caja",
                        Icono = "bi-cash-stack",
                        Orden = 10,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/caja/cierre"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/caja/historial-cierres"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Historial de Cierres",
                        Descripcion = "Consulta de cierres de caja anteriores",
                        Icono = "bi-clock-history",
                        Orden = 11,
                        IdModuloPadre = ventas.IdModulo,
                        Activo = true,
                        RutaPagina = "/caja/historial-cierres"
                    });
                }
            }

            // Agregar submódulos de Informes de Caja (bajo Reportes) si no existen
            if (reportes != null)
            {
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/resumen-caja"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Resumen de Caja",
                        Descripcion = "Informe resumen de movimientos de caja",
                        Icono = "bi-journal-check",
                        Orden = 10,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/resumen-caja"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/ventas-clasificacion"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Ventas por Clasificación",
                        Descripcion = "Informe de ventas agrupado por clasificación de productos",
                        Icono = "bi-pie-chart",
                        Orden = 11,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/ventas-clasificacion"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/movimientos-productos"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Movimientos de Inventario",
                        Descripcion = "Informe de movimientos de inventario con trazabilidad y valorización",
                        Icono = "bi-arrow-left-right",
                        Orden = 12,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/movimientos-productos"
                    });
                }

                // Informes de Notas de Crédito de Compras
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/nc-compras-detallado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "NC Compras Detallado",
                        Descripcion = "Informe detallado de notas de crédito de compras",
                        Icono = "bi-file-earmark-minus",
                        Orden = 13,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/nc-compras-detallado"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/nc-compras-agrupado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "NC Compras Agrupado",
                        Descripcion = "Informe agrupado de notas de crédito de compras",
                        Icono = "bi-file-earmark-minus",
                        Orden = 14,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/nc-compras-agrupado"
                    });
                }

                // Informes de Notas de Crédito de Ventas
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/nc-detallado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "NC Ventas Detallado",
                        Descripcion = "Informe detallado de notas de crédito de ventas",
                        Icono = "bi-file-earmark-minus",
                        Orden = 15,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/nc-detallado"
                    });
                }

                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/nc-agrupado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "NC Ventas Agrupado",
                        Descripcion = "Informe agrupado de notas de crédito de ventas",
                        Icono = "bi-file-earmark-minus",
                        Orden = 16,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/nc-agrupado"
                    });
                }

                // Informe de Ventas Agrupado
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/ventas-agrupado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Ventas Agrupado",
                        Descripcion = "Informe de ventas agrupado por fecha/cliente/producto",
                        Icono = "bi-bar-chart",
                        Orden = 17,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/ventas-agrupado"
                    });
                }

                // Informe de Ventas Detallado
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/ventas-detallado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Ventas Detallado",
                        Descripcion = "Informe detallado de ventas con desglose de productos",
                        Icono = "bi-list-ul",
                        Orden = 18,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/ventas-detallado"
                    });
                }

                // Informe de Productos Valorizado
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/productos-valorizado"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Inventario Valorizado",
                        Descripcion = "Informe de productos con valorización de stock",
                        Icono = "bi-currency-dollar",
                        Orden = 19,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/productos-valorizado"
                    });
                }

                // Informe de Ajustes de Stock
                if (!await ctx.Modulos.AnyAsync(m => m.RutaPagina == "/informes/ajustes-stock"))
                {
                    nuevosModulos.Add(new Modulo
                    {
                        Nombre = "Ajustes de Stock",
                        Descripcion = "Informe de ajustes de inventario realizados",
                        Icono = "bi-clipboard-data",
                        Orden = 20,
                        IdModuloPadre = reportes.IdModulo,
                        Activo = true,
                        RutaPagina = "/informes/ajustes-stock"
                    });
                }
            }

            if (nuevosModulos.Any())
            {
                ctx.Modulos.AddRange(nuevosModulos);
                await ctx.SaveChangesAsync();

                // Asignar permisos al rol Admin para los nuevos módulos
                var rolAdmin = await ctx.Roles.FirstOrDefaultAsync(r => r.NombreRol.ToLower().Contains("admin"));
                if (rolAdmin != null)
                {
                    var todosLosPermisos = await ctx.Permisos.ToListAsync();
                    var permisosAdmin = new List<RolModuloPermiso>();

                    foreach (var modulo in nuevosModulos)
                    {
                        foreach (var permiso in todosLosPermisos)
                        {
                            permisosAdmin.Add(new RolModuloPermiso
                            {
                                IdRol = rolAdmin.Id_Rol,
                                IdModulo = modulo.IdModulo,
                                IdPermiso = permiso.IdPermiso,
                                Concedido = true,
                                FechaAsignacion = DateTime.Now,
                                UsuarioAsignacion = "Sistema"
                            });
                        }
                    }

                    ctx.RolesModulosPermisos.AddRange(permisosAdmin);
                    await ctx.SaveChangesAsync();
                }

                Console.WriteLine($"[ActualizarModulos] Se agregaron {nuevosModulos.Count} nuevos módulos");
            }
            else
            {
                Console.WriteLine($"[ActualizarModulos] Todos los módulos ya existen");
            }

            // Asegurar que el rol Administrador tenga TODOS los permisos para TODOS los módulos
            await AsegurarPermisosAdminAsync(ctx);
        }

        /// <summary>
        /// Asegura que el rol Administrador tenga todos los permisos para todos los módulos.
        /// Añade los permisos faltantes sin duplicar los existentes.
        /// </summary>
        private static async Task AsegurarPermisosAdminAsync(AppDbContext ctx)
        {
            var rolAdmin = await ctx.Roles.FirstOrDefaultAsync(r => r.NombreRol.ToLower().Contains("admin"));
            if (rolAdmin == null)
            {
                Console.WriteLine("[AsegurarPermisosAdmin] No se encontró rol Administrador");
                return;
            }

            var todosLosModulos = await ctx.Modulos.ToListAsync();
            var todosLosPermisos = await ctx.Permisos.ToListAsync();
            var permisosExistentes = await ctx.RolesModulosPermisos
                .Where(rmp => rmp.IdRol == rolAdmin.Id_Rol)
                .Select(rmp => new { rmp.IdModulo, rmp.IdPermiso })
                .ToListAsync();

            var permisosFaltantes = new List<RolModuloPermiso>();

            foreach (var modulo in todosLosModulos)
            {
                foreach (var permiso in todosLosPermisos)
                {
                    // Verificar si ya existe este permiso
                    var existe = permisosExistentes.Any(p => p.IdModulo == modulo.IdModulo && p.IdPermiso == permiso.IdPermiso);
                    if (!existe)
                    {
                        permisosFaltantes.Add(new RolModuloPermiso
                        {
                            IdRol = rolAdmin.Id_Rol,
                            IdModulo = modulo.IdModulo,
                            IdPermiso = permiso.IdPermiso,
                            Concedido = true,
                            FechaAsignacion = DateTime.Now,
                            UsuarioAsignacion = "Sistema"
                        });
                    }
                }
            }

            if (permisosFaltantes.Any())
            {
                ctx.RolesModulosPermisos.AddRange(permisosFaltantes);
                await ctx.SaveChangesAsync();
                Console.WriteLine($"[AsegurarPermisosAdmin] Se agregaron {permisosFaltantes.Count} permisos faltantes al rol Administrador");
            }
            else
            {
                Console.WriteLine("[AsegurarPermisosAdmin] El rol Administrador ya tiene todos los permisos");
            }
        }

        public static async Task InicializarPermisosAsync(IDbContextFactory<AppDbContext> dbFactory)
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();

            // Verificar si ya existen permisos
            if (await ctx.Permisos.AnyAsync())
                return;

            // 1. Crear permisos estándar
            var permisos = new List<Permiso>
            {
                new Permiso { Nombre = "Ver", Codigo = "VIEW", Descripcion = "Permite ver y consultar información", Orden = 1, Activo = true },
                new Permiso { Nombre = "Crear", Codigo = "CREATE", Descripcion = "Permite crear nuevos registros", Orden = 2, Activo = true },
                new Permiso { Nombre = "Editar", Codigo = "EDIT", Descripcion = "Permite modificar registros existentes", Orden = 3, Activo = true },
                new Permiso { Nombre = "Eliminar", Codigo = "DELETE", Descripcion = "Permite eliminar registros", Orden = 4, Activo = true },
                new Permiso { Nombre = "Exportar", Codigo = "EXPORT", Descripcion = "Permite exportar datos a Excel/PDF", Orden = 5, Activo = true },
                new Permiso { Nombre = "Imprimir", Codigo = "PRINT", Descripcion = "Permite imprimir documentos", Orden = 6, Activo = true }
            };

            ctx.Permisos.AddRange(permisos);
            await ctx.SaveChangesAsync();

            // 2. Crear módulos del sistema (estructura jerárquica)
            var modulos = new List<Modulo>();

            // Módulo principal: Ventas
            var ventas = new Modulo
            {
                Nombre = "Ventas",
                Descripcion = "Gestión de ventas y facturación",
                Icono = "bi-cart",
                Orden = 1,
                Activo = true,
                RutaPagina = "/ventas"
            };
            modulos.Add(ventas);

            // Módulo principal: Compras
            var compras = new Modulo
            {
                Nombre = "Compras",
                Descripcion = "Gestión de compras a proveedores",
                Icono = "bi-bag",
                Orden = 2,
                Activo = true,
                RutaPagina = "/compras"
            };
            modulos.Add(compras);

            // Módulo principal: Inventario
            var inventario = new Modulo
            {
                Nombre = "Inventario",
                Descripcion = "Control de stock y productos",
                Icono = "bi-box-seam",
                Orden = 3,
                Activo = true,
                RutaPagina = "/inventario"
            };
            modulos.Add(inventario);

            // Módulo principal: Clientes
            var clientes = new Modulo
            {
                Nombre = "Clientes",
                Descripcion = "Gestión de clientes",
                Icono = "bi-people",
                Orden = 4,
                Activo = true,
                RutaPagina = "/clientes"
            };
            modulos.Add(clientes);

            // Módulo principal: Proveedores
            var proveedores = new Modulo
            {
                Nombre = "Proveedores",
                Descripcion = "Gestión de proveedores",
                Icono = "bi-truck",
                Orden = 5,
                Activo = true,
                RutaPagina = "/proveedores"
            };
            modulos.Add(proveedores);

            // Módulo principal: Reportes
            var reportes = new Modulo
            {
                Nombre = "Reportes",
                Descripcion = "Informes y estadísticas",
                Icono = "bi-graph-up",
                Orden = 6,
                Activo = true,
                RutaPagina = "/reportes"
            };
            modulos.Add(reportes);

            // Módulo principal: Personal
            var personal = new Modulo
            {
                Nombre = "Gestión de Personal",
                Descripcion = "Administración de empleados y recursos humanos",
                Icono = "bi-person-badge",
                Orden = 7,
                Activo = true,
                RutaPagina = "/personal"
            };
            modulos.Add(personal);

            // Módulo principal: Configuración
            var configuracion = new Modulo
            {
                Nombre = "Configuración",
                Descripcion = "Configuración del sistema",
                Icono = "bi-gear",
                Orden = 8,
                Activo = true,
                RutaPagina = "/configuracion"
            };
            modulos.Add(configuracion);

            ctx.Modulos.AddRange(modulos);
            await ctx.SaveChangesAsync();

            // 3. Crear submódulos (después de tener IDs de los padres)
            var submodulos = new List<Modulo>();

            // Submódulos de Ventas
            submodulos.Add(new Modulo
            {
                Nombre = "Nueva Venta",
                Descripcion = "Crear nueva venta",
                Icono = "bi-plus-circle",
                Orden = 1,
                IdModuloPadre = ventas.IdModulo,
                Activo = true,
                RutaPagina = "/ventas"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Presupuestos",
                Descripcion = "Gestión de presupuestos",
                Icono = "bi-receipt",
                Orden = 2,
                IdModuloPadre = ventas.IdModulo,
                Activo = true,
                RutaPagina = "/ventas/presupuestos"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Historial de Ventas",
                Descripcion = "Consulta de ventas realizadas",
                Icono = "bi-clock-history",
                Orden = 3,
                IdModuloPadre = ventas.IdModulo,
                Activo = true,
                RutaPagina = "/ventas/explorar"
            });

            // Submódulos de Inventario
            submodulos.Add(new Modulo
            {
                Nombre = "Productos",
                Descripcion = "Gestión de productos",
                Icono = "bi-box",
                Orden = 1,
                IdModuloPadre = inventario.IdModulo,
                Activo = true,
                RutaPagina = "/productos"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Depósitos",
                Descripcion = "Gestión de depósitos",
                Icono = "bi-building",
                Orden = 2,
                IdModuloPadre = inventario.IdModulo,
                Activo = true,
                RutaPagina = "/inventario/depositos"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Ajustes de Stock",
                Descripcion = "Ajustes manuales de inventario",
                Icono = "bi-arrow-repeat",
                Orden = 3,
                IdModuloPadre = inventario.IdModulo,
                Activo = true,
                RutaPagina = "/inventario/ajustes"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Historial de Ajustes",
                Descripcion = "Consulta de ajustes realizados",
                Icono = "bi-clock-history",
                Orden = 4,
                IdModuloPadre = inventario.IdModulo,
                Activo = true,
                RutaPagina = "/inventario/ajustes/explorar"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Informe de Productos",
                Descripcion = "Informe detallado de productos",
                Icono = "bi-file-earmark-text",
                Orden = 5,
                IdModuloPadre = inventario.IdModulo,
                Activo = true,
                RutaPagina = "/reportes/productos-detallado"
            });

            // Submódulos de Clientes
            submodulos.Add(new Modulo
            {
                Nombre = "Gestión de Clientes",
                Descripcion = "ABM de clientes",
                Icono = "bi-person-plus",
                Orden = 1,
                IdModuloPadre = clientes.IdModulo,
                Activo = true,
                RutaPagina = "/clientes"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Historial de Clientes",
                Descripcion = "Consulta de clientes",
                Icono = "bi-people",
                Orden = 2,
                IdModuloPadre = clientes.IdModulo,
                Activo = true,
                RutaPagina = "/clientes/explorar"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Cobros",
                Descripcion = "Gestión de cobros a clientes",
                Icono = "bi-cash-coin",
                Orden = 3,
                IdModuloPadre = clientes.IdModulo,
                Activo = true,
                RutaPagina = "/clientes/cobros"
            });

            // Submódulos de Proveedores
            submodulos.Add(new Modulo
            {
                Nombre = "Gestión de Proveedores",
                Descripcion = "ABM de proveedores",
                Icono = "bi-truck",
                Orden = 1,
                IdModuloPadre = proveedores.IdModulo,
                Activo = true,
                RutaPagina = "/proveedores"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Historial de Proveedores",
                Descripcion = "Consulta de proveedores",
                Icono = "bi-list-ul",
                Orden = 2,
                IdModuloPadre = proveedores.IdModulo,
                Activo = true,
                RutaPagina = "/proveedores/explorar"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Proveedores SIFEN",
                Descripcion = "Catálogo de proveedores SIFEN",
                Icono = "bi-database",
                Orden = 3,
                IdModuloPadre = proveedores.IdModulo,
                Activo = true,
                RutaPagina = "/proveedores/sifen"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Pagos a Proveedores",
                Descripcion = "Gestión de pagos",
                Icono = "bi-cash-stack",
                Orden = 4,
                IdModuloPadre = proveedores.IdModulo,
                Activo = true,
                RutaPagina = "/pagos"
            });

            // Submódulos de Reportes
            submodulos.Add(new Modulo
            {
                Nombre = "Informe de Ventas",
                Descripcion = "Informe detallado de ventas",
                Icono = "bi-file-bar-graph",
                Orden = 1,
                IdModuloPadre = reportes.IdModulo,
                Activo = true,
                RutaPagina = "/reportes/ventas-detallado"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Informe de Compras General",
                Descripcion = "Informe general de compras",
                Icono = "bi-file-earmark-bar-graph",
                Orden = 2,
                IdModuloPadre = reportes.IdModulo,
                Activo = true,
                RutaPagina = "/reportes/compras-general"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Informe de Compras Detallado",
                Descripcion = "Informe detallado de compras",
                Icono = "bi-file-earmark-spreadsheet",
                Orden = 3,
                IdModuloPadre = reportes.IdModulo,
                Activo = true,
                RutaPagina = "/reportes/compras-detallado"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Informe de Asistencias",
                Descripcion = "Informe de asistencias del personal",
                Icono = "bi-calendar2-check",
                Orden = 4,
                IdModuloPadre = reportes.IdModulo,
                Activo = true,
                RutaPagina = "/reportes/asistencias"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Listado de Asistencias",
                Descripcion = "Listado de asistencias",
                Icono = "bi-list-check",
                Orden = 5,
                IdModuloPadre = reportes.IdModulo,
                Activo = true,
                RutaPagina = "/reportes/listado-asistencias"
            });

            // Submódulos de Personal
            submodulos.Add(new Modulo
            {
                Nombre = "Empleados",
                Descripcion = "Gestión de empleados",
                Icono = "bi-person",
                Orden = 1,
                IdModuloPadre = personal.IdModulo,
                Activo = true,
                RutaPagina = "/personal/empleados"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Menú de Usuarios",
                Descripcion = "Gestión de usuarios del sistema",
                Icono = "bi-person-circle",
                Orden = 2,
                IdModuloPadre = personal.IdModulo,
                Activo = true,
                RutaPagina = "/personal/usuarios"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Registro de Asistencias",
                Descripcion = "Registro de entrada/salida",
                Icono = "bi-clock",
                Orden = 3,
                IdModuloPadre = personal.IdModulo,
                Activo = true,
                RutaPagina = "/personal/asistencias/registro"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Horarios",
                Descripcion = "Gestión de horarios",
                Icono = "bi-calendar3",
                Orden = 4,
                IdModuloPadre = personal.IdModulo,
                Activo = true,
                RutaPagina = "/personal/horarios"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Asignación de Horarios",
                Descripcion = "Asignación de horarios a empleados",
                Icono = "bi-calendar-plus",
                Orden = 5,
                IdModuloPadre = personal.IdModulo,
                Activo = true,
                RutaPagina = "/personal/asignacion-horarios"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Permisos de Usuarios",
                Descripcion = "Configuración de permisos por rol",
                Icono = "bi-shield-lock",
                Orden = 6,
                IdModuloPadre = personal.IdModulo,
                Activo = true,
                RutaPagina = "/personal/permisos-usuarios"
            });

            // Submódulos de Configuración
            submodulos.Add(new Modulo
            {
                Nombre = "Usuarios",
                Descripcion = "Gestión de usuarios del sistema",
                Icono = "bi-person-circle",
                Orden = 1,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/usuarios"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Roles",
                Descripcion = "Gestión de roles",
                Icono = "bi-diagram-3",
                Orden = 2,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/roles"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Cajas",
                Descripcion = "Configuración de cajas",
                Icono = "bi-cash-register",
                Orden = 3,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/cajas"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Tipos de Pago",
                Descripcion = "Configuración de tipos de pago",
                Icono = "bi-credit-card",
                Orden = 4,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/tipos-pago"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Tipos de Documento",
                Descripcion = "Configuración de tipos de documento",
                Icono = "bi-file-text",
                Orden = 5,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/tipos-documento"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Tipos de IVA",
                Descripcion = "Configuración de tipos de IVA",
                Icono = "bi-percent",
                Orden = 6,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/tipos-iva"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Listas de Precios",
                Descripcion = "Configuración de listas de precios",
                Icono = "bi-tag",
                Orden = 7,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/listas-precios"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Correo Electrónico",
                Descripcion = "Configuración de correo para envío de informes y notificaciones",
                Icono = "bi-envelope-at",
                Orden = 8,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/correo"
            });

            submodulos.Add(new Modulo
            {
                Nombre = "Auditoría",
                Descripcion = "Registro de acciones del sistema",
                Icono = "bi-journal-text",
                Orden = 9,
                IdModuloPadre = configuracion.IdModulo,
                Activo = true,
                RutaPagina = "/configuracion/auditoria"
            });

            ctx.Modulos.AddRange(submodulos);
            await ctx.SaveChangesAsync();

            // 4. Asignar TODOS los permisos al rol de Administrador (Id_Rol = 1, típicamente)
            var rolAdmin = await ctx.Roles.FirstOrDefaultAsync(r => r.NombreRol.ToLower().Contains("admin"));
            if (rolAdmin != null)
            {
                var todosLosModulos = await ctx.Modulos.ToListAsync();
                var todosLosPermisos = await ctx.Permisos.ToListAsync();

                var permisosAdmin = new List<RolModuloPermiso>();
                foreach (var modulo in todosLosModulos)
                {
                    foreach (var permiso in todosLosPermisos)
                    {
                        permisosAdmin.Add(new RolModuloPermiso
                        {
                            IdRol = rolAdmin.Id_Rol,
                            IdModulo = modulo.IdModulo,
                            IdPermiso = permiso.IdPermiso,
                            Concedido = true,
                            FechaAsignacion = DateTime.Now,
                            UsuarioAsignacion = "Sistema"
                        });
                    }
                }

                ctx.RolesModulosPermisos.AddRange(permisosAdmin);
                await ctx.SaveChangesAsync();
            }

            Console.WriteLine($"[SeedPermisos] Inicialización completada: {permisos.Count} permisos, {modulos.Count + submodulos.Count} módulos");
        }
    }
}
