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
        public async Task<List<DistribucionCategoriaEspecieRow>> GetRowsDistribucionCategoriaAsync(string codigoEmpresa, string codigoEspecie, string codigoTemporada, string? idCategoria)
        {
            var list = new List<DistribucionCategoriaEspecieRow>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_CATEGORIA_ESPECIE", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@CODIGOEMPRESA", SqlDbType.VarChar, 10) { Value = codigoEmpresa });
            cmd.Parameters.Add(new SqlParameter("@CODIGOESPECIE", SqlDbType.VarChar, 10) { Value = codigoEspecie });
            cmd.Parameters.Add(new SqlParameter("@CODIGOTEMPORADA", SqlDbType.VarChar, 10) { Value = codigoTemporada });
            cmd.Parameters.Add(new SqlParameter("@ID_CATEGORIA", SqlDbType.VarChar, 10) { Value = (object?)idCategoria ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new DistribucionCategoriaEspecieRow
                {
                    IdEstimacion = rdr.Get<string?>("ID_ESTIMACION") ?? "",
                    IdCategoria = rdr.Get<string?>("IDCATEGORIA") ?? "",
                    CategoriaNombre = rdr.FirstExistingAsString("CATEGORIA"), // fallback a "" si no existe o es null
                    PorcDefectoCategoria = rdr.Get<int?>("PORCENTAJEPORDEFECTOCATEGORIA"),
                    SemanaAnio = rdr.Get<int?>("SEMANAANO") ?? 0,
                    SemanaNumero = rdr.Get<string?>("SEMANANUMERO") ?? "",
                    PorcentajeSemana = rdr.Get<int?>("PORCENTAJEPORSEMANA"),
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

        public async Task<List<DistribucionFrigorificoDto>> GetRowsDistribucionFrigorificoAsync(DistribucionPackingQueryDto q)
        {
            var list = new List<DistribucionFrigorificoDto>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_BISEMANAL_FRIGORIFICO_SEMANAANO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@CODIGOEMPRESA", SqlDbType.VarChar, 10) { Value = q.CodigoEmpresa });
            cmd.Parameters.Add(new SqlParameter("@CODIGOESPECIE", SqlDbType.VarChar, 10) { Value = q.CodigoEspecie });
            cmd.Parameters.Add(new SqlParameter("@CODIGOTEMPORADA", SqlDbType.VarChar, 10) { Value = q.CodigoTemporada });
            cmd.Parameters.Add(new SqlParameter("@ANIO", SqlDbType.Int) { Value = (object?)q.Anio ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@SEMANA", SqlDbType.VarChar,10) { Value = (object?)q.Semana ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@FECHADIA", SqlDbType.DateTime) { Value = (object?)q.FechaDia ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new DistribucionFrigorificoDto
                {
                    IdEstimacion = rdr.Get<string?>("IDESTIMACION") ?? "",
                    IdEspecie = rdr.Get<string?>("IDESPECIE") ?? "",
                    IdEstimacionBisemanal = rdr.Get<string?>("IDESTIMACIONBISEMANAL") ?? "",
                    Anio = rdr.Get<int?>("BISEMANALANIO") ?? 0,
                    Semana = rdr.Get<string?>("BISEMANALSEMANA") ?? "",
                    FechaDia = rdr.Get<DateTime?>("FECHADIA"),
                    DiaNombre = rdr.Get<string?>("DIANOMBRE") ?? "",
                    TotalCajasBisemanal = rdr.Get<int?>("TOTALCAJASBISEMANAL") ?? 0,
                    IdDistribucionFrigorifico = rdr.FirstExistingAsString("IDDISTRUBUCIONFRIGORIFICO"),
                    IdFrigorifico = rdr.Get<string?>("IDFRIGORIFICO") ?? "",
                    Porcentaje = rdr.Get<int?>("PORCENTAJE") ?? 0,
                    FrigorificoNombre = rdr.FirstExistingAsString("FRIGORIFICO"),
                    SumaPorcentajeEs100 = rdr.Get<bool?>("SumaPorcentajeEs100") ?? false
                });
            }

            return list;
        }


        public async Task<List<DistribucionPackingDto>> GetRowsDistribucionPackingAsync(DistribucionPackingQueryDto q)
        {
            var list = new List<DistribucionPackingDto>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_DISTRIBUCION_BISEMANAL_PACKING_SEMANAANO", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@CODIGOEMPRESA", SqlDbType.VarChar, 10) { Value = q.CodigoEmpresa });
            cmd.Parameters.Add(new SqlParameter("@CODIGOESPECIE", SqlDbType.VarChar, 10) { Value = q.CodigoEspecie });
            cmd.Parameters.Add(new SqlParameter("@CODIGOTEMPORADA", SqlDbType.VarChar, 10) { Value = q.CodigoTemporada });
            cmd.Parameters.Add(new SqlParameter("@ANIO", SqlDbType.Int) { Value = (object?)q.Anio ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@SEMANA", SqlDbType.VarChar, 10) { Value = (object?)q.Semana ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@FECHADIA", SqlDbType.DateTime) { Value = (object?)q.FechaDia ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new DistribucionPackingDto
                {
                    IdEstimacion = rdr.Get<string?>("IDESTIMACION") ?? "",
                    IdEspecie = rdr.Get<string?>("IDESPECIE") ?? "",
                    IdEstimacionBisemanal = rdr.Get<string?>("IDESTIMACIONBISEMANAL") ?? "",
                    Anio = rdr.Get<int?>("BISEMANALANIO") ?? 0,
                    Semana = rdr.Get<string?>("BISEMANALSEMANA") ?? "",
                    FechaDia = rdr.Get<DateTime?>("FECHADIA"),
                    DiaNombre = rdr.Get<string?>("DIANOMBRE") ?? "",
                    TotalCajasBisemanal = rdr.Get<int?>("TOTALCAJASBISEMANAL") ?? 0,
                    // tolerante al typo
                    IdDistribucionPacking = rdr.FirstExistingAsString("IDDISTRUBUCIONPACKING", "IDDISTRIBUCIONPACKING"),
                    IdPacking = rdr.Get<string?>("IDPACKING") ?? "",
                    Porcentaje = rdr.Get<int?>("PORCENTAJE") ?? 0,
                    PackingNombre = rdr.FirstExistingAsString("PACKING"),
                    SumaPorcentajeEs100 = rdr.Get<bool?>("SumaPorcentajeEs100") ?? false
                });
            }

            return list;
        }
    }
}
