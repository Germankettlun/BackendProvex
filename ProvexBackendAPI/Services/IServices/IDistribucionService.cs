using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IDistribucionService
    {
        Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

        Task<List<DistribucionCalibreEspecieResponseDto>> GetDistribucionCalibreAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

     
        Task<List<DistribucionPackingDiaDto>> GetDistribucionPackingAgrupadoAsync(int idBisemanal);

        Task<List<DistribucionFrigorificoDiaDto>> GetDistribucionFrigorificoAgrupadoAsync(int idBisemanal);

        Task<List<DistribucionExportacionEstimacionResponseDto>> GetRowsDistribucionPorcentajeExportacionAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

        Task<SpResponse> DistribucionCategoriaGuardarAsync(DistribucionCategoriaGuardarRequest req, Guid usuarioId);

        Task<SpResponse> DistribucionCalibreGuardarAsync(DistribucionCalibreGuardarRequest req, Guid usuarioId);

        Task<SpResponse> DistribucionFrigorificoGuardarAsync(DistribucionFrigorificoGuardarRequest req, Guid usuarioId);

        Task<SpResponse> DistribucionPackingGuardarAsync(DistribucionPackingGuardarRequest req, Guid usuarioId);

        Task DistribucionPorcentajeExportacionGuardarAsync(DistribucionPorcentajeExportacionGuardarRequest req, Guid userId);

    }
}
