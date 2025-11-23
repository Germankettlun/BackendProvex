using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace ProvexApi.Controllers.BackEnd.Configuration
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(
            IConfiguration configuration, 
            IWebHostEnvironment environment,
            ILogger<ConfigController> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Retorna información de versión y commit del despliegue actual
        /// </summary>
        [HttpGet("version")]
        [AllowAnonymous]
        public IActionResult GetVersion()
        {
            try
            {
                // Intentar obtener desde variables de entorno (setadas por el pipeline)
                var version = Environment.GetEnvironmentVariable("BUILD_VERSION") 
                             ?? _configuration["App:Version"]
                             ?? _configuration["AppVersion"]
                             ?? GetAssemblyVersion();

                var commitFull = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION")
                                ?? _configuration["App:Commit"]
                                ?? _configuration["GitCommit"]
                                ?? "unknown";

                var commit = commitFull.Length > 7 ? commitFull.Substring(0, 7) : commitFull;

                var buildDate = GetBuildDate();

                _logger.LogDebug(
                    "Version info requested - Version: {Version}, Commit: {Commit}, BuildDate: {BuildDate}",
                    version, commit, buildDate);

                return Ok(new
                {
                    version = version,
                    commit = commit,
                    commitFull = commitFull,
                    environment = _environment.EnvironmentName,
                    buildDate = buildDate,
                    machineName = Environment.MachineName,
                    frameworkVersion = Environment.Version.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo información de versión");
                return StatusCode(500, new
                {
                    error = "Error al obtener información de versión",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene la versión del ensamblado compilado
        /// </summary>
        private string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version != null 
                ? $"{version.Major}.{version.Minor}.{version.Build}" 
                : "1.0.0";
        }

        /// <summary>
        /// Obtiene la fecha de compilación del binario
        /// </summary>
        private string GetBuildDate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileLocation = assembly.Location;
            
            if (System.IO.File.Exists(fileLocation))
            {
                var lastWrite = System.IO.File.GetLastWriteTimeUtc(fileLocation);
                return lastWrite.ToString("yyyy-MM-dd HH:mm:ss UTC");
            }
            
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }

        /// <summary>
        /// Health check extendido con información de configuración
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult GetHealth()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var process = System.Diagnostics.Process.GetCurrentProcess();

                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    version = GetAssemblyVersion(),
                    environment = _environment.EnvironmentName,
                    uptime = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).ToString(@"dd\.hh\:mm\:ss"),
                    processId = process.Id,
                    workingSet = $"{process.WorkingSet64 / 1024 / 1024} MB",
                    assemblyLocation = assembly.Location
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check");
                return StatusCode(500, new { status = "unhealthy", error = ex.Message });
            }
        }

        /// <summary>
        /// Fuerza que todos los proveedores de configuración se recarguen
        /// </summary>
        [HttpPost("reload")]
        [Authorize(Policy = "BasicOnly")]
        public IActionResult Reload()
        {
            try
            {
                if (_configuration is IConfigurationRoot configRoot)
                {
                    configRoot.Reload();
                    _logger.LogInformation("Configuración recargada exitosamente");
                    return Ok(new { message = "Configuración recargada exitosamente" });
                }
                
                return BadRequest(new { message = "No se puede recargar la configuración" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recargar configuración");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
