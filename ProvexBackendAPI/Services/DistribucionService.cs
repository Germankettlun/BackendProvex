using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie;
using ProvexBackendAPI.Helpers.Validation;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionCalibreEspecieDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie.DistribucionesDto;

namespace ProvexBackendAPI.Services
{
    public class DistribucionService : IDistribucionService
    {
        private readonly IDistribucionRepository _repo;
        private readonly IGenericRepository repository;

        public DistribucionService(IDistribucionRepository repo, IGenericRepository repository)
        {
            _repo = repo;
            this.repository = repository;
        }

        public async Task<List<DistribucionCategoriaEspecieResponseDto>> GetDistribucionCategoriaAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {

            if (idEstimacion <= 0)
                throw new ArgumentException("idEstimacion inválido.", nameof(idEstimacion));

            var parameters = new SqlParameter[]
              {
                    new SqlParameter("@ID_ESTIMACION", idEstimacion),
                   // new SqlParameter("@SEM_ANT",(object?)semanasAntes ?? DBNull.Value),
                   // new SqlParameter("@SEM_SIG",(object?)semanasDespues ?? DBNull.Value),
              };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_DISTRIBUCION_CATEGORIA_ESPECIE", parameters);

            var list = new List<DistribucionCategoriaEspecieRow>();

            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(new DistribucionCategoriaEspecieRow
                {
                    IdEstimacion = row["ID_ESTIMACION"] == DBNull.Value ? "" : Convert.ToString(row["ID_ESTIMACION"]),

                    CodEspecie = row["CODESPECIE"] == DBNull.Value ? "" : Convert.ToString(row["CODESPECIE"]),
                    Especie = row["ESPECIE"] == DBNull.Value ? "" : Convert.ToString(row["ESPECIE"]),
                    IdCategoria = row["IDCATEGORIA"] == DBNull.Value ? "" : Convert.ToString(row["IDCATEGORIA"]),
                    CategoriaNombre = row["CATEGORIA"] == DBNull.Value ? "" : Convert.ToString(row["CATEGORIA"]),
                    SemanaAnio = row["SEMANAANO"] == DBNull.Value ? 0 : Convert.ToInt32(row["SEMANAANO"]),
                    SemanaNumero = row["SEMANANUMERO"] == DBNull.Value ? "" : Convert.ToString(row["SEMANANUMERO"]),
                    IdDistribucionDefecto = row["IDDISTRIBUCIONDEFECTO"] == DBNull.Value ? 0 : Convert.ToInt32(row["IDDISTRIBUCIONDEFECTO"]),
                    PorcDefectoCategoria = row["PORCENTAJE_POR_DEFECTO_CATEGORIA"] == DBNull.Value ? 0 : Convert.ToInt32(row["PORCENTAJE_POR_DEFECTO_CATEGORIA"]),
                    IdDistribucionPorSemana = row["DISTRIBUCIONPORSEMANAID"] == DBNull.Value ? 0 : Convert.ToInt32(row["DISTRIBUCIONPORSEMANAID"]),
                    PorcentajeSemana = row["PORCENTAJE_POR_SEMANA_CATEGORIA"] == DBNull.Value ? 0 : Convert.ToInt32(row["PORCENTAJE_POR_SEMANA_CATEGORIA"]),

                  //  EsSemanaActual = row["ES_SEMANA_ACTUAL"] == DBNull.Value ? false : Convert.ToBoolean(row["ES_SEMANA_ACTUAL"]),

                });
            }

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = list
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

            if (idEstimacion <= 0)
                throw new ArgumentException("idEstimacion inválido.", nameof(idEstimacion));

            var parameters = new SqlParameter[]
              {
                    new SqlParameter("@ID_ESTIMACION", idEstimacion),
                   // new SqlParameter("@SEM_ANT",(object?)semanasAntes ?? DBNull.Value),
                   // new SqlParameter("@SEM_SIG",(object?)semanasDespues ?? DBNull.Value),
              };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_DISTRIBUCION_CALIBRE_ESPECIE", parameters);

            var list = new List<DistribucionCalibreEspecieRow>();

            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(new DistribucionCalibreEspecieRow
                {
                    IdEstimacion = row["ID_ESTIMACION"] == DBNull.Value ? "" : Convert.ToString(row["ID_ESTIMACION"]),

                    CodEspecie = row["CODESPECIE"] == DBNull.Value ? "" : Convert.ToString(row["CODESPECIE"]),
                    Especie = row["ESPECIE"] == DBNull.Value ? "" : Convert.ToString(row["ESPECIE"]),
                    IdCalibre = row["IDCALIBRE"] == DBNull.Value ? "" : Convert.ToString(row["IDCALIBRE"]),
                    CalibreNombre = row["IDCALIBRE"] == DBNull.Value ? "" : Convert.ToString(row["IDCALIBRE"]),
                    SemanaAnio = row["SEMANAANO"] == DBNull.Value ? 0 : Convert.ToInt32(row["SEMANAANO"]),
                    SemanaNumero = row["SEMANANUMERO"] == DBNull.Value ? "" : Convert.ToString(row["SEMANANUMERO"]),
                    IdDistribucionDefecto = row["IDDISTRIBUCIONDEFECTO"] == DBNull.Value ? 0 : Convert.ToInt32(row["IDDISTRIBUCIONDEFECTO"]),
                    PorcDefectoCalibre = row["PORCENTAJE_POR_DEFECTO_CALIBRE"] == DBNull.Value ? 0 : Convert.ToInt32(row["PORCENTAJE_POR_DEFECTO_CALIBRE"]),
                    IdDistribucionPorSemana = row["DISTRIBUCIONPORSEMANAID"] == DBNull.Value ? 0 : Convert.ToInt32(row["DISTRIBUCIONPORSEMANAID"]),
                    PorcentajeSemana = row["PORCENTAJE_POR_SEMANA_CALIBRE"] == DBNull.Value ? 0 : Convert.ToInt32(row["PORCENTAJE_POR_SEMANA_CALIBRE"]),

                    //  EsSemanaActual = row["ES_SEMANA_ACTUAL"] == DBNull.Value ? false : Convert.ToBoolean(row["ES_SEMANA_ACTUAL"]),

                });
            }

            // Grouping por (IdEstimacion, IdCategoria)
            var grouped = list
                .GroupBy(r => new { r.IdEstimacion, r.IdCalibre, r.CodEspecie, r.Especie, r.CalibreNombre, r.IdDistribucionDefecto, r.PorcDefectoCalibre })
                .Select(g => new DistribucionCalibreEspecieResponseDto
                {
                    IdEstimacion = g.Key.IdEstimacion,
                    CalibreId = g.Key.IdCalibre,
                    CalibreNombre = g.Key.CalibreNombre,
                    CodigoEspecie = g.Key.CodEspecie,
                    Especie = g.Key.Especie,
                    IdPorcentajePredeterminado = g.Key.IdDistribucionDefecto,
                    PorcentajePredeterminado = g.Key.PorcDefectoCalibre,
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

        public async Task DistribucionCategoriaGuardarAsync(DistribucionCategoriaGuardarRequest req, Guid usuarioId)
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
                    usuarioId
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
                        usuarioId
                    );
                }
            }
        }

        public async Task DistribucionCalibreGuardarAsync(DistribucionCalibreGuardarRequest req, Guid usuarioId)
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
                    usuarioId
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
                        usuarioId
                    );
                }
            }
        }

        public async Task DistribucionFrigorificoGuardarAsync(DistribucionFrigorificoGuardarRequest req, Guid usuarioId)
        {

            if (req.IdEstimacionBisemanal <= 0) throw new ArgumentException("IdEstimacionBisemanal inválido.");
            if (req.Frigorificos is null || req.Frigorificos.Count == 0) throw new ArgumentException("Debe enviar al menos un frigorífico.");
            foreach (var it in req.Frigorificos)
            {
                if (it.IdFrigorifico <= 0) throw new ArgumentException("IdFrigorifico inválido.");
                if (it.Porcentaje < 0 || it.Porcentaje > 100)
                    throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");
            }

            //Borrado para realizar una actualización completa

            await _repo.EliminaDistribucionFrigorificoAsync(req.IdEstimacionBisemanal, req.ReplicarASemana);

            await _repo.InsertUpdateDistribucionFrigorificoAsync(req, usuarioId);
        }

        public async Task DistribucionPackingGuardarAsync(DistribucionPackingGuardarRequest req, Guid usuarioId)
        {

            if (req.IdEstimacionBisemanal <= 0) throw new ArgumentException("IdEstimacionBisemanal inválido.");
            if (req.Packings is null || req.Packings.Count == 0) throw new ArgumentException("Debe enviar al menos un packing.");
            foreach (var it in req.Packings)
            {
                if (it.IdPacking <= 0) throw new ArgumentException("IdPacking inválido.");
                if (it.Porcentaje < 0 || it.Porcentaje > 100)
                    throw new ArgumentException("El porcentaje debe estar entre 0 y 100.");
            }

            //Borrado para realizar una actualización completa

            await _repo.EliminaDistribucionPackingAsync(req.IdEstimacionBisemanal, req.ReplicarASemana);

            await _repo.InsertUpdateDistribucionPackingAsync(req, usuarioId);
        }

        public async Task DistribucionPorcentajeExportacionGuardarAsync(DistribucionPorcentajeExportacionGuardarRequest req, Guid userId)
        {
            if (req is null || req.IdEstimacion <= 0)
                throw new ArgumentException("Parámetros inválidos.");

            // Guardar predeterminados + semanas, usando tu repo actual (cada método abre su conexión)
          
                // Porcentaje predeterminado
                await _repo.InsertUpdateDistribucionPorcentajeExportacionPredeterminadoAsync(req.IdEstimacion, req.PorcentajePredeterminado, userId);

                // Semanas
                foreach (var s in req.Semanas ?? Enumerable.Empty<PorcentajePorSemanaGuardarDto>())
                {


                    await _repo.InsertUpdateDistribucionPorcentajeExportacionPorSemanaAsync(
                        req.IdEstimacion,
                        s.Anio,
                        s.Semana,
                        s.Porcentaje ?? 0,
                        userId
                    );
                }
         
        }

    }
}
