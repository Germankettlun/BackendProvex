using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Services.Reports;

namespace ProvexApi.Controllers.Reportes.SDT
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api/reportes/ConsultaEmbarques")]
    public sealed class ReportesEmbarquesController : ControllerBase
    {
        private readonly EmbarquesReportService _svc;
        public ReportesEmbarquesController(EmbarquesReportService svc) => _svc = svc;

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string empresa, [FromQuery] string temporada, CancellationToken ct)
        {
            try
            {
                var (stream, savedPath, fileName) = await _svc.BuildAsync(empresa, temporada, ct);
                stream.Dispose(); // no vamos a enviar el archivo

                // ruta absoluta en disco
                var absolutePath = Path.GetFullPath(savedPath);

                // URL pública (si sirves wwwroot como estático)
                var url = $"{Request.Scheme}://{Request.Host}/output/{fileName}";

                return Ok(new
                {
                    ok = true,
                    fileName,
                    path = absolutePath,
                    url
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    ok = false,
                    error = ex.Message
                });
            }
        }
    }
}
