using ProvexBackendAPI.Dto;
using static ProvexBackendAPI.Dto.EstimacionesDto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IEstimacionService
    {
        Task IngresarEstimacion(IngresarEstimacionRequest request, Guid userId);
        Task IngresarPorcentajeExportacionSemanal(PorcentajeExportacionSemanalDTO input, Guid userId);
        Task<List<ZonaDTO>> ObtenerZonas(string codEmpresa);
        Task UpsertDiaAsync(UpdateEstimacionBisemanalRequest dto, Guid userId);

        Task<EstructuraDistribucionDto> GetEstimacionBisemanalAsync(
         EstimacionBisemanalQueryDto req);

        Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion);


    }
}
