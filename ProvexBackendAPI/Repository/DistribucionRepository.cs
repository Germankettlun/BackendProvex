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
       


        public async Task<List<DistribucionFrigorificoDiaDto>> GetRowsDistribucionFrigorificoAgrupadoAsync(int idBisemanal)
        {
            // key -> objeto por día
            var byDay = new Dictionary<string, DistribucionFrigorificoDiaDto>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_BISEMANAL_FRIGORIFICO_SEMANAANO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@ID_ESTIMACION_BISEMANAL", SqlDbType.Int) { Value = idBisemanal });

          

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                // Leer fila plana (tus mismos nombres de columnas)
                var idEstimacion = rdr.Get<string?>("IDESTIMACION") ?? "";
                var idEspecie = rdr.Get<string?>("IDESPECIE") ?? "";
                var idEstimacionBisemanal = rdr.Get<string?>("IDESTIMACIONBISEMANAL") ?? "";
                var anio = rdr.Get<int?>("BISEMANALANIO") ?? 0;
                var semana = rdr.Get<string?>("BISEMANALSEMANA") ?? "";
                var fechaDia = rdr.Get<DateTime?>("FECHADIA");
                var diaNombre = rdr.Get<string?>("DIANOMBRE") ?? "";
                var totalCajasBisemanal = rdr.Get<int?>("TOTALCAJASBISEMANAL") ?? 0;
                var idDistribFrigo = rdr.FirstExistingAsString("IDDISTRIBUCIONFIGORIFICO");
                var idFrigorifico = rdr.Get<string?>("IDFRIGORIFICO") ?? "";
                var porcentaje = rdr.Get<int?>("PORCENTAJE") ?? 0;
                var frigorificoNombre = rdr.FirstExistingAsString("FRIGORIFICO");
                var suma100 = rdr.Get<bool?>("SumaPorcentajeEs100") ?? false;

                // Key compuesta por día (incluye los campos relevantes para no repetir)
                var fechaKey = fechaDia.HasValue ? fechaDia.Value.ToString("yyyy-MM-dd") : "null";
                var key = string.Join("|", idEstimacion, idEspecie, idEstimacionBisemanal, anio, semana, fechaKey, diaNombre, totalCajasBisemanal, suma100);

                if (!byDay.TryGetValue(key, out var dia))
                {
                    dia = new DistribucionFrigorificoDiaDto
                    {
                        IdEstimacion = idEstimacion,
                        IdEspecie = idEspecie,
                        IdEstimacionBisemanal = idEstimacionBisemanal,
                        Anio = anio,
                        Semana = semana,
                        FechaDia = fechaDia,
                        DiaNombre = diaNombre,
                        TotalCajasBisemanal = totalCajasBisemanal,
                        SumaPorcentajeEs100 = suma100
                    };

                    byDay[key] = dia;
                }

                // Normaliza el ID vacío a null
                var normIdDistrib = string.IsNullOrWhiteSpace(idDistribFrigo) ? null : idDistribFrigo;

                // Agrega item de frigorífico
                dia.FrigorificoPorDia.Add(new FrigorificoItemDto
                {
                    IdDistribucionFrigorifico = normIdDistrib,
                    IdFrigorifico = idFrigorifico,
                    Nombre = frigorificoNombre,
                    Porcentaje = porcentaje
                });
            }

            // Si quieres, puedes ordenar los ítems por nombre o porcentaje:
            foreach (var d in byDay.Values)
            {
                d.FrigorificoPorDia = d.FrigorificoPorDia
                    .OrderByDescending(x => x.Porcentaje)
                    .ThenBy(x => x.Nombre)
                    .ToList();
            }

            return byDay.Values
                .OrderBy(d => d.FechaDia ?? DateTime.MinValue)
                .ThenBy(d => d.IdEstimacion)
                .ToList();
        }

        public async Task<List<DistribucionPackingDiaDto>> GetRowsDistribucionPackingAgrupadoAsync(int idBisemanal)
        {
            // key -> objeto por día
            var byDay = new Dictionary<string, DistribucionPackingDiaDto>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_BISEMANAL_PACKING_SEMANAANO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@ID_ESTIMACION_BISEMANAL", SqlDbType.Int) { Value = idBisemanal });


            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                // Leer fila plana (tus mismos nombres de columnas)
                var idEstimacion = rdr.Get<string?>("IDESTIMACION") ?? "";
                var idEspecie = rdr.Get<string?>("IDESPECIE") ?? "";
                var idEstimacionBisemanal = rdr.Get<string?>("IDESTIMACIONBISEMANAL") ?? "";
                var anio = rdr.Get<int?>("BISEMANALANIO") ?? 0;
                var semana = rdr.Get<string?>("BISEMANALSEMANA") ?? "";
                var fechaDia = rdr.Get<DateTime?>("FECHADIA");
                var diaNombre = rdr.Get<string?>("DIANOMBRE") ?? "";
                var totalCajasBisemanal = rdr.Get<int?>("TOTALCAJASBISEMANAL") ?? 0;
                var idDistribucionPacking = rdr.FirstExistingAsString("IDDISTRIBUCIONPACKING");
                var idPacking = rdr.Get<string?>("IDPACKING") ?? "";
                var porcentaje = rdr.Get<int?>("PORCENTAJE") ?? 0;
                var packingNombre = rdr.FirstExistingAsString("PACKING");
                var suma100 = rdr.Get<bool?>("SumaPorcentajeEs100") ?? false;

                // Key compuesta por día (incluye los campos relevantes para no repetir)
                var fechaKey = fechaDia.HasValue ? fechaDia.Value.ToString("yyyy-MM-dd") : "null";
                var key = string.Join("|", idEstimacion, idEspecie, idEstimacionBisemanal, anio, semana, fechaKey, diaNombre, totalCajasBisemanal, suma100);

                if (!byDay.TryGetValue(key, out var dia))
                {
                    dia = new DistribucionPackingDiaDto
                    {
                        IdEstimacion = idEstimacion,
                        IdEspecie = idEspecie,
                        IdEstimacionBisemanal = idEstimacionBisemanal,
                        Anio = anio,
                        Semana = semana,
                        FechaDia = fechaDia,
                        DiaNombre = diaNombre,
                        TotalCajasBisemanal = totalCajasBisemanal,
                        SumaPorcentajeEs100 = suma100
                    };

                    byDay[key] = dia;
                }

                // Normaliza el ID vacío a null
                var normIdDistrib = string.IsNullOrWhiteSpace(idDistribucionPacking) ? null : idDistribucionPacking;

                // Agrega item de frigorífico
                dia.PackingPorDia.Add(new PackingItemDto
                {
                    IdDistribucionPacking = normIdDistrib,
                    IdPacking = idPacking,
                    Nombre = packingNombre,
                    Porcentaje = porcentaje
                });
            }

            // Si quieres, puedes ordenar los ítems por nombre o porcentaje:
            foreach (var d in byDay.Values)
            {
                d.PackingPorDia = d.PackingPorDia
                    .OrderByDescending(x => x.Porcentaje)
                    .ThenBy(x => x.Nombre)
                    .ToList();
            }

            return byDay.Values
                .OrderBy(d => d.FechaDia ?? DateTime.MinValue)
                .ThenBy(d => d.IdEstimacion)
                .ToList();
        }

        public async Task<List<DistribucionExportacionEstimacionRow>> GetRowsDistribucionPorcentajeExportacionAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {
            var list = new List<DistribucionExportacionEstimacionRow>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_PORC_EXPORTACION", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@ID_ESTIMACION", SqlDbType.Int) { Value = idEstimacion });
            //cmd.Parameters.Add(new SqlParameter("@SEM_ANT", SqlDbType.Int) { Value = (object?)semanasAntes ?? DBNull.Value });
            //cmd.Parameters.Add(new SqlParameter("@SEM_SIG", SqlDbType.Int) { Value = (object?)semanasDespues ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new DistribucionExportacionEstimacionRow
                {
                    IdEstimacion = rdr.Get<string?>("ID_ESTIMACION") ?? "",
                    CodEspecie = rdr.Get<string?>("CODESPECIE") ?? "",
                    Especie = rdr.Get<string?>("ESPECIE") ?? "",
                    SemanaAnio = rdr.Get<int?>("SEMANAANO") ?? 0,
                    SemanaNumero = rdr.Get<string?>("SEMANANUMERO") ?? "",                   
                    PorcDefecto = rdr.Get<int?>("PORCENTAJE_POR_DEFECTO"),
                    IdDistribucionPorSemana = rdr.Get<int?>("ID_DISTRIBUCION_PORCENTAJE_EXPORTACION"),
                    PorcentajeSemana = rdr.Get<int?>("PORCENTAJE_POR_SEMANA"),
                    EsSemanaActual = rdr.Get<bool?>("ES_SEMANA_ACTUAL") ?? false
                });
            }

            return list;
        }

        public async Task InsertUpdateDistribucionCategoriaPredeterminadoAsync(int idEstimacion, string idCategoria, int? porcentaje, Guid idUsuario)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_INSERT_UPDATE_DistribucionCategoria_Predeterminado", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdEstimacion", idEstimacion);
            cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
            cmd.Parameters.AddWithValue("@PorcentajePredeterminado", (object?)porcentaje ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUpdateDistribucionCategoriaPorSemanaAsync(int idEstimacion, string idCategoria, int anio, string semana, int porcentaje, Guid idUsuario)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_INSERT_UPDATE_DistribucionCategoria_Semana", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdEstimacion", idEstimacion);
            cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);
            cmd.Parameters.AddWithValue("@Anio", anio);
            cmd.Parameters.AddWithValue("@Semana", semana);
            cmd.Parameters.AddWithValue("@Porcentaje", porcentaje);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUpdateDistribucionCalibrePredeterminadoAsync(int idEstimacion, string idCalibre, int? porcentaje, Guid idUsuario)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_INSERT_UPDATE_DistribucionCalibre_Predeterminado", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdEstimacion", idEstimacion);
            cmd.Parameters.AddWithValue("@IdCalibre", idCalibre);
            cmd.Parameters.AddWithValue("@PorcentajePredeterminado", (object?)porcentaje ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUpdateDistribucionCalibrePorSemanaAsync(int idEstimacion, string idCalibre, int anio, string semana, int porcentaje, Guid idUsuario)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_INSERT_UPDATE_DistribucionCalibre_Semana", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdEstimacion", idEstimacion);
            cmd.Parameters.AddWithValue("@IdCalibre", idCalibre);
            cmd.Parameters.AddWithValue("@Anio", anio);
            cmd.Parameters.AddWithValue("@Semana", semana);
            cmd.Parameters.AddWithValue("@Porcentaje", porcentaje);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUpdateDistribucionFrigorificoAsync(DistribucionFrigorificoGuardarRequest req, Guid usuarioId)
        {
           

            // Si no se debe replicar a toda la semana
            if (req.ReplicarASemana == null || req.ReplicarASemana == false)
            {
                using var conn = new SqlConnection(_connString);
                await conn.OpenAsync();

                foreach (var it in req.Frigorificos)
                {
                    using var cmd = new SqlCommand("[Estimaciones].[usp_INSERT_UPDATE_DistribucionFrigorifico_Dia]", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = req.IdEstimacionBisemanal });
                    cmd.Parameters.Add(new SqlParameter("@IdFrigorifico", SqlDbType.Int) { Value = it.IdFrigorifico });
                    cmd.Parameters.Add(new SqlParameter("@Porcentaje", SqlDbType.Int) { Value = (object?)it.Porcentaje ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.UniqueIdentifier) { Value = usuarioId });

                    await cmd.ExecuteNonQueryAsync();
                }

                return;
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
                        foreach (var it in req.Frigorificos)
                        {
                            using var cmd = new SqlCommand("[Estimaciones].[usp_INSERT_UPDATE_DistribucionFrigorifico_Dia]", conn)
                            {
                                CommandType = CommandType.StoredProcedure
                            };

                            cmd.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = idSemana });
                            cmd.Parameters.Add(new SqlParameter("@IdFrigorifico", SqlDbType.Int) { Value = it.IdFrigorifico });
                            cmd.Parameters.Add(new SqlParameter("@Porcentaje", SqlDbType.Int) { Value = (object?)it.Porcentaje ?? DBNull.Value });
                            cmd.Parameters.Add(new SqlParameter("@IdUsuario", SqlDbType.UniqueIdentifier) { Value = usuarioId });

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }

                
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

    

      public async Task EliminaDistribucionFrigorificoAsync(int idBisemanal, bool? replicarASemana)
        {
           if (replicarASemana == null || replicarASemana == false){
                using var conn = new SqlConnection(_connString);
                await conn.OpenAsync();


                using var cmd = new SqlCommand("[Estimaciones].[usp_delete_DistribucionFrigorifico_Dia]", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = idBisemanal });

                await cmd.ExecuteNonQueryAsync();
            }
            else { 
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
                        using var cmdDel = new SqlCommand("[Estimaciones].[usp_delete_DistribucionFrigorifico_Dia]", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmdDel.Parameters.Add(new SqlParameter("@IdBisemanal", SqlDbType.Int) { Value = id });

                        await cmdDel.ExecuteNonQueryAsync();
                    }
                }
            }

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
