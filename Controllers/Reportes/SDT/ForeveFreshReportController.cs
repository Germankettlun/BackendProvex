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
    public class ForeveFreshReportController : ControllerBase
    {
        private readonly string _conn;

        public ForeveFreshReportController(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("ProvexDb")!;
        }

        [HttpGet("ForeverFresh")]
        public async Task<IActionResult> Get()
        {
            string json = "[]";
            try
            {             
                var sql = new StringBuilder("SELECT * FROM PROVEX.dbo.FF_View");
                LogHelper.Log("Foreve Fresh | Query : " + sql, "Reportes");
                await using var conn = new SqlConnection(_conn);
                json = await conn.QueryJsonAsync(sql.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Log("Foreve Fresh | Error : " + ex.Message, "Reportes");
            }
            return Content(json, "application/json");
        }
    }
}