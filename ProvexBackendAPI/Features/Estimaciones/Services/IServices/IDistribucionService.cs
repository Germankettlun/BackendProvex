using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCategoriaEspecieDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IDistribucionService
    {
        Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(DistribucionCategoriaEspecieRequestDto req);

        Task<List<DistribucionCalibreEspecieResponseDto>> GetDistribucionCalibreAsync(DistribucionCalibreEspecieRequestDto req);
    }
}
