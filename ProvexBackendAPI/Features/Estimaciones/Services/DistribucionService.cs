using Microsoft.Data.SqlClient;
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

        public async Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {

            idEstimacion = Guard.Require(nameof(idEstimacion), idEstimacion);

            if (idEstimacion < 0)
                throw new ValidationException("idEstimacion inválido");

            var rows = await _repo.GetRowsDistribucionCategoriaAsync(idEstimacion, semanasAntes, semanasDespues);

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = rows
                .GroupBy(r => new { r.IdEstimacion, r.IdCategoria, r.CodEspecie, r.Especie, r.CategoriaNombre, r.IdDistribucionDefecto, r.PorcDefectoCategoria })
                .Select(g => new DistribucionCategoriaEspecieResponseDto
                {
                    IdEstimacion = g.Key.IdEstimacion,
                    CategoriaId = g.Key.IdCategoria,
                    CategoriaNombre = g.Key.CategoriaNombre,
                    CodigoEspecie = g.Key.CodEspecie,
                    Especie = g.Key.Especie, 
                    IdPorcentajePredeterminado = g.Key.IdDistribucionDefecto,
                    PorcentajePredeterminado = g.Key.PorcDefectoCategoria,
                    Semanas = g
                        .Select(r => new SemanaPorcentajeDto
                        {
                            Anio = r.SemanaAnio,
                            Semana = r.SemanaNumero,
                            IdPorcentajePorSemana = r.IdDistribucionPorSemana,
                            PorcentajePorSemana = r.PorcentajeSemana,
                            EsSemanaActual = r.EsSemanaActual
                        })
                        .DistinctBy(x => new { x.Anio, x.Semana }) 
                        .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                        .ToList()
                })
                .OrderBy(x => x.IdEstimacion)
                .ThenBy(x => x.CategoriaId)
                .ToList();

            return grouped;
        }


        public async Task<List<DistribucionCalibreEspecieResponseDto>> GetDistribucionCalibreAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {
            idEstimacion = Guard.Require(nameof(idEstimacion), idEstimacion);

            if (idEstimacion < 0)
                throw new ValidationException("idEstimacion inválido");


            var rows = await _repo.GetRowsDistribucionCalibreAsync(idEstimacion, semanasAntes, semanasDespues);

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = rows
                .GroupBy(r => new { r.IdEstimacion, r.IdCalibre, r.CodEspecie, r.Especie, r.CalibreNombre, r.IdDistribucionDefecto, r.PorcDefectoCategoria })
                .Select(g => new DistribucionCalibreEspecieResponseDto
                {
                    IdEstimacion = g.Key.IdEstimacion,
                    CalibreId = g.Key.IdCalibre,
                    CalibreNombre = g.Key.CalibreNombre,
                    CodigoEspecie = g.Key.CodEspecie,
                    Especie = g.Key.Especie,
                    IdPorcentajePredeterminado = g.Key.IdDistribucionDefecto,
                    PorcentajePredeterminado = g.Key.PorcDefectoCategoria,
                    Semanas = g
                        .Select(r => new SemanaPorcentajeDto
                        {
                            Anio = r.SemanaAnio,
                            Semana = r.SemanaNumero,
                            IdPorcentajePorSemana = r.IdDistribucionPorSemana,
                            PorcentajePorSemana = r.PorcentajeSemana,
                            EsSemanaActual = r.EsSemanaActual
                        })
                        .DistinctBy(x => new { x.Anio, x.Semana }) 
                        .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                        .ToList()
                })
                .OrderBy(x => x.IdEstimacion)
                .ThenBy(x => x.CalibreId)
                .ToList();

            return grouped;
        }






        public async Task<List<DistribucionFrigorificoDiaDto>> GetDistribucionFrigorificoAgrupadoAsync(
       int idBisemanal )
        {
          

            idBisemanal = Guard.Require( nameof(idBisemanal), idBisemanal);
           
            if (idBisemanal < 0 )
                throw new ValidationException("idBisemanal inválido");


            return await _repo.GetRowsDistribucionFrigorificoAgrupadoAsync(idBisemanal);
        }

        public async Task<List<DistribucionPackingDiaDto>> GetDistribucionPackingAgrupadoAsync(int idBisemanal)
        {
            idBisemanal = Guard.Require(nameof(idBisemanal), idBisemanal);

            if (idBisemanal < 0)
                throw new ValidationException("idBisemanal inválido");
            return await _repo.GetRowsDistribucionPackingAgrupadoAsync(idBisemanal);
        }

        public async Task<List<DistribucionExportacionEstimacionResponseDto>> GetRowsDistribucionPorcentajeExportacionAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {

            idEstimacion = Guard.Require(nameof(idEstimacion), idEstimacion);

            if (idEstimacion < 0)
                throw new ValidationException("idEstimacion inválido");

            var rows = await _repo.GetRowsDistribucionPorcentajeExportacionAsync(idEstimacion, semanasAntes, semanasDespues);

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = rows
                .GroupBy(r => new { r.IdEstimacion, r.CodEspecie, r.Especie, r.PorcDefecto })
                .Select(g => new DistribucionExportacionEstimacionResponseDto
                {
                    IdEstimacion = g.Key.IdEstimacion,
                    CodigoEspecie = g.Key.CodEspecie,
                    Especie = g.Key.Especie,
                    PorcentajePredeterminado = g.Key.PorcDefecto,
                    Semanas = g
                        .Select(r => new SemanaPorcentajeDto
                        {
                            Anio = r.SemanaAnio,
                            Semana = r.SemanaNumero,
                            IdPorcentajePorSemana = r.IdDistribucionPorSemana,
                            PorcentajePorSemana = r.PorcentajeSemana,
                            EsSemanaActual = r.EsSemanaActual
                        })
                        .DistinctBy(x => new { x.Anio, x.Semana })
                        .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                        .ToList()
                })
                .OrderBy(x => x.IdEstimacion)
                .ThenBy(x => x.CodigoEspecie)
                .ToList();

            return grouped;
        }

        public async Task DistribucionCategoriaGuardarAsync(DistribucionCategoriaGuardarRequest req)
        {
            if (req is null || req.IdEstimacion <= 0)
                throw new ArgumentException("Parámetros inválidos.");

            // Guardar predeterminados + semanas, usando tu repo actual (cada método abre su conexión)
            foreach (var cat in req.Categorias ?? Enumerable.Empty<DistribucionCategoriaPredeterminadoGuardarDto>())
            {
                // Predeterminado
                await _repo.InsertUpdateDistribucionCategoriaPredeterminadoAsync(
                    req.IdEstimacion,
                    cat.IdCategoria,
                    cat.PorcentajePredeterminado,
                    req.IdUsuario 
                );

                // Semanas
                foreach (var s in cat.Semanas ?? Enumerable.Empty<PorcentajePorSemanaGuardarDto>())
                {
                   

                    await _repo.InsertUpdateDistribucionCategoriaPorSemanaAsync(
                        req.IdEstimacion,
                        cat.IdCategoria,
                        s.Anio,
                        s.Semana,
                        s.Porcentaje ?? 0,
                        req.IdUsuario 
                    );
                }
            }
        }

        public async Task DistribucionCalibreGuardarAsync(DistribucionCalibreGuardarRequest req)
        {
            if (req is null || req.IdEstimacion <= 0)
                throw new ArgumentException("Parámetros inválidos.");

            // Guardar predeterminados + semanas, usando tu repo actual (cada método abre su conexión)
            foreach (var cal in req.Calibres ?? Enumerable.Empty<DistribucionCalibrePredeterminadoGuardarDto>())
            {
                // Predeterminado
                await _repo.InsertUpdateDistribucionCalibrePredeterminadoAsync(
                    req.IdEstimacion,
                    cal.IdCalibre,
                    cal.PorcentajePredeterminado,
                    req.IdUsuario
                );

                // Semanas
                foreach (var s in cal.Semanas ?? Enumerable.Empty<PorcentajePorSemanaGuardarDto>())
                {


                    await _repo.InsertUpdateDistribucionCalibrePorSemanaAsync(
                        req.IdEstimacion,
                        cal.IdCalibre,
                        s.Anio,
                        s.Semana,
                        s.Porcentaje ?? 0,
                        req.IdUsuario
                    );
                }
            }
        }

        public async Task DistribucionFrigorificoGuardarAsync(DistribucionFrigorificoGuardarRequest req)
        {

            if (req.IdEstimacionBisemanal <= 0) throw new ArgumentException("IdEstimacionBisemanal inválido.");
            if (req.Frigorificos is null || req.Frigorificos.Count == 0) throw new ArgumentException("Debe enviar al menos un frigorífico.");
            foreach (var it in req.Frigorificos)
            {
                if (it.IdFrigorifico <= 0) throw new ArgumentException("IdFrigorifico inválido.");
                if ((it.Porcentaje < 0 || it.Porcentaje > 100))
                    throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");
            }

            //Borrado para realizar una actualización completa

            await _repo.EliminaDistribucionFrigorificoAsync(req.IdEstimacionBisemanal);

            await _repo.InsertUpdateDistribucionFrigorificoAsync(req);
        }

        public async Task DistribucionPackingGuardarAsync(DistribucionPackingGuardarRequest req)
        {

            if (req.IdEstimacionBisemanal <= 0) throw new ArgumentException("IdEstimacionBisemanal inválido.");
            if (req.Packings is null || req.Packings.Count == 0) throw new ArgumentException("Debe enviar al menos un packing.");
            foreach (var it in req.Packings)
            {
                if (it.IdPacking <= 0) throw new ArgumentException("IdPacking inválido.");
                if ((it.Porcentaje < 0 || it.Porcentaje > 100))
                    throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");
            }

            //Borrado para realizar una actualización completa

            await _repo.EliminaDistribucionPackingAsync(req.IdEstimacionBisemanal);

            await _repo.InsertUpdateDistribucionPackingAsync(req);
        }

        public async Task DistribucionPorcentajeExportacionGuardarAsync(DistribucionPorcentajeExportacionGuardarRequest req)
        {
            if (req is null || req.IdEstimacion <= 0)
                throw new ArgumentException("Parámetros inválidos.");

            // Guardar predeterminados + semanas, usando tu repo actual (cada método abre su conexión)
          
                // Porcentaje predeterminado
                await _repo.InsertUpdateDistribucionPorcentajeExportacionPredeterminadoAsync(req.IdEstimacion, req.PorcentajePredeterminado, req.IdUsuario);

                // Semanas
                foreach (var s in req.Semanas ?? Enumerable.Empty<PorcentajePorSemanaGuardarDto>())
                {


                    await _repo.InsertUpdateDistribucionPorcentajeExportacionPorSemanaAsync(
                        req.IdEstimacion,
                        s.Anio,
                        s.Semana,
                        s.Porcentaje ?? 0,
                        req.IdUsuario
                    );
                }
         
        }

    }
}
