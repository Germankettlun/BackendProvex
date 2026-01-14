using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface ICierreService
    {
        Task<IReadOnlyList<CierreVersionDto>> GetListadoCierreVersion(string idEmpresa, string idTemporada, int? version, string? descripcion);
        Task<SpResponse> GenerarCierre(IngresarCierreRequest request, Guid userId);
    }
}
