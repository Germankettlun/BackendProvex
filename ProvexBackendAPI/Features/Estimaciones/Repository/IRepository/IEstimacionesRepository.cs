using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IEstimacionesRepository
    {
        Task<EstimacionDistribucionDto> GetEstimacionBisemanalAsync(
            EstimacionBisemanalQueryDto req);
    }
}
