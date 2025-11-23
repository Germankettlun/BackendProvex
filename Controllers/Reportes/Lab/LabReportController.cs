using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProvexApi.Helper;
using System.Text;

namespace ProvexApi.Controllers.Reportes.Lab
{

    [Authorize(Policy = "BasicOnly")]
    [ApiController]
    [Route("reportes")]
    public class LabReportController : Controller
    {
        private readonly string _conn;

        public LabReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("ReporteLab")]
        public async Task<IActionResult> GetReporteAsoex()
        {
            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();
                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.Lab_View");
                //if (whereClauses.Count > 0)
                //{
                //    sql.Append(" WHERE ");
                //    sql.Append(string.Join(" AND ", whereClauses));
                //}
                LogHelper.Log("Lab | Query : " + sql, "Reportes");
                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Ñab | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}
