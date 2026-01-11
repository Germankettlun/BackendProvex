using Microsoft.EntityFrameworkCore;
using ProvexApi.Models.BackEnd.Logs;
using ProvexApi.Models.Rossi;
using static ProvexApi.Models.Rossi.RossiRoot;

namespace ProvexApi.Data.Rossi
{

    public class RossiDbContext : DbContext
    {
        public RossiDbContext(DbContextOptions<RossiDbContext> options)
            : base(options)
        {
        }
        public DbSet<ServiceApiCallLog> ServiceApiCallLogs { get; set; }  // [logs bd]
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Embarque> Embarques { get; set; }
        public DbSet<Facturacion> Facturaciones { get; set; }
        public DbSet<Pallet> Pallets { get; set; }
        public DbSet<PDetalle> PDetalles { get; set; }
        public DbSet<ArchivoUrl> ArchivoUrls { get; set; }
        public DbSet<DespachoNaves> DespachosNaves { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Solo es para la deszerializacion
            modelBuilder.Ignore<Archivos>();
            modelBuilder.Ignore<ArchivoItem>();

            // Cada entidad se vincula a su tabla con prefijo "Rossi_"
            modelBuilder.Entity<Documento>().ToTable("Rossi_Documento");
            modelBuilder.Entity<Embarque>().ToTable("Rossi_Embarque");
            modelBuilder.Entity<Facturacion>().ToTable("Rossi_Facturacion");
            modelBuilder.Entity<Pallet>().ToTable("Rossi_Pallet");
            modelBuilder.Entity<PDetalle>().ToTable("Rossi_PDetalle");
            modelBuilder.Entity<ArchivoUrl>().ToTable("Rossi_ArchivoUrl");
            modelBuilder.Entity<ServiceApiCallLog>().ToTable("ServiceApiCallLogs");

            // Configurar las relaciones de forma explícita si deseas
            modelBuilder.Entity<Embarque>()
                .HasOne(e => e.Documento)
                .WithMany(d => d.Embarque)
                .HasForeignKey(e => e.DocumentoId);

            // Rossi
            modelBuilder.Entity<DespachoNaves>().ToView("Rossi_ViewNaves");
        }
    }
}
