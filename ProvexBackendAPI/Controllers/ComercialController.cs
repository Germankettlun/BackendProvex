using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Services.IServices;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ComercialController : Controller
    {
        private readonly IComercial comercial;

        public ComercialController(IComercial comercial)
        {
            this.comercial = comercial;
        }

        [AllowAnonymous]
        [HttpPost("obtenerAgrupacionEspecieCalibre")]
        public async Task<List<ComboItemDto>> obtenerAgrupacionEspecieCalibre(RequestContextDTO contextDTO)
        {
            var res = await comercial.ObtenerAgrupacionEspecieCalibre(contextDTO);

            return res;
        }

        [AllowAnonymous]
        [HttpGet("obtenerCalibres")]
        public async Task<List<ComboItemDto>> ObtenerCalibres([FromQuery] string empresa, [FromQuery] string especie)
        {
            var res = await comercial.ObtenerCalibres(empresa, especie);

            return res;
        }

        [AllowAnonymous]
        [HttpPost("crearAgrupacion")]
        public void CrearAgrupacion(CrearAgrupacionRequest request)
        {
            comercial.CrearAgrupacion(request);
        }

    }
}
