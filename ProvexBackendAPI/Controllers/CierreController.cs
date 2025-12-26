using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Services.IServices;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/cierre")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize]

    public class CierreController : ControllerBase
    {
        private readonly ICierreService cierre;
        private readonly ITokenService token;

        public CierreController(ICierreService cierre, ITokenService token)
        {
            this.cierre = cierre;
            this.token = token;
        }

        // GET api/v{version}/cierre/versiones
        [HttpGet("versiones", Name = "GetListadoCierreVersion")]
        public async Task<IActionResult> GetListadoCierreVersion(
            [FromQuery] string idEmpresa,
            [FromQuery] string idTemporada,
            [FromQuery] string? idEspecie,
            [FromQuery] string? descripcion
        )
        {
            var data = await cierre.GetListadoCierreVersion(idEmpresa,idTemporada,idEspecie,descripcion);
            return Ok(data);
        }

        [HttpPost("generarCierre")]
        public async Task<ActionResult> GenerarCierre(IngresarCierreRequest request)
        {
            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
                var result = await cierre.GenerarCierre(request, userId.Value);
                return Ok(result);

            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }


    }
}
