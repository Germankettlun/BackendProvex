using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas.SemanasDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface ITemporadasService
    {
        Task<List<TemporadaDto>> ListAsync(string? codTem, string codEmp, int? vigente);
        Task<TemporadaDto?> GetByIdAsync(string codTem);
        Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem, string codEmp, int? vigente, string? semana = null, int? ano = null);

        Task<SemanaVigenteRow?> GetSemanaAsync(string codigoEmpresa, string? codigoTemporada = null, bool? soloVigente = null);
        Task<IReadOnlyList<SemanaVigenteRow>> ListSemanaAsync(string codigoEmpresa, string codigoTemporada, bool? soloVigente = null);
    }
}
