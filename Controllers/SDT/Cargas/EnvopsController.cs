// Controllers/SDT/Cargas/EnvasesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Services.SDT;
using ProvexApi.Models.SDT;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class EnvasesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public EnvasesController(
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

        [HttpGet("SdtEnvops")]
        public async Task<IActionResult> GetEnvops(
            [FromQuery] bool guardarEnBD = true,
            [FromQuery] string? empresa = null)
        {
            try
            {
                if (string.IsNullOrEmpty(empresa))
                    return BadRequest("Falta el parámetro 'empresa'.");

                empresa = empresa.ToUpper();

                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeEnvop");
                var dataResponse = await SdtApiHelper.GetDataAsync<EnvopsResponse>(api.Url, api.ApiAuthUser, api.ApiAuthPassword, api.ApiTimeOut );

                // Asignamos la empresa a cada ítem
                dataResponse.Data.ForEach(x => x.CodigoEmpresa = empresa);

                if (!guardarEnBD)
                    return Ok(dataResponse);

                // Eliminamos los existentes
                string deleteCondition = $"WHERE CodigoEmpresa = '{empresa}'";
                int filasEliminadas = await BulkCopyHelper.BulkInsertAsync(
                    items: dataResponse.Data,
                    destinationTable: api.Tabla,
                    connectionString: api.ConBDProvex,
                    deleteCondition: deleteCondition,
                    excludedProps: new HashSet<string> { "Id" },
                    batchSize: 500
                );

                return Ok(new
                {
                    Message = $"Empresa: '{empresa}' " +
                              $"Datos eliminados en BD={filasEliminadas} " +
                              $"Datos insertados en BD={dataResponse.Data.Count}"
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Envops | Error: {ex.Message}", "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Envops");
            }
        }
    }
}
