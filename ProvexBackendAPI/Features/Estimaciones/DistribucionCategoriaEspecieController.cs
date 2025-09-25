using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCategoriaEspecieDto;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [Route("api/v{version:apiVersion}/estimaciones/distribucion-categoria-especie")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize]
    public class DistribucionCategoriaEspecieController : ControllerBase
    {
        private readonly IDistribucionCategoriaEspecieService _service;

        public DistribucionCategoriaEspecieController(IDistribucionCategoriaEspecieService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get(
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

            var data = await _service.GetAsync(req);
            return Ok(data);
        }
    }
}
