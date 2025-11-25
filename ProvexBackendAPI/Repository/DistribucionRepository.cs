using Azure.Core;
using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using ProvexBackendAPI.Repository.IRepository;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace ProvexBackendAPI.Repository
{
    public class DistribucionRepository : IDistribucionRepository
    {
        private readonly string _connString;
        public DistribucionRepository(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")!;
        }
       

        public async Task InsertUpdateDistribucionPorcentajeExportacionPredeterminadoAsync(int idEstimacion, int? porcentaje, Guid idUsuario)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].[usp_INSERT_UPDATE_DistribucionPorcentajeExportacion_Predeterminado]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdEstimacion", idEstimacion);
            cmd.Parameters.AddWithValue("@PorcentajePredeterminado", (object?)porcentaje ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id_usuario_guid", idUsuario);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUpdateDistribucionPorcentajeExportacionPorSemanaAsync(int idEstimacion, int anio, string semana, int porcentaje, Guid idUsuario)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].[usp_INSERT_UPDATE_DistribucionPorcentajeExportacion_Semana]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdEstimacion", idEstimacion);
            cmd.Parameters.AddWithValue("@Anio", anio);
            cmd.Parameters.AddWithValue("@Semana", semana);
            cmd.Parameters.AddWithValue("@Porcentaje", porcentaje);
            cmd.Parameters.AddWithValue("@id_usuario_guid", idUsuario);

            await cmd.ExecuteNonQueryAsync();
        }

}

}
