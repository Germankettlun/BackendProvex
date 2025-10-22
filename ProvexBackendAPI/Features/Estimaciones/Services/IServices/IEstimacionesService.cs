using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IEstimacionesService
    {
        Task<EstructuraDistribucionDto> GetEstimacionBisemanalAsync(
            EstimacionBisemanalQueryDto req);

        Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion);

        Task<SpResultEstimacionBisemanalDto> UpsertDiaAsync(UpdateEstimacionBisemanalRequest dto, int? userId);
    }
}
