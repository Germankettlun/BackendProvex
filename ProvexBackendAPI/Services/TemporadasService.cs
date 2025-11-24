using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
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

        public async Task<IReadOnlyList<SemanasDto.SemanaVigenteRow>> ListSemanaAsync(string codigoEmpresa,string? codigoTemporada = null,bool? soloVigente = null)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                throw new ArgumentException("codigoEmpresa es obligatorio.", nameof(codigoEmpresa));

            // Normalizar 
            var codEmpNorm = codigoEmpresa.Trim().ToUpperInvariant();
            var codTempNorm = string.IsNullOrWhiteSpace(codigoTemporada) ? null : codigoTemporada.Trim().ToUpperInvariant();

            // Query
            var query = repository.GetQueryable<Semana>().AsNoTracking().Where(s => s.codEmp == codEmpNorm);

            // Filtro opcional por temporada
            if (codTempNorm is not null)
            {
                query = query.Where(s => s.codTem == codTempNorm);
            }

            // Filtro opcional SOLOVIGENTE
            if (soloVigente.HasValue && soloVigente.Value)
            {
                var ahora = DateTime.Now;

                query = query.Where(s =>
                    s.inicio <= ahora &&
                    s.termino >= ahora);
            }

            var result = await query
                .Select(s => new SemanasDto.SemanaVigenteRow
                {
                    CodigoEmpresa = s.codEmp,
                    CodigoTemporada = s.codTem,
                    SemanaBase = s.semana,
                    AnioBase = s.anio,
                    Inicio = s.inicio,
                    Termino = s.termino
                })
                .ToListAsync();

            return result;
        }

        public async Task<SemanasDto.SemanaVigenteRow?> GetSemanaAsync(string codigoEmpresa, string? codigoTemporada = null,bool? soloVigente = null)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                throw new ArgumentException("codigoEmpresa es obligatorio.", nameof(codigoEmpresa));

            // Normalizar 
            var codEmpNorm = codigoEmpresa.Trim().ToUpperInvariant();
            var codTempNorm = string.IsNullOrWhiteSpace(codigoTemporada) ? null : codigoTemporada.Trim().ToUpperInvariant();

            // Query Semanas
            var query = repository.GetQueryable<Semana>().AsNoTracking().Where(s => s.codEmp == codEmpNorm);

            // Filtro opcional temporada
            if (codTempNorm is not null)
            {
                query = query.Where(s => s.codTem == codTempNorm);
            }

            // Filtro opcional SOLOVIGENTE
            if (soloVigente.HasValue && soloVigente.Value)
            {
                var ahora = DateTime.Now;

                query = query.Where(s =>
                    s.inicio <= ahora &&
                    s.termino >= ahora);
            }

            var semana = await query.FirstOrDefaultAsync();

            if (semana is null)
                return null;

            return new SemanasDto.SemanaVigenteRow
            {
                CodigoEmpresa = semana.codEmp,
                CodigoTemporada = semana.codTem,
                SemanaBase = semana.semana,
                AnioBase = semana.anio,
                Inicio = semana.inicio,
                Termino = semana.termino
            };
        }



    }
}
