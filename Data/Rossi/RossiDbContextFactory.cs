using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ProvexApi.Data.Rossi
{
    public class RossiDbContextFactory : IDesignTimeDbContextFactory<RossiDbContext>
    {
        public RossiDbContext CreateDbContext(string[] args)
        {
            // 1. Determinar la ruta base (donde está appsettings.json).
            var basePath = Directory.GetCurrentDirectory();

            // 2. Obtener variable de entorno de ambiente (ej. Development, Production).
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // 3. Construir la configuración
            // Ajusta si tu archivo se llama distinto o está en otra ruta
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // 4. Tomar la cadena de conexión. Ajusta "DatabaseConnection" a la key que uses.
            var connectionString = config.GetConnectionString("DatabaseConnection");

            // 5. Construir las opciones para tu DbContext
            var builder = new DbContextOptionsBuilder<RossiDbContext>();
            builder.UseSqlServer(connectionString);

            // 6. Instanciar y retornar tu DbContext
            return new RossiDbContext(builder.Options);
        }
    }
}
