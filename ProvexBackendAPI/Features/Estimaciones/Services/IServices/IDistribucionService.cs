using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IDistribucionService
    {
        Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

        Task<List<DistribucionCalibreEspecieResponseDto>> GetDistribucionCalibreAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

     
        Task<List<DistribucionPackingDiaDto>> GetDistribucionPackingAgrupadoAsync(int idBisemanal);

        Task<List<DistribucionFrigorificoDiaDto>> GetDistribucionFrigorificoAgrupadoAsync(int idBisemanal);
        Task DistribucionCategoriaGuardarAsync(DistribucionCategoriaGuardarRequest req);

        Task DistribucionCalibreGuardarAsync(DistribucionCalibreGuardarRequest req);

        Task DistribucionFrigorificoGuardarAsync(DistribucionFrigorificoGuardarRequest req);

        Task DistribucionPackingGuardarAsync(DistribucionPackingGuardarRequest req);

    }
}
