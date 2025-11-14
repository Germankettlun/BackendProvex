using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IEstimacionService
    {
        Task IngresarEstimacion(IngresarEstimacionRequest request);
        Task IngresarPorcentajeExportacionSemanal(PorcentajeExportacionSemanalDTO input);
        Task<List<ZonaDTO>> ObtenerZonas(string codEmpresa);
    }
}
