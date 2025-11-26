using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Services.IServices;
using static ProvexBackendAPI.Dto.EstimacionesDto;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/estimaciones")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize]
    public class EstimacionController : ControllerBase
    {
        private readonly IEstimacionService estimacion;
        private readonly ITokenService token;

        public EstimacionController(IEstimacionService estimacion, ITokenService token)
        {
            this.estimacion = estimacion;
            this.token = token;
        }

        //    // GET api/v{version}/estimacion/GetEstimacionBisemanal
        [HttpGet("GetEstimacionBisemanal", Name = "GetEstimacionBisemanal")]
        public async Task<IActionResult> GetEstimacionBisemanal(
            [FromQuery] EstimacionBisemanalQueryDto q
    )
        {
            var data = await estimacion.GetEstimacionBisemanalAsync(q);
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
            var data = await estimacion.GetResumenSemanalAsync(codigoEmpresa,idTemporada,idEstimacion);
            return Ok(data);
        }

        // POST api/v{version}/estimaciones/bisemanal/dia
        [HttpPost("dia", Name = "UpdateInsertBisemanalDia")]
        

        public async Task<IActionResult> UpdateInsertBisemanalDia([FromBody] UpdateEstimacionBisemanalRequest request)
        {
               

            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
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
                var userId = await token.GetUserIdFromClaimsAsync(User);
                await estimacion.IngresarEstimacion(request, userId.Value);
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
            var userId = await token.GetUserIdFromClaimsAsync(User);
            await estimacion.IngresarPorcentajeExportacionSemanal(input, userId.Value);
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
            var userId = await token.GetUserIdFromClaimsAsync(User);

            return Ok();
        }
    }
}
