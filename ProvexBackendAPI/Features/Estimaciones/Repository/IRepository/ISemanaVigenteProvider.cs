using static ProvexBackendAPI.Features.Estimaciones.Dto.Semanas.SemanasDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface ISemanaVigenteProvider
    {
        Task<SemanaVigenteRow?> GetAsync(string codigoEmpresa, string? codigoTemporada = null, bool? soloVigente = null);
    }
}
