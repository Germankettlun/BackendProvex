using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProvexApi.Helper;
using System.Text;

namespace ProvexApi.Controllers.Reportes.Estimaciones
{
    [Authorize(Policy = "BasicOnly")]
    [ApiController]
    [Route("reportes")]
    public class BisemanalReportController : ControllerBase
    {
        private readonly string _conn;

        public BisemanalReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("ReporteEstimacionBisemanalFull")]
        public async Task<IActionResult> GetFull(
            [FromQuery] string? temporada,
            [FromQuery] string? empresa)
        {
            if (string.IsNullOrWhiteSpace(temporada) || string.IsNullOrWhiteSpace(empresa))
            {
                return BadRequest(new { message = "Los parámetros 'temporada' y 'empresa' son obligatorios." });
            }

            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();

                whereClauses.Add("ID_TEMPORADA = @temporada");
                sqlParams.Add(new SqlParameter("@temporada", temporada));

                whereClauses.Add("ID_EMPRESA = @empresa");
                sqlParams.Add(new SqlParameter("@empresa", empresa));

                var sql = new StringBuilder("SELECT * FROM PROVEX.Estimaciones.VW_Estimacion_Bisemanal");
                sql.Append(" WHERE ");
                sql.Append(string.Join(" AND ", whereClauses));

                LogHelper.Log("Estimacion Bisemanal Full | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Estimacion Bisemanal  Full | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }


        [HttpGet("ReporteEstimacionBisemanal")]
        public async Task<IActionResult> GetBisemanal(
          [FromQuery] string? temporada,
          [FromQuery] string? empresa)
        {
            if (string.IsNullOrWhiteSpace(temporada) || string.IsNullOrWhiteSpace(empresa))
            {
                return BadRequest(new { message = "Los parámetros 'temporada' y 'empresa' son obligatorios." });
            }

            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();

                whereClauses.Add("ID_TEMPORADA = @temporada");
                sqlParams.Add(new SqlParameter("@temporada", temporada));

                whereClauses.Add("ID_EMPRESA = @empresa");
                sqlParams.Add(new SqlParameter("@empresa", empresa));

                var sql = new StringBuilder("SELECT * FROM PROVEX.Estimaciones.VW_Estimacion_Bisemanal_Reporte");
                sql.Append(" WHERE ");
                sql.Append(string.Join(" AND ", whereClauses));

                LogHelper.Log("Estimacion Bisemanal Reporte | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Estimacion Bisemanal Reporte | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }


        [HttpGet("ReporteEstimacionSemanal")]
        public async Task<IActionResult> GetSemanal(
            [FromQuery] string? temporada,
            [FromQuery] string? empresa)
        {
            if (string.IsNullOrWhiteSpace(temporada) || string.IsNullOrWhiteSpace(empresa))
            {
                return BadRequest(new { message = "Los parámetros 'temporada' y 'empresa' son obligatorios." });
            }

            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();

                whereClauses.Add("ID_TEMPORADA = @temporada");
                sqlParams.Add(new SqlParameter("@temporada", temporada));

                whereClauses.Add("ID_EMPRESA = @empresa");
                sqlParams.Add(new SqlParameter("@empresa", empresa));

                var sql = new StringBuilder("SELECT * FROM PROVEX.Estimaciones.VW_EstimacionSemanal_Reporte");
                sql.Append(" WHERE ");
                sql.Append(string.Join(" AND ", whereClauses));

                LogHelper.Log("Estimacion Semanal | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Estimacion Semanal | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}
