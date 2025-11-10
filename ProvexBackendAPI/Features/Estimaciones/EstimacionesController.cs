using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Services.IServices;
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
        private readonly IEstimacionService estimacion;

        public EstimacionesController(IEstimacionesService estimacionesService, IEstimacionService estimacion)
        {
            _estimacionesService = estimacionesService;
            this.estimacion = estimacion;
        }

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

        // POST api/v{version}/estimaciones/bisemanal/dia
        [HttpPost("dia", Name = "UpdateInsertBisemanalDia")]
        //[Authorize] 
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateInsertBisemanalDia(
        [FromBody] UpdateEstimacionBisemanalRequest request)
        {
            var userId = 1;      

            try
            {
                _ = await _estimacionesService.UpsertDiaAsync(request, userId);
                return Ok("OK"); // 200, data:null
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("ingresarEstimacion")]
        public async Task<ActionResult> IngresarEstimacion(IngresarEstimacionRequest request)
        {
            try
            {
                await estimacion.IngresarEstimacion(request);
                return Ok();

            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }
    }
}
