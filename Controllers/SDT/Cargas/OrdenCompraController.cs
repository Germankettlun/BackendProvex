using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Services.SDT;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using ProvexApi.Models.SDT.Cargas;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Microsoft.Graph.Constants;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class OrdenCompraController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public OrdenCompraController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ProvexDbContext db, TemporadaService temporadaService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _db = db;
            _temporadaService = temporadaService; _temporadaService = temporadaService;
        }

        [HttpGet("SdtOrdenCompra")]
        public async Task<IActionResult> GetOrdenCompra([FromQuery, BindRequired] int? ANIO,
        [FromQuery] bool guardarEnBD = true, [FromQuery] string? empresa = null)
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

                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeOrdenCompra");
                var url = api.Url.Replace("{anio}", ANIO.ToString());
                var dataResponse = await SdtApiHelper.GetDataAsync<OrdenCompraResponse>(url, api.ApiAuthUser, api.ApiAuthPassword, api.ApiTimeOut);

                // Esta tabla no tiene temporada pero si se requiere para la api, asi que la inserto manual
                //dataResponse.Data.ForEach(item => item.CodigoTemporada = temporada);
                dataResponse.Data.ForEach(item => item.CodigoEmpresa = empresa);

                if (!guardarEnBD)
                {
                    return Ok(dataResponse);
                }

                // Excluimos 'Id' si la columna es Identity en la BD --andCodigoTemporada = '{temporada}'
                string deleteCondition = $"WHERE CodigoEmpresa = '{empresa}' and Ano = '{ANIO}' ";

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
                    Message = $"Empresa: '{empresa}' " +
                                $"\"Datos éliminados en la BD={filasEliminadas}" +
                                $"\"Datos guardados en la BD={dataResponse.Data.Count}"
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("Orden de Compra | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Orden de Compra");
            }
        }
    }
}
