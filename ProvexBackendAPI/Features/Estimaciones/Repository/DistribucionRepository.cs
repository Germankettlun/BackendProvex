using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using System.Data;
using ProvexBackendAPI.Helpers.Shared.Extensions;

namespace ProvexBackendAPI.Features.Estimaciones.Repository
{
    public class DistribucionRepository : IDistribucionRepository
    {
        private readonly string _connString;
        public DistribucionRepository(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")!;
        }
        public async Task<List<DistribucionCategoriaEspecieRow>> GetRowsDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {
            var list = new List<DistribucionCategoriaEspecieRow>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_CATEGORIA_ESPECIE", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@ID_ESTIMACION", SqlDbType.Int) { Value = idEstimacion });
            //cmd.Parameters.Add(new SqlParameter("@SEM_ANT", SqlDbType.Int) { Value = (object?)semanasAntes ?? DBNull.Value });
            //cmd.Parameters.Add(new SqlParameter("@SEM_SIG", SqlDbType.Int) { Value = (object?)semanasDespues ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new DistribucionCategoriaEspecieRow
                {
                    IdEstimacion = rdr.Get<string?>("ID_ESTIMACION") ?? "",
                    CodEspecie = rdr.Get<string?>("CODESPECIE") ?? "",
                    Especie = rdr.Get<string?>("ESPECIE") ?? "",
                    IdCategoria = rdr.Get<string?>("IDCATEGORIA") ?? "",
                    CategoriaNombre = rdr.FirstExistingAsString("CATEGORIA"), 
                    SemanaAnio = rdr.Get<int?>("SEMANAANO") ?? 0,
                    SemanaNumero = rdr.Get<string?>("SEMANANUMERO") ?? "",
                    IdDistribucionDefecto = rdr.Get<int?>("IDDISTRIBUCIONDEFECTO"),
                    PorcDefectoCategoria = rdr.Get<int?>("PORCENTAJE_POR_DEFECTO_CATEGORIA"),
                    IdDistribucionPorSemana = rdr.Get<int?>("DISTRIBUCIONPORSEMANAID"),
                    PorcentajeSemana = rdr.Get<int?>("PORCENTAJE_POR_SEMANA_CATEGORIA"),
                   
                    EsSemanaActual = rdr.Get<bool?>("ES_SEMANA_ACTUAL") ?? false   
                });
            }

            return list;
        }

        public async Task<List<DistribucionCalibreEspecieRow>> GetRowsDistribucionCalibreAsync(string codigoEmpresa, string codigoEspecie, string codigoTemporada, string? idCalibre)
        {
            var list = new List<DistribucionCalibreEspecieRow>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_CALIBRE_ESPECIE", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@CODIGOEMPRESA", SqlDbType.VarChar, 10) { Value = codigoEmpresa });
            cmd.Parameters.Add(new SqlParameter("@CODIGOESPECIE", SqlDbType.VarChar, 10) { Value = codigoEspecie });
            cmd.Parameters.Add(new SqlParameter("@CODIGOTEMPORADA", SqlDbType.VarChar, 10) { Value = codigoTemporada });
            cmd.Parameters.Add(new SqlParameter("@ID_CALIBRE", SqlDbType.VarChar, 10) { Value = (object?)idCalibre ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new DistribucionCalibreEspecieRow
                {
                    IdEstimacion = rdr.Get<string?>("ID_ESTIMACION") ?? "",
                    IdCalibre = rdr.Get<string?>("IDCALIBRE") ?? "",
                    CalibreNombre = rdr.FirstExistingAsString("CALIBRE"), // fallback a "" si no existe o es null
                    PorcDefectoCalibre = rdr.Get<int?>("PORCENTAJEPORDEFECTOCALIBRE"),
                    SemanaAnio = rdr.Get<int?>("SEMANAANO") ?? 0,
                    SemanaNumero = rdr.Get<string?>("SEMANANUMERO") ?? "",
                    PorcentajeSemana = rdr.Get<int?>("PORCENTAJEPORSEMANA"),
                    EsSemanaActual = rdr.Get<bool?>("ES_SEMANA_ACTUAL") ?? false
                });
            }

            return list;
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



      
    }
}
