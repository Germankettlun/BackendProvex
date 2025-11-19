using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Services;
using ProvexBackendAPI.Services.IServices;
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
        private readonly ITokenService token;

        public EstimacionesController(IEstimacionesService estimacionesService, IEstimacionService estimacion, ITokenService token)
        {
            _estimacionesService = estimacionesService;
            this.estimacion = estimacion;
            this.token = token;
        }

        //    // GET api/v{version}/estimacion/GetEstimacionBisemanal
        [HttpGet("GetEstimacionBisemanal", Name = "GetEstimacionBisemanal")]
        public async Task<IActionResult> GetEstimacionBisemanal(
            [FromQuery] EstimacionesDto.EstimacionBisemanalQueryDto q
    )
        {
            var data = await _estimacionesService.GetEstimacionBisemanalAsync(q);
            return Ok(data);
        }


        //    // GET api/v{version}/estimacion/GetResumenSemanal
        [HttpGet("GetResumenSemanal", Name = "GetResumenSemanal")]
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
        [Authorize]
        [HttpPost("dia", Name = "UpdateInsertBisemanalDia")]
        

        public async Task<IActionResult> UpdateInsertBisemanalDia([FromBody] UpdateEstimacionBisemanalRequest request)
        {
               

            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);

                if (userId is null)
                    throw new UnauthorizedAccessException("No se pudo determinar el usuario.");

                await estimacion.UpsertDiaAsync(request, userId.Value);
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

        [HttpPost("ActualizarExportacionSemanal")]
        public async Task ActualizarExportacionSemanal(PorcentajeExportacionSemanalDTO input)
        {
            await estimacion.IngresarPorcentajeExportacionSemanal(input);
            return;
        }

        [HttpGet("ObtenerZonas/{codEmpresa}")]
        public async Task<ActionResult<List<ZonaDTO>>> ObtenerZonas(string codEmpresa)
        {
            var res = await estimacion.ObtenerZonas(codEmpresa);
            return Ok(res);
        }

        [HttpPost("Publicar")]
        public async Task<ActionResult> Publicar(PublicacionDTO publicacion)
        {
            return Ok();
        }
    }
}
