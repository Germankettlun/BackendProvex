using ProvexBackendAPI.Dto;
using static ProvexBackendAPI.Dto.EstimacionesDto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IEstimacionService
    {
        Task<SpResponse> IngresarEstimacion(IngresarEstimacionRequest request, Guid userId);
        Task<SpResponse> IngresarPorcentajeExportacionSemanal(PorcentajeExportacionSemanalDTO input, Guid userId);
        Task<List<ZonaDTO>> ObtenerZonas(string codEmpresa);

        Task<EstimacionDto> ObtenerEstimacion(int idEstimacion);
        Task UpsertDiaAsync(UpdateEstimacionBisemanalRequest dto, Guid userId);

        Task<EstructuraDistribucionDto> GetEstimacionBisemanalAsync(EstimacionBisemanalQueryDto req);

        Task<List<ResumenSemanalEstimacionDto>> GetResumenSemanalAsync(int idEstimacion);

        Task<DetalleDistribucionesEstimacionDto> GetDetalleDistribucionesAsync(int idEstimacion);


    }
}
