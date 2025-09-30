using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IEstimacionesService
    {
        Task<EstimacionDistribucionPorProductorDto> GetEstimacionBisemanalAsync(
            EstimacionBisemanalQueryDto req);
    }
}
