using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using System.Numerics;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IDistribucionRepository
    {
        Task<List<DistribucionCategoriaEspecieRow>> GetRowsDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);


        Task<List<DistribucionCalibreEspecieRow>> GetRowsDistribucionCalibreAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);



        Task<List<DistribucionPackingDiaDto>> GetRowsDistribucionPackingAgrupadoAsync(int idBisemanal);



        Task<List<DistribucionFrigorificoDiaDto>> GetRowsDistribucionFrigorificoAgrupadoAsync(int idBisemanal);

        Task<List<DistribucionExportacionEstimacionRow>> GetRowsDistribucionPorcentajeExportacionAsync(int idEstimacion, int? semanasAntes, int? semanasDespues);

        Task InsertUpdateDistribucionCategoriaPredeterminadoAsync(int idEstimacion, string idCategoria, int? porcentaje, int idUsuario);

        Task InsertUpdateDistribucionCategoriaPorSemanaAsync(int idEstimacion, string idCategoria, int anio, string semana, int porcentaje, int idUsuario);

        Task InsertUpdateDistribucionCalibrePredeterminadoAsync(int idEstimacion, string idCalibre, int? porcentaje, int idUsuario);

        Task InsertUpdateDistribucionCalibrePorSemanaAsync(int idEstimacion, string idCalibre, int anio, string semana, int porcentaje, int idUsuario);

       
        Task InsertUpdateDistribucionFrigorificoAsync(DistribucionFrigorificoGuardarRequest req);
        Task InsertUpdateDistribucionPackingAsync(DistribucionPackingGuardarRequest req);

        Task InsertUpdateDistribucionPorcentajeExportacionPredeterminadoAsync(int idEstimacion, int? porcentaje, int idUsuario);

        Task InsertUpdateDistribucionPorcentajeExportacionPorSemanaAsync(int idEstimacion, int anio, string semana, int porcentaje, int idUsuario);

        Task EliminaDistribucionFrigorificoAsync(int idBisemanal, bool? replicarASemana);
        Task EliminaDistribucionPackingAsync(int idBisemanal);

    }
}
