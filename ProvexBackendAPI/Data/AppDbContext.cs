using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Repository;
using System;

namespace ProvexBackendAPI.Data
{
  
    public class AppDbContext : IdentityDbContext<
        ApplicationUser, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Zona> Zonas { get; set; }
        public virtual DbSet<EstimacionBisemanal> EstimacionBisemanales { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // —— Usar esquema Seguridad para todas las tablas de Identity
            const string schema = "Seguridad";

            builder.Entity<ApplicationUser>().ToTable("Usuarios", schema);
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles", schema);
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UsuarioRoles", schema);
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UsuarioClaims", schema);
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UsuarioLogins", schema);
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RolClaims", schema);
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UsuarioTokens", schema);

            // —— (Opcional) si quieres que SQL genere GUIDs secuenciales
            builder.Entity<ApplicationUser>()
                   .Property(u => u.Id)
                   .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<IdentityRole<Guid>>() 
                    .Property(r => r.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Entity<Zona>().ToTable("Zona", "Estimaciones");
            builder.Entity<EstimacionBisemanal>().ToTable("ESTIMACION_BISEMANAL", "Estimaciones");
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }


    }

}