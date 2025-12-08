using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Services.IServices;


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

    }
}
