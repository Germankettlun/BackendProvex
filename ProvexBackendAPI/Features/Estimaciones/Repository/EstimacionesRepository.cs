using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using System.Data;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
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

            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_EstimacionBisemanal_new", conn)
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
                    
                    Bis_AnioBase = rdr.Get<int?>("ANIO"),
                    Bis_SemanaBase = rdr.FirstExistingAsString("SEMANA_NRO"),                    
                    Bis_PorcExport = rdr.Get<int?>("PCT_EXP_PORC") ?? 0,

                    // Días
                    Bis_ID = rdr.Get<int?>("ID_ESTIMACION_BISEMANAL"),
                    Dia_Nombre = rdr.FirstExistingAsString("NOMBRE_DIA"),
                    Dia_Fecha = rdr.Get<DateTime?>("DIA"),
                    Dia_Estimado = rdr.Get<decimal?>("CAJAS_ESTIMADAS_SIN_PORC"),
                    Dia_Producido = rdr.Get<decimal?>("CAJAS_P"),
                    Dia_DistribucionFrio = rdr.Get<bool?>("DIST_FRI"),
                    Dia_DistribucionPacking = rdr.Get<bool?>("DIST_PACK")
                });
            }

            return BuildTree(rows, req, semanas);
        }


        public async Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion)
        {
            var estimaciones = new Dictionary<string, EstimacionSemanalDto>(StringComparer.OrdinalIgnoreCase);

            // Índice auxiliar por estimación para "get or create" de semanas
            var semanasIndexPorEstim = new Dictionary<string, Dictionary<string, SemanaEstimacionDto>>(StringComparer.OrdinalIgnoreCase);

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
                    
                    var idEstim = rdr.Get<string?>("ID_ESTIMACION") ?? string.Empty;

                    if (!estimaciones.TryGetValue(idEstim, out var estim))
                    {
                        estim = new EstimacionSemanalDto
                        {
                            IdEstimacion = idEstim,
                            Contratado = rdr.Get<int?>("CONTRATADO") ?? 0,
                            KilosBaseEspecie = rdr.Get<int?>("KILOS_BASE") ?? 0,
                            EnvaseCosechero = new EnvaseCosecheroNode
                            {
                                Id = rdr.Get<string?>("ID_ENVASE_COSECHA"),
                                Nombre = rdr.Get<string?>("NOMBRE_ENVASE_COSECHA") ?? "",
                                Kilo = rdr.Get<double?>("KILOS_ENVASE") ?? 0.0,
                            },
                            Totales = new TotalesEstimacionDto
                            {
                                EstimadoSinPorcentaje = rdr.Get<int?>("TOTAL_E_SIN_PORC"),
                                EstimadoConPorcentaje = rdr.Get<int?>("TOTAL_E_CON_PORC"),
                                Proyectado = rdr.Get<int?>("TOTAL_P"),
                                DiferenciaEstimadoConProyectado = rdr.Get<int?>("DIF_E_CON_P")
                            },
                            Semanas = new List<SemanaEstimacionDto>()
                        };

                        estimaciones[idEstim] = estim;
                        semanasIndexPorEstim[idEstim] = new Dictionary<string, SemanaEstimacionDto>(StringComparer.OrdinalIgnoreCase);
                    }

                    var semanasIndex = semanasIndexPorEstim[idEstim];

                    // ====== CLAVE DE SEMANA ======
                    var pos = rdr.Get<int?>("POS");
                    var anio = rdr.Get<int?>("ANIO") ?? 0;
                    var semNro = rdr.Get<string?>("SEMANA_NRO");

                    // clave compuesta para no duplicar semanas
                    var weekKey = $"{anio}|{semNro}|{(pos.HasValue ? pos.Value.ToString() : "-")}";

                    // ====== GET OR CREATE Semana ======
                    if (!semanasIndex.TryGetValue(weekKey, out var semana))
                    {
                        semana = new SemanaEstimacionDto
                        {
                            Pos = pos,
                            Anio = anio,
                            SemanaNumero = semNro,
                            EstimadoSinPorcentaje = rdr.Get<int?>("E_SIN_PORC"),
                            EstimadoConPorcentaje = rdr.Get<int?>("E_CON_PORC"),
                            PorcentajeSemana = rdr.Get<int?>("P_SEMANA"),

                            // Inicializa listas
                            DistribucionCategoria = new List<DistribucionCategoriaPorSemanaNode>(),
                            DistribucionCalibre = new List<DistribucionCalibrePorSemanaNode>(),
                            PackingPorDia = new List<Semana_DistribucionPackingPorDia>(),
                            FrigorificoPorDia = new List<Semana_DistribucionFrigorificoPorDia>()
                        };

                        semanasIndex[weekKey] = semana;
                        estim.Semanas.Add(semana);
                    }
                    else
                    {
                        // Si ya existe la semana, opcionalmente refresca métricas base cuando vengan nulas
                        semana.EstimadoSinPorcentaje ??= rdr.Get<int?>("E_SIN_PORC");
                        semana.EstimadoConPorcentaje ??= rdr.Get<int?>("E_CON_PORC");
                        semana.PorcentajeSemana ??= rdr.Get<int?>("P_SEMANA");
                    }

                    // ====== NODOS: Distribución por CATEGORÍA (por semana) ======
                    // Reemplaza XXX_XXX por tus columnas reales
                    var categorias = rdr.Get<string?>("CATEGORIAS_SEMANAS");


                    semana.DistribucionCategoria = MapPairs(categorias, (nombre, porcentajeTxt) => new DistribucionCategoriaPorSemanaNode
                    {
                        nombreCategoria = nombre,
                        Porcentaje = porcentajeTxt
                    }
                    );

                    // ====== NODOS: Distribución por CALIBRE (por semana) ======
                    var calibres = rdr.Get<string?>("CALIBRES_SEMANA");


                    semana.DistribucionCalibre = MapPairs(calibres, (nombre, porcentajeTxt) => new DistribucionCalibrePorSemanaNode
                    {
                        nombreCalibre = nombre,
                        Porcentaje = porcentajeTxt
                    }
                    );

                    // ====== NODOS: PACKING por DÍA ======
                    string? PackingPorDia = rdr.Get<string?>("PACKINGS_DIA_SEMANA");
                    // Ej: "lunes (ProvAgro:60%), martes (ProvAgro:40%)"

                  

                    semana.PackingPorDia = BuildPackingPorDia(PackingPorDia);
                    

                    // ====== NODOS: FRIGORÍFICO por DÍA ======
                    string? FrigorificoPorDia = rdr.Get<string?>("FRIGORIFICOS_DIA_SEMANA");
                    // Ej: "lunes (ProvAgro:60%), martes (ProvAgro:40%)"

                    semana.FrigorificoPorDia = BuildFrigorificoPorDia(FrigorificoPorDia);
                }
            }
            catch (Exception ex)
            {
                // Manejo mínimo; si ya tienes middleware/log, propaga o registra:
                // _logger.LogError(ex, "Error en GetResumenSemanalAsync");
                throw;
            }

            // Orden semanas
            foreach (var e in estimaciones.Values)
            {
                e.Semanas = e.Semanas
                    .OrderBy(s => s.Pos ?? int.MaxValue)
                    .ThenBy(s => s.SemanaNumero)
                    .ToList();
            }

            return estimaciones.Values.ToList();
        }



        public async Task<SpResultEstimacionBisemanalDto> UpsertDiaAsync(UpdateEstimacionBisemanalRequest dto, int? userId)
        {
            using var cn = new SqlConnection(_connString);
            await cn.OpenAsync();
           

            try
            {
                // Verifica si la estimación bisemanal ya existe
                var exists = false;
                using (var check = new SqlCommand("[Estimaciones].[usp_EXISTE_ESTIMACION_BISEMANAL]", cn))
                {
                    check.CommandType = CommandType.StoredProcedure;
                    check.Parameters.AddWithValue("@IDESTIMACION", dto.IdEstimacion);
                    check.Parameters.AddWithValue("@FECHA", dto.Dia);

                    var scalar = await check.ExecuteScalarAsync();
                    
                    exists = Convert.ToInt32(scalar) == 1;
                }

                var cajas = Convert.ToInt32(Math.Round(dto.ValorNuevo, 0, MidpointRounding.AwayFromZero));

                //  UPDATE si existe; INSERT si no existe
                using var cmd = new SqlCommand(exists
                    ? "[Estimaciones].[usp_UPDATE_EstimacionBisemanal_Dia]"
                    : "[Estimaciones].[usp_INSERT_EstimacionBisemanal_Dia]", cn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60
                };

                
                    cmd.Parameters.AddWithValue("@IDESTIMACION", dto.IdEstimacion);                   
                    cmd.Parameters.AddWithValue("@CAJAS", cajas);
                    cmd.Parameters.AddWithValue("@FECHA", dto.Dia);
                    cmd.Parameters.AddWithValue("@IDUSUARIO", (object?)userId ?? DBNull.Value);
               

                var result = new SpResultEstimacionBisemanalDto();
                using (var rdr2 = await cmd.ExecuteReaderAsync())
                {
                    if (await rdr2.ReadAsync())
                    {
                        int? sInt = rdr2.Get<int?>("SUCCESS");
                       
                        bool ok = (sInt.HasValue && sInt.Value == 1);


                        if (!ok)
                        {
                            var errMsg = rdr2.Get<string?>("ERROR_MESSAGE")
                                      ?? rdr2.Get<string?>("MENSAJE")
                                      ?? "Operación fallida.";
                            throw new InvalidOperationException(errMsg);
                        }


                        result.IdEstimacion = exists? (rdr2.Get<int?>("ID_ESTIMACION") ?? dto.IdEstimacion) : (rdr2.Get<int?>("ID_INSERTADO") ?? dto.IdEstimacion);

                        result.Message = rdr2.Get<string?>("MENSAJE")
                                       ?? (exists ? "Actualizado" : "Insertado");
                    }
                    else
                    {
                        throw new InvalidOperationException("El procedimiento no devolvió resultado. No se pudo confirmar la operación.");
                    }
                   
                }

               
                return result;
            }
            catch
            {
               
                throw;
            }
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

                // 3) Agrupar por semana (sin distribución en la clave)
                var bisGroups = g.Where(r => r.Bis_ID.HasValue
                                          || (r.Bis_AnioBase.HasValue && !string.IsNullOrWhiteSpace(r.Bis_SemanaBase)))
                                 .GroupBy(r => new
                                 {
                                     r.Bis_AnioBase,                       // int?
                                     Semana = ToWeek2(r.Bis_SemanaBase!),  // "01".."53"
                                     
                                     r.Bis_PorcExport
                                 });

                // 4) Pisar placeholders con datos reales
                foreach (var bg in bisGroups)
                {
                    if (!bg.Key.Bis_AnioBase.HasValue) continue;
                    var key = $"{bg.Key.Bis_AnioBase.Value:D4}-{bg.Key.Semana}";
                    if (!byKey.TryGetValue(key, out var bis)) continue;

                    // Metadatos de la semana
                   
                    bis.AnioBase = bg.Key.Bis_AnioBase;
                    bis.SemanaBase = bg.Key.Semana;
                    bis.PorcentajeExportacion = bg.Key.Bis_PorcExport;

                    // Días: mapear valores y distribución POR DÍA
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

                        // Valores base
                        dia.IdBisemanal = d.Bis_ID;
                        dia.Estimado = d.Dia_Estimado;
                        dia.Producido = d.Dia_Producido;
                        if (d.Dia_Fecha.HasValue) dia.FechaDia = d.Dia_Fecha;
                        if (!string.IsNullOrWhiteSpace(d.Dia_Nombre)) dia.NombreDia = d.Dia_Nombre;

                        // NUEVO: distribución por día
                        // Ideal: si tu SP ya trae columnas por día (p.ej. Dia_DistFrio / Dia_DistPacking)
                        // usa esas; si no existen aún, caes por defecto al valor de la semana del registro (si venían).
                        dia.DistribucionFrio = d.Dia_DistribucionFrio;
                        dia.DistribucionPacking = d.Dia_DistribucionPacking;
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

       

        // Construye una semana (BisemanalNode) con 7 días en null usando INICIO..TERMINO
        private static BisemanalNode BuildEmptyFromSemanaRow(SemanaVigenteRow m)
        {
            // Si INICIO no fuera lunes, puedes alinear así:
            // var monday = m.Inicio.Date.AddDays((7 + (int)DayOfWeek.Monday - (int)m.Inicio.DayOfWeek) % 7);
            var monday = m.Inicio.Date;

            var dias = new List<DiaValorNode>(7);
            for (int i = 0; i < 7; i++)
            {
                dias.Add(new DiaValorNode
                {
                    IdBisemanal = null,
                    NombreDia = _diasEs[i],
                    FechaDia = monday.AddDays(i),
                    Estimado = null,
                    Producido = null,
                    DistribucionFrio = null, 
                    DistribucionPacking = null
                });
            }

            return new BisemanalNode
            {
               
                AnioBase = m.AnioBase,
                SemanaBase = ToWeek2(m.SemanaBase), // siempre 2 dígitos              
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

        //Helper distribución packing / frigorifico

        public static List<Semana_DistribucionPackingPorDia> BuildPackingPorDia(string? raw)
        {
            var parsed = ParseDayNamePercentList(raw);
            return parsed
                .GroupBy(x => x.Day, StringComparer.OrdinalIgnoreCase)
                .Select(g => new Semana_DistribucionPackingPorDia
                {
                    nombreDia = g.Key,
                    Packings = g.Select(p => new NombrePorcentajeDto
                    {
                        Nombre = p.Name,
                        Porcentaje = p.PercentText
                    }).ToList()
                })
                .ToList();
        }

        public static List<Semana_DistribucionFrigorificoPorDia> BuildFrigorificoPorDia(string? raw)
        {
            var parsed = ParseDayNamePercentList(raw);
            return parsed
                .GroupBy(x => x.Day, StringComparer.OrdinalIgnoreCase)
                .Select(g => new Semana_DistribucionFrigorificoPorDia
                {
                    nombreDia = g.Key,
                    Frigorificos = g.Select(p => new NombrePorcentajeDto
                    {
                        Nombre = p.Name,
                        Porcentaje = p.PercentText
                    }).ToList()
                })
                .ToList();
        }
        public static List<(string Day, string Name, string PercentText, double? PercentValue)>
          ParseDayNamePercentList(
              string? raw,
              char[]? daySeps = null,          // separadores entre días (fuera de paréntesis)
              char[]? innerPairSeps = null,    // separadores entre pares dentro del paréntesis
              char[]? innerKvSeps = null)      // separadores nombre:porcentaje
        {
            var result = new List<(string Day, string Name, string PercentText, double? PercentValue)>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            daySeps ??= new[] { ';', ',' };
            innerPairSeps ??= new[] { ';', ',' };
            innerKvSeps ??= new[] { ':', '=' };

            // 1) separar en "bloques de día" solamente por separadores FUERA de paréntesis
            var dayChunks = SplitOutsideParentheses(raw, daySeps);

            // 2) regex para capturar Día y el contenido entre paréntesis
            var dayAndInnerRegex = new Regex(
                @"^\s*([^(]+?)\s*\(\s*(.*?)\s*\)\s*$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
            );

            foreach (var chunk in dayChunks)
            {
                var part = chunk.Trim();
                if (part.Length == 0) continue;

                var m = dayAndInnerRegex.Match(part);
                if (!m.Success)
                {
                    // Si no matchea, devolvemos el texto como "día" sin items (fallback suave)
                    result.Add((part, "", "", null));
                    continue;
                }

                var day = m.Groups[1].Value.Trim();    // "viernes"
                var inner = m.Groups[2].Value.Trim();  // "La Providencia:60%, Packing Test:40%"

                var pairs = ParseNamePercentPairs(inner, innerPairSeps, innerKvSeps);
                if (pairs.Count == 0)
                {
                    result.Add((day, "", "", null));
                    continue;
                }

                foreach (var (name, pctText, pctVal) in pairs)
                    result.Add((day, name, pctText, pctVal));
            }

            return result;
        }

        // ==========================================
        //   Helper: split fuera de paréntesis
        //   "A (x, y), B (z)" -> ["A (x, y)", "B (z)"]
        // ==========================================
        private static List<string> SplitOutsideParentheses(string text, char[] seps)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(text)) return list;

            int depth = 0;
            int start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '(') depth++;
                else if (c == ')') depth = Math.Max(0, depth - 1);
                else if (depth == 0 && Array.IndexOf(seps, c) >= 0)
                {
                    // separador a nivel tope → cortamos
                    var seg = text.Substring(start, i - start).Trim();
                    if (seg.Length > 0) list.Add(seg);
                    start = i + 1; // siguiente después del separador
                }
            }

            // último segmento
            var last = text.Substring(start).Trim();
            if (last.Length > 0) list.Add(last);

            return list;
        }

        // ==========================================
        //   Helper: "Nombre:60%, Otro:40%" → pares
        //   (split normal, aquí ya estamos dentro de paréntesis)
        // ==========================================
        private static List<(string Name, string PercentText, double? PercentValue)>
            ParseNamePercentPairs(string? inner, char[]? pairSeps, char[]? kvSeps)
        {
            var pairs = new List<(string, string, double?)>();
            if (string.IsNullOrWhiteSpace(inner)) return pairs;

            pairSeps ??= new[] { ';', ',' };
            kvSeps ??= new[] { ':', '=' };

            var chunks = inner.Split(pairSeps, StringSplitOptions.RemoveEmptyEntries);

            foreach (var chunk in chunks)
            {
                var part = chunk.Trim();
                if (part.Length == 0) continue;

                int idx = -1;
                foreach (var sep in kvSeps)
                {
                    idx = part.IndexOf(sep);
                    if (idx >= 0) break;
                }

                if (idx < 0)
                {
                    // No hay separador clave-valor → guarda el nombre y porcentaje vacío
                    pairs.Add((part, "", null));
                    continue;
                }

                var name = part[..idx].Trim();
                var rawVal = part[(idx + 1)..].Trim();

                var percentText = rawVal; // conserva "60%" tal cual

                // Limpieza para número (si te sirve PercentValue)
                var cleaned = rawVal.Replace("%", "").Trim().Replace(',', '.');
                if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    pairs.Add((name, percentText, val));
                else
                    pairs.Add((name, percentText, null));
            }

            return pairs;
        }

        public static List<TOut> MapPairs<TOut>(
        string? raw,
        Func<string, string, TOut> factory,
        char[]? pairSeps = null,
        char[]? kvSeps = null)
        {
            var pairs = ParseNamePercentPairs(raw, pairSeps, kvSeps);
            var list = new List<TOut>(pairs.Count);
            foreach (var (name, percentText, _) in pairs)
            {
                list.Add(factory(name, percentText));
            }
            return list;
        }

       
    }

}
