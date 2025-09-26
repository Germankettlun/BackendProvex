using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCategoriaEspecieDto;

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
    }
}
