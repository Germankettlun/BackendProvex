using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using System.Data;
using System.Globalization;
using System.Xml.Linq;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Semanas.SemanasDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository
{
    public class EstimacionesRepository : IEstimacionesRepository
    {
        private readonly string _connString;
        private readonly ISemanaVigenteProvider _semanaProvider;
        public EstimacionesRepository(IConfiguration cfg, ISemanaVigenteProvider semanaProvider)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")!;
            _semanaProvider = semanaProvider;
        }
        public async Task<EstructuraDistribucionDto> GetEstimacionBisemanalAsync(EstimacionBisemanalQueryDto req)
        {

            var rows = new List<RowFlat>();

            var semanas = await _semanaProvider.ListAsync(codigoEmpresa: req.CodEmpresa, codigoTemporada: req.IdTemporada);

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_EstimacionBisemanal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parámetros del SPU (mismo patrón que tu ejemplo base)
            cmd.Parameters.Add(new SqlParameter("@COD_EMPRESA", SqlDbType.VarChar, 10) { Value = req.CodEmpresa });
            cmd.Parameters.Add(new SqlParameter("@ID_TEMPORADA", SqlDbType.VarChar, 10) { Value = (object?)req.IdTemporada ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@COD_GRUPO_PRODUCTOR", SqlDbType.VarChar, 10) { Value = (object?)req.CodGrupoProductor ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@ID_ESPECIE", SqlDbType.VarChar, 10) { Value = (object?)req.IdEspecie ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@ID_PRODUCTOR", SqlDbType.VarChar, 20) { Value = (object?)req.IdProductor ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@ID_VARIEDAD", SqlDbType.VarChar, 20) { Value = (object?)req.IdVariedad ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@ANIO_BASE", SqlDbType.Int) { Value = (object?)req.AnioBase ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@SEMANA_BASE", SqlDbType.Int) { Value = (object?)req.SemanaBase ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@PAGE", SqlDbType.Int) { Value = req.Page });
            cmd.Parameters.Add(new SqlParameter("@WEEKS_PER_PAGE", SqlDbType.Int) { Value = req.WeeksPerPage });

            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
               
                rows.Add(new RowFlat
                {
                    // Raíz
                    PesoBaseEspecie = rdr.Get<double?>("ESPECIE_KILO_BASE") ?? 0.0, 
                    Especie = rdr.FirstExistingAsString("NOM_ESP"),

                    // Item
                    IdProductor = rdr.FirstExistingAsString("ID_PRODUCTOR"),
                    Productor = rdr.FirstExistingAsString("NOM_PROD"),
                    Variedad = rdr.FirstExistingAsString("NOM_VAR"),
                    Agronomo = rdr.FirstExistingAsString("NOM_USUARIO_AGRONOMO") ?? "", 
                    DistribucionCalibre = rdr.Get<bool?>("DIST_CAL"),
                    DistribucionCategoria = rdr.Get<bool?>("DIST_CAT"),

                    // Envase
                    EnvaseId = rdr.FirstExistingAsString("ENVASE_ID") ?? "", //Falta
                    EnvaseNombre = rdr.FirstExistingAsString("NOM_ENVASE_COSECHA") ?? "", 
                    EnvaseKilo = rdr.Get<int?>("KG_DIA_ENVASE") ?? 0, 

                    // Estimación + semanas
                    Est_ID = rdr.Get<int?>("ID_ESTIMACION"),
                    Est_Contratado = rdr.Get<int?>("CAJAS_CONTRATADAS") ?? 0, 
                    Est_FCosecha = rdr.FirstExistingAsString("FECHA_INICIO_COSECHA_YM") ?? "", 

                    Ant_Estimado = rdr.Get<int?>("CAJAS_E_ANTERIOR_SIN_PORC"),
                    Ant_Producido = rdr.Get<int?>("CAJAS_P_ANTERIOR"),
                    Sig_Estimado = rdr.Get<int?>("CAJAS_E_SIGUIENTE_SIN_PORC"),
                    Sig_Producido = rdr.Get<int?>("CAJAS_P_SIGUIENTE_SIN_PORC"),

                    // Bisemanal
                    Bis_ID = rdr.Get<int?>("ID_ESTIMACION_BISEMANAL"),
                    Bis_AnioBase = rdr.Get<int?>("ANIO"),
                    Bis_SemanaBase = rdr.FirstExistingAsString("SEMANA_NRO"),
                    Bis_DistFrio = rdr.Get<int?>("DIST_FRI"),
                    Bis_DistPacking = rdr.Get<int?>("DIST_PACK"),
                    Bis_PorcExport = rdr.Get<int?>("PCT_EXP_PORC") ?? 0, 

                    // Días
                    Dia_Nombre = rdr.FirstExistingAsString("NOMBRE_DIA"),
                    Dia_Fecha = rdr.Get<DateTime?>("DIA"),
                    Dia_Estimado = rdr.Get<decimal?>("CAJAS_ESTIMADAS_SIN_PORC"),
                    Dia_Producido = rdr.Get<decimal?>("CAJAS_P")
                });
            }

            return BuildTree(rows, req, semanas);
        }


        public async Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion)
        {
            var dict = new Dictionary<string, EstimacionSemanalDto>(StringComparer.OrdinalIgnoreCase);

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("Estimaciones.usp_UI_Estimacion_ResumenSemanal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };


            cmd.Parameters.Add(new SqlParameter("@COD_EMPRESA", SqlDbType.NVarChar, 20) { Value = codigoEmpresa.Trim().ToUpperInvariant() });
            cmd.Parameters.Add(new SqlParameter("@ID_TEMPORADA", SqlDbType.NVarChar, 20) { Value = idTemporada.Trim().ToUpperInvariant() });
            cmd.Parameters.Add(new SqlParameter("@ID_ESTIMACION", SqlDbType.Int) { Value = idEstimacion });

            try
            {
                await using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    // ----- NIVEL ESTIMACIÓN (se repite por fila) -----
                    var idEstim = rdr.Get<string?>("ID_ESTIMACION") ?? string.Empty;

                    if (!dict.TryGetValue(idEstim, out var estim))
                    {
                        estim = new EstimacionSemanalDto
                        {
                            IdEstimacion = idEstim,
                            Contratado = rdr.Get<int?>("CONTRATADO") ?? 0,
                            IdEnvaseCosecha = rdr.Get<string?>("ID_ENVASE_COSECHA"),

                            Totales = new TotalesEstimacionDto
                            {
                                EstimadoSinPorcentaje = rdr.Get<int?>("TOTAL_E_SIN_PORC"),
                                EstimadoConPorcentaje = rdr.Get<int?>("TOTAL_E_CON_PORC"),
                                Proyectado = rdr.Get<int?>("TOTAL_P"),
                                DiferenciaEstimadoConProyectado = rdr.Get<int?>("DIF_E_CON_P")
                            },

                            Semanas = new List<SemanaEstimacionDto>()
                        };

                        dict[idEstim] = estim;
                    }

                    // ----- NIVEL SEMANA (varía por fila) -----
                    var semana = new SemanaEstimacionDto
                    {
                        Pos = rdr.Get<int?>("POS"),
                        Anio = rdr.Get<int?>("ANIO") ?? 0,
                        SemanaNumero = rdr.Get<string?>("SEMANA_NRO"),
                        EstimadoSinPorcentaje = rdr.Get<int?>("E_SIN_PORC"),
                        EstimadoConPorcentaje = rdr.Get<int?>("E_CON_PORC"),
                        PorcentajeSemana = rdr.Get<int?>("P_SEMANA")
                    };

                    estim.Semanas.Add(semana);
                }
            }
            catch (Exception ex)
            {

            }

            // Orden opcional de semanas
            foreach (var e in dict.Values)
            {
                e.Semanas = e.Semanas
                    .OrderBy(s => s.Pos ?? int.MaxValue)
                    .ThenBy(s => s.SemanaNumero)
                    .ToList();
            }

            return dict.Values.ToList();
        }

        private static EstructuraDistribucionDto BuildTree(
     List<RowFlat> rows,
     EstimacionBisemanalQueryDto req,
     IReadOnlyList<SemanaVigenteRow> semanasProvider // ← lista resuelta
 )
        {
            var root = new EstructuraDistribucionDto
            {
                PesoBaseEspecie = rows.FirstOrDefault()?.PesoBaseEspecie,
                Especie = rows.FirstOrDefault()?.Especie,
                Items = new List<ItemNode>()
            };

            var itemGroups = rows.GroupBy(r => new
            {
                r.IdProductor,
                r.Productor,
                r.Variedad,
                r.Agronomo,
                r.EnvaseId,
                r.EnvaseNombre,
                r.EnvaseKilo
            });

            foreach (var g in itemGroups)
            {
                var any = g.First();

                var item = new ItemNode
                {
                    Id_Productor = g.Key.IdProductor,
                    Productor = g.Key.Productor,
                    Variedad = g.Key.Variedad,
                    Agronomo = g.Key.Agronomo,
                    DistribucionCalibre = any.DistribucionCalibre,
                    DistribucionCategoria = any.DistribucionCategoria,
                    EnvaseCosechero = new EnvaseCosecheroNode
                    {
                        Id = g.Key.EnvaseId,
                        Nombre = g.Key.EnvaseNombre,
                        Kilo = g.Key.EnvaseKilo
                    }
                };

                // Representante con estimación (si existe)
                var anyEst = g.FirstOrDefault(r => r.Est_ID.HasValue && r.Est_ID.Value >= 0);

                // Estimación SIEMPRE “llena pero null”
                var est = new EstimacionNode
                {
                    ID = anyEst?.Est_ID,
                    Contratado = anyEst?.Est_Contratado,
                    FCosecha = anyEst?.Est_FCosecha,
                    Semanas = new SemanasNode
                    {
                        Anterior = new SemanaValorNode { Estimado = anyEst?.Ant_Estimado, Producido = anyEst?.Ant_Producido },
                        Siguiente = new SemanaValorNode { Estimado = anyEst?.Sig_Estimado, Producido = anyEst?.Sig_Producido },
                        Bisemanal = new List<BisemanalNode>()
                    }
                };

                // 1) Elegir semanas esperadas (N consecutivas) desde el provider
                int n = req.WeeksPerPage <= 0 ? 2 : req.WeeksPerPage;
                var expectedRows = PickWeeks(semanasProvider, req.AnioBase, req.SemanaBase, g, n);

                // 2) Sembrar placeholders desde provider (7 días con fechas + nulls)
                var byKey = expectedRows.ToDictionary(
                    s => $"{s.AnioBase:D4}-{ToWeek2(s.SemanaBase)}",
                    s => BuildEmptyFromSemanaRow(s)
                );

                // 3) Agrupar lo que traiga el SP por (Año + Semana string normalizada)
                var bisGroups = g.Where(r => r.Bis_ID.HasValue
                                          || (r.Bis_AnioBase.HasValue && !string.IsNullOrWhiteSpace(r.Bis_SemanaBase)))
                                 .GroupBy(r => new
                                 {
                                     r.Bis_AnioBase,                         // int?
                                     Semana = ToWeek2(r.Bis_SemanaBase!),   // string "01".."53"
                                     r.Bis_ID,
                                     r.Bis_DistFrio,
                                     r.Bis_DistPacking,
                                     r.Bis_PorcExport
                                 });

                // 4) Pisar placeholders con los datos reales (si caen dentro del rango N)
                foreach (var bg in bisGroups)
                {
                    if (!bg.Key.Bis_AnioBase.HasValue) continue;
                    var key = $"{bg.Key.Bis_AnioBase.Value:D4}-{bg.Key.Semana}";
                    if (!byKey.TryGetValue(key, out var bis)) continue; // fuera del slice pedido

                    // Metadatos de la semana
                    bis.ID = bg.Key.Bis_ID;
                    bis.AnioBase = bg.Key.Bis_AnioBase;
                    bis.SemanaBase = bg.Key.Semana;
                    bis.DistribucionFrio = bg.Key.Bis_DistFrio;
                    bis.DistribucionPacking = bg.Key.Bis_DistPacking;
                    bis.PorcentajeExportacion = bg.Key.Bis_PorcExport;

                    // Días: mapeo por nombre o por fecha exacta
                    foreach (var d in bg)
                    {
                        int idx = -1;
                        if (!string.IsNullOrWhiteSpace(d.Dia_Nombre))
                        {
                            var nom = d.Dia_Nombre.Trim().ToUpperInvariant();
                            idx = Array.FindIndex(_diasEs, x => x == nom);
                        }
                        if (idx < 0 && d.Dia_Fecha.HasValue)
                        {
                            idx = bis.Dias!.FindIndex(x => x.FechaDia.HasValue &&
                                                           x.FechaDia.Value.Date == d.Dia_Fecha.Value.Date);
                        }
                        if (idx < 0 || idx >= 7) continue;

                        var dia = bis.Dias![idx];
                        dia.Estimado = d.Dia_Estimado;
                        dia.Producido = d.Dia_Producido;

                        if (d.Dia_Fecha.HasValue) dia.FechaDia = d.Dia_Fecha;
                        if (!string.IsNullOrWhiteSpace(d.Dia_Nombre)) dia.NombreDia = d.Dia_Nombre;
                    }
                }

                // 5) Orden final: exactamente como expectedRows
                est.Semanas!.Bisemanal = expectedRows
                    .Select(s => byKey[$"{s.AnioBase:D4}-{ToWeek2(s.SemanaBase)}"])
                    .ToList();

                item.Estimacion = est;
                root.Items!.Add(item);
            }

            return root;
        }

        //HELPERS

        private static readonly string[] _diasEs = new[] { "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SABADO", "DOMINGO" };


        private static string ToWeek2(string s)
        {
            s = (s ?? "").Trim();
            return s.Length == 1 ? "0" + s : s;
        }

        private static string ToWeek2(int w) => w.ToString("00", CultureInfo.InvariantCulture);

        // Construye una semana (BisemanalNode) con 7 días en null usando INICIO..TERMINO
        private static BisemanalNode BuildEmptyFromSemanaRow(SemanaVigenteRow m)
        {
            // Si INICIO no fuera lunes, puedes alinear así:
            // var monday = m.Inicio.Date.AddDays((7 + (int)DayOfWeek.Monday - (int)m.Inicio.DayOfWeek) % 7);
            var monday = m.Inicio.Date;

            var dias = new List<DiaNode>(7);
            for (int i = 0; i < 7; i++)
            {
                dias.Add(new DiaNode
                {
                    NombreDia = _diasEs[i],
                    FechaDia = monday.AddDays(i),
                    Estimado = null,
                    Producido = null
                });
            }

            return new BisemanalNode
            {
                ID = null,
                AnioBase = m.AnioBase,
                SemanaBase = ToWeek2(m.SemanaBase), // siempre 2 dígitos
                DistribucionFrio = null,
                DistribucionPacking = null,
                PorcentajeExportacion = null,
                Dias = dias
            };
        }

        private static List<SemanaVigenteRow> PickWeeks(
    IEnumerable<SemanaVigenteRow> all,
    int? reqAnioBase,
    string? reqSemanaBase,
    IEnumerable<RowFlat> grupoFilas, // por si quieres tomar la menor del grupo
    int weeksPerPage
)
        {
            var ordered = all
                .OrderBy(x => x.AnioBase)
                .ThenBy(x => int.Parse(ToWeek2(x.SemanaBase)))
                .ToList();

            // 1) Semilla desde request
            if (reqAnioBase.HasValue && !string.IsNullOrWhiteSpace(reqSemanaBase))
            {
                var ww = ToWeek2(reqSemanaBase!);
                var idx = ordered.FindIndex(s => s.AnioBase == reqAnioBase.Value && ToWeek2(s.SemanaBase) == ww);
                if (idx >= 0) return ordered.Skip(idx).Take(weeksPerPage).ToList();
            }

            // 2) Semilla desde los datos del grupo (la menor año/semana que exista en provider)
            var cand = grupoFilas
                .Where(r => r.Bis_AnioBase.HasValue && !string.IsNullOrWhiteSpace(r.Bis_SemanaBase))
                .Select(r => new { Anio = r.Bis_AnioBase!.Value, Semana = ToWeek2(r.Bis_SemanaBase!) })
                .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                .FirstOrDefault();

            if (cand is not null)
            {
                var idx2 = ordered.FindIndex(s => s.AnioBase == cand.Anio && ToWeek2(s.SemanaBase) == cand.Semana);
                if (idx2 >= 0) return ordered.Skip(idx2).Take(weeksPerPage).ToList();
            }

            // 3) Semilla por semana vigente (hoy ∈ [Inicio, Termino])
            var today = DateTime.Today;
            var vigente = ordered.FirstOrDefault(s => s.Inicio.Date <= today && today <= s.Termino.Date);
            if (vigente is not null)
            {
                var idx3 = ordered.IndexOf(vigente);
                return ordered.Skip(idx3).Take(weeksPerPage).ToList();
            }

            // 4) Fallback: primeras N de la temporada
            return ordered.Take(weeksPerPage).ToList();
        }


    }

}
