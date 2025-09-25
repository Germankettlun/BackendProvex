using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IDistribucionCategoriaEspecieRepository
    {
        Task<List<DistribucionCategoriaEspecieRow>> GetRowsAsync(
       string codigoEmpresa,
       string codigoEspecie,
       string codigoTemporada,
       string? idCategoria);
    }
}
