using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProvexApi.Helper;
using System.Text;

namespace ProvexApi.Controllers.Reportes.SDT
{
    [Authorize(Policy = "BasicOnly")]
    [ApiController]
    [Route("reportes")]
    public class AsoexReportController : ControllerBase
    {
        private readonly string _conn;

        public AsoexReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("ReporteAsoex")]
        public async Task<IActionResult> GetReporteAsoex()
        {
            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();       
                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.View_Asoex_All");
                //if (whereClauses.Count > 0)
                //{
                //    sql.Append(" WHERE ");
                //    sql.Append(string.Join(" AND ", whereClauses));
                //}
                LogHelper.Log("Asoex | Query : " + sql, "Reportes");
                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Asoex | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }


        [HttpGet("ReporteAsoexBruto")]
        public async Task<IActionResult> GetBruto()
        {
            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();
                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.View_Asoex_API");
                //if (whereClauses.Count > 0)
                //{
                //    sql.Append(" WHERE ");
                //    sql.Append(string.Join(" AND ", whereClauses));
                //}
                LogHelper.Log("Asoex Bruto | Query : " + sql, "Reportes");
                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Asoex Bruto | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}
