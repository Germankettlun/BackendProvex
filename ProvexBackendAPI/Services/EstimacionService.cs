using Azure.Core;
using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Helpers.Builders;
using ProvexBackendAPI.Helpers.Validation;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using static ProvexBackendAPI.Dto.EstimacionesDto;

namespace ProvexBackendAPI.Services
{
    public class EstimacionService : IEstimacionService
    {
        private readonly IGenericRepository repository;
        private readonly ITemporadasService temporada;

        public EstimacionService(IGenericRepository repository, ITemporadasService temporada)
        {
            this.repository = repository;
            this.temporada = temporada;
        }

        public async Task<EstructuraDistribucionDto> GetEstimacionBisemanalAsync(EstimacionBisemanalQueryDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (string.IsNullOrWhiteSpace(req.CodEmpresa))
                throw new ArgumentException("CodEmpresa es obligatorio.", nameof(req.CodEmpresa));

            if (req.Page <= 0)
                throw new ArgumentException("Page debe ser mayor a 0.", nameof(req.Page));

            if (req.WeeksPerPage <= 0)
                throw new ArgumentException("WeeksPerPage debe ser mayor a 0.", nameof(req.WeeksPerPage));


            var rows = new List<RowFlat>();

            var semanas = await temporada.ListSemanaAsync(codigoEmpresa: req.CodEmpresa, codigoTemporada: req.IdTemporada);

            var parameters = new SqlParameter[]
                {
                    new SqlParameter("@COD_EMPRESA", req.CodEmpresa.Trim().ToUpperInvariant()),
                    new SqlParameter("@ID_TEMPORADA", req.IdTemporada.Trim().ToUpperInvariant()),
                    new SqlParameter("@COD_GRUPO_PRODUCTOR", req.CodGrupoProductor),
                    new SqlParameter("@ID_ESPECIE", req.IdEspecie.Trim().ToUpperInvariant()),
                    new SqlParameter("@ID_PRODUCTOR",(object?)req.IdProductor?.Trim().ToUpperInvariant() ?? DBNull.Value),
                    new SqlParameter("@ID_VARIEDAD", (object?)req.IdVariedad?.Trim().ToUpperInvariant() ?? DBNull.Value),
                    new SqlParameter("@ANIO_BASE", req.AnioBase),
                    new SqlParameter("@SEMANA_BASE", req.SemanaBase),
                    new SqlParameter("@PAGE", req.Page),
                    new SqlParameter("@WEEKS_PER_PAGE", req.WeeksPerPage),
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_EstimacionBisemanal", parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                rows.Add(MapRowFlat(row));
            }

            return EstimacionesTreeBuilder.BuildTree(rows, req, semanas);

        }

        public async Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion)
        {
            //Diccionarios auxiliares 
            var estimaciones = new Dictionary<string, EstimacionSemanalDto>(StringComparer.OrdinalIgnoreCase);
            var semanasIndexPorEstim = new Dictionary<string, Dictionary<string, SemanaEstimacionDto>>(StringComparer.OrdinalIgnoreCase);

            var parameters = new SqlParameter[]
                {
                    new SqlParameter("@COD_EMPRESA", codigoEmpresa.Trim().ToUpperInvariant()),
                    new SqlParameter("@ID_TEMPORADA", idTemporada.Trim().ToUpperInvariant()),
                    new SqlParameter("@ID_ESTIMACION", idEstimacion),
                };

            var dataTable = await repository.GetDataTable("Estimaciones.usp_UI_Estimacion_ResumenSemanal", parameters);

            //Mapear DataTable a lista tipada
            var filas = dataTable.AsEnumerable().Select(MapResumenSemanalRow).ToList();

            //Recorrer las filas tipadas y armar los DTOs
            foreach (var fila in filas)
            {
                var idEstim = fila.IdEstimacion ?? "";

                if (!estimaciones.TryGetValue(idEstim, out var estim))
                {
                    estim = new EstimacionSemanalDto
                    {
                        IdEstimacion = idEstim,
                        Contratado = fila.Contratado ?? 0,
                        KilosBaseEspecie = fila.KilosBaseEspecie ?? 0,

                        EnvaseCosechero = new EnvaseCosecheroNode
                        {
                            Id = fila.IdEnvaseCosecha,
                            Nombre = fila.NomEnvaseCosecha ?? "",
                            Kilo = fila.KilosEnvase ?? 0.0
                        },

                        Totales = new TotalesEstimacionDto
                        {
                            EstimadoSinPorcentaje = fila.Total_E_Sin_Porc,
                            EstimadoConPorcentaje = fila.Total_E_Con_Porc,
                            Proyectado = fila.Total_P,
                            DiferenciaEstimadoConProyectado = fila.Dif_E_Con_P
                        },

                        Semanas = new List<SemanaEstimacionDto>()
                    };

                    estimaciones[idEstim] = estim;
                    semanasIndexPorEstim[idEstim] =
                        new Dictionary<string, SemanaEstimacionDto>(StringComparer.OrdinalIgnoreCase);
                }

                var semanasIndex = semanasIndexPorEstim[idEstim];

                // CLAVE DE SEMANA
                var pos = fila.Pos;
                var anio = fila.Anio ?? 0;
                var semNro = fila.Semana_Nro;

                var weekKey = $"{anio}|{semNro}|{(pos.HasValue ? pos.Value.ToString() : "-")}";

           
                if (!semanasIndex.TryGetValue(weekKey, out var semana))
                {
                    semana = new SemanaEstimacionDto
                    {
                        Pos = pos,
                        Anio = anio,
                        SemanaNumero = semNro,
                        EstimadoSinPorcentaje = fila.E_Sin_Porc,
                        EstimadoConPorcentaje = fila.E_Con_Porc,
                        PorcentajeSemana = fila.P_Semana,

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
                    // refrescar métricas si vienen nulas en la semana
                    semana.EstimadoSinPorcentaje ??= fila.E_Sin_Porc;
                    semana.EstimadoConPorcentaje ??= fila.E_Con_Porc;
                    semana.PorcentajeSemana ??= fila.P_Semana;
                }

                //Distribución por CATEGORÍA 
                semana.DistribucionCategoria = MapPairs(
                    fila.Categorias_Semanas,
                    (nombre, porcentajeTxt) => new DistribucionCategoriaPorSemanaNode
                    {
                        nombreCategoria = nombre,
                        Porcentaje = porcentajeTxt
                    });

                //Distribución por CALIBRE 
                semana.DistribucionCalibre = MapPairs(
                    fila.Calibres_Semana,
                    (nombre, porcentajeTxt) => new DistribucionCalibrePorSemanaNode
                    {
                        nombreCalibre = nombre,
                        Porcentaje = porcentajeTxt
                    });

                //PACKING por día
                semana.PackingPorDia = BuildPackingPorDia(fila.Packings_Dia_Semana);

                //FRIGORÍFICO por día
                semana.FrigorificoPorDia = BuildFrigorificoPorDia(fila.Frigorificos_Dia_Semana);
            }

            // Ordenar semanas
            foreach (var estim in estimaciones.Values)
            {
                estim.Semanas = estim.Semanas
                    .OrderBy(s => s.Pos ?? int.MaxValue)
                    .ThenBy(s => s.SemanaNumero)
                    .ToList();
            }

            return estimaciones.Values.ToList();
        }

        public async Task IngresarEstimacion(IngresarEstimacionRequest request, Guid userId)
        {
            try
            {
               

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@id_estimacion", request.idEstimacion ?? null),
                    new SqlParameter("@id_empresa", request.idEmpresa),
                    new SqlParameter("@id_temporada", request.idTemporada),
                    new SqlParameter("@id_especie", request.idEspecie),
                    new SqlParameter("@id_variedad", request.idVariedad),
                    new SqlParameter("@id_productor", request.idProductor),
                    new SqlParameter("@semana_inicio", request.semanaInicio),
                    new SqlParameter("@anio_inicio", request.anioInicio),
                    new SqlParameter("@porc_exportacion", request.porcExportacion),
                    new SqlParameter("@frigorifico", request.frigorifico),
                    new SqlParameter("@packing", request.packing),
                    new SqlParameter("@envase", request.envase),
                    new SqlParameter("@contratado", request.contratado),
                    new SqlParameter("@id_usuario_guid", userId)
                };
                    
                await repository.SpVoid("Estimaciones.sp_IngresarEstimacion", parameters);
                
            }
            catch (Exception)
            {
                throw new Exception("Error al crear estimación");
            }
        }

        public async Task IngresarPorcentajeExportacionSemanal(PorcentajeExportacionSemanalDTO input, Guid userId)
        {
            
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@idEstimacion", input.idEstimacion),
                    new SqlParameter("@anio", input.anio),
                    new SqlParameter("@semana", input.semana),
                    new SqlParameter("@porcentaje", input.porcentaje),
                    new SqlParameter("@idUsuario_guid", userId)
                };

                await repository.SpVoid("Estimaciones.usp_INSERT_UPDATE_Procentaje_Exportacion_Semanal", parameters);
            }
            catch (Exception)
            {
                throw new Exception("Error al actualizar el porcentaje semanal");
            }
        }

        public async Task<List<ZonaDTO>> ObtenerZonas(string codEmpresa)
        {
            try
            {
                var res = await repository.GetList<Zona>(z => z.idEmpresa == codEmpresa);

                List<ZonaDTO> zonas = new List<ZonaDTO>();
                
                zonas = [.. res.Select(item => new ZonaDTO
                {
                    idEmpresa = item.idEmpresa,
                    nombre = item.nombre
                })];

                return zonas;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public async Task UpsertDiaAsync(UpdateEstimacionBisemanalRequest dto, Guid userId)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (dto.IdEstimacion <= 0)
                throw new ArgumentException("IdEstimacion inválido.", nameof(dto.IdEstimacion));

            if (dto.ValorNuevo < 0)
                throw new ArgumentException("ValorNuevo no puede ser negativo.", nameof(dto.ValorNuevo));
            try
            {

                var exists = await repository.Exists<EstimacionBisemanal>(e => e.idEstimacion == dto.IdEstimacion&& e.fecha.Date == dto.Dia.FechaDia.Date);

                var cajas = Convert.ToInt32(Math.Round(dto.ValorNuevo, 0, MidpointRounding.AwayFromZero));

                //  UPDATE si existe; INSERT si no existe
                var query = exists ? "[Estimaciones].[usp_UPDATE_EstimacionBisemanal_Dia]" : "[Estimaciones].[usp_INSERT_EstimacionBisemanal_Dia]";

                var parameters = new SqlParameter[]
               {
                    new SqlParameter("IDESTIMACION", dto.IdEstimacion),
                    new SqlParameter("FECHA", dto.Dia.FechaDia),
                    new SqlParameter("CAJAS", cajas),
                    new SqlParameter("@IDUSUARIO_GUID", userId)
               };

                await repository.SpVoid(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar UpsertDiaAsync para estimación bisemanal.", ex);
            }


        }
        //HELPERS ResumenSemanal
        //HELPER MAPEO
        private static ResumenSemanalRowDto MapResumenSemanalRow(DataRow row)
        {
            return new ResumenSemanalRowDto
            {
                IdEstimacion = row.IsNull("ID_ESTIMACION") ? null : row["ID_ESTIMACION"]?.ToString(),
                Contratado = row["CONTRATADO"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CONTRATADO"]),
                KilosBaseEspecie = row["KILOS_BASE"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["KILOS_BASE"]),

                IdEnvaseCosecha = row.IsNull("ID_ENVASE_COSECHA") ? null : row["ID_ENVASE_COSECHA"]?.ToString(),
                NomEnvaseCosecha = row.Table.Columns.Contains("NOM_ENVASE_COSECHA") ? (row.IsNull("NOM_ENVASE_COSECHA") ? "" : row["NOM_ENVASE_COSECHA"]?.ToString()) : "",
                KilosEnvase = row.IsNull("KILOS_ENVASE") ? (double?)null : Convert.ToDouble(row["KILOS_ENVASE"]),

                Total_E_Sin_Porc = row["TOTAL_E_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_E_SIN_PORC"]),
                Total_E_Con_Porc = row["TOTAL_E_CON_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_E_CON_PORC"]),
                Total_P = row["TOTAL_P"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_P"]),
                Dif_E_Con_P = row["DIF_E_CON_P"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["DIF_E_CON_P"]),

                Pos = row["POS"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["POS"]),
                Anio = row["ANIO"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ANIO"]),
                Semana_Nro = row.IsNull("SEMANA_NRO") ? null : row["SEMANA_NRO"]?.ToString(),

                E_Sin_Porc = row["TOTAL_E_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_E_SIN_PORC"]),
                E_Con_Porc = row["TOTAL_E_CON_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_E_CON_PORC"]),
                P_Semana = row["P_SEMANA"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["P_SEMANA"]),

                Categorias_Semanas = row.IsNull("CATEGORIAS_SEMANA") ? null : row["CATEGORIAS_SEMANA"]?.ToString(),
                Calibres_Semana = row.IsNull("CALIBRES_SEMANA") ? null : row["CALIBRES_SEMANA"]?.ToString(),
                Packings_Dia_Semana = row.IsNull("PACKINGS_DIA_SEMANA") ? null : row["PACKINGS_DIA_SEMANA"]?.ToString(),
                Frigorificos_Dia_Semana = row.IsNull("FRIGORIFICOS_DIA_SEMANA")? null : row["FRIGORIFICOS_DIA_SEMANA"]?.ToString(),
            };
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
        ParseDayNamePercentList(string? raw, char[]? daySeps = null, char[]? innerPairSeps = null, char[]? innerKvSeps = null)
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


        //   Helper: split fuera de paréntesis

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


        //   Helper: "Nombre:60%, Otro:40%" → pares

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

        //HELPERS FRIGORIFICO Y PACKING
        public static List<TOut> MapPairs<TOut>(string? raw,Func<string, string, TOut> factory,char[]? pairSeps = null, char[]? kvSeps = null)
        {
            var pairs = ParseNamePercentPairs(raw, pairSeps, kvSeps);
            var list = new List<TOut>(pairs.Count);
            foreach (var (name, percentText, _) in pairs)
            {
                list.Add(factory(name, percentText));
            }
            return list;
        }

        //HELPERS EstimacionBisemanal

        private static RowFlat MapRowFlat(DataRow row)
        {
            return new RowFlat
            {
                // Raíz
                PesoBaseEspecie = row["ESPECIE_KILO_BASE"] == DBNull.Value? 0.0 : Convert.ToDouble(row["ESPECIE_KILO_BASE"]),

                Especie = row.Table.Columns.Contains("NOM_ESP") ? (row.IsNull("NOM_ESP") ? "" : row["NOM_ESP"]?.ToString() ?? "") : "",

                // Item
                IdProductor = row.Table.Columns.Contains("ID_PRODUCTOR") ? (row.IsNull("ID_PRODUCTOR") ? "" : row["ID_PRODUCTOR"]?.ToString() ?? ""): "",

                Productor = row.Table.Columns.Contains("NOM_PROD") ? (row.IsNull("NOM_PROD") ? "" : row["NOM_PROD"]?.ToString() ?? "") : "",

                Variedad = row.Table.Columns.Contains("NOM_VAR") ? (row.IsNull("NOM_VAR") ? "" : row["NOM_VAR"]?.ToString() ?? "") : "",

                Agronomo = row.Table.Columns.Contains("NOM_USUARIO_AGRONOMO")  ? (row.IsNull("NOM_USUARIO_AGRONOMO") ? "" : row["NOM_USUARIO_AGRONOMO"]?.ToString() ?? "") : "",

                DistribucionCalibre = row["DIST_CAL"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_CAL"]),

                DistribucionCategoria = row["DIST_CAT"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_CAT"]),

                PorcentajeExportacion = row["PCT_EXP_PORC"] == DBNull.Value ? 0 : Convert.ToInt32(row["PCT_EXP_PORC"]),

                // Envase
                EnvaseId = row.Table.Columns.Contains("ENVASE_ID") ? (row.IsNull("ENVASE_ID") ? "" : row["ENVASE_ID"]?.ToString() ?? "") : "",

                EnvaseNombre = row.Table.Columns.Contains("NOM_ENVASE_COSECHA") ? (row.IsNull("NOM_ENVASE_COSECHA") ? "" : row["NOM_ENVASE_COSECHA"]?.ToString() ?? "") : "",

                EnvaseKilo = row["KG_DIA_ENVASE"] == DBNull.Value ? 0 : Convert.ToInt32(row["KG_DIA_ENVASE"]),

                // Estimación
                Est_ID = row["ID_ESTIMACION"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ID_ESTIMACION"]),

                Est_Contratado = row["CAJAS_CONTRATADAS"] == DBNull.Value ? 0 : Convert.ToInt32(row["CAJAS_CONTRATADAS"]),

                Est_FCosecha = row.Table.Columns.Contains("FECHA_INICIO_COSECHA_YM") ? (row.IsNull("FECHA_INICIO_COSECHA_YM") ? "" : row["FECHA_INICIO_COSECHA_YM"]?.ToString() ?? "") : "",

                Ant_Estimado = row["CAJAS_E_ANTERIOR_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_E_ANTERIOR_SIN_PORC"]),

                Ant_Producido = row["CAJAS_P_ANTERIOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_P_ANTERIOR"]),

                Sig_Estimado = row["CAJAS_E_SIGUIENTE_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_E_SIGUIENTE_SIN_PORC"]),

                Sig_Producido = row["CAJAS_P_SIGUIENTE_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_P_SIGUIENTE_SIN_PORC"]),

                // Bisemanal
                Bis_AnioBase = row["ANIO"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ANIO"]),

                Bis_SemanaBase = row.Table.Columns.Contains("SEMANA_NRO") ? (row.IsNull("SEMANA_NRO") ? "" : row["SEMANA_NRO"]?.ToString() ?? "") : "",

                // Días
                Bis_ID = row["ID_ESTIMACION_BISEMANAL"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ID_ESTIMACION_BISEMANAL"]),

                Dia_Nombre = row.Table.Columns.Contains("NOMBRE_DIA") ? (row.IsNull("NOMBRE_DIA") ? "" : row["NOMBRE_DIA"]?.ToString() ?? "") : "",

                Dia_Fecha = row["DIA"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DIA"]),

                Dia_Estimado = row["CAJAS_E_DISTRIB_SIN_PORC"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["CAJAS_E_DISTRIB_SIN_PORC"]),

                Dia_Producido = row["CAJAS_P"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["CAJAS_P"]),

                Dia_DistribucionFrio = row["DIST_FRI"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_FRI"]),

                Dia_DistribucionPacking = row["DIST_PACK"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_PACK"])
            };
        }
    }
}
