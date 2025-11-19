using ProvexBackendAPI.Dto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IEstimacionService
    {
        Task IngresarEstimacion(IngresarEstimacionRequest request);
        Task IngresarPorcentajeExportacionSemanal(PorcentajeExportacionSemanalDTO input, Guid userId);
        Task<List<ZonaDTO>> ObtenerZonas(string codEmpresa);
        Task UpsertDiaAsync(UpdateEstimacionBisemanalRequest dto, Guid userId);
    }
}
