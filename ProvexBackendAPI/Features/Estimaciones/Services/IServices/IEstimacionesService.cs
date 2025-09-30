using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IEstimacionesService
    {
        Task<EstimacionDistribucionDto> GetEstimacionBisemanalAsync(
            EstimacionBisemanalQueryDto req);
    }
}
