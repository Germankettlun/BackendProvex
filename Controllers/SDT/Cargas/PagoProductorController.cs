using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Models.SDT.Cargas;
using ProvexApi.Services.SDT;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class PagoProductorController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public PagoProductorController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ProvexDbContext db, TemporadaService temporadaService)
        {
            // Usar cliente nombrado "SdtApi"
            _httpClient = httpClientFactory.CreateClient("SdtApi");
            _configuration = configuration;
            _db = db;
            _temporadaService = temporadaService;
        }

        [HttpGet("SdtPagoProductor")]
        public async Task<IActionResult> GetDespachos(
        [FromQuery] bool guardarEnBD = true, [FromQuery] string? temporada = null, [FromQuery] string? empresa = null)
        {
            try
            {
                if (string.IsNullOrEmpty(temporada)){
                    return BadRequest("Falta el parámetro 'temporada'.");
                }else { 
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

                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformePagoProductor");
                var url = api.Url.Replace("{temporada}", codigoInterno);
                var dataResponse = await SdtApiHelper.GetDataAsync<PagoProductorResponse>(url, api.ApiAuthUser, api.ApiAuthPassword, api.ApiTimeOut);

                dataResponse.Data.ForEach(item => item.CodigoEmpresa = empresa);
                dataResponse.Data.ForEach(item => item.CodigoTemporada = temporada);

                if (!guardarEnBD)
                {
                    return Ok(dataResponse);
                }

                // Excluimos 'Id' si la columna es Identity en la BD
                string deleteCondition = $"WHERE CodigoTemporada = '{temporada}' and CodigoEmpresa = '{empresa}'";

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
                    Message = $"Empresa: '{empresa}' y Temporada: '{temporada}' homologada a => (Api:{codigoSDT}, Interno:{codigoInterno}). " +
                                $"\"Datos éliminados en la BD={filasEliminadas}" +
                                $"\"Datos guardados en la BD={dataResponse.Data.Count}"
                });

            }
            catch (Exception ex)
            {
                LogHelper.Log("Pago productor | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Pago productor");
            }
        }
    }
}
