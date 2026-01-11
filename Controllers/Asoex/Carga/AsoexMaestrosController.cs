using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

[Authorize(Policy = "LocalOnly")]
[ApiController]
[Route("api/asoex/maestros")]
public class AsoexMaestrosController : ControllerBase
{
    private readonly IAsoexMaestrosService _service;
    private readonly ILogger<AsoexMaestrosController> _logger;

    public AsoexMaestrosController(IAsoexMaestrosService service, ILogger<AsoexMaestrosController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("temporadas/sync")]
    public async Task<IActionResult> SyncTemporadas([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarTemporadasAsync(guardarEnBD));

    [HttpPost("exportadores/sync")]
    public async Task<IActionResult> SyncExportadores([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarExportadoresAsync(guardarEnBD));

    [HttpPost("semanas/sync")]
    public async Task<IActionResult> SyncSemanas([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarSemanasAsync(guardarEnBD));

    [HttpPost("consignatarios/sync")]
    public async Task<IActionResult> SyncConsignatarios([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarConsignatariosAsync(guardarEnBD));

    [HttpPost("regionesorigen/sync")]
    public async Task<IActionResult> SyncRegionesOrigen([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarRegionesOrigenAsync(guardarEnBD));

    [HttpPost("regionesdestino/sync")]
    public async Task<IActionResult> SyncRegionesDestino([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarRegionesDestinoAsync(guardarEnBD));

    [HttpPost("paisesdestino/sync")]
    public async Task<IActionResult> SyncPaisesDestino([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarPaisesDestinoAsync(guardarEnBD));

    [HttpPost("puertosembarque/sync")]
    public async Task<IActionResult> SyncPuertosEmbarque([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarPuertosEmbarqueAsync(guardarEnBD));

    [HttpPost("puertosdestino/sync")]
    public async Task<IActionResult> SyncPuertosDestino([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarPuertosDestinoAsync(guardarEnBD));

    [HttpPost("tipoespecies/sync")]
    public async Task<IActionResult> SyncTipoEspecies([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarTipoEspeciesAsync(guardarEnBD));

    [HttpPost("especies/sync")]
    public async Task<IActionResult> SyncEspecies([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarEspeciesAsync(guardarEnBD));

    [HttpPost("variedades/sync")]
    public async Task<IActionResult> SyncVariedades([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarVariedadesAsync(guardarEnBD));

    [HttpPost("naves/sync")]
    public async Task<IActionResult> SyncNaves([FromQuery] bool guardarEnBD = true)
        => await Procesar(async () => await _service.SincronizarNavesAsync(guardarEnBD));

    private async Task<IActionResult> Procesar(Func<Task<string>> accion)
    {
        try
        {
            var res = await accion();
            return Ok(new { success = true, message = res });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en sincronización de maestro");
            return StatusCode(500, $"Error durante la sincronización: {ex.Message}");
        }
    }
}
