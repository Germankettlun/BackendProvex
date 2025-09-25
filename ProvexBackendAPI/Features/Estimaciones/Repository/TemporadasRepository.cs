using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Sql.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using System.Data;
using System.Runtime.Intrinsics.Arm;

namespace ProvexBackendAPI.Features.Estimaciones.Repository
{
    public class TemporadasRepository : ITemporadasRepository
    {

        private readonly string _connString;
        public TemporadasRepository(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }
        public Task<TemporadaDto?> GetByIdAsync(string codTem)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem, string codEmp, int? vigente)
        {
            var list = new List<SemanaDto>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();


            await using var cmd = new SqlCommand("USP_UI_SEMANAS_TEMPORADA", conn)
            {
                CommandType = CommandType.StoredProcedure
            };


            cmd.Parameters.Add(new SqlParameter("@COD_TEM", SqlDbType.NVarChar, 50) { Value = codTem });
            cmd.Parameters.Add(new SqlParameter("@COD_EMP", SqlDbType.NVarChar, 50) { Value = codEmp });
            cmd.Parameters.Add(new SqlParameter("@VIGENTE", SqlDbType.Int) { Value = (object?)vigente ?? DBNull.Value });

            await using var rd = await cmd.ExecuteReaderAsync();


            while (await rd.ReadAsync())
            {
                list.Add(new SemanaDto
                {
                    CodTem = rd.Get<string>("COD_TEM")!,
                    TemporadaDesc = rd.Get<string>("TEMPORADADESC")!,
                    TempInicio = rd.Get<DateTime>("TEMPINICIO"),
                    TempOrden = rd.Get<int>("TEMPORDEN"),
                    TempVigente = rd.Get<int>("TEMPVIGENTE"),
                    Semana = rd.Get<int>("SEMANA"),
                    Ano = rd.Get<int>("ANO"),
                    SemanaInicio = rd.Get<DateTime>("SEMANAINICIO"),
                    SemanaTermino = rd.Get<DateTime>("SEMANATERMINO")
                });
            }
            return list;


        }

        public Task<List<TemporadaDto>> ListAsync(string codEmp, int? vigente)
        {
            throw new NotImplementedException();
        }
    }
}
