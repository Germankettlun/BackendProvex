using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCategoriaEspecieDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IDistribucionCategoriaEspecieService
    {
        Task<List<DistribucionCategoriaEspecieResponseDto>> GetAsync(DistribucionCategoriaEspecieRequestDto req);
    }
}
