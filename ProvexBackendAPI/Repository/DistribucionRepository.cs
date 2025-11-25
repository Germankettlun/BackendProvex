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
       




        public async Task InsertUpdateDistribucionPackingAsync(DistribucionPackingGuardarRequest req, Guid usuarioId)
        {
            // Si no se debe replicar a toda la semana
            if (req.ReplicarASemana == null || req.ReplicarASemana == false)
            {
                using var conn = new SqlConnection(_connString);
                await conn.OpenAsync();

                // Reutilizamos el SqlCommand para cada item
                foreach (var it in req.Packings)
                {
                    using var cmd = new SqlCommand("[Estimaciones].[usp_INSERT_UPDATE_DistribucionPacking_Dia]", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = req.IdEstimacionBisemanal });
                    cmd.Parameters.Add(new SqlParameter("@IdPacking", SqlDbType.Int) { Value = it.IdPacking });
                    cmd.Parameters.Add(new SqlParameter("@Porcentaje", SqlDbType.Int) { Value = (object?)it.Porcentaje ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.UniqueIdentifier) { Value = usuarioId });


                    await cmd.ExecuteNonQueryAsync();
                }
            }
            else
            {
                //Replicar a toda la semana
                using (var conn = new SqlConnection(_connString))
                {
                    await conn.OpenAsync();

                    //Traer todos los IdBisemanal de la semana
                    var idsSemana = new List<int>();

                    using (var cmdIds = new SqlCommand("[Estimaciones].[usp_IDS_BISEMANAL_DE_LA_SEMANA]", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    })
                    {
                        cmdIds.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = req.IdEstimacionBisemanal });

                        using var rdr = await cmdIds.ExecuteReaderAsync();
                        int ord = rdr.GetOrdinal("IdBisemanal");
                        while (await rdr.ReadAsync())
                        {
                            if (!rdr.IsDBNull(ord))
                            {
                                int id = rdr.GetInt32(ord);
                                if (id > 0) idsSemana.Add(id);
                            }
                        }
                    }

                    // Asegurar incluir el id base por si el SP no lo devuelve
                    if (!idsSemana.Contains(req.IdEstimacionBisemanal))
                        idsSemana.Add(req.IdEstimacionBisemanal);

                    //Ejecutar el insert/update para cada IdBisemanal encontrado
                    foreach (var idSemana in idsSemana)
                    {
                        foreach (var it in req.Packings)
                        {
                            using var cmd = new SqlCommand("[Estimaciones].[usp_INSERT_UPDATE_DistribucionPacking_Dia]", conn)
                            {
                                CommandType = CommandType.StoredProcedure
                            };

                            cmd.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = idSemana });
                            cmd.Parameters.Add(new SqlParameter("@IdPacking", SqlDbType.Int) { Value = it.IdPacking });
                            cmd.Parameters.Add(new SqlParameter("@Porcentaje", SqlDbType.Int) { Value = (object?)it.Porcentaje ?? DBNull.Value });
                            cmd.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.UniqueIdentifier) { Value = usuarioId });

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
               
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

    


        public async Task EliminaDistribucionPackingAsync(int idBisemanal, bool? replicarASemana)
        {
            if (replicarASemana == null || replicarASemana == false)
            {
                using var conn = new SqlConnection(_connString);
                await conn.OpenAsync();


                using var cmd = new SqlCommand("[Estimaciones].[usp_delete_DistribucionPacking_Dia]", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = idBisemanal });

                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                // replicar a toda la semana: primero obtengo todos los IdBisemanal de esa semana
                using (var conn = new SqlConnection(_connString))
                {
                    await conn.OpenAsync();

                    // Traigo los IDs de la semana del idBisemanal base
                    var idsSemana = new List<int>();
                    using (var cmdIds = new SqlCommand("[Estimaciones].[usp_IDS_BISEMANAL_DE_LA_SEMANA]", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    })
                    {
                        cmdIds.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = idBisemanal });

                        using var rdr = await cmdIds.ExecuteReaderAsync();
                        int ord = rdr.GetOrdinal("IdBisemanal");
                        while (await rdr.ReadAsync())
                        {
                            if (!rdr.IsDBNull(ord))
                            {
                                int id = rdr.GetInt32(ord);
                                if (id > 0) idsSemana.Add(id);
                            }
                        }




                    }

                    // Borrar por cada IdBisemanal de esa semana
                    foreach (var id in idsSemana)
                    {
                        using var cmdDel = new SqlCommand("[Estimaciones].[usp_delete_DistribucionPacking_Dia]", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmdDel.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = id });

                        await cmdDel.ExecuteNonQueryAsync();
                    }
                }
            }
                


        }

    }

}
