using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [ApiController]
    [Route("api/v{version:apiVersion}/temporadas")]
    [ApiVersionNeutral]
    //[Authorize]

    public class TemporadasController : ControllerBase
    {
        private readonly ITemporadasService _temporadasService;
        public TemporadasController(ITemporadasService temporadasService) => _temporadasService = temporadasService;


        
        [HttpGet("{codTem}/semanas")]
        [ProducesResponseType(typeof(List<SemanaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetSemanas(string codTem, [FromQuery]string codEmp, [FromQuery] int? vigente = null, [FromQuery] string? semana = null, [FromQuery] int? ano = null)
        {
            if (string.IsNullOrWhiteSpace(codTem) || string.IsNullOrWhiteSpace(codEmp))
                return BadRequest("El código de temporada y el código de empresa son requeridos.");

            var data = await _temporadasService.GetSemanasTemporadaAsync(codTem, codEmp, vigente, semana, ano);
            return Ok(data);
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(List<TemporadaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
       
        public async Task<IActionResult> GetTemporadas(
            [FromQuery] string? codigoTemporada,
            [FromQuery] string codigoEmpresa,
            [FromQuery] int? soloVigentes)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                return BadRequest("El código de la empresa es requeridos.");
            var data = await _temporadasService.ListAsync(codigoTemporada,codigoEmpresa, soloVigentes);
            return Ok(data);
        }
    }
}
