using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using System.Numerics;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IDistribucionRepository
    {

        Task<List<DistribucionCalibreEspecieRow>> GetRowsDistribucionCalibreAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);



        Task<List<DistribucionPackingDiaDto>> GetRowsDistribucionPackingAgrupadoAsync(int idBisemanal);



        Task<List<DistribucionFrigorificoDiaDto>> GetRowsDistribucionFrigorificoAgrupadoAsync(int idBisemanal);

        Task<List<DistribucionExportacionEstimacionRow>> GetRowsDistribucionPorcentajeExportacionAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

        Task InsertUpdateDistribucionCategoriaPredeterminadoAsync(int idEstimacion, string idCategoria, int? porcentaje, Guid idUsuario);

        Task InsertUpdateDistribucionCategoriaPorSemanaAsync(int idEstimacion, string idCategoria, int anio, string semana, int porcentaje, Guid idUsuario);

        Task InsertUpdateDistribucionCalibrePredeterminadoAsync(int idEstimacion, string idCalibre, int? porcentaje, Guid idUsuario);

        Task InsertUpdateDistribucionCalibrePorSemanaAsync(int idEstimacion, string idCalibre, int anio, string semana, int porcentaje, Guid idUsuario);

       
        Task InsertUpdateDistribucionFrigorificoAsync(DistribucionFrigorificoGuardarRequest req, Guid idUsuario);
        Task InsertUpdateDistribucionPackingAsync(DistribucionPackingGuardarRequest req, Guid idUsuario);

        Task InsertUpdateDistribucionPorcentajeExportacionPredeterminadoAsync(int idEstimacion, int? porcentaje, Guid idUsuario);

        Task InsertUpdateDistribucionPorcentajeExportacionPorSemanaAsync(int idEstimacion, int anio, string semana, int porcentaje, Guid idUsuario);

        Task EliminaDistribucionFrigorificoAsync(int idBisemanal, bool? replicarASemana);
        Task EliminaDistribucionPackingAsync(int idBisemanal, bool? replicarASemana);

    }
}
