using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IEstimacionesRepository
    {
        Task<EstimacionDistribucionPorProductorDto> GetEstimacionBisemanalAsync(
            EstimacionBisemanalQueryDto req);

        Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion);
    }
}
