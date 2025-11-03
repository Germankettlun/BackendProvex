using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [Route("api/v{version:apiVersion}/distribucion")]
    [ApiController]
    [ApiVersionNeutral]
    //[Authorize]
    public class DistribucionController : ControllerBase
    {
        private readonly IDistribucionService _service;

        public DistribucionController(IDistribucionService service)
        {
            _service = service;
        }

        // GET api/v{version}/distribucion/categoria
        [HttpGet("categoria", Name = "GetDistribucionCategoria")]
        [ProducesResponseType(typeof(List<DistribucionCategoriaEspecieResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCategoria(
        [FromQuery] int idEstimacion,
        [FromQuery] int? semanasAntes,
        [FromQuery] int? semanasDespues
    )
        {
            if (idEstimacion <= 0)
                return BadRequest("El idEstimacion debe ser mayor que cero.");



            var data = await _service.GetDistribucionCategoriaAsync(idEstimacion, semanasAntes, semanasDespues);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/calibre
        [HttpGet("calibre", Name = "GetDistribucionCalibre")]
        [ProducesResponseType(typeof(List<DistribucionCalibreEspecieResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCalibre(
        [FromQuery] int idEstimacion,
        [FromQuery] int? semanasAntes,
        [FromQuery] int? semanasDespues
    )
        {
            if (idEstimacion <= 0)
                return BadRequest("El idEstimacion debe ser mayor que cero.");

            var data = await _service.GetDistribucionCalibreAsync(idEstimacion, semanasAntes, semanasDespues);
            return Ok(data);
        }

        

        // GET api/v{version}/distribucion/frigorifico
        [HttpGet("frigorifico", Name = "GetFrigorificoAgrupado")]
        [ProducesResponseType(typeof(List<DistribucionFrigorificoDiaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFrigorificoAgrupado(
            [FromQuery] int idBisemanal
    )
        {

            var data = await _service.GetDistribucionFrigorificoAgrupadoAsync(idBisemanal);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/packing
        [HttpGet("packing", Name = "GetPackingAgrupado")]
        [ProducesResponseType(typeof(List<DistribucionPackingDiaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFrigoriGetPackingAgrupadoficoAgrupado(
            [FromQuery] int idBisemanal
    )
        {

            var data = await _service.GetDistribucionPackingAgrupadoAsync(idBisemanal);
            return Ok(data);
        }
    }
}
