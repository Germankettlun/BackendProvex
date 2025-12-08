using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Services.IServices;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace ProvexBackendAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/temporadas")]
    [ApiVersionNeutral]
    //[Authorize]

    public class TemporadasController : ControllerBase
    {
        private readonly ITemporadasService temporada;
        public TemporadasController(ITemporadasService temporada) => this.temporada = temporada;


        
        [HttpGet("{codTem}/semanas")]
        public async Task<ActionResult> GetSemanas(string codTem, [FromQuery]string codEmp, [FromQuery] int? vigente = null, [FromQuery] string? semana = null, [FromQuery] int? ano = null)
        {

            var data = await temporada.GetSemanasTemporadaAsync(codTem, codEmp, vigente, semana, ano);
            return Ok(data);
        }

        [HttpGet]
       
        public async Task<IActionResult> GetTemporadas(
            [FromQuery] string? codigoTemporada,
            [FromQuery] string codigoEmpresa,
            [FromQuery] int? soloVigentes)
        {
            var data = await temporada.ListAsync(codigoTemporada,codigoEmpresa, soloVigentes);
            return Ok(data);
        }
    }
}
