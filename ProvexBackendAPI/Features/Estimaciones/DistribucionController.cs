using Asp.Versioning;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Services;
using ProvexBackendAPI.Services.IServices;
using System.Security.Claims;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Features.Estimaciones
{
    [Route("api/v{version:apiVersion}/distribucion")]
    [ApiController]
    [ApiVersionNeutral]
    [Authorize]
    public class DistribucionController : ControllerBase
    {
        private readonly IDistribucionService _service;
        private readonly ITokenService _tokenService;

        public DistribucionController(IDistribucionService service, ITokenService tokenService)
        {
            _service = service;
            _tokenService = tokenService;
        }

        // GET api/v{version}/distribucion/categoria
        [HttpGet("categoria", Name = "GetDistribucionCategoria")]
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
        public async Task<IActionResult> GetFrigorificoAgrupado(
            [FromQuery] int idBisemanal
    )
        {

            var data = await _service.GetDistribucionFrigorificoAgrupadoAsync(idBisemanal);
            return Ok(data);
        }

        // GET api/v{version}/distribucion/packing
        [HttpGet("packing", Name = "GetPackingAgrupado")]
        public async Task<IActionResult> GetFrigoriGetPackingAgrupadoficoAgrupado(
            [FromQuery] int idBisemanal
    )
        {

            var data = await _service.GetDistribucionPackingAgrupadoAsync(idBisemanal);
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
            if (idEstimacion <= 0)
                return BadRequest("El idEstimacion debe ser mayor que cero.");



            var data = await _service.GetRowsDistribucionPorcentajeExportacionAsync(idEstimacion, semanasAntes, semanasDespues);
            return Ok(data);
        }

 
        // POST api/v{version}/distribucion/categoria
        [HttpPost("categoria", Name = "SaveDistribucionCategoria")]
        public async Task<IActionResult> SaveCategoria([FromBody] DistribucionCategoriaGuardarRequest req)
        {
            try
            {
                var userId = await _tokenService.GetUserIdFromClaimsAsync(User);
                if (userId is null)
                    throw new UnauthorizedAccessException("No se pudo determinar el usuario.");

                await _service.DistribucionCategoriaGuardarAsync(req, userId.Value);
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
                var userId = await _tokenService.GetUserIdFromClaimsAsync(User);
                if (userId is null)
                    throw new UnauthorizedAccessException("No se pudo determinar el usuario.");

                await _service.DistribucionCalibreGuardarAsync(req, userId.Value);
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
                var userId = await _tokenService.GetUserIdFromClaimsAsync(User);
                if (userId is null)
                    throw new UnauthorizedAccessException("No se pudo determinar el usuario.");

                await _service.DistribucionFrigorificoGuardarAsync(req, userId.Value);
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
                var userId = await _tokenService.GetUserIdFromClaimsAsync(User);
                if (userId is null)
                    throw new UnauthorizedAccessException("No se pudo determinar el usuario.");

                await _service.DistribucionPackingGuardarAsync(req, userId.Value);
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
                var userId = await _tokenService.GetUserIdFromClaimsAsync(User);
                if (userId is null)
                    throw new UnauthorizedAccessException("No se pudo determinar el usuario.");
                await _service.DistribucionPorcentajeExportacionGuardarAsync(req, userId.Value);
                return Ok();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
