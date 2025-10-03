using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Semanas;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using System.Data;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Semanas.SemanasDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository
{
    public class SemanaVigenteProvider : ISemanaVigenteProvider
    {
        private readonly string _connString;
        public SemanaVigenteProvider(IConfiguration cfg) => _connString = cfg.GetConnectionString("DefaultConnection")!;
        public async Task<SemanasDto.SemanaVigenteRow?> GetAsync(string codigoEmpresa, string? codigoTemporada = null, bool? soloVigente = true)
        {
            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_SEMANA_VIGENTE", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Ajusta nombres/params si tu SPU los usa
            cmd.Parameters.Add(new SqlParameter("@CodigoEmpresa", SqlDbType.NVarChar, 10) { Value = codigoEmpresa });
            cmd.Parameters.Add(new SqlParameter("@CodigoTemporada", SqlDbType.NVarChar, 10) { Value = (object?)codigoTemporada ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@SoloVigente", SqlDbType.Bit) { Value = (object?)soloVigente ?? DBNull.Value });

            await using var rdr = await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync()) return null;

            return new SemanaVigenteRow
            {
                CodigoEmpresa = rdr.Get<string?>("CODEMP"),
                CodigoTemporada = rdr.Get<string?>("CODTEMP"),
                SemanaBase = rdr.FirstExistingAsString("SEMANA"),
                AnioBase = rdr.Get<int>("ANIO"),
                Inicio = rdr.Get<DateTime>("INICIO"),
                Termino = rdr.Get<DateTime>("TERMINO")
            };
        }
    }
}
