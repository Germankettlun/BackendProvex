using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Services.IServices;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // [ApiVersion("1.0")]
    // [ApiVersion("2.0")]
    [ApiVersionNeutral]
    [AllowAnonymous]
    public class ComboController : ControllerBase
    {
        private readonly IComboService _comboService;

        public ComboController(IComboService comboService)
        {
            _comboService = comboService;

        }

        [HttpGet("GetCombo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ComboItemDto>>> GetCombo(
            [FromQuery] string nombreCombo,
            [FromQuery] string codigoEmpresa)
        {
            var data = await _comboService.GetComboGenericoAsync(nombreCombo, codigoEmpresa);
            return Ok(data);
        }
    }
}
