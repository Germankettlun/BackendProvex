using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using System.Numerics;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IDistribucionRepository
    {
        Task<List<DistribucionCategoriaEspecieRow>> GetRowsDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);


        Task<List<DistribucionCalibreEspecieRow>> GetRowsDistribucionCalibreAsync(
      string codigoEmpresa,
      string codigoEspecie,
      string codigoTemporada,
      string? idCalibre);



        Task<List<DistribucionPackingDiaDto>> GetRowsDistribucionPackingAgrupadoAsync(int idBisemanal);



     Task<List<DistribucionFrigorificoDiaDto>> GetRowsDistribucionFrigorificoAgrupadoAsync(int idBisemanal);
    }
}
