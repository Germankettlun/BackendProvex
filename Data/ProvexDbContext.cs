using Microsoft.EntityFrameworkCore;
using ProvexApi.Entities.ASOEX;
using ProvexApi.Entities.Login;
using ProvexApi.Entities.Packinglist;
using ProvexApi.Entities.SensiWatch; // NUEVO: Entidades de SensiWatch
using ProvexApi.Models.API;
using ProvexApi.Models.BackEnd.Logs;
using ProvexApi.Models.SDT;


namespace ProvexApi.Data
{
    public class ProvexDbContext : DbContext
    {
        public DbSet<SessionToken> SessionTokens { get; set; }
        // Constructor que recibe las opciones
        public ProvexDbContext(DbContextOptions<ProvexDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceApiCallLog> ServiceApiCallLogs { get; set; }  // [logs bd]
        public DbSet<ApiToken> ApiTokens { get; set; }
        public DbSet<Proceso> Asoex_Procesos { get; set; }
        public DbSet<ProcesoViaje> Asoex_ProcesoViajes { get; set; }
        public DbSet<ProcesoDetalle> Asoex_ProcesoDetalles { get; set; }
        public DbSet<Temporada> Asoex_Temporada { get; set; }
        public DbSet<Exportador> Asoex_Exportador { get; set; }
        public DbSet<Semana> Asoex_Semana { get; set; }
        public DbSet<Consignatario> Asoex_Consignatario { get; set; }
        public DbSet<RegionOrigen> Asoex_RegionOrigen { get; set; }
        public DbSet<RegionDestino> Asoex_RegionesDestino { get; set; }
        public DbSet<PaisDestino> Asoex_PaisesDestino { get; set; }
        public DbSet<PuertoEmbarque> Asoex_PuertoEmbarque { get; set; }
        public DbSet<PuertoDestino> Asoex_PuertosDestino { get; set; }
        public DbSet<TipoEspecie> Asoex_TipoEspecie { get; set; }
        public DbSet<Especie> Asoex_Especie { get; set; }
        public DbSet<Variedad> Asoex_Variedad { get; set; }
        public DbSet<Nave> Asoex_Nave { get; set; }

        // NUEVO: Entidades de SensiWatch
        public DbSet<ThermographDevice> ThermographDevices { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<ThermographEvent> ThermographEvents { get; set; }
        public DbSet<ThermographSensorReading> ThermographSensorReadings { get; set; }

        public DbSet<ViewPLPangea> ViewPLPangea { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApiToken>().ToTable("ApiTokens");                   
            modelBuilder.Entity<ServiceApiCallLog>().ToTable("ServiceApiCallLogs");

            // Relación explícita para ProcesoViaje
            modelBuilder.Entity<ProcesoViaje>()
                .HasOne(v => v.Proceso)
                .WithMany(p => p.Viajes)
                .HasForeignKey(v => v.IdProceso)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación explícita para ProcesoDetalle
            modelBuilder.Entity<ProcesoDetalle>()
                .HasOne(d => d.Proceso)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdProceso)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Temporada>().ToTable("Asoex_Temporada");
            modelBuilder.Entity<Exportador>().ToTable("Asoex_Exportador");
            modelBuilder.Entity<Semana>().ToTable("Asoex_Semana");
            modelBuilder.Entity<Consignatario>().ToTable("Asoex_Consignatario");
            modelBuilder.Entity<RegionOrigen>().ToTable("Asoex_RegionOrigen");
            modelBuilder.Entity<RegionDestino>().ToTable("Asoex_RegionesDestino");
            modelBuilder.Entity<PaisDestino>().ToTable("Asoex_PaisesDestino");
            modelBuilder.Entity<PuertoEmbarque>().ToTable("Asoex_PuertoEmbarque");
            modelBuilder.Entity<PuertoDestino>().ToTable("Asoex_PuertosDestino");
            modelBuilder.Entity<TipoEspecie>().ToTable("Asoex_TipoEspecie");
            modelBuilder.Entity<Especie>().ToTable("Asoex_Especie");
            modelBuilder.Entity<Variedad>().ToTable("Asoex_Variedad");
            modelBuilder.Entity<Nave>().ToTable("Asoex_Nave");

            //UserModule
            modelBuilder.Entity<UserModule>().HasKey(um => new { um.UserId, um.ModuleId }); // Clave compuesta

            modelBuilder.Entity<UserModule>()
                .HasOne(um => um.User)
                .WithMany(u => u.UserModules)
                .HasForeignKey(um => um.UserId);

            modelBuilder.Entity<UserModule>()
                .HasOne(um => um.Module)
                .WithMany(m => m.UserModules)
                .HasForeignKey(um => um.ModuleId);

            modelBuilder.Entity<RoleModule>().HasKey(rm => new { rm.RoleId, rm.ModuleId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            // NUEVO: Configuración de entidades SensiWatch
            // Índices para mejorar performance
            modelBuilder.Entity<ThermographDevice>()
                .HasIndex(d => d.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<Trip>()
                .HasIndex(t => t.SwpTripId)
                .IsUnique();

            modelBuilder.Entity<Trip>()
                .HasIndex(t => t.InternalTripId);

            modelBuilder.Entity<ThermographEvent>()
                .HasIndex(e => new { e.DeviceId, e.ReceivedAt });

            modelBuilder.Entity<ThermographSensorReading>()
                .HasIndex(r => new { r.SensorType, r.DeviceTime });

            // Relaciones de SensiWatch
            modelBuilder.Entity<ThermographEvent>()
                .HasOne(e => e.Device)
                .WithMany(d => d.Events)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ThermographEvent>()
                .HasOne(e => e.Trip)
                .WithMany(t => t.Events)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ThermographSensorReading>()
                .HasOne(r => r.Event)
                .WithMany(e => e.SensorReadings)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewPLPangea>().HasNoKey().ToView("View_PL_Pangea");
        }
    }
}
