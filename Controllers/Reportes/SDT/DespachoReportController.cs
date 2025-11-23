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
    public class DespachoReportController : ControllerBase
    {
        private readonly string _conn;

        public DespachoReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("Despachos")]
        public async Task<IActionResult> Get(
            [FromQuery] string? temporada,
            [FromQuery] string? empresa)
        {
            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(temporada))
                {
                    whereClauses.Add("CodigoTemporada = @temporada");
                    sqlParams.Add(new SqlParameter("@temporada", temporada));
                }

                if (!string.IsNullOrWhiteSpace(empresa))
                {
                    whereClauses.Add("CodigoEmpresa = @empresa");
                    sqlParams.Add(new SqlParameter("@empresa", empresa));
                }

                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.SDT_View_Despacho");
                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(string.Join(" AND ", whereClauses));
                }

                LogHelper.Log("Despachos | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Despachos | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}
