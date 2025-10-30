using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Builders;
using ProvexBackendAPI.Helpers.Mapping;
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
                // Usa tu mapper de feature:
                rows.Add(rdr.MapRowFlat());
            }

            return EstimacionesTreeBuilder.BuildTree(rows, req, semanas);
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
                                Nombre = rdr.Get<string?>("NOM_ENVASE_COSECHA") ?? "",
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
                    check.Parameters.AddWithValue("@FECHA", dto.Dia.FechaDia);

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
                    cmd.Parameters.AddWithValue("@FECHA", dto.Dia.FechaDia);
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


        //HELPERS

        private static readonly string[] _diasEs = new[] { "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SABADO", "DOMINGO" };



        private static string StripDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";
            var norm = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(capacity: norm.Length);
            foreach (var ch in norm)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }



        private static string ToWeek2(string s)
        { 
            s = (s ?? "").Trim();
            return s.Length == 1 ? "0" + s : s;
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
        ParseDayNamePercentList(string? raw,char[]? daySeps = null, char[]? innerPairSeps = null, char[]? innerKvSeps = null)      
        {
            var result = new List<(string Day, string Name, string PercentText, double? PercentValue)>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            
            daySeps ??= new[] { '|', ';', ',' };
            innerPairSeps ??= new[] { ',', ';' };
            innerKvSeps ??= new[] { ':', '=' };

            var dayChunks = SplitOutsideParentheses(raw, daySeps);

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
                    result.Add((part, "", "", null));
                    continue;
                }

                var day = m.Groups[1].Value.Trim();
                var inner = m.Groups[2].Value.Trim();

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
                   
                    pairs.Add((part, "", null));
                    continue;
                }

                var name = part[..idx].Trim();
                var rawVal = part[(idx + 1)..].Trim();

                var percentText = rawVal; 

              
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
