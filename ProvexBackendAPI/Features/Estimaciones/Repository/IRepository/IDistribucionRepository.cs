using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IDistribucionRepository
    {
        Task<List<DistribucionCategoriaEspecieRow>> GetRowsDistribucionCategoriaAsync(
       string codigoEmpresa,
       string codigoEspecie,
       string codigoTemporada,
       string? idCategoria);


        Task<List<DistribucionCalibreEspecieRow>> GetRowsDistribucionCalibreAsync(
      string codigoEmpresa,
      string codigoEspecie,
      string codigoTemporada,
      string? idCalibre);

     Task<List<DistribucionPackingDto>> GetRowsDistribucionPackingAsync(DistribucionPackingQueryDto q);
    }
}
