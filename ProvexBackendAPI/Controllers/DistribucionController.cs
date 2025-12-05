using Asp.Versioning;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Services;
using ProvexBackendAPI.Services.IServices;
using System.Security.Claims;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Controllers
{
    [Route("api/v{version:apiVersion}/distribucion")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize]
    public class DistribucionController : ControllerBase
    {
        private readonly IDistribucionService distribucion;
        private readonly ITokenService token;

        public DistribucionController(IDistribucionService distribucion, ITokenService token)
        {
            this.distribucion = distribucion;
            this.token = token;
        }

        // GET api/v{version}/distribucion/categoria
        [HttpGet("categoria", Name = "GetDistribucionCategoria")]
        public async Task<IActionResult> GetCategoria(
        [FromQuery] int idEstimacion,
        [FromQuery] int? semanasAntes,
        [FromQuery] int? semanasDespues
    )
        {

            var data = await distribucion.GetDistribucionCategoriaAsync(idEstimacion, semanasAntes, semanasDespues);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/calibre
        [HttpGet("calibre", Name = "GetDistribucionCalibre")]
        public async Task<IActionResult> GetCalibre(
        [FromQuery] int idEstimacion,
        [FromQuery] int? semanasAntes,
        [FromQuery] int? semanasDespues
    )
        {

            var data = await distribucion.GetDistribucionCalibreAsync(idEstimacion, semanasAntes, semanasDespues);
            return Ok(data);
        }

        

        // GET api/v{version}/distribucion/frigorifico
        [HttpGet("frigorifico", Name = "GetFrigorificoAgrupado")]
        public async Task<IActionResult> GetFrigorificoAgrupado(
            [FromQuery] int idBisemanal
    )
        {

            var data = await distribucion.GetDistribucionFrigorificoAgrupadoAsync(idBisemanal);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/packing
        [HttpGet("packing", Name = "GetPackingAgrupado")]
        public async Task<IActionResult> GetFrigoriGetPackingAgrupadoficoAgrupado(
            [FromQuery] int idBisemanal
    )
        {

            var data = await distribucion.GetDistribucionPackingAgrupadoAsync(idBisemanal);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/porcentajeExportacion
        [HttpGet("porcentajeExportacion", Name = "GetDistribucionPorcentajeExportacion")]
        public async Task<IActionResult> GetPorcentajeExportacion(
        [FromQuery] int idEstimacion,
        [FromQuery] int? semanasAntes,
        [FromQuery] int? semanasDespues
    )
        {

            var data = await distribucion.GetRowsDistribucionPorcentajeExportacionAsync(idEstimacion, semanasAntes, semanasDespues);
            return Ok(data);
        }

 
        // POST api/v{version}/distribucion/categoria
        [HttpPost("categoria", Name = "SaveDistribucionCategoria")]
        public async Task<IActionResult> SaveCategoria([FromBody] DistribucionCategoriaGuardarRequest req)
        {
            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
                await distribucion.DistribucionCategoriaGuardarAsync(req, userId.Value);
                return NoContent();
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
       
        // POST api/v{version}/distribucion/calibre
        [HttpPost("calibre", Name = "SaveDistribucionCalibre")]
        public async Task<IActionResult> SaveCalibre([FromBody] DistribucionCalibreGuardarRequest req)
        {
            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
                await distribucion.DistribucionCalibreGuardarAsync(req, userId.Value);
                return NoContent();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // POST api/v{version}/distribucion/frigorifico
        [HttpPost("frigorifico", Name = "SaveDistribucionFrigorifico")]
        public async Task<IActionResult> SaveFrigorifico([FromBody] DistribucionFrigorificoGuardarRequest req)
        {
            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
                await distribucion.DistribucionFrigorificoGuardarAsync(req, userId.Value);
                return NoContent();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
       
        // POST api/v{version}/distribucion/packing
        [HttpPost("packing", Name = "SaveDistribucionPacking")]
        public async Task<IActionResult> SavePacking([FromBody] DistribucionPackingGuardarRequest req)
        {
            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
                await distribucion.DistribucionPackingGuardarAsync(req, userId.Value);
                return NoContent();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // POST api/v{version}/distribucion/porcentajeExportacion
        [HttpPost("porcentajeExportacion", Name = "SaveDistribucionPorcentajeExportacion")]
        public async Task<IActionResult> SavePorcentajeExportacion([FromBody] DistribucionPorcentajeExportacionGuardarRequest req)
        {
            try
            {
                var userId = await token.GetUserIdFromClaimsAsync(User);
                await   distribucion.DistribucionPorcentajeExportacionGuardarAsync(req, userId.Value);
                return Ok();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
