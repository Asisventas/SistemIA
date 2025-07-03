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
        public DbSet<Ciudades> Ciudades { get; set; }
        public DbSet<TipoOperacion> TiposOperacion { get; set; }
        public DbSet<Sucursal> Sucursal { get; set; }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<HorarioTrabajo> HorariosTrabajo { get; set; }

        public DbSet<AsignacionHorario> AsignacionesHorarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuraciones de Claves Primarias (si no siguen la convención) ---
            modelBuilder.Entity<TiposDocumentosIdentidad>().HasKey(t => t.TipoDocumento);
            modelBuilder.Entity<TiposContribuyentes>().HasKey(t => t.IdTipoContribuyente);
            modelBuilder.Entity<Paises>().HasKey(p => p.CodigoPais);
            modelBuilder.Entity<Ciudades>().HasKey(c => c.IdCiudad);
            modelBuilder.Entity<TipoOperacion>().HasKey(t => t.Codigo);
            modelBuilder.Entity<Sucursal>().HasKey(s => s.Id); // Confirma que 'Id' es la PK de Sucursal

            // --- Configuraciones de Tablas y Propiedades ---
            modelBuilder.Entity<Ciudades>().ToTable("Ciudades");

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

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Ciudad)
                .WithMany(c => c.Clientes)
                .HasForeignKey(c => c.IdCiudad)
                .HasPrincipalKey(c => c.IdCiudad);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoOperacionNavigation)
                .WithMany(t => t.Clientes)
                .HasForeignKey(c => c.TipoOperacion)
                .HasPrincipalKey(t => t.Codigo);

            modelBuilder.Entity<Sucursal>()
                .HasOne(s => s.Ciudad)
                .WithMany(c => c.Sucursales)
                .HasForeignKey(s => s.IdCiudad);

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

            modelBuilder.Entity<Proveedor>()
                .HasOne(p => p.TipoContribuyente)
                .WithMany()
                .HasForeignKey(p => p.IdTipoContribuyente);

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
        }
    }
}