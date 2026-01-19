using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using System.Linq;

namespace SistemIA.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TiposDocumentosIdentidad> TiposDocumentosIdentidad { get; set; }
        public DbSet<TiposContribuyentes> TiposContribuyentes { get; set; }
    public DbSet<Paises> Paises { get; set; }
        public DbSet<TipoOperacion> TiposOperacion { get; set; }
        public DbSet<Sucursal> Sucursal { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        [Obsolete("Use ProveedoresSifen en su lugar")]
        public DbSet<ProveedorLegacy> Proveedores { get; set; }
        public DbSet<ProveedorSifenMejorado> ProveedoresSifen { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<HorarioTrabajo> HorariosTrabajo { get; set; }
        
        public DbSet<AsignacionHorario> AsignacionesHorarios { get; set; }
        public DbSet<TiposIva> TiposIva { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<TipoCambio> TiposCambio { get; set; }
        public DbSet<TipoCambioHistorico> TiposCambioHistorico { get; set; }
        public DbSet<ListaPrecio> ListasPrecios { get; set; }
        public DbSet<ListaPrecioDetalle> ListasPreciosDetalles { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Clasificacion> Clasificaciones { get; set; }
        public DbSet<TiposItem> TiposItem { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        public DbSet<ProductoDeposito> ProductosDepositos { get; set; }
        public DbSet<ProductoLote> ProductosLotes { get; set; }
        public DbSet<MovimientoLote> MovimientosLotes { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<ProductoComponente> ProductosComponentes { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraDetalle> ComprasDetalles { get; set; }
        public DbSet<Caja> Cajas { get; set; }
    public DbSet<TipoPago> TiposPago { get; set; }
    public DbSet<TipoDocumentoOperacion> TiposDocumentoOperacion { get; set; }
    public DbSet<AjusteStock> AjustesStock { get; set; }
    public DbSet<AjusteStockDetalle> AjustesStockDetalles { get; set; }
    public DbSet<TransferenciaDeposito> TransferenciasDeposito { get; set; }
    public DbSet<TransferenciaDepositoDetalle> TransferenciasDepositoDetalle { get; set; }
    public DbSet<ClientePrecio> ClientesPrecios { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<VentaDetalle> VentasDetalles { get; set; }
    public DbSet<RecetaVenta> RecetasVentas { get; set; }
    public DbSet<Presupuesto> Presupuestos { get; set; }
    public DbSet<PresupuestoDetalle> PresupuestosDetalles { get; set; }
    public DbSet<Sociedad> Sociedades { get; set; }
    public DbSet<SociedadActividadEconomica> SociedadesActividades { get; set; }
    
    // Configuración del sistema
    public DbSet<ConfiguracionSistema> ConfiguracionSistema { get; set; }
    
    // Descuentos por categoría (marca/clasificación/todos)
    public DbSet<DescuentoCategoria> DescuentosCategorias { get; set; }
        
    // Catálogo SIFEN nuevo (departamento/ciudad/distrito)
    public DbSet<DepartamentoCatalogo> DepartamentosCatalogo { get; set; }
    public DbSet<DistritoCatalogo> DistritosCatalogo { get; set; }
    public DbSet<CiudadCatalogo> CiudadesCatalogo { get; set; }

        // Nuevas entidades para pagos y numeración
        public DbSet<Timbrado> Timbrados { get; set; }
        public DbSet<ComposicionCaja> ComposicionesCaja { get; set; }
        public DbSet<ComposicionCajaDetalle> ComposicionesCajaDetalles { get; set; }
        public DbSet<VentaPago> VentasPagos { get; set; }
        public DbSet<VentaPagoDetalle> VentasPagosDetalles { get; set; }
        public DbSet<VentaCuota> VentasCuotas { get; set; }

        // Catálogo RUC DNIT
        public DbSet<RucDnit> RucDnit { get; set; }

        // Sistema de Créditos y Remisiones
        public DbSet<CuentaPorCobrar> CuentasPorCobrar { get; set; }
        public DbSet<CuentaPorCobrarCuota> CuentasPorCobrarCuotas { get; set; }
        public DbSet<RemisionInterna> RemisionesInternas { get; set; }
        public DbSet<RemisionInternaDetalle> RemisionesInternasDetalles { get; set; }
        public DbSet<CobroCuota> CobrosCuotas { get; set; }
        public DbSet<CobroDetalle> CobrosDetalles { get; set; }

        // Sistema de Pagos a Proveedores
        public DbSet<CuentaPorPagar> CuentasPorPagar { get; set; }
        public DbSet<CuentaPorPagarCuota> CuentasPorPagarCuotas { get; set; }
        public DbSet<PagoProveedor> PagosProveedores { get; set; }
        public DbSet<PagoProveedorDetalle> PagosProveedoresDetalles { get; set; }

        // Sistema de Permisos y Auditoría
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolModuloPermiso> RolesModulosPermisos { get; set; }
        public DbSet<AuditoriaAccion> AuditoriasAcciones { get; set; }

        // Sistema de Cierre de Caja
        public DbSet<CierreCaja> CierresCaja { get; set; }
        public DbSet<EntregaCaja> EntregasCaja { get; set; }

        // Notas de Crédito Ventas
        public DbSet<NotaCreditoVenta> NotasCreditoVentas { get; set; }
        public DbSet<NotaCreditoVentaDetalle> NotasCreditoVentasDetalles { get; set; }

        // Notas de Crédito Compras
        public DbSet<NotaCreditoCompra> NotasCreditoCompras { get; set; }
        public DbSet<NotaCreditoCompraDetalle> NotasCreditoComprasDetalles { get; set; }

        // Configuración de Correo Electrónico
        public DbSet<ConfiguracionCorreo> ConfiguracionesCorreo { get; set; }
        public DbSet<DestinatarioInforme> DestinatariosInforme { get; set; }

        // Asistente IA
        public DbSet<SistemIA.Models.AsistenteIA.RegistroError> RegistrosErrorIA { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.ConversacionAsistente> ConversacionesAsistente { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.ArticuloConocimientoDB> ArticulosConocimiento { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.PreguntaSinRespuesta> PreguntasSinRespuesta { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.CategoriaConocimiento> CategoriasConocimiento { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.ConfiguracionAsistenteIA> ConfiguracionesAsistenteIA { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.SolicitudSoporteIA> SolicitudesSoporteIA { get; set; }
        public DbSet<SistemIA.Models.AsistenteIA.AccionUsuario> AccionesUsuario { get; set; }

        // Presupuestos del Sistema (comercial)
        public DbSet<PresupuestoSistema> PresupuestosSistema { get; set; }
        public DbSet<PresupuestoSistemaDetalle> PresupuestosSistemaDetalles { get; set; }

        // Historial de Cambios del Sistema y Conversaciones IA (para contexto y documentación)
        public DbSet<HistorialCambioSistema> HistorialCambiosSistema { get; set; }
        public DbSet<ConversacionIAHistorial> ConversacionesIAHistorial { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuraciones de Claves Primarias (si no siguen la convención) ---
            modelBuilder.Entity<TiposDocumentosIdentidad>().HasKey(t => t.TipoDocumento);
            modelBuilder.Entity<TiposContribuyentes>().HasKey(t => t.IdTipoContribuyente);
            modelBuilder.Entity<Paises>().HasKey(p => p.CodigoPais);
            modelBuilder.Entity<TipoOperacion>().HasKey(t => t.Codigo);
            modelBuilder.Entity<Sucursal>().HasKey(s => s.Id); // Confirma que 'Id' es la PK de Sucursal

            // --- Configuraciones de Tablas y Propiedades ---
            // Tabla "Ciudades" eliminada en favor del catálogo SIFEN (ciudad)

            // --- Sociedad (datos del emisor para SIFEN) ---
            modelBuilder.Entity<Sociedad>(entity =>
            {
                entity.ToTable("Sociedades");
                entity.HasKey(s => s.IdSociedad);
                entity.Property(s => s.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(s => s.RUC).HasMaxLength(15).IsRequired();
                entity.Property(s => s.Direccion).HasMaxLength(255).IsRequired();
                entity.Property(s => s.Email).HasMaxLength(200);
                entity.Property(s => s.Telefono).HasMaxLength(50);
                entity.Property(s => s.IdCsc).HasMaxLength(50);
                entity.Property(s => s.Csc).HasMaxLength(50);
                entity.Property(s => s.PathCertificadoP12).HasMaxLength(400);
                entity.Property(s => s.PasswordCertificadoP12).HasMaxLength(400);
                entity.Property(s => s.PathCertificadoPem).HasMaxLength(400);
                entity.Property(s => s.PathCertificadoCrt).HasMaxLength(400);
                entity.Property(s => s.PathArchivoSinFirma).HasMaxLength(400);
                entity.Property(s => s.PathArchivoFirmado).HasMaxLength(400);
                entity.Property(s => s.DeUrlQr).HasMaxLength(400);
                entity.Property(s => s.DeUrlEnvioDocumento).HasMaxLength(400);
                entity.Property(s => s.DeUrlEnvioEvento).HasMaxLength(400);
                entity.Property(s => s.DeUrlEnvioDocumentoLote).HasMaxLength(400);
                entity.Property(s => s.DeUrlConsultaDocumento).HasMaxLength(400);
                entity.Property(s => s.DeUrlConsultaDocumentoLote).HasMaxLength(400);
                entity.Property(s => s.DeUrlConsultaRuc).HasMaxLength(400);
                entity.Property(s => s.ServidorSifen).HasMaxLength(50);
                entity.Property(s => s.Usuario).HasMaxLength(10);
                entity.Property(s => s.NumeroCasa).HasMaxLength(10);
            });

            // --- Sociedad Actividad Económica ---
            modelBuilder.Entity<SociedadActividadEconomica>(entity =>
            {
                entity.ToTable("SociedadesActividades");
                entity.HasKey(a => a.Numero);
                entity.Property(a => a.CodigoActividad).HasMaxLength(50).HasColumnType("varchar(50)").IsRequired();
                entity.Property(a => a.NombreActividad).HasMaxLength(300).HasColumnType("varchar(300)");
                entity.Property(a => a.ActividadPrincipal).HasMaxLength(1).HasColumnType("char(1)");
                entity.HasOne(a => a.Sociedad)
                      .WithMany()
                      .HasForeignKey(a => a.IdSociedad)
                      .HasPrincipalKey(s => s.IdSociedad)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(a => new { a.IdSociedad, a.CodigoActividad }).IsUnique();
            });

            // Tablas antiguas 'Ciudades' y 'Distritos' han sido retiradas

            // --- Catálogo geográfico SIFEN (nuevas tablas) ---
            modelBuilder.Entity<DepartamentoCatalogo>(entity =>
            {
                entity.ToTable("departamento");
                entity.HasKey(d => d.Numero);
                entity.Property(d => d.Numero).ValueGeneratedNever();
                entity.Property(d => d.Nombre).HasMaxLength(100).IsRequired().HasColumnType("varchar(100)");
                entity.HasIndex(d => d.Nombre);
            });

            modelBuilder.Entity<DistritoCatalogo>(entity =>
            {
                entity.ToTable("distrito");
                entity.HasKey(d => d.Numero);
                entity.Property(d => d.Numero).ValueGeneratedNever();
                entity.Property(d => d.Nombre).HasMaxLength(120).IsRequired().HasColumnType("varchar(120)");
                entity.Property(d => d.Departamento).IsRequired();

                entity.HasOne(d => d.DepartamentoNavigation)
                      .WithMany(p => p.Distritos)
                      .HasForeignKey(d => d.Departamento)
                      .HasPrincipalKey(p => p.Numero)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => new { d.Departamento, d.Nombre });
            });

            modelBuilder.Entity<CiudadCatalogo>(entity =>
            {
                entity.ToTable("ciudad");
                entity.HasKey(c => c.Numero);
                entity.Property(c => c.Numero).ValueGeneratedNever();
                entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired().HasColumnType("varchar(100)");
                entity.Property(c => c.Departamento).IsRequired();
                entity.Property(c => c.Distrito).IsRequired();

                entity.HasOne(c => c.DepartamentoNavigation)
                      .WithMany(p => p.Ciudades)
                      .HasForeignKey(c => c.Departamento)
                      .HasPrincipalKey(p => p.Numero)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.DistritoNavigation)
                      .WithMany(p => p.Ciudades)
                      .HasForeignKey(c => c.Distrito)
                      .HasPrincipalKey(p => p.Numero)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new { c.Departamento, c.Distrito, c.Nombre });
            });

            // Relaciones antiguas basadas en 'Distrito'/'Ciudades' eliminadas

            // Sociedad -> Catálogo (opcional)
            modelBuilder.Entity<Sociedad>()
                .HasOne<DepartamentoCatalogo>()
                .WithMany()
                .HasForeignKey(s => s.Departamento)
                .HasPrincipalKey(d => d.Numero)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sociedad>()
                .HasOne<CiudadCatalogo>()
                .WithMany()
                .HasForeignKey(s => s.Ciudad)
                .HasPrincipalKey(c => c.Numero)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sociedad>()
                .HasOne<DistritoCatalogo>()
                .WithMany()
                .HasForeignKey(s => s.Distrito)
                .HasPrincipalKey(d => d.Numero)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Sucursal>(entity =>
            {
                entity.ToTable("Sucursal");
                entity.Property(e => e.NumSucursal).IsRequired().HasMaxLength(7).IsFixedLength();
                entity.Property(e => e.RUC).IsRequired().HasMaxLength(8).IsFixedLength();
                // Aquí, si DV es int?, entonces no debería ser IsRequired(). Si es requerido, quita el '?' del modelo.
                entity.Property(e => e.DV).IsRequired(); // Revisar si DV puede ser nulo en la BD
                entity.Property(e => e.NombreSucursal).HasMaxLength(150).IsRequired();
                entity.Property(e => e.NombreEmpresa).HasMaxLength(150).IsRequired();
                entity.Property(e => e.RubroEmpresa).HasMaxLength(150);
                entity.Property(e => e.Direccion).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Correo).HasMaxLength(100);
                entity.Property(e => e.CertificadoRuta).HasMaxLength(250);
                entity.Property(e => e.CertificadoPassword).HasMaxLength(250);
                entity.Property(e => e.PuntoExpedicion).HasMaxLength(10);
                entity.Property(e => e.IpConsola).HasMaxLength(15);
                entity.Property(e => e.PuertoConsola).HasMaxLength(10);
                entity.Property(e => e.Conexion).HasMaxLength(200);
                entity.Property(e => e.Logo).HasColumnType("VARBINARY(MAX)");
            });

            // Ignorar propiedades que no se mapean a la BD
            modelBuilder.Entity<Usuario>().Ignore(u => u.Salario_Neto);

            // --- Configuraciones de Relaciones (Foreign Keys) ---

        // Evitar columnas sombra en ClientePrecio (ClienteIdCliente / ProductoIdProducto)
        modelBuilder.Entity<ClientePrecio>(entity =>
        {
            entity.HasOne(cp => cp.Cliente)
                .WithMany()
                .HasForeignKey(cp => cp.IdCliente)
                .HasPrincipalKey(c => c.IdCliente)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Producto)
                .WithMany()
                .HasForeignKey(cp => cp.IdProducto)
                .HasPrincipalKey(p => p.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cp => new { cp.IdCliente, cp.IdProducto });
        });

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoDocumentoIdentidad)
                .WithMany(t => t.Clientes)
                .HasForeignKey(c => c.TipoDocumento)
                .HasPrincipalKey(t => t.TipoDocumento);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoContribuyente)
                .WithMany(t => t.Clientes)
                .HasForeignKey(c => c.IdTipoContribuyente)
                .HasPrincipalKey(t => t.IdTipoContribuyente);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Pais)
                .WithMany(p => p.Clientes)
                .HasForeignKey(c => c.CodigoPais)
                .HasPrincipalKey(p => p.CodigoPais);

            // Cliente -> CiudadCatalogo (FK: IdCiudad -> ciudad.Numero)
            modelBuilder.Entity<Cliente>()
                .HasOne<CiudadCatalogo>()
                .WithMany()
                .HasForeignKey(c => c.IdCiudad)
                .HasPrincipalKey(cc => cc.Numero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoOperacionNavigation)
                .WithMany(t => t.Clientes)
                .HasForeignKey(c => c.TipoOperacion)
                .HasPrincipalKey(t => t.Codigo);

            // Sucursal -> CiudadCatalogo (FK: IdCiudad -> ciudad.Numero)
            modelBuilder.Entity<Sucursal>()
                .HasOne<CiudadCatalogo>()
                .WithMany()
                .HasForeignKey(s => s.IdCiudad)
                .HasPrincipalKey(cc => cc.Numero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.Id_Rol)
                .OnDelete(DeleteBehavior.Restrict);

            // --- REEMPLAZA LA CONFIGURACIÓN ANTERIOR DE USUARIO-HORARIO ---
            modelBuilder.Entity<AsignacionHorario>()
                .HasOne(a => a.Usuario)
                .WithMany(u => u.Asignaciones) // Se relaciona con la colección en Usuario
                .HasForeignKey(a => a.Id_Usuario)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra un usuario, se borran sus asignaciones

            modelBuilder.Entity<AsignacionHorario>()
                .HasOne(a => a.HorarioTrabajo)
                .WithMany(h => h.Asignaciones) // Se relaciona con la colección en HorarioTrabajo
                .HasForeignKey(a => a.Id_Horario)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra un horario, se borran sus asignaciones

            // ...

            // --- Configuración de ProveedorLegacy (tabla existente con columnas limitadas) ---
            // OBSOLETO: Mantenido por compatibilidad con datos históricos
            modelBuilder.Entity<ProveedorLegacy>(entity =>
            {
                entity.ToTable("Proveedores");
                entity.HasKey(p => p.Id_Proveedor);

                // Columnas existentes en la tabla
                entity.Property(p => p.CodigoProveedor);
                entity.Property(p => p.Nombre).IsRequired();
                entity.Property(p => p.RUC).IsRequired();
                entity.Property(p => p.Direccion);
                entity.Property(p => p.Telefono);
                entity.Property(p => p.Rubro);
                entity.Property(p => p.Contacto);

                // Ignorar propiedades que NO existen en la tabla física
                entity.Ignore(p => p.DV);
                entity.Ignore(p => p.Correo);
                entity.Ignore(p => p.FotoLogo);
                entity.Ignore(p => p.EstadoProveedor);
                entity.Ignore(p => p.VencimientoTimbrado);
                entity.Ignore(p => p.Timbrado);
                entity.Ignore(p => p.IdTipoContribuyente);
                entity.Ignore(p => p.CodigoContribuyente);
                entity.Ignore(p => p.FechaCreacion);
                entity.Ignore(p => p.UsuarioCreacion);
                entity.Ignore(p => p.FechaModificacion);
                entity.Ignore(p => p.UsuarioModificacion);
                entity.Ignore(p => p.TipoContribuyente);
            });

            // --- Configuración para ProveedorSifenMejorado ---
            modelBuilder.Entity<ProveedorSifenMejorado>(entity =>
            {
                entity.ToTable("ProveedoresSifen");
                entity.HasKey(p => p.IdProveedor);

                // Configuraciones de propiedades específicas
                entity.Property(p => p.RUC)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsFixedLength();

                entity.Property(p => p.Timbrado)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsFixedLength();

                // Campos opcionales
                entity.Property(p => p.Establecimiento)
                    .HasMaxLength(3);

                entity.Property(p => p.PuntoExpedicion)
                    .HasMaxLength(3);

                entity.Property(p => p.CodigoActividadEconomica)
                    .HasMaxLength(5);

                entity.Property(p => p.CodigoDepartamento)
                    .HasMaxLength(2);

                entity.Property(p => p.CodigoPais)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasDefaultValue("PRY");

                // Configuraciones de precisión decimal
                entity.Property(p => p.LimiteCredito)
                    .HasColumnType("decimal(18,4)");

                entity.Property(p => p.SaldoPendiente)
                    .HasColumnType("decimal(18,4)")
                    .HasDefaultValue(0);

                // Relación con TiposContribuyentes
                entity.HasOne(p => p.TipoContribuyenteCatalogo)
                    .WithMany()
                    .HasForeignKey(p => p.IdTipoContribuyenteCatalogo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.Id_Usuario)
                .HasPrincipalKey(u => u.Id_Usu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.AprobadoPorUsuario)
                .WithMany()
                .HasForeignKey(a => a.AprobadoPorId_Usuario)
                .HasPrincipalKey(u => u.Id_Usu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.SucursalNavigation)
                .WithMany()
                .HasForeignKey(a => a.Sucursal)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Configuración de Producto ---
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(p => p.IdProducto);
                entity.Property(p => p.CodigoInterno).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Descripcion).HasMaxLength(200).IsRequired();
                entity.Property(p => p.CodigoBarras).HasMaxLength(14);
                entity.Property(p => p.UnidadMedidaCodigo).HasMaxLength(3).IsRequired();
                entity.Property(p => p.CostoUnitarioGs).HasColumnType("decimal(18,4)");
                entity.Property(p => p.PrecioUnitarioGs).HasColumnType("decimal(18,4)").IsRequired();
                entity.Property(p => p.PrecioUnitarioUsd).HasColumnType("decimal(18,4)");
                entity.Property(p => p.Stock).HasColumnType("decimal(18,4)");
                entity.Property(p => p.StockMinimo).HasColumnType("decimal(18,4)");
                entity.Property(p => p.Foto).HasColumnType("varchar(180)");
                entity.Property(p => p.UndMedida).HasColumnType("char(10)").HasMaxLength(10).IsRequired();
                entity.Property(p => p.IP).HasMaxLength(50);
                entity.Property(p => p.EsCombo).HasDefaultValue(false);

                entity.Property(p => p.IdSucursal).HasColumnName("suc").IsRequired();

                entity.HasOne(p => p.Marca)
                    .WithMany()
                    .HasForeignKey(p => p.IdMarca)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.TipoIva)
                    .WithMany()
                    .HasForeignKey(p => p.IdTipoIva)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Sucursal)
                    .WithMany()
                    .HasForeignKey(p => p.IdSucursal)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Clasificacion)
                    .WithMany()
                    .HasForeignKey(p => p.IdClasificacion)
                    .OnDelete(DeleteBehavior.SetNull);

                // Depósito predeterminado (opcional)
                entity.HasOne(p => p.DepositoPredeterminado)
                    .WithMany()
                    .HasForeignKey(p => p.IdDepositoPredeterminado)
                    .OnDelete(DeleteBehavior.SetNull);

                // FK opcional al catálogo TiposItem usando la columna existente int TipoItem
                entity.HasOne<TiposItem>()
                    .WithMany()
                    .HasForeignKey(p => p.TipoItem)
                    .HasPrincipalKey(t => t.IdTipoItem)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductosComponentes (definición de combos)
            modelBuilder.Entity<ProductoComponente>(entity =>
            {
                entity.ToTable("ProductosComponentes");
                entity.HasKey(pc => pc.IdProductoComponente);
                entity.Property(pc => pc.Cantidad).HasColumnType("decimal(18,4)");

                entity.HasOne(pc => pc.Producto)
                    .WithMany(p => p.Componentes!)
                    .HasForeignKey(pc => pc.IdProducto)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Componente)
                    .WithMany()
                    .HasForeignKey(pc => pc.IdComponente)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(pc => new { pc.IdProducto, pc.IdComponente }).IsUnique();
            });

            // Catálogo TiposItem
            modelBuilder.Entity<TiposItem>(entity =>
            {
                entity.ToTable("TiposItem");
                entity.HasKey(t => t.IdTipoItem);
                entity.Property(t => t.Nombre).HasMaxLength(50).IsRequired();
            });

            // --- Depósitos ---
            modelBuilder.Entity<Deposito>(entity =>
            {
                entity.ToTable("Depositos");
                entity.HasKey(d => d.IdDeposito);
                entity.Property(d => d.Nombre).HasMaxLength(120).IsRequired();
                entity.Property(d => d.Descripcion).HasMaxLength(250);
                entity.Property(d => d.IdSucursal).HasColumnName("suc").IsRequired();

                entity.HasOne(d => d.Sucursal)
                    .WithMany()
                    .HasForeignKey(d => d.IdSucursal)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => new { d.IdSucursal, d.Nombre }).IsUnique();
            });

            // --- ProductoDeposito (stock por depósito) ---
            modelBuilder.Entity<ProductoDeposito>(entity =>
            {
                entity.ToTable("ProductosDepositos");
                entity.HasKey(pd => pd.IdProductoDeposito);
                entity.Property(pd => pd.Stock).HasColumnType("decimal(18,4)");
                entity.Property(pd => pd.StockMinimo).HasColumnType("decimal(18,4)");

                entity.HasOne(pd => pd.Producto)
                    .WithMany(p => p.StocksPorDeposito!)
                    .HasForeignKey(pd => pd.IdProducto)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pd => pd.Deposito)
                    .WithMany(d => d.Productos!)
                    .HasForeignKey(pd => pd.IdDeposito)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(pd => new { pd.IdProducto, pd.IdDeposito }).IsUnique();
            });

            // --- ProductoLote (lotes con vencimiento) ---
            modelBuilder.Entity<ProductoLote>(entity =>
            {
                entity.ToTable("ProductosLotes");
                entity.HasKey(l => l.IdProductoLote);
                entity.Property(l => l.NumeroLote).HasMaxLength(50).IsRequired();
                entity.Property(l => l.Stock).HasColumnType("decimal(18,4)");
                entity.Property(l => l.StockInicial).HasColumnType("decimal(18,4)");
                entity.Property(l => l.CostoUnitario).HasColumnType("decimal(18,4)");
                entity.Property(l => l.Estado).HasMaxLength(20).HasDefaultValue("Activo");
                entity.Property(l => l.Observacion).HasMaxLength(500);

                entity.HasOne(l => l.Producto)
                    .WithMany(p => p.Lotes)
                    .HasForeignKey(l => l.IdProducto)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Deposito)
                    .WithMany()
                    .HasForeignKey(l => l.IdDeposito)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Compra)
                    .WithMany()
                    .HasForeignKey(l => l.IdCompra)
                    .OnDelete(DeleteBehavior.SetNull);

                // Índice único: un lote no puede repetirse para el mismo producto/depósito
                entity.HasIndex(l => new { l.IdProducto, l.IdDeposito, l.NumeroLote }).IsUnique();
                
                // Índice para búsqueda rápida por vencimiento
                entity.HasIndex(l => l.FechaVencimiento);
            });

            // --- MovimientoLote (trazabilidad de lotes) ---
            modelBuilder.Entity<MovimientoLote>(entity =>
            {
                entity.ToTable("MovimientosLotes");
                entity.HasKey(m => m.IdMovimientoLote);
                entity.Property(m => m.TipoMovimiento).HasMaxLength(30).IsRequired();
                entity.Property(m => m.Cantidad).HasColumnType("decimal(18,4)");
                entity.Property(m => m.StockAnterior).HasColumnType("decimal(18,4)");
                entity.Property(m => m.StockPosterior).HasColumnType("decimal(18,4)");
                entity.Property(m => m.TipoDocumento).HasMaxLength(50);
                entity.Property(m => m.ReferenciaDocumento).HasMaxLength(100);
                entity.Property(m => m.Motivo).HasMaxLength(500);

                entity.HasOne(m => m.ProductoLote)
                    .WithMany(l => l.Movimientos)
                    .HasForeignKey(m => m.IdProductoLote)
                    .OnDelete(DeleteBehavior.Cascade);

                // Índice para búsqueda por documento
                entity.HasIndex(m => new { m.TipoDocumento, m.IdDocumento });
                
                // Índice para búsqueda por fecha
                entity.HasIndex(m => m.FechaMovimiento);
            });

            // --- Movimientos de Inventario ---
            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.ToTable("MovimientosInventario");
                entity.HasKey(m => m.IdMovimiento);
                entity.Property(m => m.Cantidad).HasColumnType("decimal(18,4)");

                entity.HasOne(m => m.Producto)
                    .WithMany()
                    .HasForeignKey(m => m.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Deposito)
                    .WithMany()
                    .HasForeignKey(m => m.IdDeposito)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Ajustes de Stock (detalle) ---
            modelBuilder.Entity<AjusteStockDetalle>(entity =>
            {
                entity.ToTable("AjustesStockDetalles");
                entity.HasKey(d => d.IdAjusteStockDetalle);

                // FKs explícitas para evitar columnas sombra
                entity.HasOne(d => d.Ajuste)
                    .WithMany(a => a.Detalles)
                    .HasForeignKey(d => d.IdAjusteStock)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                    .WithMany()
                    .HasForeignKey(d => d.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(d => d.Deposito)
                    .WithMany()
                    .HasForeignKey(d => d.IdDeposito)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Presupuestos (cabecera) ---
            modelBuilder.Entity<Presupuesto>(entity =>
            {
                entity.ToTable("Presupuestos");
                entity.HasKey(p => p.IdPresupuesto);
                entity.Property(p => p.IdSucursal).HasColumnName("suc").IsRequired();
                entity.Property(p => p.NumeroPresupuesto).HasMaxLength(20);
                entity.Property(p => p.Total).HasColumnType("decimal(18,4)");
                entity.Property(p => p.CambioDelDia).HasColumnType("decimal(18,4)");

                entity.HasOne(p => p.Sucursal)
                    .WithMany()
                    .HasForeignKey(p => p.IdSucursal)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Cliente)
                    .WithMany()
                    .HasForeignKey(p => p.IdCliente)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Moneda)
                    .WithMany()
                    .HasForeignKey(p => p.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => new { p.IdSucursal, p.NumeroPresupuesto });
            });

            // --- Presupuestos (detalle) ---
            modelBuilder.Entity<PresupuestoDetalle>(entity =>
            {
                entity.ToTable("PresupuestosDetalles");
                entity.HasKey(d => d.IdPresupuestoDetalle);
                entity.Property(d => d.Cantidad).HasColumnType("decimal(18,4)");
                entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,4)");
                entity.Property(d => d.Importe).HasColumnType("decimal(18,4)");
                entity.Property(d => d.IVA10).HasColumnType("decimal(18,4)");
                entity.Property(d => d.IVA5).HasColumnType("decimal(18,4)");
                entity.Property(d => d.Exenta).HasColumnType("decimal(18,4)");
                entity.Property(d => d.Grabado10).HasColumnType("decimal(18,4)");
                entity.Property(d => d.Grabado5).HasColumnType("decimal(18,4)");
                entity.Property(d => d.CambioDelDia).HasColumnType("decimal(18,4)");

                entity.HasOne(d => d.Presupuesto)
                    .WithMany()
                    .HasForeignKey(d => d.IdPresupuesto)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                    .WithMany()
                    .HasForeignKey(d => d.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

                // --- Compras (cabecera) ---
                modelBuilder.Entity<Compra>(entity =>
                {
                    entity.ToTable("Compras");
                    entity.HasKey(c => c.IdCompra);

                    entity.Property(c => c.IdSucursal).HasColumnName("suc").IsRequired();
                    entity.Property(c => c.NumeroFactura).HasMaxLength(7);
                    entity.Property(c => c.Timbrado).HasMaxLength(8);
                    entity.Property(c => c.Establecimiento).HasMaxLength(3).IsFixedLength();
                    entity.Property(c => c.PuntoExpedicion).HasMaxLength(3).IsFixedLength();
                    entity.Property(c => c.FormaPago).HasMaxLength(50);
                    entity.Property(c => c.Estado).HasMaxLength(20);
                    entity.Property(c => c.TipoDocumento).HasMaxLength(12).IsRequired();
                    entity.Property(c => c.IdTipoDocumentoOperacion);
                    entity.Property(c => c.TipoIngreso).HasMaxLength(13).IsRequired();
                    entity.Property(c => c.Total).HasColumnType("decimal(18,4)");
                    entity.Property(c => c.CambioDelDia).HasColumnType("decimal(18,4)");
                    entity.Property(c => c.CreditoSaldo).HasColumnType("decimal(18,4)");
                    entity.Property(c => c.SimboloMoneda).HasMaxLength(4);
                    entity.Property(c => c.TotalEnLetras).HasMaxLength(280);
                    entity.Property(c => c.Comentario).HasMaxLength(280);
                    entity.Property(c => c.TipoRegistro).HasMaxLength(20).IsFixedLength(false);
                    entity.Property(c => c.CodigoCondicion).HasMaxLength(10);
                    entity.Property(c => c.Vendedor).HasMaxLength(20).IsFixedLength();

                    // Relación Compras -> ProveedoresSifen (ProveedorSifenMejorado)
                    entity.HasOne(c => c.Proveedor)
                        .WithMany()
                        .HasForeignKey(c => c.IdProveedor)
                        .HasPrincipalKey(p => p.IdProveedor)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(c => c.Sucursal)
                        .WithMany()
                        .HasForeignKey(c => c.IdSucursal)
                        .HasPrincipalKey(s => s.Id)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(c => c.Usuario)
                        .WithMany()
                        .HasForeignKey(c => c.IdUsuario)
                        .HasPrincipalKey(u => u.Id_Usu)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(c => c.Moneda)
                        .WithMany()
                        .HasForeignKey(c => c.IdMoneda)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(c => c.Deposito)
                        .WithMany()
                        .HasForeignKey(c => c.IdDeposito)
                        .OnDelete(DeleteBehavior.SetNull);

                    entity.HasOne(c => c.Caja)
                        .WithMany()
                        .HasForeignKey(c => c.IdCaja)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(c => c.TipoPago)
                        .WithMany()
                        .HasForeignKey(c => c.IdTipoPago)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne<TipoDocumentoOperacion>()
                        .WithMany()
                        .HasForeignKey(c => c.IdTipoDocumentoOperacion)
                        .OnDelete(DeleteBehavior.Restrict);
                });

                // --- Compras Detalle ---
                modelBuilder.Entity<CompraDetalle>(entity =>
                {
                    entity.ToTable("ComprasDetalles");
                    entity.HasKey(d => d.IdCompraDetalle);
                    entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.Cantidad).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.Importe).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.IVA10).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.IVA5).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.Exenta).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.Grabado10).HasColumnType("decimal(18,4)");
                    entity.Property(d => d.Grabado5).HasColumnType("decimal(18,4)");

                    entity.HasOne(d => d.Compra)
                        .WithMany(c => c.Detalles)
                        .HasForeignKey(d => d.IdCompra)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(d => d.Producto)
                        .WithMany()
                        .HasForeignKey(d => d.IdProducto)
                        .OnDelete(DeleteBehavior.Restrict);
                });

            // --- Configuraciones para Listas de Precios ---
            
            modelBuilder.Entity<TipoCambio>()
                .HasOne(tc => tc.MonedaOrigen)
                .WithMany()
                .HasForeignKey(tc => tc.IdMonedaOrigen)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TipoCambio>()
                .HasOne(tc => tc.MonedaDestino)
                .WithMany()
                .HasForeignKey(tc => tc.IdMonedaDestino)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ListaPrecio>()
                .HasOne(lp => lp.Moneda)
                .WithMany()
                .HasForeignKey(lp => lp.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ListaPrecioDetalle>()
                .HasOne(lpd => lpd.ListaPrecio)
                .WithMany(lp => lp.Detalles)
                .HasForeignKey(lpd => lpd.IdListaPrecio)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Cajas ---
            modelBuilder.Entity<Caja>(entity =>
            {
                entity.ToTable("Cajas");
                entity.HasKey(c => c.IdCaja);
                entity.Property(c => c.Nivel1).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.Nivel2).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.FacturaInicial).HasMaxLength(7).IsFixedLength();
                entity.Property(c => c.Timbrado).HasMaxLength(8).IsFixedLength();
                entity.Property(c => c.NombreImpresora).HasMaxLength(40);
                entity.Property(c => c.Nivel1R).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.Nivel2R).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.FacturaInicialR).HasMaxLength(7).IsFixedLength();
                entity.Property(c => c.TimbradoR).HasMaxLength(8).IsFixedLength();
                entity.Property(c => c.Nivel1NC).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.Nivel2NC).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.NumeroNC).HasMaxLength(7).IsFixedLength();
                entity.Property(c => c.TimbradoNC).HasMaxLength(8).IsFixedLength();
                entity.Property(c => c.Nivel1Recibo).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.Nivel2Recibo).HasMaxLength(3).IsFixedLength();
                entity.Property(c => c.NumeroRecibo).HasMaxLength(7).IsFixedLength();
                entity.Property(c => c.TimbradoRecibo).HasMaxLength(8).IsFixedLength();
                entity.Property(c => c.modelo_factura).HasMaxLength(10);
                entity.Property(c => c.anular_item).HasMaxLength(13);
                entity.Property(c => c.cierre_simultaneo).HasMaxLength(50);
            });

            // --- Tipos de Pago ---
            modelBuilder.Entity<TipoPago>(entity =>
            {
                entity.ToTable("TiposPago");
                entity.HasKey(t => t.IdTipoPago);
                entity.Property(t => t.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(t => t.Activo).HasDefaultValue(true);
                entity.Property(t => t.EsCredito).HasDefaultValue(false);
                entity.Property(t => t.Orden).HasDefaultValue(0);
                entity.HasIndex(t => t.Nombre).IsUnique();
            });

            // --- Ventas (cabecera) ---
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("Ventas");
                entity.HasKey(v => v.IdVenta);

                // Columnas y tamaños
                entity.Property(v => v.IdSucursal).HasColumnName("suc").IsRequired();
                entity.Property(v => v.NumeroFactura).HasMaxLength(7);
                entity.Property(v => v.Timbrado).HasMaxLength(8);
                entity.Property(v => v.Establecimiento).HasMaxLength(3).IsFixedLength();
                entity.Property(v => v.PuntoExpedicion).HasMaxLength(3).IsFixedLength();
                entity.Property(v => v.FormaPago).HasMaxLength(50);
                entity.Property(v => v.Estado).HasMaxLength(20);
                entity.Property(v => v.TipoDocumento).HasMaxLength(12);
                entity.Property(v => v.TipoIngreso).HasMaxLength(13).IsRequired();
                entity.Property(v => v.CodigoCondicion).HasMaxLength(10);
                entity.Property(v => v.MedioPago).HasMaxLength(13);
                entity.Property(v => v.SimboloMoneda).HasMaxLength(4);
                entity.Property(v => v.TotalEnLetras).HasMaxLength(280);
                entity.Property(v => v.Comentario).HasMaxLength(280);

                // Decimales
                entity.Property(v => v.Total).HasColumnType("decimal(18,4)");
                entity.Property(v => v.CambioDelDia).HasColumnType("decimal(18,4)");
                entity.Property(v => v.CreditoSaldo).HasColumnType("decimal(18,4)");

                // Relaciones (usar las FK explícitas para evitar columnas sombra)
                entity.HasOne(v => v.Sucursal)
                    .WithMany()
                    .HasForeignKey(v => v.IdSucursal)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Cliente)
                    .WithMany()
                    .HasForeignKey(v => v.IdCliente)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Moneda)
                    .WithMany()
                    .HasForeignKey(v => v.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Caja)
                    .WithMany()
                    .HasForeignKey(v => v.IdCaja)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.TipoPago)
                    .WithMany()
                    .HasForeignKey(v => v.IdTipoPago)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Usuario)
                    .WithMany()
                    .HasForeignKey(v => v.IdUsuario)
                    .HasPrincipalKey(u => u.Id_Usu)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.TipoDocumentoOperacion)
                    .WithMany()
                    .HasForeignKey(v => v.IdTipoDocumentoOperacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Timbrado ---
            modelBuilder.Entity<Timbrado>(entity =>
            {
                entity.ToTable("Timbrados");
                entity.HasKey(t => t.IdTimbrado);
                entity.Property(t => t.NumeroTimbrado).HasMaxLength(8).IsRequired();
                entity.Property(t => t.Establecimiento).HasMaxLength(3).IsRequired();
                entity.Property(t => t.PuntoExpedicion).HasMaxLength(3).IsRequired();
                entity.Property(t => t.TipoDocumento).HasMaxLength(12);
                entity.HasOne(t => t.Sucursal)
                    .WithMany()
                    .HasForeignKey(t => t.IdSucursal)
                    .HasPrincipalKey(s => s.Id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Caja)
                    .WithMany()
                    .HasForeignKey(t => t.IdCaja)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => new { t.NumeroTimbrado, t.Establecimiento, t.PuntoExpedicion, t.TipoDocumento, t.IdSucursal, t.IdCaja })
                    .IsUnique();
            });

            // --- Composición de Caja (cabecera) ---
            modelBuilder.Entity<ComposicionCaja>(entity =>
            {
                entity.ToTable("ComposicionesCaja");
                entity.HasKey(c => c.IdComposicionCaja);
                entity.Property(c => c.MontoTotal).HasColumnType("decimal(18,4)");
                entity.Property(c => c.TipoCambioAplicado).HasColumnType("decimal(18,4)");
                entity.HasOne(c => c.Venta)
                    .WithMany()
                    .HasForeignKey(c => c.IdVenta)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Moneda)
                    .WithMany()
                    .HasForeignKey(c => c.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => c.IdVenta).IsUnique(); // una composición por venta
            });

            // --- Composición de Caja (detalle) ---
            modelBuilder.Entity<ComposicionCajaDetalle>(entity =>
            {
                entity.ToTable("ComposicionesCajaDetalles");
                entity.HasKey(d => d.IdComposicionCajaDetalle);
                entity.Property(d => d.Monto).HasColumnType("decimal(18,4)");
                entity.Property(d => d.MontoGs).HasColumnType("decimal(18,4)");
                entity.Property(d => d.TipoCambio).HasColumnType("decimal(18,4)");
                entity.Property(d => d.Factor).HasColumnType("decimal(18,4)");
                entity.HasOne(d => d.ComposicionCaja)
                    .WithMany(c => c.Detalles!)
                    .HasForeignKey(d => d.IdComposicionCaja)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Moneda)
                    .WithMany()
                    .HasForeignKey(d => d.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- VentaPago (cabecera E7) ---
            modelBuilder.Entity<VentaPago>(entity =>
            {
                entity.ToTable("VentasPagos");
                entity.HasKey(vp => vp.IdVentaPago);
                entity.Property(vp => vp.ImporteTotal).HasColumnType("decimal(18,4)");
                entity.Property(vp => vp.Anticipo).HasColumnType("decimal(18,4)");
                entity.Property(vp => vp.DescuentoTotal).HasColumnType("decimal(18,4)");
                entity.Property(vp => vp.RecargoTotal).HasColumnType("decimal(18,4)");
                entity.Property(vp => vp.TipoCambio).HasColumnType("decimal(18,4)");
                entity.HasOne(vp => vp.Venta)
                    .WithMany()
                    .HasForeignKey(vp => vp.IdVenta)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(vp => vp.Moneda)
                    .WithMany()
                    .HasForeignKey(vp => vp.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(vp => vp.IdVenta).IsUnique(); // una cabecera de pago por venta
            });

            modelBuilder.Entity<VentaPagoDetalle>(entity =>
            {
                entity.ToTable("VentasPagosDetalles");
                entity.HasKey(vpd => vpd.IdVentaPagoDetalle);
                entity.Property(vpd => vpd.Monto).HasColumnType("decimal(18,4)");
                entity.Property(vpd => vpd.MontoGs).HasColumnType("decimal(18,4)");
                entity.Property(vpd => vpd.TipoCambio).HasColumnType("decimal(18,4)");
                entity.HasOne(vpd => vpd.VentaPago)
                    .WithMany(vp => vp.Detalles!)
                    .HasForeignKey(vpd => vpd.IdVentaPago)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(vpd => vpd.Moneda)
                    .WithMany()
                    .HasForeignKey(vpd => vpd.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VentaCuota>(entity =>
            {
                entity.ToTable("VentasCuotas");
                entity.HasKey(vc => vc.IdVentaCuota);
                entity.Property(vc => vc.MontoCuota).HasColumnType("decimal(18,4)");
                entity.HasOne(vc => vc.VentaPago)
                    .WithMany(vp => vp.Cuotas!)
                    .HasForeignKey(vc => vc.IdVentaPago)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(vc => new { vc.IdVentaPago, vc.NumeroCuota }).IsUnique();
            });

        // --- Ventas Detalles ---
        modelBuilder.Entity<VentaDetalle>(entity =>
        {
            entity.ToTable("VentasDetalles");
            entity.HasKey(d => d.IdVentaDetalle);
            entity.Property(d => d.Cantidad).HasColumnType("decimal(18,4)");
            entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Importe).HasColumnType("decimal(18,4)");
            entity.Property(d => d.IVA10).HasColumnType("decimal(18,4)");
            entity.Property(d => d.IVA5).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Exenta).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Grabado10).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Grabado5).HasColumnType("decimal(18,4)");
            entity.Property(d => d.CambioDelDia).HasColumnType("decimal(18,4)");

            entity.HasOne(d => d.Venta)
                .WithMany()
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Sistema de Créditos y Remisiones ---
        
        // CuentaPorCobrar (cabecera de créditos)
        modelBuilder.Entity<CuentaPorCobrar>(entity =>
        {
            entity.ToTable("CuentasPorCobrar");
            entity.HasKey(cpc => cpc.IdCuentaPorCobrar);
            entity.Property(cpc => cpc.MontoTotal).HasColumnType("decimal(18,4)");
            entity.Property(cpc => cpc.SaldoPendiente).HasColumnType("decimal(18,4)");
            entity.Property(cpc => cpc.Estado).HasMaxLength(20).IsRequired();
            
            entity.HasOne(cpc => cpc.Venta)
                .WithMany()
                .HasForeignKey(cpc => cpc.IdVenta)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cpc => cpc.Cliente)
                .WithMany()
                .HasForeignKey(cpc => cpc.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cpc => cpc.Sucursal)
                .WithMany()
                .HasForeignKey(cpc => cpc.IdSucursal)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CuentaPorCobrarCuota (cuotas individuales)
        modelBuilder.Entity<CuentaPorCobrarCuota>(entity =>
        {
            entity.ToTable("CuentasPorCobrarCuotas");
            entity.HasKey(cpcc => cpcc.IdCuota);
            entity.Property(cpcc => cpcc.MontoCuota).HasColumnType("decimal(18,4)");
            entity.Property(cpcc => cpcc.SaldoCuota).HasColumnType("decimal(18,4)");
            entity.Property(cpcc => cpcc.Estado).HasMaxLength(20).IsRequired();
            
            entity.HasOne(cpcc => cpcc.CuentaPorCobrar)
                .WithMany(cpc => cpc.Cuotas)
                .HasForeignKey(cpcc => cpcc.IdCuentaPorCobrar)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(cpcc => new { cpcc.IdCuentaPorCobrar, cpcc.NumeroCuota }).IsUnique();
        });

        // RemisionInterna (cabecera de remisiones)
        modelBuilder.Entity<RemisionInterna>(entity =>
        {
            entity.ToTable("RemisionesInternas");
            entity.HasKey(ri => ri.IdRemision);
            entity.Property(ri => ri.NumeroRemision).HasMaxLength(20).IsRequired();
            entity.Property(ri => ri.MontoTotal).HasColumnType("decimal(18,4)");
            entity.Property(ri => ri.Estado).HasMaxLength(20).IsRequired();
            
            entity.HasOne(ri => ri.Venta)
                .WithMany()
                .HasForeignKey(ri => ri.IdVenta)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ri => ri.Cliente)
                .WithMany()
                .HasForeignKey(ri => ri.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(ri => ri.NumeroRemision).IsUnique();
        });

        // RemisionInternaDetalle (líneas de remisiones)
        modelBuilder.Entity<RemisionInternaDetalle>(entity =>
        {
            entity.ToTable("RemisionesInternasDetalles");
            entity.HasKey(rid => rid.IdRemisionDetalle);
            entity.Property(rid => rid.Cantidad).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.PrecioUnitario).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.Subtotal).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.Gravado5).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.Gravado10).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.Exenta).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.IVA5).HasColumnType("decimal(18,4)");
            entity.Property(rid => rid.IVA10).HasColumnType("decimal(18,4)");
            
            entity.HasOne(rid => rid.RemisionInterna)
                .WithMany(ri => ri.Detalles)
                .HasForeignKey(rid => rid.IdRemision)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(rid => rid.Producto)
                .WithMany()
                .HasForeignKey(rid => rid.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CobroCuota (cabecera de cobros)
        modelBuilder.Entity<CobroCuota>(entity =>
        {
            entity.ToTable("CobrosCuotas");
            entity.HasKey(cc => cc.IdCobro);
            entity.Property(cc => cc.MontoTotal).HasColumnType("decimal(18,4)");
            entity.Property(cc => cc.Estado).HasMaxLength(20).IsRequired();
            entity.Property(cc => cc.NumeroRecibo).HasMaxLength(30);
            
            entity.HasOne(cc => cc.CuentaPorCobrar)
                .WithMany(cpc => cpc.Cobros)
                .HasForeignKey(cc => cc.IdCuentaPorCobrar)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(cc => cc.Cliente)
                .WithMany()
                .HasForeignKey(cc => cc.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cc => cc.Caja)
                .WithMany()
                .HasForeignKey(cc => cc.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cc => cc.Usuario)
                .WithMany()
                .HasForeignKey(cc => cc.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(cc => cc.Sucursal)
                .WithMany()
                .HasForeignKey(cc => cc.IdSucursal)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CobroDetalle (detalle de medios de pago)
        modelBuilder.Entity<CobroDetalle>(entity =>
        {
            entity.ToTable("CobrosDetalles");
            entity.HasKey(cd => cd.IdCobroDetalle);
            entity.Property(cd => cd.MedioPago).HasMaxLength(20).IsRequired();
            entity.Property(cd => cd.Monto).HasColumnType("decimal(18,4)");
            entity.Property(cd => cd.BancoTarjeta).HasMaxLength(100);
            entity.Property(cd => cd.Ultimos4Tarjeta).HasMaxLength(4);
            entity.Property(cd => cd.NumeroAutorizacion).HasMaxLength(50);
            entity.Property(cd => cd.NumeroCheque).HasMaxLength(30);
            entity.Property(cd => cd.BancoCheque).HasMaxLength(100);
            entity.Property(cd => cd.NumeroTransferencia).HasMaxLength(100);
            
            entity.HasOne(cd => cd.CobroCuota)
                .WithMany(cc => cc.Detalles)
                .HasForeignKey(cd => cd.IdCobro)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(cd => cd.Cuota)
                .WithMany()
                .HasForeignKey(cd => cd.IdCuota)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Sistema de Pagos a Proveedores ---
        // CuentaPorPagar (cabecera de deuda a proveedores)
        modelBuilder.Entity<CuentaPorPagar>(entity =>
        {
            entity.ToTable("CuentasPorPagar");
            entity.HasKey(cpp => cpp.IdCuentaPorPagar);
            entity.Property(cpp => cpp.MontoTotal).HasColumnType("decimal(18,4)");
            entity.Property(cpp => cpp.SaldoPendiente).HasColumnType("decimal(18,4)");
            entity.Property(cpp => cpp.Estado).HasMaxLength(20).IsRequired();
            
            entity.HasOne(cpp => cpp.Compra)
                .WithMany()
                .HasForeignKey(cpp => cpp.IdCompra)
                .OnDelete(DeleteBehavior.Restrict);
            
            // FK a ProveedorSifenMejorado (tabla ProveedoresSifen)
            entity.HasOne(cpp => cpp.Proveedor)
                .WithMany()
                .HasForeignKey(cpp => cpp.IdProveedor)
                .HasPrincipalKey(p => p.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(cpp => cpp.Moneda)
                .WithMany()
                .HasForeignKey(cpp => cpp.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CuentaPorPagarCuota (detalle de cuotas de deuda)
        modelBuilder.Entity<CuentaPorPagarCuota>(entity =>
        {
            entity.ToTable("CuentasPorPagarCuotas");
            entity.HasKey(cppc => cppc.IdCuota);
            entity.Property(cppc => cppc.MontoCuota).HasColumnType("decimal(18,4)");
            entity.Property(cppc => cppc.Estado).HasMaxLength(20).IsRequired();
            
            entity.HasOne(cppc => cppc.CuentaPorPagar)
                .WithMany(cpp => cpp.Cuotas)
                .HasForeignKey(cppc => cppc.IdCuentaPorPagar)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PagoProveedor (cabecera de pagos)
        modelBuilder.Entity<PagoProveedor>(entity =>
        {
            entity.ToTable("PagosProveedores");
            entity.HasKey(pp => pp.IdPagoProveedor);
            entity.Property(pp => pp.MontoTotal).HasColumnType("decimal(18,4)");
            entity.Property(pp => pp.CambioDelDia).HasColumnType("decimal(18,4)");
            entity.Property(pp => pp.Estado).HasMaxLength(20).IsRequired();
            entity.Property(pp => pp.NumeroRecibo).HasMaxLength(30);
            
            entity.HasOne(pp => pp.Compra)
                .WithMany()
                .HasForeignKey(pp => pp.IdCompra)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(pp => pp.Proveedor)
                .WithMany()
                .HasForeignKey(pp => pp.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pp => pp.Moneda)
                .WithMany()
                .HasForeignKey(pp => pp.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pp => pp.Caja)
                .WithMany()
                .HasForeignKey(pp => pp.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pp => pp.Usuario)
                .WithMany()
                .HasForeignKey(pp => pp.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PagoProveedorDetalle (detalle de medios de pago)
        modelBuilder.Entity<PagoProveedorDetalle>(entity =>
        {
            entity.ToTable("PagosProveedoresDetalles");
            entity.HasKey(ppd => ppd.IdPagoProveedorDetalle);
            entity.Property(ppd => ppd.MedioPago).HasMaxLength(20).IsRequired();
            entity.Property(ppd => ppd.Monto).HasColumnType("decimal(18,4)");
            entity.Property(ppd => ppd.CambioDelDia).HasColumnType("decimal(18,4)");
            entity.Property(ppd => ppd.BancoCheque).HasMaxLength(100);
            entity.Property(ppd => ppd.NumeroCheque).HasMaxLength(30);
            entity.Property(ppd => ppd.BancoTransferencia).HasMaxLength(100);
            entity.Property(ppd => ppd.NumeroTransferencia).HasMaxLength(100);
            entity.Property(ppd => ppd.BancoTarjeta).HasMaxLength(100);
            entity.Property(ppd => ppd.MarcaTarjeta).HasMaxLength(50);
            entity.Property(ppd => ppd.Ultimos4Tarjeta).HasMaxLength(4);
            entity.Property(ppd => ppd.NumeroAutorizacion).HasMaxLength(50);
            
            entity.HasOne(ppd => ppd.PagoProveedor)
                .WithMany(pp => pp.Detalles)
                .HasForeignKey(ppd => ppd.IdPagoProveedor)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ppd => ppd.Cuota)
                .WithMany(cppc => cppc.PagoDetalles)
                .HasForeignKey(ppd => ppd.IdCuota)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración CierreCaja
        modelBuilder.Entity<CierreCaja>(entity =>
        {
            entity.ToTable("CierresCaja");
            entity.HasKey(c => c.IdCierreCaja);
            entity.Property(c => c.TotalVentasContado).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalVentasCredito).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalCobrosCredito).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalAnulaciones).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalEfectivo).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalTarjetas).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalCheques).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalTransferencias).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalQR).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalOtros).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalEsperado).HasColumnType("decimal(18,2)");
            entity.Property(c => c.TotalEntregado).HasColumnType("decimal(18,2)");
            entity.Property(c => c.Diferencia).HasColumnType("decimal(18,2)");
            entity.Property(c => c.UsuarioCierre).HasMaxLength(100);
            entity.Property(c => c.Observaciones).HasMaxLength(500);
            entity.Property(c => c.Estado).HasMaxLength(20);

            entity.HasOne(c => c.Caja)
                .WithMany()
                .HasForeignKey(c => c.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.IdCaja, c.FechaCaja, c.Turno })
                .HasDatabaseName("IX_CierresCaja_Caja_Fecha_Turno");
        });

        // Configuración EntregaCaja
        modelBuilder.Entity<EntregaCaja>(entity =>
        {
            entity.ToTable("EntregasCaja");
            entity.HasKey(e => e.IdEntregaCaja);
            entity.Property(e => e.MontoEsperado).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MontoEntregado).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Diferencia).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReceptorEntrega).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(300);
            entity.Property(e => e.DetalleCheques).HasMaxLength(500);

            entity.HasOne(e => e.CierreCaja)
                .WithMany(c => c.Entregas)
                .HasForeignKey(e => e.IdCierreCaja)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Moneda)
                .WithMany()
                .HasForeignKey(e => e.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración NotaCreditoVenta
        modelBuilder.Entity<NotaCreditoVenta>(entity =>
        {
            entity.ToTable("NotasCreditoVentas");
            entity.HasKey(nc => nc.IdNotaCredito);
            entity.Property(nc => nc.Subtotal).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalIVA10).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalIVA5).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalExenta).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.Total).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.CambioDelDia).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.Motivo).HasMaxLength(50);
            entity.Property(nc => nc.Observaciones).HasMaxLength(500);
            entity.Property(nc => nc.Estado).HasMaxLength(20);

            entity.HasOne(nc => nc.Sucursal)
                .WithMany()
                .HasForeignKey(nc => nc.IdSucursal)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Caja)
                .WithMany()
                .HasForeignKey(nc => nc.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Cliente)
                .WithMany()
                .HasForeignKey(nc => nc.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.VentaAsociada)
                .WithMany()
                .HasForeignKey(nc => nc.IdVentaAsociada)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Moneda)
                .WithMany()
                .HasForeignKey(nc => nc.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Usuario)
                .WithMany()
                .HasForeignKey(nc => nc.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración NotaCreditoVentaDetalle
        modelBuilder.Entity<NotaCreditoVentaDetalle>(entity =>
        {
            entity.ToTable("NotasCreditoVentasDetalles");
            entity.HasKey(d => d.IdNotaCreditoDetalle);
            entity.Property(d => d.Cantidad).HasColumnType("decimal(18,4)");
            entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,4)");
            entity.Property(d => d.PorcentajeDescuento).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Descuento).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Importe).HasColumnType("decimal(18,4)");
            entity.Property(d => d.IVA10).HasColumnType("decimal(18,4)");
            entity.Property(d => d.IVA5).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Exenta).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Grabado10).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Grabado5).HasColumnType("decimal(18,4)");
            entity.Property(d => d.CambioDelDia).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Lote).HasMaxLength(50);

            entity.HasOne(d => d.NotaCredito)
                .WithMany(nc => nc.Detalles)
                .HasForeignKey(d => d.IdNotaCredito)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Deposito)
                .WithMany()
                .HasForeignKey(d => d.IdDeposito)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración NotaCreditoCompra
        modelBuilder.Entity<NotaCreditoCompra>(entity =>
        {
            entity.ToTable("NotasCreditoCompras");
            entity.HasKey(nc => nc.IdNotaCreditoCompra);
            entity.Property(nc => nc.Subtotal).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalIVA10).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalIVA5).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalExenta).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.TotalDescuento).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.Total).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.CambioDelDia).HasColumnType("decimal(18,4)");
            entity.Property(nc => nc.Motivo).HasMaxLength(50);
            entity.Property(nc => nc.Observaciones).HasMaxLength(500);
            entity.Property(nc => nc.Estado).HasMaxLength(20);

            entity.HasOne(nc => nc.Sucursal)
                .WithMany()
                .HasForeignKey(nc => nc.IdSucursal)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Caja)
                .WithMany()
                .HasForeignKey(nc => nc.IdCaja)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Proveedor)
                .WithMany()
                .HasForeignKey(nc => nc.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.CompraAsociada)
                .WithMany()
                .HasForeignKey(nc => nc.IdCompraAsociada)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Moneda)
                .WithMany()
                .HasForeignKey(nc => nc.IdMoneda)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Deposito)
                .WithMany()
                .HasForeignKey(nc => nc.IdDeposito)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(nc => nc.Usuario)
                .WithMany()
                .HasForeignKey(nc => nc.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración NotaCreditoCompraDetalle
        modelBuilder.Entity<NotaCreditoCompraDetalle>(entity =>
        {
            entity.ToTable("NotasCreditoComprasDetalles");
            entity.HasKey(d => d.IdNotaCreditoCompraDetalle);
            entity.Property(d => d.Cantidad).HasColumnType("decimal(18,4)");
            entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(18,4)");
            entity.Property(d => d.PorcentajeDescuento).HasColumnType("decimal(18,4)");
            entity.Property(d => d.MontoDescuento).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Importe).HasColumnType("decimal(18,4)");
            entity.Property(d => d.IVA10).HasColumnType("decimal(18,4)");
            entity.Property(d => d.IVA5).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Exenta).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Grabado10).HasColumnType("decimal(18,4)");
            entity.Property(d => d.Grabado5).HasColumnType("decimal(18,4)");

            entity.HasOne(d => d.NotaCreditoCompra)
                .WithMany(nc => nc.Detalles)
                .HasForeignKey(d => d.IdNotaCreditoCompra)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DepositoItem)
                .WithMany()
                .HasForeignKey(d => d.IdDepositoItem)
                .OnDelete(DeleteBehavior.Restrict);
        });
        }
    }
}
