using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Data.DTOs.ASOEX;
using ProvexApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProvexApi.Controllers.Asoex.Carga
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api/asoex")]
    public class AsoexController : ControllerBase
    {
        private readonly IAsoexService _service;

        public AsoexController(IAsoexService service)
        {
            _service = service;
        }

        /// <summary>
        /// Carga movimientos desde la API de ASOEX en el rango indicado y guarda en BD si se indica.
        /// Además, genera el JSON unificado y retorna la ruta del archivo generado.
        /// </summary>
        /// <param name="fechaini">Fecha inicio (YYYY-MM-DD)</param>
        /// <param name="fechafin">Fecha fin (YYYY-MM-DD)</param>
        /// <param name="guardarEnBD">Si se guarda en base de datos o solo consulta</param>
        /// <returns>Resultado del proceso y ruta JSON</returns>
        [HttpPost("CargarMovimientos")]
        public async Task<IActionResult> CargarMovimientos(
            [FromQuery] DateTime? fechaini,
            [FromQuery] DateTime? fechafin,
            [FromQuery] bool guardarEnBD = true,
            [FromQuery] bool csvLog = true)
        {
            DateTime fechaInicio = fechaini?.Date ?? DateTime.Today;
            DateTime fechaFin = fechafin?.Date ?? DateTime.Today;

            try
            {

                if (fechaInicio > fechaFin)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "La fecha de inicio no puede ser posterior a la fecha de fin."
                    });
                }

                string resultado = await _service.SincronizarHechosAsoexAsync(fechaInicio, fechaFin, guardarEnBD, csvLog);
 //               string rutaJson = _service.UltimaRutaJson; // ✅ obtiene la propiedad expuesta correctamente

                return Ok(new
                {
                    success = true,
                    message = resultado,
                });
            }
            catch (Exception ex)
            {
                // 🔴 Tip: puedes incluir ex.StackTrace para debugging interno si necesitas.
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    

        [HttpGet("prosesoJson/{idProceso}")]
        public async Task<IActionResult> GuardarProcesoCompleto(int idProceso)
        {
            await _service.DescargarYGuardarProcesoCompletoAsync(idProceso);
            return Ok(new { message = $"Proceso {idProceso} descargado y guardado correctamente." });
        }
    }
}
