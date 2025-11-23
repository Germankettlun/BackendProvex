using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Services.SDT;
using Microsoft.AspNetCore.Authorization;
using ProvexApi.Models.SDT.Cargas;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class EstandaMaterialesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public EstandaMaterialesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ProvexDbContext db, TemporadaService temporadaService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _db = db;
            _temporadaService = temporadaService; _temporadaService = temporadaService;
        }

        [HttpGet("SdtEstandarMateriales")]
        public async Task<IActionResult> GetOrdenCompra(
            [FromQuery, BindRequired] int? ANIO,
            [FromQuery] bool guardarEnBD = true, 
            [FromQuery] string? empresa = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Falta el parámetro 'ANIO' o no es un entero válido.");
                }

                if (string.IsNullOrEmpty(empresa))
                {
                    return BadRequest("Falta el parámetro 'empresa'.");
                }
                else
                {
                    empresa = empresa.ToUpper();
                }

                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeEstandarMateriales");
                var url = api.Url.Replace("{anio}", ANIO.ToString());
                var dataResponse = await SdtApiHelper.GetDataAsync<EstandarResponseMaterialesResponse>(url, api.ApiAuthUser, api.ApiAuthPassword, api.ApiTimeOut);
                dataResponse.Data.ForEach(item => item.CodigoEmpresa = empresa);
                dataResponse.Data.ForEach(item => item.Anio = ANIO.ToString());

                if (!guardarEnBD)
                {
                    return Ok(dataResponse);
                }

                string deleteCondition = $"WHERE Anio = {ANIO} and CodigoEmpresa  = '{empresa}'";
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
                    Message = $"Año: '{ANIO}' y empresa {empresa} " +
                                $"\"Datos éliminados en la BD={filasEliminadas}" +
                                $"\"Datos guardados en la BD={dataResponse.Data.Count}"
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("Estandar Material | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Estandar Material");
            }
        }
    }
}