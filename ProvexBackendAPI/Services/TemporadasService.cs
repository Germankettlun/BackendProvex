using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Repository.IRepository;
using System.Data;
using Microsoft.EntityFrameworkCore;
using ProvexBackendAPI.Services.IServices;
using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Services
{
    public class TemporadasService : ITemporadasService
    {

        private readonly IGenericRepository repository;

        public TemporadasService(IGenericRepository repository)
        {
            this.repository = repository;
        }
        public Task<TemporadaDto?> GetByIdAsync(string codTem)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem,string codEmp,int? vigente,string? semana = null, int? ano = null)
        {
            if (string.IsNullOrWhiteSpace(codTem))
                throw new ArgumentException("codTem requerido");

            if (string.IsNullOrWhiteSpace(codEmp))
                throw new ArgumentException("codEmp requerido");

            // Normalizar 
            var codTemNorm = codTem.Trim().ToUpperInvariant();
            var codEmpNorm = codEmp.Trim();

            // Query: join implícito por navegación
            var query = from t in repository.GetQueryable<Temporada>() from s in t.semanas where t.codTem == codTemNorm && s.codEmp == codEmpNorm select new { t, s };

            //Filtros opcionales
            if (vigente.HasValue)
            {
                query = query.Where(x =>
                    x.t.vigente != null &&  Convert.ToInt32(x.t.vigente) == vigente.Value);
            }

            if (!string.IsNullOrWhiteSpace(semana))
            {
                query = query.Where(x => x.s.semana == semana);
            }

            if (ano.HasValue)
            {
                query = query.Where(x => x.s.anio == ano.Value);
            }

            //DTO
            var result = await query
                .OrderBy(x => x.t.orden)
                .Select(x => new SemanaDto
                {
                    CodTem = x.t.codTem,
                    TemporadaDesc = x.t.descripcion,
                    TempInicio = x.t.fechaIni,
                    TempOrden = x.t.orden,
                    TempVigente = string.IsNullOrWhiteSpace(x.t.vigente) ? 0 : Convert.ToInt32(x.t.vigente),
                    Semana = x.s.semana,
                    Ano = x.s.anio,
                    SemanaInicio = x.s.inicio,
                    SemanaTermino = x.s.termino
                })
                .ToListAsync();

            return result;
        }
        public async Task<List<TemporadaDto>> ListAsync(string? codTem, string codEmp, int? vigente)
        {
            if (string.IsNullOrWhiteSpace(codEmp))
                throw new ArgumentException("codEmp es requerido.", nameof(codEmp));

            // Normalizar filtros
            var codEmpNorm = codEmp.Trim();
            var codTemNorm = string.IsNullOrWhiteSpace(codTem) ? null : codTem.Trim().ToUpperInvariant();

            //Consulta
            var query = repository.GetQueryable<Temporada>().AsNoTracking().Where(t => t.codEmp == codEmpNorm);

            //Filtros opcionales
            if (codTemNorm is not null)
            {
                query = query.Where(t => t.codTem == codTemNorm);
            }

            if (vigente.HasValue)
            {
                query = query.Where(t =>!string.IsNullOrWhiteSpace(t.vigente) && Convert.ToInt32(t.vigente) == vigente.Value);
            }


            var result = await query.OrderBy(t => t.orden)
                .Select(t => new TemporadaDto
                {
                 CodTem = t.codTem,
                 Descripcion = t.descripcion,
                 FechaIni = t.fechaIni,
                 Orden = t.orden,
                 Vigente = string.IsNullOrWhiteSpace(t.vigente) ? 0 : Convert.ToInt32(t.vigente)  }).ToListAsync();

            return result;
        }

        public async Task<IReadOnlyList<SemanasDto.SemanaVigenteRow>> ListSemanaAsync(string codigoEmpresa, string codigoTemporada, bool? soloVigente = null)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                throw new ArgumentException("codigoEmpresa es obligatorio.", nameof(codigoEmpresa));

            var parameters = new[]
            {
                new SqlParameter("@CodigoEmpresa", codigoEmpresa.Trim().ToUpperInvariant()),
                new SqlParameter("@CodigoTemporada", string.IsNullOrWhiteSpace(codigoTemporada) ? DBNull.Value : codigoTemporada.Trim().ToUpperInvariant()),
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
