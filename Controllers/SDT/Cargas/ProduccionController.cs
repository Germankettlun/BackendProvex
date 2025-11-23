using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using ProvexApi.Data;
using System.Text.Json.Serialization;
using ProvexApi.Services.SDT;
using ProvexApi.Helper;
using Microsoft.AspNetCore.Authorization;
using ProvexApi.Models.SDT.Cargas;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class ProduccionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public ProduccionController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ProvexDbContext db, TemporadaService temporadaService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _db = db;
            _temporadaService = temporadaService; _temporadaService = temporadaService;
        }
       
        [HttpGet("SdtProduccion")]
        public async Task<IActionResult> GetProduccion(
            [FromQuery] bool guardarEnBD = true, [FromQuery] string? temporada = null, [FromQuery] string? empresa = null)
        {
            try
            {
                if (string.IsNullOrEmpty(temporada))
                {
                    return BadRequest("Falta el parámetro 'temporada'.");
                }
                else
                {
                    temporada = temporada.ToUpper();
                }

                if (string.IsNullOrEmpty(empresa))
                {
                    return BadRequest("Falta el parámetro 'empresa'.");
                }
                else
                {
                    empresa = empresa.ToUpper();
                }

                (string? codigoSDT, string? codigoInterno, string? FechaInicio) = await _temporadaService.GetTemporadaMappingAsync(temporada, empresa);

                //  Si no existe, retornamos BadRequest
                if (codigoSDT == null || codigoInterno == null)
                {
                    return BadRequest($"No existe conversión para empresa '{empresa}' y temporada '{temporada}'.");
                }

                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeProduccion");
                var url = api.Url.Replace("{temporada}", codigoInterno);
                var dataResponse = await SdtApiHelper.GetDataAsync<ProduccionResponse>(url, api.ApiAuthUser, api.ApiAuthPassword, api.ApiTimeOut);

                // Esta tabla no tiene temporada pero si se requiere para la api, asi que la inserto manual
                dataResponse.Data.ForEach(item => item.CodTemporada = temporada);
                dataResponse.Data.ForEach(item => item.CodEmpresa = empresa);

                if (!guardarEnBD)
                {
                    return Ok(dataResponse);
                }

                // Excluimos 'Id' si la columna es Identity en la BD
                string deleteCondition = $"WHERE CodTemporada = '{temporada}' and CodEmpresa = '{empresa}' ";

                // 3) Llamar a BulkCopyHelper
                string connectionString = api.ConBDProvex;
                string destinationTable = api.Tabla;
                var excludedProps = new HashSet<string> { "Id" };

                int filasEliminadas = await BulkCopyHelper.BulkInsertAsync(
                    items: dataResponse.Data,
                    destinationTable: destinationTable,
                    connectionString: connectionString,
                    deleteCondition: deleteCondition,
                    excludedProps: excludedProps,
                batchSize: 500
                );

                return Ok(new
                {
                    Message = $"Temporada: '{temporada}' homologada a => (Api:{codigoSDT}, Interno:{codigoInterno}). " +
                                $"\"Datos éliminados en la BD={filasEliminadas}" +
                                $"\"Datos guardados en la BD={dataResponse.Data.Count}"
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("Produccion | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Produccion");
            }
        }
    }
}
