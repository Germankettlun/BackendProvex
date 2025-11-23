using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvexApi.Models.Lab;
using ProvexApi.Services.Lab;

[Authorize]
[ApiController]
[Route("api/labresiduos")]
public sealed class LabResiduosController : ControllerBase
{
    private readonly LabResiduoImportService _svc;
    public LabResiduosController(LabResiduoImportService svc) => _svc = svc;

    [HttpPost("import")]
    [Consumes("application/json")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Import([FromBody] LabResiduoPayloadV2 payload, CancellationToken ct)
    {
        if (payload?.Data is null || payload.Data.Count == 0) return BadRequest("DATA vacío.");
        var (ins, upd, tot) = await _svc.UpsertAsync(payload.Data, ct);
        return Ok(new { inserted = ins, updated = upd, total = tot });
    }
}
