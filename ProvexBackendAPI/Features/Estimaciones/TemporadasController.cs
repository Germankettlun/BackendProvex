using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [ApiController]
    [Route("api/v{version:apiVersion}/temporadas")]
    [ApiVersionNeutral]
    [Authorize]

    public class TemporadasController : ControllerBase
    {
        private readonly ITemporadasService _temporadasService;
        public TemporadasController(ITemporadasService temporadasService) => _temporadasService = temporadasService;


        // GET /api/v1/temporadas/T6/semanas?codEmp=PRX&vigente=1
        [HttpGet("{codTem}/semanas")]
        [ProducesResponseType(typeof(List<SemanaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSemanas(string codTem, [FromQuery] string codEmp, [FromQuery] int? vigente = null, [FromQuery] string? semana = null, [FromQuery] int? ano = null)
        {
            if (string.IsNullOrWhiteSpace(codTem) || string.IsNullOrWhiteSpace(codEmp))
                return BadRequest("codTem y codEmp son requeridos.");

            var data = await _temporadasService.GetSemanasTemporadaAsync(codTem, codEmp, vigente, semana, ano);
            return Ok(data);
        }
    }
}
