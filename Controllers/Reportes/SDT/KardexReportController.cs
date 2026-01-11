using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProvexApi.Helper;

namespace ProvexApi.Controllers.Reportes.SDT
{
    [Authorize(Policy = "BasicOnly")]
    [ApiController]
    [Route("reportes")]
    public class KardexReportController : ControllerBase
    {
        private readonly string _conn;

        public KardexReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("Kardex")]
        public async Task<IActionResult> Get(
            [FromQuery, BindRequired, DataType(DataType.Date)]
            DateTime fechaInicio,
            [FromQuery, BindRequired, DataType(DataType.Date)]
            DateTime fechaFin)
        {
            // Validación adicional: fechaInicio <= fechaFin
            if (fechaInicio > fechaFin)
                return BadRequest("El parámetro 'fechaInicio' no puede ser mayor que 'fechaFin'.");

            string json = "[]";
            try
            {
                var whereClauses = new List<string>
                {
                    "FechaMovimiento BETWEEN @fechaInicio AND @fechaFin"
                };

                        var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@fechaInicio", fechaInicio),
                    new SqlParameter("@fechaFin",    fechaFin)
                };

                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.SDT_View_Kardex");

                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(string.Join(" AND ", whereClauses));
                }

                LogHelper.Log("Kardex | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Kardex | Error: {ex.Message}", "Reportes");
                return StatusCode(500, "Ocurrió un error al generar el reporte " + ex.Message);
            }

            return Content(json, "application/json");
        }
    }
}
