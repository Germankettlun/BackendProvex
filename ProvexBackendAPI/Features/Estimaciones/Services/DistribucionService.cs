using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Helpers.Validation;
using System.ComponentModel.DataAnnotations;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Services
{
    public class DistribucionService : IDistribucionService
    {
        private readonly IDistribucionRepository _repo;

        public DistribucionService(IDistribucionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(DistribucionCategoriaEspecieRequestDto req)
        {
            var rows = await _repo.GetRowsDistribucionCategoriaAsync(
           req.CodigoEmpresa,
           req.CodigoEspecie,
           req.CodigoTemporada,
           req.IdCategoria
       );

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = rows
                .GroupBy(r => new { r.IdEstimacion, r.IdCategoria, r.CategoriaNombre, r.PorcDefectoCategoria })
                .Select(g => new DistribucionCategoriaEspecieResponseDto
                {
                    IdEstimacion = g.Key.IdEstimacion,
                    CategoriaId = g.Key.IdCategoria,
                    CategoriaNombre = g.Key.CategoriaNombre,
                    Predeterminado = g.Key.PorcDefectoCategoria,
                    Semanas = g
                        .Select(r => new SemanaPorcentajeDto
                        {
                            Anio = r.SemanaAnio,
                            Semana = r.SemanaNumero,
                            Porcentaje = r.PorcentajeSemana,
                            EsSemanaActual = r.EsSemanaActual
                        })
                        .DistinctBy(x => new { x.Anio, x.Semana }) // por si viniera repetido
                        .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                        .ToList()
                })
                .OrderBy(x => x.IdEstimacion)
                .ThenBy(x => x.CategoriaId)
                .ToList();

            return grouped;
        }


        public async Task<List<DistribucionCalibreEspecieResponseDto>> GetDistribucionCalibreAsync(DistribucionCalibreEspecieRequestDto req)
        {
            var rows = await _repo.GetRowsDistribucionCalibreAsync(
           req.CodigoEmpresa,
           req.CodigoEspecie,
           req.CodigoTemporada,
           req.IdCalibre
       );

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = rows
                .GroupBy(r => new { r.IdEstimacion, r.IdCalibre, r.CalibreNombre, r.PorcDefectoCalibre })
                .Select(g => new DistribucionCalibreEspecieResponseDto
                {
                    IdEstimacion = g.Key.IdEstimacion,
                    CalibreId = g.Key.IdCalibre,
                    CalibreNombre = g.Key.CalibreNombre,
                    Predeterminado = g.Key.PorcDefectoCalibre,
                    Semanas = g
                        .Select(r => new SemanaPorcentajeDto
                        {
                            Anio = r.SemanaAnio,
                            Semana = r.SemanaNumero,
                            Porcentaje = r.PorcentajeSemana,
                            EsSemanaActual = r.EsSemanaActual
                        })
                        .DistinctBy(x => new { x.Anio, x.Semana }) // por si viniera repetido
                        .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                        .ToList()
                })
                .OrderBy(x => x.IdEstimacion)
                .ThenBy(x => x.CalibreId)
                .ToList();

            return grouped;
        }

        public async Task<List<DistribucionPackingDto>> GetDistribucionPackingAsync(
         DistribucionPackingQueryDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

           req.CodigoEmpresa = Guard.RequireAndUpper(req.CodigoEmpresa, nameof(req.CodigoEmpresa));
            req.CodigoEspecie = Guard.RequireAndUpper(req.CodigoEspecie, nameof(req.CodigoEspecie));
            req.CodigoTemporada = Guard.RequireAndUpper(req.CodigoTemporada, nameof(req.CodigoTemporada));

            if (req.Anio < 2000 || req.Anio > 2100)
                throw new ValidationException("Año fuera de rango");

           
            req.Semana = (req.Semana ?? string.Empty).Trim(); 
            var weekAttr = new WeekIsoStringAttribute();
            var result = weekAttr.GetValidationResult(
                req.Semana,
                new ValidationContext(req) { MemberName = nameof(req.Semana) }
            );
            if (result != ValidationResult.Success)
                throw new ValidationException(result!.ErrorMessage!);


            return await _repo.GetRowsDistribucionPackingAsync(req);
        }

        public async Task<List<DistribucionFrigorificoDto>> GetDistribucionFrigorificoAsync(
        DistribucionPackingQueryDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            req.CodigoEmpresa = Guard.RequireAndUpper(req.CodigoEmpresa, nameof(req.CodigoEmpresa));
            req.CodigoEspecie = Guard.RequireAndUpper(req.CodigoEspecie, nameof(req.CodigoEspecie));
            req.CodigoTemporada = Guard.RequireAndUpper(req.CodigoTemporada, nameof(req.CodigoTemporada));

            if (req.Anio < 2000 || req.Anio > 2100)
                throw new ValidationException("Año fuera de rango");


            req.Semana = (req.Semana ?? string.Empty).Trim();
            var weekAttr = new WeekIsoStringAttribute();
            var result = weekAttr.GetValidationResult(
                req.Semana,
                new ValidationContext(req) { MemberName = nameof(req.Semana) }
            );
            if (result != ValidationResult.Success)
                throw new ValidationException(result!.ErrorMessage!);
            return await _repo.GetRowsDistribucionFrigorificoAsync(req);
        }

       
    }
}
