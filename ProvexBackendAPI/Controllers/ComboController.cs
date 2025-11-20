using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;


namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    //[Authorize]
    public class ComboController : ControllerBase
    {
        private readonly IComboService combo;

        public ComboController(IComboService combo)
        {
            this.combo = combo;

        }

        [HttpGet("GetCombo")]
        public async Task<ActionResult<List<ComboItemDto>>> GetCombo([FromQuery] ComboRequest q)
        {
            var data = await combo.GetComboGenericoAsync(q);
            return Ok(data);
        }

        [HttpGet("GetComboEnvase")]
        public async Task<ActionResult<List<ComboItemDto>>> GetComboEnvase(
            [FromQuery] string codigoProductor,
            [FromQuery] string codigoEspecie,
            [FromQuery] string codigoVariedad)
        {
            var data = await combo.GetComboEnvaseProductorEspecieVariedadAsync(codigoProductor, codigoEspecie, codigoVariedad);
            return Ok(data);
        }
    }
}
