using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Models.SensiWatch;
using ProvexApi.Services.SensiWatch;

namespace ProvexApi.Controllers.SensiWatch
{
    /// <summary>
    /// Controlador para recibir datos push de SensiWatch y proveer APIs de consulta
    /// </summary>
    [ApiController]
    [Route("api/sensiwatch")]
    public class SensiWatchController : ControllerBase
    {
        private readonly ISensiWatchService _sensiWatchService;
        private readonly ILogger<SensiWatchController> _logger;

        public SensiWatchController(
            ISensiWatchService sensiWatchService,
            ILogger<SensiWatchController> logger)
        {
            _sensiWatchService = sensiWatchService;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint para recibir activaciones de dispositivos desde SensiWatch
        /// </summary>
        /// <param name="activation">Datos de activación del dispositivo</param>
        /// <returns>Confirmación de recepción</returns>
        [HttpPost("device/activation")]
        [AllowAnonymous] // La autenticación se maneja mediante header personalizado
        public async Task<IActionResult> DeviceActivation([FromBody] DeviceActivationDto activation)
        {
            try
            {
                // Validar autenticación mediante header
                if (!ValidateAuthenticationHeader())
                {
                    _logger.LogWarning("[SensiWatch] Intento de acceso no autorizado a /device/activation desde IP: {RemoteIp}",
                        Request.HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { error = "Authentication failed" });
                }

                _logger.LogInformation("[SensiWatch] Recibida activación de dispositivo: {DeviceSerial}",
                    activation.DeviceIdentity?.SensitechSerialNumber);

                // Procesar de forma asíncrona para responder rápido a SensiWatch
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _sensiWatchService.ProcessDeviceActivationAsync(activation);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[SensiWatch] Error procesando activación en background");
                    }
                });

                return Ok(new { 
                    status = "received", 
                    message = "Device activation received successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error en endpoint de activación");
                return Ok(new { 
                    status = "received", 
                    message = "Request acknowledged" 
                }); // Siempre responder OK para evitar reintentos de SensiWatch
            }
        }

        /// <summary>
        /// Endpoint para recibir reportes de datos desde SensiWatch
        /// </summary>
        /// <param name="report">Datos del reporte del dispositivo</param>
        /// <returns>Confirmación de recepción</returns>
        [HttpPost("device/report")]
        [AllowAnonymous] // La autenticación se maneja mediante header personalizado
        public async Task<IActionResult> DeviceReport([FromBody] DeviceReportDto report)
        {
            try
            {
                // Validar autenticación mediante header
                if (!ValidateAuthenticationHeader())
                {
                    _logger.LogWarning("[SensiWatch] Intento de acceso no autorizado a /device/report desde IP: {RemoteIp}",
                        Request.HttpContext.Connection.RemoteIpAddress);
                    return Unauthorized(new { error = "Authentication failed" });
                }

                _logger.LogInformation("[SensiWatch] Recibido reporte de dispositivo: {DeviceSerial}, Sensores: {SensorCount}",
                    report.DeviceSerial, report.Sensors?.Count ?? 0);

                // Procesar de forma asíncrona para responder rápido a SensiWatch
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _sensiWatchService.ProcessDeviceReportAsync(report);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[SensiWatch] Error procesando reporte en background");
                    }
                });

                return Ok(new { 
                    status = "received", 
                    message = "Device report received successfully",
                    timestamp = DateTime.UtcNow,
                    sensors = report.Sensors?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error en endpoint de reporte");
                return Ok(new { 
                    status = "received", 
                    message = "Request acknowledged" 
                }); // Siempre responder OK para evitar reintentos de SensiWatch
            }
        }

        /// <summary>
        /// Obtiene lecturas de temperatura por rango de fechas
        /// </summary>
        /// <param name="startDate">Fecha de inicio (formato: yyyy-MM-dd)</param>
        /// <param name="endDate">Fecha de fin (formato: yyyy-MM-dd)</param>
        /// <param name="deviceSerial">Serial del dispositivo (opcional)</param>
        /// <returns>Lista de lecturas de temperatura</returns>
        [HttpGet("temperature-readings")]
        [Authorize(Policy = "BasicOnly")]
        public async Task<IActionResult> GetTemperatureReadings(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? deviceSerial = null)
        {
            try
            {
                if (endDate < startDate)
                {
                    return BadRequest(new { error = "End date must be greater than start date" });
                }

                var readings = await _sensiWatchService.GetTemperatureReadingsAsync(startDate, endDate, deviceSerial);

                return Ok(new
                {
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd"),
                    deviceSerial = deviceSerial ?? "All devices",
                    totalReadings = readings.Count,
                    readings = readings,
                    summary = readings.Any() ? new
                    {
                        minTemperature = readings.Min(r => r.Temperature),
                        maxTemperature = readings.Max(r => r.Temperature),
                        avgTemperature = readings.Average(r => r.Temperature),
                        devicesCount = readings.Select(r => r.DeviceSerial).Distinct().Count(),
                        tripsCount = readings.Where(r => !string.IsNullOrEmpty(r.InternalTripId))
                                           .Select(r => r.InternalTripId).Distinct().Count()
                    } : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error obteniendo lecturas de temperatura");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene resumen de un trip específico
        /// </summary>
        /// <param name="tripId">ID interno del trip</param>
        /// <returns>Resumen del trip</returns>
        [HttpGet("trip/{tripId}/summary")]
        [Authorize(Policy = "BasicOnly")]
        public async Task<IActionResult> GetTripSummary(string tripId)
        {
            try
            {
                var summary = await _sensiWatchService.GetTripSummaryAsync(tripId);

                if (summary == null)
                {
                    return NotFound(new { error = $"Trip '{tripId}' not found" });
                }

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error obteniendo resumen de trip: {TripId}", tripId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtiene estado de todos los dispositivos activos
        /// </summary>
        /// <returns>Lista de dispositivos con su estado actual</returns>
        [HttpGet("devices/status")]
        [Authorize(Policy = "BasicOnly")]
        public async Task<IActionResult> GetDevicesStatus()
        {
            try
            {
                var devices = await _sensiWatchService.GetActiveDevicesAsync();

                return Ok(new
                {
                    totalDevices = devices.Count,
                    statusSummary = devices.GroupBy(d => d.Status)
                                          .ToDictionary(g => g.Key, g => g.Count()),
                    devices = devices.OrderByDescending(d => d.LastSeen)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error obteniendo estado de dispositivos");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Endpoint de información del sistema SensiWatch
        /// </summary>
        /// <returns>Información sobre la integración</returns>
        [HttpGet("system-info")]
        [Authorize(Policy = "BasicOnly")]
        public IActionResult GetSystemInfo()
        {
            try
            {
                return Ok(new
                {
                    integration = new
                    {
                        name = "SensiWatch Thermograph Integration",
                        version = "1.0.0",
                        description = "Recepción y procesamiento de datos de termógrafos SensiWatch",
                        endpoints = new
                        {
                            pushActivation = "/api/sensiwatch/device/activation",
                            pushReport = "/api/sensiwatch/device/report",
                            temperatureReadings = "/api/sensiwatch/temperature-readings",
                            tripSummary = "/api/sensiwatch/trip/{tripId}/summary",
                            deviceStatus = "/api/sensiwatch/devices/status"
                        }
                    },
                    requirements = new
                    {
                        https = "Required - Certificate must be valid",
                        authentication = "HTTP Basic Auth header required for push endpoints",
                        ipWhitelist = "Recommended - Allow only SensiWatch IP (20.124.145.167)",
                        responseTime = "Must respond within 30 seconds with HTTP 200"
                    },
                    configuration = new
                    {
                        database = "SensiWatch tables created in ProvexDbContext",
                        logging = "Structured logging with [SensiWatch] prefix",
                        errorHandling = "Always responds OK to prevent SensiWatch retries"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SensiWatch] Error obteniendo información del sistema");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Valida el header de autenticación para endpoints de push de SensiWatch
        /// </summary>
        private bool ValidateAuthenticationHeader()
        {
            // TODO: Configurar las credenciales en appsettings.json o variables de entorno
            // Por ahora usar credenciales de ejemplo (cambiar en producción)
            const string expectedAuth = "Basic c2Vuc2l0ZWNoOnNlY3JldG8xMjM="; // base64("sensitech:secreto123")

            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return false;
            }

            return authHeader.ToString() == expectedAuth;
        }
    }
}