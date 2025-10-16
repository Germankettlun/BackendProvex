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
        [FromQuery] string codEmpresa,
        [FromQuery] string codEspecie,
        [FromQuery] string codTemporada,
        [FromQuery] string? categoriaId
    )
        {
            if (string.IsNullOrWhiteSpace(codEmpresa) ||
                string.IsNullOrWhiteSpace(codEspecie) ||
                string.IsNullOrWhiteSpace(codTemporada))
            {
                return BadRequest("el código de empresa, código de especie y código de temporada son requeridos.");
            }

            var req = new DistribucionCategoriaEspecieRequestDto
            {
                CodigoEmpresa = codEmpresa,
                CodigoEspecie = codEspecie,
                CodigoTemporada = codTemporada,
                IdCategoria = categoriaId
            };

            var data = await _service.GetDistribucionCategoriaAsync(req);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/calibre
        [HttpGet("calibre", Name = "GetDistribucionCalibre")]
        [ProducesResponseType(typeof(List<DistribucionCalibreEspecieResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCalibre(
        [FromQuery] string codEmpresa,
        [FromQuery] string codEspecie,
        [FromQuery] string codTemporada,
        [FromQuery] string? calibreId
    )
        {
            if (string.IsNullOrWhiteSpace(codEmpresa) ||
                string.IsNullOrWhiteSpace(codEspecie) ||
                string.IsNullOrWhiteSpace(codTemporada))
            {
                return BadRequest("el código de empresa, código de especie y código de temporada son requeridos.");
            }

            var req = new DistribucionCalibreEspecieRequestDto
            {
                CodigoEmpresa = codEmpresa,
                CodigoEspecie = codEspecie,
                CodigoTemporada = codTemporada,
                IdCalibre = calibreId
            };

            var data = await _service.GetDistribucionCalibreAsync(req);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/packing
        [HttpGet("packing", Name = "GetDistribucionPacking")]
        [ProducesResponseType(typeof(List<DistribucionPackingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPacking(
            [FromQuery] DistribucionPackingQueryDto q
    )
        {
           
            var data = await _service.GetDistribucionPackingAsync(q);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/frigorifico
        [HttpGet("frigorifico", Name = "GetDistribucionFrigorifico")]
        [ProducesResponseType(typeof(List<DistribucionFrigorificoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFrigorifico(
            [FromQuery] DistribucionPackingQueryDto q
    )
        {

            var data = await _service.GetDistribucionFrigorificoAsync(q);
            return Ok(data);
        }
    }
}
