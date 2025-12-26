using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface ICierreService
    {
        Task<IReadOnlyList<CierreVersionDto>> GetListadoCierreVersion(string idEmpresa, string idTemporada, string? idEspecie, string? descripcion);
        Task<SpResponse> GenerarCierre(IngresarCierreRequest request, Guid userId);
    }
}
