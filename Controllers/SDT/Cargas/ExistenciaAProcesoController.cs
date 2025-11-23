using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Services.SDT;
using ProvexApi.Models.SDT.Cargas;
using Microsoft.AspNetCore.Authorization;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class ExistenciaAProcesoController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public ExistenciaAProcesoController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ProvexDbContext db,
            TemporadaService temporadaService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _db = db;
            _temporadaService = temporadaService;
        }

        // -------------------------------------------------------------
        // GET api/SdtExistenciaAProceso
        // -------------------------------------------------------------
        [HttpGet("SdtExistenciaAProceso")]
        public async Task<IActionResult> GetExistenciaAProceso(
            [FromQuery] bool guardarEnBD = true,
            [FromQuery] string? temporada = null,
            [FromQuery] string? empresa = null)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrEmpty(temporada))
                    return BadRequest("Falta el parámetro 'temporada'.");
                temporada = temporada!.ToUpper();

                if (string.IsNullOrEmpty(empresa))
                    return BadRequest("Falta el parámetro 'empresa'.");
                empresa = empresa!.ToUpper();          

                // Obtenemos el mapeo de temporada→API (códigoSDT, códigoInterno, FechaInicio)
                (string? codigoSDT, string? codigoInterno, string? FechaInicio) =
                    await _temporadaService.GetTemporadaMappingAsync(temporada, empresa);

                if (codigoSDT == null || codigoInterno == null)
                    return BadRequest($"No existe conversión para empresa '{empresa}' y temporada '{temporada}'.");

                // Construcción de la URL (la API requiere el parámetro {facilitie})
                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeExistenciaAProceso");
                var urlParaFac = api.Url.Replace("{temporada}", codigoInterno);

                // Llamada a la API SDT
                var dataResponse = await SdtApiHelper.GetDataAsync<ExistenciaAProcesoResponse>(urlParaFac, api.ApiAuthUser, api.ApiAuthPassword, api.ApiTimeOut);

                // Asignamos los valores de temporada y empresa a cada item
                dataResponse.Data.ForEach(item =>
                {
                    item.CodigoTemporada = temporada;
                    item.CodigoEmpresa = empresa;
                });

                // Si no queremos persistir en BD, devolvemos la respuesta JSON de la API:
                if (!guardarEnBD)
                {
                    return Ok(dataResponse);
                }

                // Preparamos el BulkCopy (solo borrar antes de insertar)
                var connectionStringProvex = api.ConBDProvex;
                var nombreTablaDestino = api.Tabla;      // ej. "API_SDT_ExistenciaAProceso"
                var excludedProps = new HashSet<string> { "Id" }; // “Id” supuestamente es Identity

                // Condición de borrado para este único idFacilitie
                var deleteCondition = $"WHERE CodigoTemporada = '{temporada}' " +
                                      $"AND CodigoEmpresa  = '{empresa}' ";

                // BulkInsertAsync hará:
                //   1) DELETE FROM <tabla> <deleteCondition>
                //   2) INSERT en bloque los dataResponse.Data (solo para ese facility)
                int filasAfectadas = await BulkCopyHelper.BulkInsertAsync(
                    items: dataResponse.Data,
                    destinationTable: nombreTablaDestino,
                    connectionString: connectionStringProvex,
                    deleteCondition: deleteCondition,
                    excludedProps: excludedProps,
                    batchSize: 500);

                return Ok(new
                {
                    Message = $"Empresa: '{empresa}', Temporada: '{temporada}', " +
                              $"Filas eliminadas + insertadas: {filasAfectadas}."
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("Existencia | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Existencia a proceso");
            }
        }

    }
}
