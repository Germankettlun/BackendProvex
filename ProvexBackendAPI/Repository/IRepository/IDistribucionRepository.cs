using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using System.Numerics;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IDistribucionRepository
    {

        
      
        Task InsertUpdateDistribucionPackingAsync(DistribucionPackingGuardarRequest req, Guid idUsuario);

        Task InsertUpdateDistribucionPorcentajeExportacionPredeterminadoAsync(int idEstimacion, int? porcentaje, Guid idUsuario);

        Task InsertUpdateDistribucionPorcentajeExportacionPorSemanaAsync(int idEstimacion, int anio, string semana, int porcentaje, Guid idUsuario);

         Task EliminaDistribucionPackingAsync(int idBisemanal, bool? replicarASemana);

    }
}
