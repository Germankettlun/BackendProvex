using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data;
using ProvexApi.Helper;
using ProvexApi.Services.SDT;
using Microsoft.AspNetCore.Authorization;
using ProvexApi.Models.SDT.Cargas;
using Microsoft.Data.SqlClient;
using static Microsoft.Graph.Constants;

namespace ProvexApi.Controllers.SDT.Cargas
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api")]
    public class CuadraturaProcesosDiariosController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ProvexDbContext _db;
        private readonly TemporadaService _temporadaService;

        public CuadraturaProcesosDiariosController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ProvexDbContext db, TemporadaService temporadaService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _db = db;
            _temporadaService = temporadaService; _temporadaService = temporadaService;
        }

        [HttpGet("SdtCuadraturaProcesoDiarios")]
        public async Task<IActionResult> GetCuadraturaProcesosDiarios(
                            [FromQuery] bool guardarEnBD = true, 
                            [FromQuery] string? temporada = null, 
                            [FromQuery] string? empresa = null,
                            [FromQuery] string? Facilitie = null)
        {
            try
            {
                DateTime dtIni;
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

                if (string.IsNullOrEmpty(Facilitie))
                    return BadRequest("Falta el parámetro 'Facilitie'.");
                Facilitie = Facilitie!.ToUpper();

                (string? codigoSDT, string? codigoInterno, string? FechaInicio) = await _temporadaService.GetTemporadaMappingAsync(temporada, empresa);
              
                //  Si no existe, retornamos BadRequest
                if (codigoSDT == null || codigoInterno == null)
                {
                    return BadRequest($"No existe conversión para temporada '{temporada}'.");
                }
                else
                {
                    if (string.IsNullOrEmpty(FechaInicio))
                    {
                        return BadRequest($"No existe 'fecha_inicio' en la BD para la temporada '{temporada}'.");
                    }

                    if (!DateTime.TryParse(FechaInicio, out dtIni))
                    {
                        return BadRequest($"La fecha_inicio '{FechaInicio}' no es una fecha válida.");
                    }
                }

                string? idFacilitie = await _temporadaService.GetFacilitieIdAsync("20169", Facilitie); //obtener codigo de empresa
                if (idFacilitie == null)
                {
                    return BadRequest($"No existe facilitie '{Facilitie}'.");
                }

                var fechaIni = dtIni.ToString("yyyy-MM-ddTHH:mm:ss");
                var fechaHasta = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

                var api = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeCuadraturaProcesosDiarios");
                var url = api.Url.Replace("{temporada}", codigoInterno);
                url = url.Replace("{facilitie}", idFacilitie);
                url = url.Replace("{fechaIni}", fechaIni);
                url = url.Replace("{FechaHasta}", fechaHasta);
                var dataResponse = await SdtApiHelper.GetDataAsync<CuadraturaProcesosDiariosResponse>(api, url);
                dataResponse.Data.ForEach(item => item.CodigoTemporada = temporada);
                dataResponse.Data.ForEach(item => item.CodigoEmpresa = empresa);

                if (!guardarEnBD)
                {
                    return Ok(dataResponse);
                }

                // Excluimos 'Id' si la columna es Identity en la BD
                string deleteCondition = $"WHERE CodigoTemporada = '{temporada}' " +
                                                 $"AND CodigoEmpresa = '{empresa}' " +
                                                 $"AND CodigoFacilitie    = '{Facilitie}'";

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
                    Message = $"Empresa: '{empresa}' y Temporada: '{temporada}' homologada a => (Api:{codigoSDT}, Interno:{codigoInterno}). Facilitie {Facilitie}" +
                                $"\"Datos éliminados en la BD={filasEliminadas}" +
                                $"\"Datos guardados en la BD={dataResponse.Data.Count}"
                });

            }
            catch (Exception ex)
            {
                LogHelper.Log("Cuadratura Procesos | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "Cuadratura Procesos");
            }
        }

        [HttpGet("SdtCuadraturaProcesoDiariosAll")]
        public async Task<IActionResult> GetCuadraturaProcesoDiariosAll(
             [FromQuery] bool guardarEnBD = true,
             [FromQuery] string? temporada = null,
             [FromQuery] string? empresa = null)
        {
            try
            {
                DateTime dtIni;
                // 1) Validaciones de 'temporada' y 'empresa'
                if (string.IsNullOrEmpty(temporada))
                    return BadRequest("Falta el parámetro 'temporada'.");
                temporada = temporada!.ToUpper();

                if (string.IsNullOrEmpty(empresa))
                    return BadRequest("Falta el parámetro 'empresa'.");
                empresa = empresa!.ToUpper();

                // 2) Obtenemos el mapeo de temporada → API
                (string? codigoSDT, string? codigoInterno, string? FechaInicio) = await _temporadaService.GetTemporadaMappingAsync(temporada, empresa);

                //  Si no existe, retornamos BadRequest
                if (codigoSDT == null || codigoInterno == null)
                {
                    return BadRequest($"No existe conversión para temporada '{temporada}'.");
                }
                else
                {
                    if (string.IsNullOrEmpty(FechaInicio))
                    {
                        return BadRequest($"No existe 'fecha_inicio' en la BD para la temporada '{temporada}'.");
                    }

                    if (!DateTime.TryParse(FechaInicio, out dtIni))
                    {
                        return BadRequest($"La fecha_inicio '{FechaInicio}' no es una fecha válida.");
                    }
                }

                var fechaIni = dtIni.ToString("yyyy-MM-ddTHH:mm:ss");
                var fechaHasta = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

                // 3) Recuperamos la lista de (IdFacilitie, CodigoFacility) de la tabla maestra [Facilitie]
                var listaFacilitieMaestra = new List<(string IdFacilitie, string CodigoFacilitie, string NombreFacilitie)>();
                var connMaster = _configuration["ConnectionProvexStrings:DatabaseConnection"];
                var sqlMaster = @"
            SELECT 
                IdFacilitie, 
                CodFacilitie,
                Nombre
            FROM [dbo].[Facilitie] 
            WHERE IdEmpresa = 20169
            AND SwActivo = 1"; //obtener empresa

                using (var sqlConn = new SqlConnection(connMaster))
                {
                    await sqlConn.OpenAsync();
                    using (var cmd = new SqlCommand(sqlMaster, sqlConn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string idFac = reader["IdFacilitie"].ToString();
                                string codFac = reader["CodFacilitie"].ToString();
                                string nomFac = reader["Nombre"].ToString();
                                listaFacilitieMaestra.Add((idFac, codFac, nomFac));
                            }
                        }
                    }
                }

                // 4) Si la lista maestra viene vacía, devolvemos un mensaje y no procesamos nada
                if (listaFacilitieMaestra.Count == 0)
                {
                    return Ok(new
                    {
                        Message = $"No se encontró ningún facilitie en la tabla maestra 'Facilitie' " +
                                  $"para IdEmpresa='{empresa}'."
                    });
                }

                // 5) Configuración de la API y tabla destino
                var apiConfig = SdtApiHelper.GetSdtApiConfig(_configuration, "InformeCuadraturaProcesosDiarios");
                var connectionStringProvex = apiConfig.ConBDProvex;
                var nombreTablaDestino = apiConfig.Tabla; // ej. "API_SDT_CuadraturaProcesosDiarios"
                var excludedProps = new HashSet<string> { "Id" };

                int totalFilasEliminadas = 0;
                int totalFilasInsertadas = 0;
                var detallePorFacilitie = new List<object>();

                // 6) Recorremos cada facility de la lista maestra
                foreach (var (idFacilitie, codigoFacilite, nombreFacilitie) in listaFacilitieMaestra)
                {
                    int filasBorradasParaEsteFac = 0;
                    int filasInsertadasParaEsteFac = 0;
                    string? mensajeErrorParaEsteFac = null;

                    try
                    {
                        // 6.1) Armar URL para llamar a la API usando 'codigoFacility'
                        var urlPorFac = apiConfig.Url
                                            .Replace("{temporada}", codigoInterno)
                                            .Replace("{facilitie}", idFacilitie)
                                            .Replace("{fechaIni}", fechaIni)
                                            .Replace("{FechaHasta}", fechaHasta);
                        // 6.2) Llamar a SDT para este facility
                        var responsePorFac = await SdtApiHelper.GetDataAsync<CuadraturaProcesosDiariosResponse>(apiConfig, urlPorFac);

                        // 6.3) Asignar empresa + temporada a cada registro
                        responsePorFac.Data.ForEach(item =>
                        {
                            item.CodigoTemporada = temporada;
                            item.CodigoEmpresa = empresa;
                            item.CodigoFacilitie = codigoFacilite; // asegurarse de que el modelo tenga esta propiedad
                            item.Facilitie = nombreFacilitie; // asegurarse de que el modelo tenga esta propiedad
                        });

                        // 6.4) Borrar todo lo anterior para este facility (usando 'CodigoFacility', no 'IdFacilitie')
                        var deleteCondition = $@"
                    WHERE CodigoTemporada = '{temporada}'
                      AND CodigoEmpresa  = '{empresa}'
                      AND CodigoFacilitie = '{codigoFacilite}'";

                        using (var sqlConn2 = new SqlConnection(connectionStringProvex))
                        {
                            await sqlConn2.OpenAsync();
                            var sqlDelete = $"DELETE FROM {nombreTablaDestino} {deleteCondition}";
                            using (var cmdDel = new SqlCommand(sqlDelete, sqlConn2))
                            {
                                filasBorradasParaEsteFac = await cmdDel.ExecuteNonQueryAsync();
                                totalFilasEliminadas += filasBorradasParaEsteFac;
                            }
                        }

                        // 6.5) Insertar en bloque (BulkCopy) sólo los registros recuperados
                        if (responsePorFac.Data.Count > 0)
                        {
                            await BulkCopyHelper.BulkInsertAsync(
                                items: responsePorFac.Data,
                                destinationTable: nombreTablaDestino,
                                connectionString: connectionStringProvex,
                                deleteCondition: null,   // ya hicimos el DELETE explícito
                                excludedProps: excludedProps,
                                batchSize: 500
                            );
                            totalFilasInsertadas += responsePorFac.Data.Count;
                        }
                    }
                    catch (Exception exFac)
                    {
                        // Si ocurre un error en esta iteración, capturamos el mensaje y seguimos con el siguiente
                        mensajeErrorParaEsteFac = exFac.Message;
                        LogHelper.Log(
                            $"Error procesando facility (IdFacilitie={idFacilitie}, CodigoFacility={codigoFacilite}) " +
                            $"para Empresa='{empresa}', Temporada='{temporada}': {exFac.Message}",
                            "CuadraturaProcesoAll"
                        );
                    }

                    // 6.6) Guardamos en el detalle de salida (conteo de filas o error)
                    if (mensajeErrorParaEsteFac == null)
                    {
                        detallePorFacilitie.Add(new
                        {
                            IdFacilitie = idFacilitie,
                            CodigoFacility = codigoFacilite,
                            FilasEliminadas = filasBorradasParaEsteFac,
                            FilasInsertadas = filasInsertadasParaEsteFac
                        });
                    }
                    else
                    {
                        detallePorFacilitie.Add(new
                        {
                            IdFacilitie = idFacilitie,
                            CodigoFacility = codigoFacilite,
                            FilasEliminadas = (int?)null,
                            FilasInsertadas = (int?)null,
                            Error = mensajeErrorParaEsteFac
                        });
                    }
                }

                // 7) Respondemos con un resumen de todo el proceso, incluyendo detalle por facility
                return Ok(new
                {
                    Message = $"Proceso finalizado para Empresa='{empresa}', Temporada='{temporada}'.",
                    FacilitiesProcesados = listaFacilitieMaestra.Count,
                    TotalFilasEliminadas = totalFilasEliminadas,
                    TotalFilasInsertadas = totalFilasInsertadas,
                    DetallePorFacilitie = detallePorFacilitie
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log("CuadraturaProcesoAll | Error : " + ex.Message, "Controller");
                return await ErrorResponseHelper.HandleExceptionAsync(ex, _configuration, "CuadraturaProcesoDiarios All");
            }
        }
    }
}
