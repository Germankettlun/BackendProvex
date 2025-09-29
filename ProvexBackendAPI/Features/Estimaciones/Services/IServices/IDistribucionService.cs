using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IDistribucionService
    {
        Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(DistribucionCategoriaEspecieRequestDto req);

        Task<List<DistribucionCalibreEspecieResponseDto>> GetDistribucionCalibreAsync(DistribucionCalibreEspecieRequestDto req);

        Task<List<DistribucionPackingDto>> GetDistribucionPackingAsync(DistribucionPackingQueryDto req);

        Task<List<DistribucionFrigorificoDto>> GetDistribucionFrigorificoAsync(DistribucionPackingQueryDto req);
    }
}
