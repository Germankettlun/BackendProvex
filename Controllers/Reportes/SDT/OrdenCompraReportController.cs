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
    public class OrdenCompraReportController : ControllerBase
    {
        private readonly string _conn;

        public OrdenCompraReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("OrdenCompra")]
        public async Task<IActionResult> Get(
            [FromQuery] string? empresa,
            [FromQuery] int? ANIO)
        {
            string json = "[]";
            try
            {
                var whereClauses = new List<string>();
                var sqlParams = new List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(empresa))
                {
                    whereClauses.Add("CodigoEmpresa = @empresa");
                    sqlParams.Add(new SqlParameter("@empresa", empresa));
                }

                if (!string.IsNullOrWhiteSpace(empresa))
                {
                    whereClauses.Add("Ano = @ANIO");
                    sqlParams.Add(new SqlParameter("@ANIO", ANIO));
                }

                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.API_SDT_OrdenCompra");
                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(string.Join(" AND ", whereClauses));
                }

                LogHelper.Log("Orden Compra | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Orden Compra | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}