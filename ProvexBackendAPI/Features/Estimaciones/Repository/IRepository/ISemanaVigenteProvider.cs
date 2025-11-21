using static ProvexBackendAPI.Dto.SemanasDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface ISemanaVigenteProvider
    {
        Task<SemanaVigenteRow?> GetAsync(string codigoEmpresa, string? codigoTemporada = null, bool? soloVigente = null);
        Task<IReadOnlyList<SemanaVigenteRow>> ListAsync(string codigoEmpresa, string codigoTemporada, bool? soloVigente = null);

    }
}
