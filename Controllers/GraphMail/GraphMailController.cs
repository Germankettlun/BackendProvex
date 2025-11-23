using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProvexApi.Models.GraphMail;
using ProvexApi.Services.GraphMail;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProvexApi.Controllers.GraphMail
{
    [Authorize(Policy = "BasicOnly")]
    [ApiController]
    [Route("api/graphmail")]
    public class GraphMailController : ControllerBase
    {
        private readonly IGraphMailService _graphMailService;
        private readonly ILogger<GraphMailController> _logger;
        
        public GraphMailController(
            IGraphMailService graphMailService,
            ILogger<GraphMailController> logger)
        {
            _graphMailService = graphMailService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint optimizado para procesamiento de buzón completo con manejo inteligente de timeouts
        /// </summary>
        /// <param name="mailbox">Correo a analizar</param>
        /// <param name="enableOptimizations">Activa optimizaciones avanzadas para evitar timeouts</param>
        /// <param name="limit">Límite opcional de mensajes</param>
        /// <returns>Resultados optimizados con análisis de adjuntos mejorado</returns>
        [HttpGet("optimized-processing")]
        public async Task<IActionResult> GetMailsWeightOptimized(
            [FromQuery] string mailbox, 
            [FromQuery] bool enableOptimizations = true,
            [FromQuery] int? limit = null)
        {
            try
            {
                _logger.LogInformation("?? Iniciando procesamiento OPTIMIZADO para mailbox: {mailbox}, optimizaciones: {opt}, límite: {limit}", 
                    mailbox, enableOptimizations, limit);

                var options = new ProcessingOptions
                {
                    MessageLimit = limit,
                    StartFromBatch = null,
                    TimeoutSeconds = enableOptimizations ? 45 : 30, // Timeout más largo si está optimizado
                    MaxRetries = enableOptimizations ? 2 : 3, // Menos reintentos pero más inteligentes
                    BatchSize = enableOptimizations ? 25 : 10 // Lotes más grandes para eficiencia
                };

                var startTime = DateTime.Now;
                var result = await _graphMailService.GetMailWeightByYearAndSenderAsync(mailbox, options);
                var endTime = DateTime.Now;
                var totalDuration = endTime - startTime;
                
                // Estadísticas mejoradas
                var totalMessages = result.Sum(g => g.Count);
                var totalSizeMB = result.Sum(g => g.TotalSizeMb);
                var averageMessageSize = totalMessages > 0 ? totalSizeMB * 1024 * 1024 / totalMessages : 0;
                var processingRate = totalDuration.TotalMinutes > 0 ? totalMessages / totalDuration.TotalMinutes : 0;

                var response = new
                {
                    procesamiento = new
                    {
                        tipo = "Procesamiento Optimizado",
                        optimizacionesActivas = enableOptimizations,
                        iniciado = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        completado = endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        duracionTotal = totalDuration.ToString(@"hh\:mm\:ss"),
                        eficiencia = new
                        {
                            mensajesPorMinuto = processingRate,
                            velocidadOptimizada = enableOptimizations ? "Sí" : "No",
                            timeoutsEvitados = enableOptimizations ? "Estrategia híbrida activa" : "Estrategia estándar"
                        }
                    },
                    resultados = new
                    {
                        mensajesProcesados = totalMessages,
                        tamanoTotalMB = totalSizeMB,
                        tamanoPromedioKB = averageMessageSize / 1024,
                        gruposPorFecha = result.Count,
                        fechaInicio = result.LastOrDefault()?.FormattedDate,
                        fechaFin = result.FirstOrDefault()?.FormattedDate,
                        totalDias = result.Select(r => r.FormattedDate).Distinct().Count()
                    },
                    analisisDetallado = new
                    {
                        top10Remitentes = result
                            .GroupBy(r => r.Sender)
                            .OrderByDescending(g => g.Sum(x => x.TotalSizeMb))
                            .Take(10)
                            .Select(g => new { 
                                remitente = g.Key, 
                                mensajes = g.Sum(x => x.Count),
                                tamanoMB = g.Sum(x => x.TotalSizeMb),
                                porcentajeTamaño = totalSizeMB > 0 ? (g.Sum(x => x.TotalSizeMb) / totalSizeMB * 100) : 0
                            }),
                        distribucionTemporal = result
                            .GroupBy(r => r.YearMonth)
                            .OrderBy(g => g.Key)
                            .Select(g => new {
                                periodo = g.Key,
                                mensajes = g.Sum(x => x.Count),
                                tamanoMB = g.Sum(x => x.TotalSizeMb),
                                dias = g.Select(x => x.FormattedDate).Distinct().Count()
                            }),
                        estadisticasGenerales = new
                        {
                            mensajesPorDia = result.Count > 0 
                                ? (double)totalMessages / Math.Max(1, result.Select(r => r.FormattedDate).Distinct().Count())
                                : 0,
                            mbPorDia = result.Count > 0
                                ? totalSizeMB / Math.Max(1, result.Select(r => r.FormattedDate).Distinct().Count())
                                : 0,
                            diasConActividad = result.Select(r => r.FormattedDate).Distinct().Count(),
                            remitentesDiferentes = result.Select(r => r.Sender).Distinct().Count()
                        }
                    },
                    archivos = new
                    {
                        rutaReporte = options.CsvPath,
                        archivoGenerado = !string.IsNullOrEmpty(options.CsvPath) ? Path.GetFileName(options.CsvPath) : null,
                        urlDescarga = !string.IsNullOrEmpty(options.CsvPath) 
                            ? $"/api/graphmail/download-csv/{Path.GetFileName(options.CsvPath)}" 
                            : null,
                        tamanoArchivoMB = !string.IsNullOrEmpty(options.CsvPath) && System.IO.File.Exists(options.CsvPath)
                            ? new FileInfo(options.CsvPath).Length / (1024.0 * 1024.0)
                            : 0
                    },
                    optimizaciones = enableOptimizations ? new
                    {
                        estrategiaAdjuntos = "Timeouts cortos con estimación inteligente",
                        manejoErrores = "Fallback a estimaciones cuando hay timeouts",
                        paralelismo = "Controlado y optimizado por tipo de operación",
                        cachingHeaders = "Prioridad a headers HTTP para tamaños precisos",
                        estimacionesInteligentes = "Basadas en bodyPreview y tipo de mensaje"
                    } : null
                };

                _logger.LogInformation("? Procesamiento optimizado completado. Buzón: {mailbox}, Mensajes: {count:N0}, Duración: {duration}, Archivo: {file}", 
                    mailbox, totalMessages, totalDuration.ToString(@"hh\:mm\:ss"), options.CsvPath);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "? Error de validación en procesamiento optimizado: {message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error en procesamiento optimizado para mailbox {mailbox}: {message}", mailbox, ex.Message);
                return StatusCode(500, new { 
                    error = ex.Message,
                    tipo = "Error en Procesamiento Optimizado",
                    solucion = "Las optimizaciones evitan la mayoría de timeouts. Verifique conectividad.",
                    recomendacion = "Intente con enableOptimizations=true para mejor rendimiento"
                });
            }
        }

        /// <summary>
        /// Endpoint de ayuda para localizar archivos tras reinicio del sistema
        /// </summary>
        /// <returns>Información detallada de ubicaciones y comandos útiles</returns>
        [HttpGet("system-info")]
        public IActionResult GetSystemInfo()
        {
            try
            {
                // NUEVA RUTA: wwwroot\Informes  
                var baseDir = Directory.GetCurrentDirectory();
                var reportDir = Path.Combine(baseDir, "wwwroot", "Informes");
                
                var ultimosArchivos = new List<object>();
                if (Directory.Exists(reportDir))
                {
                    ultimosArchivos = Directory.GetFiles(reportDir, "*.csv")
                        .OrderByDescending(System.IO.File.GetLastWriteTime)
                        .Take(5)
                        .Select(file => new {
                            archivo = Path.GetFileName(file),
                            tamaño = $"{new FileInfo(file).Length / (1024.0 * 1024.0):F2} MB",
                            modificado = System.IO.File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm:ss")
                        }).ToList<object>();
                }

                var systemInfo = new
                {
                    informacionGeneral = new
                    {
                        fechaConsulta = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        directorioTrabajo = Directory.GetCurrentDirectory(),
                        directorioAplicacion = baseDir,
                        directorioReportes = reportDir,
                        usuario = Environment.UserName,
                        nombreMaquina = Environment.MachineName
                    },
                    ubicacionArchivos = new
                    {
                        rutaCompleta = Path.GetFullPath(reportDir),
                        existe = Directory.Exists(reportDir),
                        totalArchivos = Directory.Exists(reportDir) 
                            ? Directory.GetFiles(reportDir, "*.csv").Length 
                            : 0,
                        espacioDisponible = GetAvailableSpace(baseDir)
                    },
                    comandosUtiles = new
                    {
                        windowsExplorer = $"explorer.exe \"{Path.GetFullPath(reportDir)}\"",
                        comandoDir = $"dir \"{reportDir}\\*.csv\" /O:D",
                        powershell = $"Get-ChildItem -Path \"{reportDir}\" -Filter *.csv | Sort-Object LastWriteTime -Descending"
                    },
                    ultimosArchivos = ultimosArchivos,
                    endpointsDisponibles = new
                    {
                        procesamientoOptimizado = "/api/graphmail/optimized-processing?mailbox={mailbox}&enableOptimizations=true",
                        informacionSistema = "/api/graphmail/system-info"
                    }
                };

                return Ok(systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo información del sistema");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetAvailableSpace(string path)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(path));
                var availableGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                return $"{availableGB:F2} GB disponibles";
            }
            catch
            {
                return "No disponible";
            }
        }
    }
}