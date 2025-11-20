using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Repository.IRepository;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas.SemanasDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProvexBackendAPI.Features.Estimaciones.Services
{
    public class TemporadasService : ITemporadasService
    {
        private readonly ITemporadasRepository temporadasRepository;
        private readonly IGenericRepository repository;

        public TemporadasService(IGenericRepository repository, ITemporadasRepository temporadasRepository)
        {
            this.repository = repository;
            this.temporadasRepository = temporadasRepository;
        }
        public Task<TemporadaDto?> GetByIdAsync(string codTem)
        {
            throw new NotImplementedException();
        }

       public Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem, string codEmp, int? vigente, string? semana = null, int? ano = null)
            => temporadasRepository.GetSemanasTemporadaAsync(codTem, codEmp, vigente, semana, ano);

        public Task<List<TemporadaDto>> ListAsync(string? codTem, string codEmp, int? vigente)
            => temporadasRepository.ListAsync(codTem, codEmp, vigente);

        public async Task<IReadOnlyList<SemanasDto.SemanaVigenteRow>> ListSemanaAsync(string codigoEmpresa, string codigoTemporada, bool? soloVigente = null)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                throw new ArgumentException("codigoEmpresa es obligatorio.", nameof(codigoEmpresa));

            var parameters = new[]
            {
                new SqlParameter("@CodigoEmpresa", codigoEmpresa.Trim().ToUpperInvariant()),
                new SqlParameter("@CodigoTemporada", string.IsNullOrWhiteSpace(codigoTemporada) ? (object)DBNull.Value : codigoTemporada.Trim().ToUpperInvariant()),
                new SqlParameter("@SoloVigente", (object?)soloVigente ?? DBNull.Value),
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_SEMANA_VIGENTE",parameters);

            var result = new List<SemanasDto.SemanaVigenteRow>(dataTable.Rows.Count);

            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(MapSemanaVigenteRow(row));
            }

            return result;
        }

        public async Task<SemanasDto.SemanaVigenteRow?> GetSemanaAsync(string codigoEmpresa, string? codigoTemporada = null, bool? soloVigente = null)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                throw new ArgumentException("codigoEmpresa es obligatorio.", nameof(codigoEmpresa));


            var parameters = new SqlParameter[]
               {
                    new SqlParameter("@CodigoEmpresa", codigoEmpresa.Trim().ToUpperInvariant()),
                    new SqlParameter("@CodigoTemporada", (object?)codigoTemporada.Trim().ToUpperInvariant() ?? DBNull.Value),
                    new SqlParameter("@SoloVigente", (object?)soloVigente ?? DBNull.Value),
               };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_SEMANA_VIGENTE", parameters);

            if (dataTable.Rows.Count == 0)
                return null; 

            var row = dataTable.Rows[0];

            var result = new SemanasDto.SemanaVigenteRow
            {
                CodigoEmpresa = row.IsNull("CODEMP") ? null : row["CODEMP"]?.ToString(),

                CodigoTemporada = row.IsNull("CODTEMP") ? null : row["CODTEMP"]?.ToString(),

                SemanaBase = row.IsNull("SEMANA") ? null: row["SEMANA"]?.ToString(),

                AnioBase = row["ANIO"] == DBNull.Value ? 0 : Convert.ToInt32(row["ANIO"]),

                Inicio = row["INICIO"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["INICIO"]),

                Termino = row["TERMINO"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["TERMINO"])
            };

            return result;

        }

        //MAPPER LOCAL SEMANA

        static SemanasDto.SemanaVigenteRow MapSemanaVigenteRow(DataRow row)
        {
            return new SemanasDto.SemanaVigenteRow
            {
                CodigoEmpresa = row.IsNull("CODEMP")? null : row["CODEMP"]?.ToString(),

                CodigoTemporada = row.IsNull("CODTEMP")? null: row["CODTEMP"]?.ToString(),

                SemanaBase = row.IsNull("SEMANA")? null : row["SEMANA"]?.ToString(),

                AnioBase = row["ANIO"] == DBNull.Value? 0 : Convert.ToInt32(row["ANIO"]),

                Inicio = row["INICIO"] == DBNull.Value? DateTime.MinValue : Convert.ToDateTime(row["INICIO"]),

                Termino = row["TERMINO"] == DBNull.Value? DateTime.MinValue : Convert.ToDateTime(row["TERMINO"])
            };
        }
    }
}
