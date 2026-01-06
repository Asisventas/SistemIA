-- Insertar registros de historial de cambios del sistema
INSERT INTO HistorialCambiosSistema 
(Version, FechaCambio, TituloCambio, Tema, TipoCambio, ModuloAfectado, Prioridad, DescripcionBreve, DescripcionTecnica, ArchivosModificados, Tags, Referencias, ImplementadoPor, Estado, RequiereMigracion, NombreMigracion, FechaCreacion)
VALUES 
('2.1.0', '2026-01-06 09:00:00', 'Crear modulo de Historial de Cambios del Sistema', 'Infraestructura', 'Nueva Funcionalidad', 'Sistema', 'Alta', 
'Se implemento un modulo completo para registrar y consultar los cambios realizados en el sistema, permitiendo filtrar por fecha, tema, tipo y palabras clave.',
'Creacion de modelos HistorialCambioSistema y ConversacionIAHistorial con campos para Tema, Tags, Referencias. Implementacion de servicio IHistorialCambiosService con metodos para registro automatico. Pagina de exploracion con filtros, exportacion Excel e impresion.',
'Models/HistorialCambioSistema.cs, Services/HistorialCambiosService.cs, Pages/HistorialCambiosExplorar.razor, Pages/ConversacionesIAExplorar.razor',
'historial, cambios, documentacion, IA, contexto', 'Solicitud usuario - Sistema de seguimiento', 'Claude Opus 4.5', 'Implementado', 1, 'CrearTablasHistorialCambiosYConversacionesIA', GETDATE()),

('2.1.0', '2026-01-06 09:15:00', 'Agregar pagina de Conversaciones IA', 'Asistente IA', 'Nueva Funcionalidad', 'Sistema', 'Media', 
'Se creo pagina para visualizar el historial de conversaciones con asistentes de IA (Claude, GPT, etc.)',
'Pagina ConversacionesIAExplorar.razor que muestra ID, Fecha, Modelo IA, Titulo y Resumen de cada sesion.',
'Pages/ConversacionesIAExplorar.razor',
'conversaciones, IA, historial, claude, gpt', NULL, 'Claude Opus 4.5', 'Implementado', 0, NULL, GETDATE()),

('2.1.0', '2026-01-06 09:30:00', 'Agregar links al menu de navegacion', 'UI/UX', 'Mejora', 'NavMenu', 'Media', 
'Se agregaron enlaces en el menu lateral para acceder a Historial de Cambios y Conversaciones IA',
'Modificacion de NavMenu.razor para incluir dos nuevos items en la seccion Sistema: Historial Cambios (con badge Nuevo) y Conversaciones IA.',
'Shared/NavMenu.razor',
'menu, navegacion, links, sidebar', NULL, 'Claude Opus 4.5', 'Implementado', 0, NULL, GETDATE()),

('2.1.0', '2026-01-06 09:45:00', 'Actualizar instrucciones para Copilot', 'Infraestructura', 'Mejora', 'Documentacion', 'Alta', 
'Se actualizo copilot-instructions.md con documentacion sobre el sistema de historial de cambios',
'Agregada seccion completa sobre el sistema de historial, incluyendo: estructura de tablas, temas estandar, uso del servicio, ejemplos de registro, consultas SQL.',
'.github/copilot-instructions.md',
'documentacion, copilot, instrucciones, IA', NULL, 'Claude Opus 4.5', 'Implementado', 0, NULL, GETDATE()),

('2.0.9', '2026-01-05 14:00:00', 'Implementar Asistente IA integrado', 'Asistente IA', 'Nueva Funcionalidad', 'ChatAsistente', 'Alta', 
'Se implemento un asistente conversacional integrado que ayuda a los usuarios con preguntas sobre el sistema',
'Chat flotante en todas las paginas, base de conocimiento editable, deteccion de intenciones por regex, soporte para envio a soporte tecnico.',
'Shared/ChatAsistente.razor, Services/AsistenteIAService.cs, Models/AsistenteIA/*.cs, Pages/AdminAsistenteIA.razor',
'asistente, IA, chat, soporte, conocimiento', NULL, 'Claude Opus 4.5', 'Implementado', 1, 'CrearTablasAsistenteIA', GETDATE()),

('2.0.8', '2026-01-04 10:00:00', 'Sistema de correo electronico automatico', 'Correo', 'Nueva Funcionalidad', 'ConfiguracionCorreo', 'Alta', 
'Se implemento sistema completo de envio de informes por correo electronico',
'Configuracion SMTP por sucursal, destinatarios con informes seleccionables, envio automatico al cierre, multiples tipos de informes.',
'Models/ConfiguracionCorreo.cs, Models/DestinatarioInforme.cs, Services/CorreoService.cs, Services/InformeCorreoService.cs, Pages/ConfiguracionCorreo.razor',
'correo, email, smtp, informes, automatico', NULL, 'Claude Opus 4.5', 'Implementado', 1, 'AgregarConfiguracionCorreo', GETDATE()),

('2.0.7', '2026-01-03 11:30:00', 'Mejoras en el modulo SIFEN', 'SIFEN', 'Mejora', 'Ventas', 'Alta', 
'Se corrigieron problemas con la generacion del XML y envio de facturas electronicas',
'Correccion de formato CDC, ajuste de namespaces XML, manejo de errores de conexion TLS con ekuatia.set.gov.py',
'Services/DEXmlBuilder.cs, Services/SifenService.cs, Utils/CdcGenerator.cs',
'sifen, factura electronica, xml, cdc, set', 'SIFEN_SOLUCION_FINAL.md', 'Claude Opus 4.5', 'Implementado', 0, NULL, GETDATE()),

('2.0.6', '2026-01-02 16:00:00', 'Implementar precios diferenciados por cliente', 'Ventas', 'Nueva Funcionalidad', 'Ventas', 'Media', 
'Se agrego funcionalidad para asignar precios especiales a clientes especificos',
'Tabla ClientePrecios con relacion Cliente-Producto-Precio. Al cargar cliente en venta, se aplican sus precios personalizados.',
'Models/ClientePrecio.cs, Pages/ClientePreciosExplorar.razor, Pages/Ventas.razor',
'precios, clientes, descuentos, personalizado', 'IMPLEMENTACION_PRECIOS_DIFERENCIADOS.md', 'Claude Opus 4.5', 'Implementado', 1, 'AgregarClientePrecios', GETDATE()),

('2.0.5', '2025-12-28 09:00:00', 'Centro de Actualizaciones (SistemIA.Actualizador)', 'Actualizador', 'Nueva Funcionalidad', 'Sistema', 'Alta', 
'Proyecto independiente para manejar actualizaciones del sistema sin cerrar la aplicacion',
'Aplicacion Blazor Server en puerto 5096, detecta SistemIA, crea backup, extrae ZIP, actualiza archivos, reinicia servicio.',
'SistemIA.Actualizador/Pages/Index.razor, SistemIA.Actualizador/Program.cs',
'actualizador, update, versiones, instalador', 'MODULO_ACTUALIZACION_README.md', 'Claude Opus 4.5', 'Implementado', 0, NULL, GETDATE()),

('2.0.4', '2025-12-20 14:30:00', 'Sistema de permisos y roles', 'Usuarios', 'Nueva Funcionalidad', 'Seguridad', 'Alta', 
'Se implemento sistema completo de permisos por modulo y rol',
'Componentes PageProtection y RequirePermission, tabla Permisos, PermisosModulo, asignacion por rol.',
'Components/PageProtection.razor, Components/RequirePermission.razor, Services/PermisosService.cs, Pages/PermisosUsuarios.razor',
'permisos, roles, seguridad, autorizacion', 'SISTEMA_PERMISOS_README.md', 'Claude Opus 4.5', 'Implementado', 1, 'AgregarSistemaPermisos', GETDATE());

-- Insertar conversacion de ejemplo
INSERT INTO ConversacionesIAHistorial
(FechaInicio, FechaFin, ModeloIA, Titulo, ResumenEjecutivo, ObjetivosSesion, ResultadosObtenidos, TareasPendientes, ModulosTrabajados, ArchivosCreados, ArchivosModificados, Etiquetas, Complejidad, DuracionMinutos, CantidadCambios, FechaCreacion)
VALUES
(DATEADD(HOUR, -2, GETDATE()), GETDATE(), 'Claude Opus 4.5', 
'Implementacion del modulo de Historial de Cambios',
'Se implemento un modulo completo para registrar y consultar cambios del sistema. Incluye modelos, servicio, paginas de exploracion y documentacion.',
'Crear sistema de seguimiento de cambios con busqueda por Fecha, Tema, Tags, Referencias',
'Modelos creados, servicio implementado, paginas funcionales, menu actualizado, documentacion agregada a copilot-instructions.md',
'Ninguna - modulo completamente funcional',
'Sistema, Infraestructura, UI/UX',
'Pages/HistorialCambiosExplorar.razor, Pages/ConversacionesIAExplorar.razor, Services/HistorialCambiosService.cs',
'Models/HistorialCambioSistema.cs, Shared/NavMenu.razor, .github/copilot-instructions.md',
'historial, cambios, documentacion, IA',
'Complejo', 120, 4, GETDATE());

PRINT 'Datos insertados correctamente';
