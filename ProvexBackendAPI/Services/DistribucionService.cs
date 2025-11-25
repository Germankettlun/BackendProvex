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


        public async Task<List<DistribucionFrigorificoDiaDto>> GetDistribucionFrigorificoAgrupadoAsync(int idBisemanal)
        {
                  
            if (idBisemanal < 0 )
                throw new ValidationException("idBisemanal inválido");

            var parameters = new SqlParameter[]
            {
                    new SqlParameter("@ID_ESTIMACION_BISEMANAL", idBisemanal),
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_DISTRIBUCION_BISEMANAL_FRIGORIFICO_SEMANAANO", parameters);

            var rows = dataTable.AsEnumerable()
                .Select(r => new DistribucionFrigorificoFlatRow
                {
                    IdEstimacion = r["IDESTIMACION"] == DBNull.Value ? "" : Convert.ToString(r["IDESTIMACION"]),
                    IdEspecie = r["IDESPECIE"] == DBNull.Value ? "" : Convert.ToString(r["IDESPECIE"]),
                    IdEstimacionBisemanal = r["IDESTIMACIONBISEMANAL"] == DBNull.Value ? 0 : Convert.ToInt32(r["IDESTIMACIONBISEMANAL"]),
                    BisemanalAnio = r["BISEMANALANIO"] == DBNull.Value ? 0 : Convert.ToInt32(r["BISEMANALANIO"]),
                    BisemanalSemana = r["BISEMANALSEMANA"] == DBNull.Value ? "" : Convert.ToString(r["BISEMANALSEMANA"]),
                    FechaDia = r.Field<DateTime?>("FECHADIA"),
                    DiaNombre = r["DIANOMBRE"] == DBNull.Value ? "" : Convert.ToString(r["DIANOMBRE"]),
                    TotalCajasBisemanal = r.Field<int?>("TOTALCAJASBISEMANAL") ?? 0,
                    IdDistribucionFrigorifico = r["IDDISTRIBUCIONFIGORIFICO"] == DBNull.Value ? "" : Convert.ToString(r["IDDISTRIBUCIONFIGORIFICO"]),
                    IdFrigorifico = r["IDFRIGORIFICO"] == DBNull.Value ? "" : Convert.ToString(r["IDFRIGORIFICO"]),
                    Frigorifico = r["FRIGORIFICO"] == DBNull.Value ? "" : Convert.ToString(r["FRIGORIFICO"]),
                    Porcentaje = r["PORCENTAJE"] == DBNull.Value ? 0 : Convert.ToInt32(r["PORCENTAJE"]),
                    SumaPorcentajeEs100 = r["SumaPorcentajeEs100"] == DBNull.Value ? false : Convert.ToBoolean(r["SumaPorcentajeEs100"]),
                })
                .ToList();

            var result = rows
            .GroupBy(r => new
            {
                r.IdEstimacion,
                r.IdEspecie,
                r.IdEstimacionBisemanal,
                r.BisemanalAnio,
                r.BisemanalSemana,
                r.FechaDia,
                r.DiaNombre,
                r.TotalCajasBisemanal,
                r.SumaPorcentajeEs100
            })
            .Select(g =>
            {
                var k = g.Key;

                var frigorificos = g
                    .Select(r => new FrigorificoItemDto
                    {
                        IdDistribucionFrigorifico = string.IsNullOrWhiteSpace(r.IdDistribucionFrigorifico)
                            ? null
                            : r.IdDistribucionFrigorifico,
                        IdFrigorifico = r.IdFrigorifico ?? string.Empty,
                        Nombre = r.Frigorifico ?? string.Empty,
                        Porcentaje = r.Porcentaje
                    })
                    .OrderByDescending(x => x.Porcentaje)
                    .ThenBy(x => x.Nombre)
                    .ToList();

                return new DistribucionFrigorificoDiaDto
                {
                    IdEstimacion = k.IdEstimacion ?? string.Empty,
                    IdEspecie = k.IdEspecie ?? string.Empty,
                    IdEstimacionBisemanal = Convert.ToString(k.IdEstimacionBisemanal) ?? "",
                    Anio = k.BisemanalAnio,
                    Semana = k.BisemanalSemana ?? string.Empty,
                    FechaDia = k.FechaDia,
                    DiaNombre = k.DiaNombre ?? string.Empty,
                    TotalCajasBisemanal = k.TotalCajasBisemanal,
                    SumaPorcentajeEs100 = k.SumaPorcentajeEs100,
                    FrigorificoPorDia = frigorificos
                };
            })
            .OrderBy(d => d.FechaDia ?? DateTime.MinValue)
            .ThenBy(d => d.IdEstimacion)
            .ToList();

            return result;
        }

        public async Task<List<DistribucionPackingDiaDto>> GetDistribucionPackingAgrupadoAsync(int idBisemanal)
        {
            if (idBisemanal < 0)
                throw new ValidationException("idBisemanal inválido");

            var parameters = new SqlParameter[]
            {
                    new SqlParameter("@ID_ESTIMACION_BISEMANAL", idBisemanal),
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_DISTRIBUCION_BISEMANAL_PACKING_SEMANAANO", parameters);

            var rows = dataTable.AsEnumerable()
                .Select(r => new DistribucionPackingFlatRow
                {
                    IdEstimacion = r["IDESTIMACION"] == DBNull.Value ? "" : Convert.ToString(r["IDESTIMACION"]),
                    IdEspecie = r["IDESPECIE"] == DBNull.Value ? "" : Convert.ToString(r["IDESPECIE"]),
                    IdEstimacionBisemanal = r["IDESTIMACIONBISEMANAL"] == DBNull.Value ? 0 : Convert.ToInt32(r["IDESTIMACIONBISEMANAL"]),
                    BisemanalAnio = r["BISEMANALANIO"] == DBNull.Value ? 0 : Convert.ToInt32(r["BISEMANALANIO"]),
                    BisemanalSemana = r["BISEMANALSEMANA"] == DBNull.Value ? "" : Convert.ToString(r["BISEMANALSEMANA"]),
                    FechaDia = r.Field<DateTime?>("FECHADIA"),
                    DiaNombre = r["DIANOMBRE"] == DBNull.Value ? "" : Convert.ToString(r["DIANOMBRE"]),
                    TotalCajasBisemanal = r.Field<int?>("TOTALCAJASBISEMANAL") ?? 0,
                    IdDistribucionPacking = r["IDDISTRIBUCIONPACKING"] == DBNull.Value ? "" : Convert.ToString(r["IDDISTRIBUCIONPACKING"]),
                    IdPacking = r["IDPACKING"] == DBNull.Value ? "" : Convert.ToString(r["IDPACKING"]),
                    Packing = r["PACKING"] == DBNull.Value ? "" : Convert.ToString(r["PACKING"]),
                    Porcentaje = r["PORCENTAJE"] == DBNull.Value ? 0 : Convert.ToInt32(r["PORCENTAJE"]),
                    SumaPorcentajeEs100 = r["SumaPorcentajeEs100"] == DBNull.Value ? false : Convert.ToBoolean(r["SumaPorcentajeEs100"]),
                })
                .ToList();

            var result = rows
            .GroupBy(r => new
            {
                r.IdEstimacion,
                r.IdEspecie,
                r.IdEstimacionBisemanal,
                r.BisemanalAnio,
                r.BisemanalSemana,
                r.FechaDia,
                r.DiaNombre,
                r.TotalCajasBisemanal,
                r.SumaPorcentajeEs100
            })
            .Select(g =>
            {
                var k = g.Key;

                var packings = g
                    .Select(r => new PackingItemDto
                    {
                        IdDistribucionPacking = string.IsNullOrWhiteSpace(r.IdDistribucionPacking)
                            ? null
                            : r.IdDistribucionPacking,
                        IdPacking = r.IdPacking ?? string.Empty,
                        Nombre = r.Packing ?? string.Empty,
                        Porcentaje = r.Porcentaje
                    })
                    .OrderByDescending(x => x.Porcentaje)
                    .ThenBy(x => x.Nombre)
                    .ToList();

                return new DistribucionPackingDiaDto
                {
                    IdEstimacion = k.IdEstimacion ?? string.Empty,
                    IdEspecie = k.IdEspecie ?? string.Empty,
                    IdEstimacionBisemanal = Convert.ToString(k.IdEstimacionBisemanal) ?? "",
                    Anio = k.BisemanalAnio,
                    Semana = k.BisemanalSemana ?? string.Empty,
                    FechaDia = k.FechaDia,
                    DiaNombre = k.DiaNombre ?? string.Empty,
                    TotalCajasBisemanal = k.TotalCajasBisemanal,
                    SumaPorcentajeEs100 = k.SumaPorcentajeEs100,
                    PackingPorDia = packings
                };
            })
            .OrderBy(d => d.FechaDia ?? DateTime.MinValue)
            .ThenBy(d => d.IdEstimacion)
            .ToList();

            return result;
        }

        public async Task<List<DistribucionExportacionEstimacionResponseDto>> GetRowsDistribucionPorcentajeExportacionAsync(int idEstimacion, int? semanasAntes, int? semanasDespues)
        {


            if (idEstimacion < 0)
                throw new ValidationException("idEstimacion inválido");

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ID_ESTIMACION", idEstimacion),
                //new SqlParameter("@SEM_ANT", (object?)semanasAntes   ?? DBNull.Value),
                //new SqlParameter("@SEM_SIG", (object?)semanasDespues ?? DBNull.Value),
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_DISTRIBUCION_PORC_EXPORTACION",parameters);

            var rows = dataTable.AsEnumerable()
            .Select(r => new DistribucionExportacionEstimacionRow
            {
                IdEstimacion = r["ID_ESTIMACION"] == DBNull.Value ? string.Empty : Convert.ToString(r["ID_ESTIMACION"]),
                CodEspecie = "",
                Especie = "",
                SemanaAnio = r["SEMANAANO"] == DBNull.Value ? 0 : Convert.ToInt32(r["SEMANAANO"]),
                SemanaNumero = r["SEMANANUMERO"] == DBNull.Value ? string.Empty : Convert.ToString(r["SEMANANUMERO"]),
                PorcDefecto = r["PORCENTAJE_POR_DEFECTO"] == DBNull.Value ? null : Convert.ToInt32(r["PORCENTAJE_POR_DEFECTO"]),
                IdDistribucionPorSemana = r["ID_DISTRIBUCION_PORCENTAJE_EXPORTACION"] == DBNull.Value ? null : Convert.ToInt32(r["ID_DISTRIBUCION_PORCENTAJE_EXPORTACION"]),
                PorcentajeSemana = r["PORCENTAJE_POR_SEMANA"] == DBNull.Value ? null : Convert.ToInt32(r["PORCENTAJE_POR_SEMANA"]),
                EsSemanaActual = r["ES_SEMANA_ACTUAL"] == DBNull.Value ? false : Convert.ToBoolean(r["ES_SEMANA_ACTUAL"]),
            }).ToList();

            var grouped = rows.GroupBy(r => new
                {
                    r.IdEstimacion,
                    r.CodEspecie,
                    r.Especie,
                    r.PorcDefecto
                }).Select(g => new DistribucionExportacionEstimacionResponseDto
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
                .OrderBy(x => x.Anio)
                .ThenBy(x => x.Semana)
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

            foreach (var cat in req.Categorias ?? Enumerable.Empty<DistribucionCategoriaPredeterminadoGuardarDto>())
            {
                //Predeterminado
                var predParams = new[]
                {
            new SqlParameter("@IdEstimacion", req.IdEstimacion),
            new SqlParameter("@IdCategoria", cat.IdCategoria),
            new SqlParameter("@PorcentajePredeterminado", (object?)cat.PorcentajePredeterminado ?? DBNull.Value),
            new SqlParameter("@IdUsuario", usuarioId)
        };

                await repository.SpVoid(
                    "[Estimaciones].usp_INSERT_UPDATE_DistribucionCategoria_Predeterminado",
                    predParams
                );

                // Semanas
                foreach (var s in cat.Semanas ?? Enumerable.Empty<PorcentajePorSemanaGuardarDto>())
                {
                    var semanaParams = new[]
                    {
                new SqlParameter("@IdEstimacion", req.IdEstimacion),
                new SqlParameter("@IdCategoria", cat.IdCategoria),
                new SqlParameter("@Anio", s.Anio),
                new SqlParameter("@Semana", s.Semana),
                new SqlParameter("@Porcentaje", s.Porcentaje ?? 0),
                new SqlParameter("@IdUsuario", usuarioId) 
            };

                    await repository.SpVoid(
                        "[Estimaciones].usp_INSERT_UPDATE_DistribucionCategoria_Semana",
                        semanaParams
                    );
                }
            }
        }

        public async Task DistribucionCalibreGuardarAsync(DistribucionCalibreGuardarRequest req, Guid usuarioId)
        {
            if (req is null || req.IdEstimacion <= 0)
                throw new ArgumentException("Parámetros inválidos.");

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
