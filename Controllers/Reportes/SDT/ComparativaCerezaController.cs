using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProvexApi.Helper;
using System.Data;
using System.Text;

namespace ProvexApi.Controllers.Reportes.SDT
{
    [Authorize(Policy = "BasicOnly")]
    [ApiController]
    [Route("reportes")]
    public class ComparativaCerezaController : ControllerBase
    {
        private readonly string _conn;

        public ComparativaCerezaController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("ComparativaPreciosSemana")]
        public async Task<IActionResult> GetReporteComparativa(
            [FromQuery] string? temporada,
            [FromQuery] string? empresa)
        {
            string json = "[]";
            try
            {
                // Valores por defecto si no vienen en la query
                var codigoTemporada = string.IsNullOrWhiteSpace(temporada) ? "T6" : temporada;
                var codigoEmpresa = string.IsNullOrWhiteSpace(empresa) ? "PRX" : empresa;

                // Construir el EXEC del SP con parámetros
                var sql = new StringBuilder();
                sql.Append("EXEC dbo.sp_ComparativaPreciosSemanaNave ");
                sql.Append("@CodigoTemporada = @temporada, ");
                sql.Append("@CodigoEmpresa   = @empresa");

                LogHelper.Log($"Comparativa Cereza | EXEC: {sql}", "Reportes");

                // Preparar parámetros
                var sqlParams = new[]
                {
                    new SqlParameter("@temporada", SqlDbType.NVarChar, 50) { Value = codigoTemporada },
                    new SqlParameter("@empresa",   SqlDbType.NVarChar, 50) { Value = codigoEmpresa }
                };

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(
                                                    sql: sql.ToString(),
                                                    parameters: sqlParams,
                                                    commandTimeoutSeconds: 300    // 5 min
                                                );
            }
            catch (Exception ex)
            {
                LogHelper.Log("Comparativa Cereza | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }

        [HttpGet("ComparativaPreciosSemanaCereza")]
        public async Task<IActionResult> GetReporteComparativaCE(
          [FromQuery] string? temporada,
          [FromQuery] string? empresa)
        {
            string json = "[]";

            try
            {
                // Valores por defecto si no vienen en la query
                var codigoTemporada = string.IsNullOrWhiteSpace(temporada) ? "T6" : temporada;
                var codigoEmpresa = string.IsNullOrWhiteSpace(empresa) ? "PRX" : empresa;

                // Construir el EXEC del SP con parámetros
                var sql = new StringBuilder();
                sql.Append("EXEC dbo.sp_ComparativaPreciosSemanaNave_Cereza ");
                sql.Append("@CodigoTemporada = @temporada, ");
                sql.Append("@CodigoEmpresa   = @empresa");


                LogHelper.Log($"Comparativa Cereza | EXEC: {sql}", "Reportes");

                // Preparar parámetros
                var sqlParams = new[]
                {
                    new SqlParameter("@temporada", SqlDbType.NVarChar, 50) { Value = codigoTemporada },
                    new SqlParameter("@empresa",   SqlDbType.NVarChar, 50) { Value = codigoEmpresa }
                };

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(
                                                    sql: sql.ToString(),
                                                    parameters: sqlParams,
                                                    commandTimeoutSeconds: 300    // 5 min
                                                );
            }
            catch (Exception ex)
            {
                LogHelper.Log("Comparativa Cereza | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}
