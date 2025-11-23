using Microsoft.EntityFrameworkCore;
using ProvexApi.Entities.Login;
using ProvexApi.Models.API;
using ProvexApi.Models.BackEnd.Logs;

namespace ProvexApi.Data
{
    public class ExtranetDBContext : DbContext
    {
        public DbSet<SessionToken> SessionTokens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<RoleModule> RoleModules { get; set; }
        public DbSet<UserModule> UserModules { get; set; }
        // Constructor que recibe las opciones
        public ExtranetDBContext(DbContextOptions<ExtranetDBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApiToken>().ToTable("ApiTokens");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            // UserRole: PK y FKs
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<UserRole>().HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            modelBuilder.Entity<UserRole>().HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);

            // RolePermission: PK y FKs
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
            modelBuilder.Entity<RolePermission>().HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            modelBuilder.Entity<RolePermission>().HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);

            // Module self-reference
            modelBuilder.Entity<Module>().HasMany(m => m.Children).WithOne(m => m.Parent).HasForeignKey(m => m.ParentId);

            // RoleModule: PK y FKs
            modelBuilder.Entity<RoleModule>().HasKey(rm => new { rm.RoleId, rm.ModuleId });
            modelBuilder.Entity<RoleModule>().HasOne(rm => rm.Role).WithMany(r => r.RoleModules).HasForeignKey(rm => rm.RoleId);
            modelBuilder.Entity<RoleModule>().HasOne(rm => rm.Module).WithMany(m => m.RoleModules).HasForeignKey(rm => rm.ModuleId);

            // UserModule: PK y FKs
            modelBuilder.Entity<UserModule>().HasKey(um => new { um.UserId, um.ModuleId });
            modelBuilder.Entity<UserModule>().HasOne(um => um.User).WithMany(u => u.UserModules).HasForeignKey(um => um.UserId);
            modelBuilder.Entity<UserModule>().HasOne(um => um.Module).WithMany(m => m.UserModules).HasForeignKey(um => um.ModuleId);
        }
    }
}
