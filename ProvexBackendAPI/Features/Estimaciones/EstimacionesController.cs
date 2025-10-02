using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using static ProvexBackendAPI.Dto.Users.UsersDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [Route("api/v{version:apiVersion}/estimaciones")]
    [ApiController]
    [ApiVersionNeutral]
    //[Authorize]
    public class EstimacionesController : ControllerBase
    {
        private readonly IEstimacionesService _estimacionesService;
        public EstimacionesController(IEstimacionesService estimacionesService) => _estimacionesService = estimacionesService;


        //    // GET api/v{version}/estimacion/GetEstimacionBisemanal
        [HttpGet("GetEstimacionBisemanal", Name = "GetEstimacionBisemanal")]
        [ProducesResponseType(typeof(List<EstructuraDistribucionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEstimacionBisemanal(
            [FromQuery] EstimacionesDto.EstimacionBisemanalQueryDto q
    )
        {
            var data = await _estimacionesService.GetEstimacionBisemanalAsync(q);
            return Ok(data);
        }


        //    // GET api/v{version}/estimacion/GetResumenSemanal
        [HttpGet("GetResumenSemanal", Name = "GetResumenSemanal")]
        [ProducesResponseType(typeof(List<EstimacionSemanalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResumenSemanal(
            [FromQuery] string codigoEmpresa,
            [FromQuery] string idTemporada,
            [FromQuery] int idEstimacion
    )
        {
            var data = await _estimacionesService.GetResumenSemanalAsync(codigoEmpresa,idTemporada,idEstimacion);
            return Ok(data);
        }
    }
}
