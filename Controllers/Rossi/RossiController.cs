using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProvexApi.Data.Rossi;
using ProvexApi.Helper;
using ProvexApi.Models.Rossi;
using System;
using System.Text.Json;
using static ProvexApi.Models.Rossi.RossiRoot;

namespace ProvexApi.Controllers.Rossi
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class RossiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly RossiDbContext _db;
        private readonly ILogger<RossiController> _logger;  // 🔧 Cambio: logger agregado

        public RossiController(
            RossiDbContext db,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<RossiController> logger)           // 🔧 Cambio: logger inyectado
        {
            _db = db;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;                            // 🔧 Cambio: asignación
        }

        [HttpGet("embarque")]
        public async Task<IActionResult> GetEmbarque(
            [FromQuery] string referencia,
            [FromQuery] bool guardarEnBD = true,
            [FromQuery] string empresa = null!,
            [FromQuery] string temporada = null!)
        {  
            try
            {
                HttpResponseMessage resp;
                if (string.IsNullOrWhiteSpace(referencia))
                    return BadRequest("Debes indicar 'referencia'.");

                var apiKey = _configuration["Rossi:Key"];
                var urlTpl = _configuration["Rossi:Apis:Embarque"];
                var url = urlTpl.Replace("{referencia}", referencia);

                _httpClient.DefaultRequestHeaders.Remove("x-api-key");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                resp = await _httpClient.GetAsync(url);

                if (!resp.IsSuccessStatusCode)
                    return StatusCode((int)resp.StatusCode, await resp.Content.ReadAsStringAsync());

                var content = await resp.Content.ReadAsStringAsync();
                var dataRoot = JsonSerializer.Deserialize<RossiRoot>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (dataRoot?.Documental == null || !dataRoot.Documental.Any())
                    return NotFound("No hay datos en 'Documental'.");

                if (!guardarEnBD)
                    return Ok(dataRoot.Documental);

                // 🔧 Cambio: uso del método encapsulado
                var count = await ProcessDocumentsAsync(dataRoot.Documental, empresa, temporada);
                return Ok($"Guardados {count} documentos para la referencia '{referencia}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a Rossi para {Referencia}", referencia);
                LogHelper.Log("Rossi Embarque | Error : " + ex.Message, "Rossi");
                return StatusCode(500, $"Error al llamar a la API: {ex.Message}");
            }           
        }

        [HttpGet("embarqueAllTem")]
        public async Task<IActionResult> GetEmbarqueAllTem(
            [FromQuery] bool guardarEnBD = true,
            [FromQuery] string empresa = null!,
            [FromQuery] string temporada = null!)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empresa) || string.IsNullOrWhiteSpace(temporada))
                    return BadRequest("Debes indicar 'empresa' y 'temporada' en la query.");

                var referencias = await _db.Set<DespachoNaves>()
                    .Where(d => d.CodigoEmpresa == empresa && d.CodigoTemporada == temporada)
                    .Select(d => d.CodNave)
                    .Distinct()
                    .ToListAsync();

                if (!referencias.Any())
                    return NotFound($"No se encontraron naves para {empresa} / {temporada}.");

                var apiKey = _configuration["Rossi:Key"];
                var baseUrl = _configuration["Rossi:Apis:Embarque"];
                _httpClient.DefaultRequestHeaders.Remove("x-api-key");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                int totalProcessedDocs = 0;
                int totalRefs = referencias.Count;

                for (int i = 0; i < totalRefs; i++)
                {
                    var referencia = referencias[i];
                    var pct = (int)Math.Round((i + 1) * 100.0 / totalRefs);
                    _logger.LogInformation("→ [{Index}/{Total}] {Percent}% procesando referencia {Ref}",
                        i + 1, totalRefs, pct, referencia);  // 🔧 Cambio: log de avance

                    // llamar a la API
                    var url = baseUrl.Replace("{referencia}", referencia);
                    HttpResponseMessage resp;
                    try
                    {
                        resp = await _httpClient.GetAsync(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error HTTP en referencia {Ref}", referencia);
                        continue;
                    }

                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("API devolvió {Status} en referencia {Ref}",
                            resp.StatusCode, referencia);
                        continue;
                    }

                    var content = await resp.Content.ReadAsStringAsync();
                    var dataRoot = JsonSerializer.Deserialize<RossiRoot>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (dataRoot?.Documental == null || !dataRoot.Documental.Any())
                    {
                        _logger.LogInformation("Sin datos para referencia {Ref}", referencia);
                        continue;
                    }

                    if (!guardarEnBD)
                        continue;

                    var count = await ProcessDocumentsAsync(dataRoot.Documental, empresa, temporada);
                    totalProcessedDocs += count;
                }
                _logger.LogInformation("→ Terminadas {Processed}/{Total} referencias. Docs guardados: {Docs}",totalRefs, totalRefs, totalProcessedDocs);
                return Ok($"Se procesaron {totalRefs} referencias y guardados {totalProcessedDocs} documentos en la BD.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a Rossi para temporada {temporada}", temporada);
                LogHelper.Log("Rossi Embarque All | Error : " + ex.Message, "Rossi");
                return StatusCode(500, $"Error al llamar a la API: {ex.Message}");
            }
           
        }

        [HttpGet("embarqueACierre")]
        public async Task<IActionResult> GetEmbarqueACierre(
          [FromQuery] bool guardarEnBD = true,
          [FromQuery] string empresa = null!,
          [FromQuery] string temporada = null!)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empresa) || string.IsNullOrWhiteSpace(temporada))
                    return BadRequest("Debes indicar 'empresa' y 'temporada' en la query.");

                // Calcular los límites de la ventana
                var inicioVentana = DateTime.Now.AddDays(-7);
                var finVentana = DateTime.Now.AddDays(14);

                // Traer los códigos de nave distintos en ese rango
                var referencias = await _db.Set<DespachoNaves>()
                    .Where(d => d.CodigoEmpresa == empresa
                             && d.CodigoTemporada == temporada
                             && d.FechaZarpe >= inicioVentana
                             && d.FechaZarpe <= finVentana)
                    .Select(d => d.CodNave)
                    .Distinct()
                    .ToListAsync();

                if (!referencias.Any())
                    return NotFound($"No se encontraron naves para {empresa} / {temporada}.");

                var apiKey = _configuration["Rossi:Key"];
                var baseUrl = _configuration["Rossi:Apis:Embarque"];
                _httpClient.DefaultRequestHeaders.Remove("x-api-key");
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

                int totalProcessedDocs = 0;
                int totalRefs = referencias.Count;

                for (int i = 0; i < totalRefs; i++)
                {
                    var referencia = referencias[i];
                    var pct = (int)Math.Round((i + 1) * 100.0 / totalRefs);
                    _logger.LogInformation("→ [{Index}/{Total}] {Percent}% procesando referencia {Ref}",
                        i + 1, totalRefs, pct, referencia);  // 🔧 Cambio: log de avance

                    // llamar a la API
                    var url = baseUrl.Replace("{referencia}", referencia);
                    HttpResponseMessage resp;
                    try
                    {
                        resp = await _httpClient.GetAsync(url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error HTTP en referencia {Ref}", referencia);
                        continue;
                    }

                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("API devolvió {Status} en referencia {Ref}",
                            resp.StatusCode, referencia);
                        continue;
                    }

                    var content = await resp.Content.ReadAsStringAsync();
                    var dataRoot = JsonSerializer.Deserialize<RossiRoot>(
                        content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (dataRoot?.Documental == null || !dataRoot.Documental.Any())
                    {
                        _logger.LogInformation("Sin datos para referencia {Ref}", referencia);
                        continue;
                    }

                    if (!guardarEnBD)
                        continue;

                    var count = await ProcessDocumentsAsync(dataRoot.Documental, empresa, temporada);
                    totalProcessedDocs += count;
                }
                _logger.LogInformation("→ Terminadas {Processed}/{Total} referencias. Docs guardados: {Docs}", totalRefs, totalRefs, totalProcessedDocs);
                return Ok($"Se procesaron {totalRefs} referencias y guardados {totalProcessedDocs} documentos en la BD.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a Rossi para temporada {temporada}", temporada);
                LogHelper.Log("Rossi Embarque All | Error : " + ex.Message, "Rossi");
                return StatusCode(500, $"Error al llamar a la API: {ex.Message}");
            }

        }

        /// <summary>
        /// 🔧 Cambio: Encapsula la lógica de eliminar antiguos e insertar nuevos
        /// </summary>
        private async Task<int> ProcessDocumentsAsync(IEnumerable<RossiRoot.Documento> docs, string empresa, string temporada)
        {
            int savedCount = 0;

            foreach (var doc in docs)
            {
                var despachoValue = doc.despacho;
                if (string.IsNullOrWhiteSpace(despachoValue))
                    continue;

                doc.CodigoEmpresa = empresa;    // 🔧 propagar en cada doc
                doc.CodigoTemporada = temporada;  

                // 1) Eliminar existentes
                var existing = await _db.Documentos
                    .Include(d => d.Embarque)
                    .Include(d => d.Facturacion)
                    .Include(d => d.Pallets).ThenInclude(p => p.p_detalle)
                    .Include(d => d.ArchivoUrls)
                    .Where(d => d.despacho == despachoValue)
                    .ToListAsync();

                if (existing.Any())
                {
                    _logger.LogInformation("   → Eliminando {Count} registros antiguos para despacho={Despacho}",
                        existing.Count, despachoValue);
                    foreach (var old in existing)
                    {
                        if (old.Embarque != null) _db.Embarques.RemoveRange(old.Embarque);
                        if (old.Facturacion != null) _db.Facturaciones.RemoveRange(old.Facturacion);
                        if (old.Pallets != null)
                        {
                            foreach (var p in old.Pallets)
                                if (p.p_detalle != null) _db.PDetalles.RemoveRange(p.p_detalle);
                            _db.Pallets.RemoveRange(old.Pallets);
                        }
                        if (old.ArchivoUrls != null) _db.ArchivoUrls.RemoveRange(old.ArchivoUrls);
                    }
                    _db.Documentos.RemoveRange(existing);
                    await _db.SaveChangesAsync();
                }

                // 2) Insertar nuevo
                _db.Documentos.Add(doc);
                await _db.SaveChangesAsync();  // 🔧 Cambio: doc.Id ahora está disponible

                // 3) Insertar URLs
                int urls = 0;
                if (doc.Archivos?.fullset?.url != null)
                {
                    var full = doc.Archivos.fullset.url.Select(u => new ArchivoUrl
                    {
                        DocumentoId = doc.Id,
                        TipoArchivo = "fullset",
                        Url = u
                    });
                    _db.ArchivoUrls.AddRange(full);
                    urls += full.Count();
                }
                if (doc.Archivos?.dus_legalizada?.url != null)
                {
                    var dus = doc.Archivos.dus_legalizada.url.Select(u => new ArchivoUrl
                    {
                        DocumentoId = doc.Id,
                        TipoArchivo = "dus_legalizada",
                        Url = u
                    });
                    _db.ArchivoUrls.AddRange(dus);
                    urls += dus.Count();
                }
                if (urls > 0)
                {
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("   → Insertadas {Count} URLs para despacho={Despacho}", urls, despachoValue);
                }
                savedCount++;
            }
            return savedCount;
        }
    }
}
