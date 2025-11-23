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
    public class CatastroController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public CatastroController(
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

        [HttpGet("SdtCatastro")]
        public async Task<IActionResult> GetPredios(
            [FromQuery] bool guardarEnBD = true,
            [FromQuery] string? empresa = null)
        {
            if (string.IsNullOrEmpty(empresa))
                return BadRequest("Falta el parámetro 'empresa'.");

            empresa = empresa.ToUpper();

            var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeCatastro");
            var dataResponse = await SdtApiHelper.GetDataAsync<CatastroResponse>(api.Url,api.ApiAuthUser,api.ApiAuthPassword,api.ApiTimeOut);
            // Asignamos el código de empresa a cada ítem
            
            dataResponse.Data.ForEach(x => x.CodEmpresa = empresa);

            if (!guardarEnBD)
                return Ok(dataResponse);

            // Eliminamos los registros antiguos
            string deleteCondition = $"WHERE CodEmpresa = '{empresa}'";
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
    }
}
