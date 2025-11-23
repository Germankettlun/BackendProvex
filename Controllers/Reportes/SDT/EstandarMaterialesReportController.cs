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
    public class EstandarMaterialesReportController : ControllerBase
    {
        private readonly string _conn;

        public EstandarMaterialesReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("EstandarMateriales")]
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
                    whereClauses.Add("Anio = @ANIO");
                    sqlParams.Add(new SqlParameter("@ANIO", ANIO));
                }

                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.SDT_View_EstandarMateriales");
                if (whereClauses.Count > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(string.Join(" AND ", whereClauses));
                }

                LogHelper.Log("Estandar Materiales | Query : " + sql, "Reportes");

                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString(), sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Estandar Materiales | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}