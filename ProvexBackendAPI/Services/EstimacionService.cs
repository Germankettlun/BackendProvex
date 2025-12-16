using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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

        public async Task<List<ResumenSemanalEstimacionDto>> GetResumenSemanalAsync(int idEstimacion)
        {

            var resumen = new Dictionary<string, ResumenSemanalEstimacionDto>(StringComparer.OrdinalIgnoreCase);

            //Diccionarios auxiliares
           
            var semanasIndexPorEstim = new Dictionary<string, Dictionary<string, SemanaEstimacionDto>>(StringComparer.OrdinalIgnoreCase);


            var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ID_ESTIMACION", idEstimacion),
                };

            var dataTable = await repository.GetDataTable("[Estimaciones].[usp_UI_EstimacionSemanal_Resumen]", parameters);

            var filas = dataTable.AsEnumerable().Select(MapResumenSemanalRow).Where(f => f.Anio.HasValue && f.Anio.Value > 0 && !string.IsNullOrWhiteSpace(f.Semana_Nro)).ToList();

            //Construir DTOs
            foreach (var fila in filas)
            {
                var idEst = idEstimacion.ToString();

                //CABECERA / RESUMEN
                if (!resumen.TryGetValue(idEst, out var estim))
                {
                    estim = new ResumenSemanalEstimacionDto
                    {
                        IdEstimacion = idEst,
                        Contratado = null,
                        CajasPesoFijo = null,
                        KilosBaseEspecie = fila.KilosBaseEspecie,

                        EnvaseCosechero = new EnvaseCosecheroNode(),

                        Totales = new TotalesEstimacionDto
                        {
                            EstimadoSinPorcentaje = fila.Total_E_Sin_Porc,
                            EstimadoConPorcentaje = fila.Total_E_Con_Porc,
                            Producido = fila.Total_P,
                            DiferenciaEstimadoConProducido = fila.Dif_E_Con_P
                        },

                        Semanas = new List<SemanaEstimacionDto>()
                    };

                    resumen[idEst] = estim;
                    semanasIndexPorEstim[idEst] = new Dictionary<string, SemanaEstimacionDto>(StringComparer.OrdinalIgnoreCase);
                }

                //Llenar datos de cajas contratadas y cajas peso fijo

                //Tomamos el primer valor > 0
                if ((!estim.Contratado.HasValue || estim.Contratado == 0)
                    && fila.Contratado.HasValue
                    && fila.Contratado.Value > 0)
                {
                    estim.Contratado = fila.Contratado;
                }

                //Tomamos el primer valor > 0
                if ((!estim.CajasPesoFijo.HasValue || estim.CajasPesoFijo == 0)
                    && fila.CajasPesoFijo.HasValue
                    && fila.CajasPesoFijo.Value > 0)
                {
                    estim.CajasPesoFijo = fila.CajasPesoFijo;
                }

                // LLenar datos de envase
                if (fila.IdEnvaseCosecha != null || !string.IsNullOrEmpty(fila.NomEnvaseCosecha) || fila.KilosEnvase.HasValue)
                {
                    var env = estim.EnvaseCosechero ?? new EnvaseCosecheroNode();

                    // Sólo sobrescribimos si aún no tenía datos
                    if (string.IsNullOrEmpty(env.Id) && string.IsNullOrEmpty(env.Nombre) && (env.Kilo ?? 0) == 0)
                    {
                        env.Id = fila.IdEnvaseCosecha;
                        env.Nombre = fila.NomEnvaseCosecha ?? string.Empty;
                        env.Kilo = fila.KilosEnvase ?? 0.0;

                        estim.EnvaseCosechero = env;
                    }
                }

                var semanasIndex = semanasIndexPorEstim[idEst];

                //SEMANA
                var anio = fila.Anio ?? 0;
                var semanaNro = fila.Semana_Nro ?? string.Empty;

                //Clave única por año-semana
                var weekKey = $"{anio}|{semanaNro}";

                if (!semanasIndex.TryGetValue(weekKey, out var semana))
                {
                    semana = new SemanaEstimacionDto
                    {
                        Anio = anio,
                        SemanaNumero = semanaNro,
                        EstimadoSinPorcentaje = fila.E_Sin_Porc,
                        EstimadoConPorcentaje = fila.E_Con_Porc,
                        Producido = fila.P_Semana
                    };

                    semanasIndex[weekKey] = semana;
                    estim.Semanas.Add(semana);
                }
                else
                {

                    semana.EstimadoSinPorcentaje ??= fila.E_Sin_Porc;
                    semana.EstimadoConPorcentaje ??= fila.E_Con_Porc;
                    semana.Producido ??= fila.P_Semana;
                }
            }

            //Ordenar semanas dentro de cada estimación
            foreach (var est in resumen.Values)
            {
                est.Semanas = est.Semanas
                    .OrderBy(s => s.Anio)
                    .ThenBy(s =>
                    {
                        if (int.TryParse(s.SemanaNumero, out var n))
                            return n;
                        return int.MaxValue;
                    })
                    .ToList();

            }

          
           

            return resumen.Values.ToList();
        }

        public async Task<DetalleDistribucionesEstimacionDto> GetDetalleDistribucionesAsync(int idEstimacion)
        {
            if (idEstimacion <= 0)
                throw new ValidationException("IdEstimacion inválido");

            var dtSemana = await repository.GetDataTable("[Estimaciones].[usp_UI_EstimacionSemanal_DetalleDistribucion]",new[]{new SqlParameter("@ID_ESTIMACION", idEstimacion)});

            // 2) Segundo SP
            var dtPorDia = await repository.GetDataTable("[Estimaciones].[usp_UI_EstimacionSemanal_DetalleDistribucionXDia]",new[]{new SqlParameter("@ID_ESTIMACION", idEstimacion)});

            if (dtSemana == null || dtSemana.Rows.Count == 0)
            {
                return new DetalleDistribucionesEstimacionDto
                {
                    IdEstimacion = idEstimacion,
                    Semanas = new List<DetalleDistribucionesSemanalDto>()
                };
            }

            //Agrupar por Semana-Año y armar un DetalleDistribucionesSemanalDto por cada grupo
            var semanas = dtSemana.AsEnumerable().Where(r => r["ANIO"] != DBNull.Value && r["SEMANA_NRO"] != DBNull.Value)
            .GroupBy(r => new
                {
                    Anio = r.Field<int>("ANIO"),
                    Semana = r.Field<int>("SEMANA_NRO")
                })
                 .Select(g =>
                     {
                        var any = g.First();

                        var anio = any.Field<int>("ANIO");
                        var semanaNro = any.Field<int>("SEMANA_NRO");

                        var categoriasRaw = any.Field<string?>("CATEGORIAS_SEMANA");
                        var calibresRaw = any.Field<string?>("CALIBRES_SEMANA");

                        var aplicaDefaultPctExp = any.Field<int?>("APLICA_DEFAULT_PCT_EXP");

                return new DetalleDistribucionesSemanalDto
                 {
                    Anio = anio,
                    Semana = semanaNro.ToString(),

                    DistribucionCategoria = BuildDistribucionCategoria(categoriasRaw, aplicaDefaultPctExp),
                    DistribucionCalibre = BuildDistribucionCalibre(calibresRaw, aplicaDefaultPctExp),

                    PackingPorDia = BuildPackingPorDiaSemana(dtPorDia, anio, semanaNro.ToString()),
                     FrigorificoPorDia = BuildFrigorificoPorDiaSemana(dtPorDia, anio, semanaNro.ToString())
                  };
         })
                .OrderBy(x => x.Anio)
                .ThenBy(x => x.Semana)
                .ToList();

            return new DetalleDistribucionesEstimacionDto
            {
                IdEstimacion = idEstimacion,
                Semanas = semanas
            };
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
                    new SqlParameter("@kilo_envase", SqlDbType.Decimal){Precision = 18,Scale = 2, Value = request.kiloEnvase},
                    new SqlParameter("@peso_fijo", SqlDbType.Decimal){Precision = 18,Scale = 2, Value = request.pesoFijo},
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

            var totalE = row["TOTAL_E_SIN_PORC"] == DBNull.Value? 0 : Convert.ToInt32(row["TOTAL_E_SIN_PORC"]);

            var totalP = row["TOTAL_P_SEMANA"] == DBNull.Value ? 0 : Convert.ToInt32(row["TOTAL_P_SEMANA"]);


            return new ResumenSemanalRowDto
            {
                
                Contratado = row["CAJAS_CONTRATADAS"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_CONTRATADAS"]),
                CajasPesoFijo = row["CAJAS_PESO_FIJO"] == DBNull.Value ? (int?)null : Convert.ToDouble(row["CAJAS_PESO_FIJO"]),
                KilosBaseEspecie = row.Table.Columns.Contains("KILOS_BASE") && row["KILOS_BASE"] != DBNull.Value? Convert.ToInt32(row["KILOS_BASE"]): 0,

                IdEnvaseCosecha = row.IsNull("ID_ENVASE_COSECHA") ? null : row["ID_ENVASE_COSECHA"]?.ToString(),
                NomEnvaseCosecha = row.Table.Columns.Contains("NOM_ENVASE_COSECHA") ? (row.IsNull("NOM_ENVASE_COSECHA") ? "" : row["NOM_ENVASE_COSECHA"]?.ToString()) : "",
                KilosEnvase = row.IsNull("KILOS_ENVASE") ? (double?)null : Convert.ToDouble(row["KILOS_ENVASE"]),

                Total_E_Sin_Porc = row["TOTAL_E_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_E_SIN_PORC"]),
                Total_E_Con_Porc = row["TOTAL_E_CON_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_E_CON_PORC"]),
                Total_P = row["TOTAL_P_SEMANA"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TOTAL_P_SEMANA"]),



                Dif_E_Con_P = totalE - totalP,

                Anio = row["ANIO"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ANIO"]),
                Semana_Nro = row.IsNull("SEMANA_NRO") ? null : row["SEMANA_NRO"]?.ToString(),

                E_Sin_Porc = row["E_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["E_SIN_PORC"]),
                E_Con_Porc = row["E_CON_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["E_CON_PORC"]),
                P_Semana = row["P_SEMANA"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["P_SEMANA"]),
            };
        }


        //Helper distribución packing / frigorifico / categoria / calibre

        public static List<DistribucionCategoriaPorSemanaNode> BuildDistribucionCategoria(string? raw, int? aplicaDefaultPctExp)
        {
            var parsed = ParseNombrePorcentajeCajasDefault(raw);

            bool? esDefault = aplicaDefaultPctExp.HasValue ? aplicaDefaultPctExp.Value == 1 : (bool?)null;

            return parsed
                .Select(p => new DistribucionCategoriaPorSemanaNode
                {
                    nombreCategoria = p.Nombre,
                    Porcentaje = p.Porcentaje,
                    Cajas = p.Cajas,
                    EsPorcentajeDefault = esDefault
                })
                .ToList();
        }

        public static List<DistribucionCalibrePorSemanaNode> BuildDistribucionCalibre(string? raw, int? aplicaDefaultPctExp)
        {
            var parsed = ParseNombrePorcentajeCajasDefault(raw);

            bool? esDefault = aplicaDefaultPctExp.HasValue ? aplicaDefaultPctExp.Value == 1 : (bool?)null;

            return parsed
                .Select(p => new DistribucionCalibrePorSemanaNode
                {
                    nombreCalibre = p.Nombre,
                    Porcentaje = p.Porcentaje,
                    Cajas = p.Cajas,
                    EsPorcentajeDefault = esDefault
                })
                .ToList();
        }

        private static List<Semana_DistribucionPackingPorDia> BuildPackingPorDiaSemana(DataTable dtPorDia, int anio, string semanaNro)
        {
            if (!int.TryParse(semanaNro, out var semanaFiltro))
                return new List<Semana_DistribucionPackingPorDia>();

            return dtPorDia.AsEnumerable()
                .Where(r =>
                    Convert.ToInt32(r["ANIO"]) == anio &&
                    Convert.ToInt32(r["SEMANA_NRO"]) == semanaFiltro
                )
                .OrderBy(r => r.Field<DateTime>("DIA"))
                .Select(r => BuildPackingPorDiaRow(
                    r.Field<DateTime>("DIA"),
                    r.Field<string?>("PACKINGS_DIA")
                ))
                .ToList();
        }

        private static List<Semana_DistribucionFrigorificoPorDia> BuildFrigorificoPorDiaSemana(DataTable dtPorDia,int anio, string semanaNro)
        {
            if (!int.TryParse(semanaNro, out var semanaFiltro))
                return new List<Semana_DistribucionFrigorificoPorDia>();

            return dtPorDia.AsEnumerable()
                .Where(r =>
                    Convert.ToInt32(r["ANIO"]) == anio &&
                    Convert.ToInt32(r["SEMANA_NRO"]) == semanaFiltro
                )
                .OrderBy(r => r.Field<DateTime>("DIA"))
                .Select(r => BuildFrigorificoPorDiaRow(
                    r.Field<DateTime>("DIA"),
                    r.Field<string?>("FRIGORIFICOS_DIA")
                ))
                .ToList();
        }

        public static Semana_DistribucionPackingPorDia BuildPackingPorDiaRow(DateTime fechaDia, string? rawPackings)
        {
            var cultureEs = new CultureInfo("es-ES");

            return new Semana_DistribucionPackingPorDia
            {
                nombreDia = cultureEs.TextInfo.ToTitleCase(fechaDia.ToString("dddd", cultureEs)),
                fechaDia = fechaDia,
                Packings = ParseNombrePorcentajeCajasDefault(rawPackings)
            };
        }

        public static Semana_DistribucionFrigorificoPorDia BuildFrigorificoPorDiaRow(DateTime fechaDia, string? rawFrigos)
        {
            var cultureEs = new CultureInfo("es-ES");

            return new Semana_DistribucionFrigorificoPorDia
            {
                nombreDia = cultureEs.TextInfo.ToTitleCase(fechaDia.ToString("dddd", cultureEs)),
                fechaDia = fechaDia,
                Frigorificos = ParseNombrePorcentajeCajasDefault(rawFrigos)
            };
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

        private static List<NombrePorcentajeDto> ParseNombrePorcentajeCajasDefault(string? raw)
        {
            var result = new List<NombrePorcentajeDto>();
            if (string.IsNullOrWhiteSpace(raw))
                return result;

            // Cada item separado por ';'
            var items = raw.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                var part = item.Trim();
                if (part.Length == 0) continue;

                // Formato: Nombre:Porcentaje:Cajas:Flag
                var segments = part.Split(':', StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length < 2)
                {
                    // Al menos necesitamos Nombre y Porcentaje
                    continue;
                }

                var nombre = segments[0].Trim();
                var porcentaje = segments[1].Trim();

                int? cajas = null;
                if (segments.Length >= 3)
                {
                    var rawCajas = segments[2].Trim().Replace(",", ".");
                    if (double.TryParse(rawCajas, NumberStyles.Any, CultureInfo.InvariantCulture, out var cajasDouble))
                    {
                        //Siempre convertimos a int
                        cajas = (int)Math.Round(cajasDouble);
                    }
                }

                bool? esDefault = null;
                if (segments.Length >= 4)
                {
                    var flag = segments[3].Trim();
                    if (flag.Equals("D", StringComparison.OrdinalIgnoreCase))
                        esDefault = true;
                    else if (flag.Equals("E", StringComparison.OrdinalIgnoreCase))
                        esDefault = false;
                }

                result.Add(new NombrePorcentajeDto
                {
                    Nombre = nombre,
                    Porcentaje = porcentaje,
                    Cajas = cajas,
                    EsPorcentajeDefault = esDefault
                });
            }

            return result;
        }



        //HELPERS EstimacionBisemanal

        private static RowFlat MapRowFlat(DataRow row)
        {

            decimal? cajasEstimadas = null;
            decimal? cajasProducidas = null;
            int? cajasAnteriorEstimado = null;
            int? cajasAnteriorProducido = null;
            int? cajasSiguienteEstimado = null;
            int? cajasSiguienteProducido = null;
            var unidadMedidaEspecie = row["ESPECIE_UM"] == DBNull.Value ? 1 : Convert.ToInt32(row["ESPECIE_UM"]);

            //Conversión por unidad de medida

            switch (unidadMedidaEspecie)
            {
                case 1:
                    cajasEstimadas = row["CAJAS_ESTIMADAS_SIN_PORC"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["CAJAS_ESTIMADAS_SIN_PORC"]);
                    cajasProducidas = row["CAJAS_P"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["CAJAS_P"]);
                    cajasAnteriorEstimado = row["CAJAS_E_ANTERIOR_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_E_ANTERIOR_SIN_PORC"]);
                    cajasAnteriorProducido = row["CAJAS_P_ANTERIOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_P_ANTERIOR"]);
                    cajasSiguienteEstimado = row["CAJAS_E_SIGUIENTE_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_E_SIGUIENTE_SIN_PORC"]);
                    cajasSiguienteProducido = row["CAJAS_P_SIGUIENTE_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_P_SIGUIENTE_SIN_PORC"]);
                    break;

                case 2:
                    //KILOS
                    cajasEstimadas = !row.Table.Columns.Contains("KILOS_BASE_SIN_EXP") || row["KILOS_BASE_SIN_EXP"] == DBNull.Value ? 0m : Convert.ToDecimal(row["KILOS_BASE_SIN_EXP"]); 
                    cajasProducidas = !row.Table.Columns.Contains("KILOS_P") || row["KILOS_P"] == DBNull.Value ? 0m : Convert.ToDecimal(row["KILOS_P"]);
                    cajasAnteriorEstimado = row["KILOS_E_ANTERIOR_SIN_EXP"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["KILOS_E_ANTERIOR_SIN_EXP"]);
                    cajasAnteriorProducido = row["KILOS_P_ANTERIOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["KILOS_P_ANTERIOR"]);
                    cajasSiguienteEstimado = row["KILOS_E_SIGUIENTE_SIN_EXP"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["KILOS_E_SIGUIENTE_SIN_EXP"]);
                    cajasSiguienteProducido = row["KILOS_P_SIGUIENTE"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["KILOS_P_SIGUIENTE"]);
                    break;

                case 3:
                    //ENVASE
                    cajasEstimadas = !row.Table.Columns.Contains("ENVASE_BASE_SIN_EXP") || row["ENVASE_BASE_SIN_EXP"] == DBNull.Value ? 0m : Convert.ToDecimal(row["ENVASE_BASE_SIN_EXP"]); 
                    cajasProducidas = !row.Table.Columns.Contains("ENVASES_P") || row["ENVASES_P"] == DBNull.Value ? 0m : Convert.ToDecimal(row["ENVASES_P"]);
                    cajasAnteriorEstimado = row["ENVASES_E_ANTERIOR_SIN_EXP"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ENVASES_E_ANTERIOR_SIN_EXP"]);
                    cajasAnteriorProducido = row["ENVASES_P_ANTERIOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ENVASES_P_ANTERIOR"]);
                    cajasSiguienteEstimado = row["ENVASES_E_SIGUIENTE_SIN_EXP"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ENVASES_E_SIGUIENTE_SIN_EXP"]);
                    cajasSiguienteProducido = row["ENVASES_P_SIGUIENTE"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ENVASES_P_SIGUIENTE"]);
                    break;

                default:
                    // En caso venga otro valor (no debería) se deja en cajas base por default
                    cajasEstimadas = row["CAJAS_ESTIMADAS_SIN_PORC"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["CAJAS_ESTIMADAS_SIN_PORC"]);
                    cajasProducidas = row["CAJAS_P"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["CAJAS_P"]);
                    cajasAnteriorEstimado = row["CAJAS_E_ANTERIOR_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_E_ANTERIOR_SIN_PORC"]);
                    cajasAnteriorProducido = row["CAJAS_P_ANTERIOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_P_ANTERIOR"]);
                    cajasSiguienteEstimado = row["CAJAS_E_SIGUIENTE_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_E_SIGUIENTE_SIN_PORC"]);
                    cajasSiguienteProducido = row["CAJAS_P_SIGUIENTE_SIN_PORC"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["CAJAS_P_SIGUIENTE_SIN_PORC"]);
                    break;
            }


            return new RowFlat
            {
                // Raíz
                PesoBaseEspecie = row["ESPECIE_KILO_BASE"] == DBNull.Value? 0.0 : Convert.ToDouble(row["ESPECIE_KILO_BASE"]),

                CodigoEspecie = row.Table.Columns.Contains("ID_ESPECIE") ? (row.IsNull("ID_ESPECIE") ? "" : row["ID_ESPECIE"]?.ToString() ?? "") : "",

                Especie = row.Table.Columns.Contains("NOM_ESP") ? (row.IsNull("NOM_ESP") ? "" : row["NOM_ESP"]?.ToString() ?? "") : "",

                UnidadMedidaEspecie = unidadMedidaEspecie,

                // Item
                IdProductor = row.Table.Columns.Contains("ID_PRODUCTOR") ? (row.IsNull("ID_PRODUCTOR") ? "" : row["ID_PRODUCTOR"]?.ToString() ?? ""): "",

                Productor = row.Table.Columns.Contains("NOM_PROD") ? (row.IsNull("NOM_PROD") ? "" : row["NOM_PROD"]?.ToString() ?? "") : "",

                CodigoVariedad = row.Table.Columns.Contains("ID_VARIEDAD") ? (row.IsNull("ID_VARIEDAD") ? "" : row["ID_VARIEDAD"]?.ToString() ?? "") : "",

                Variedad = row.Table.Columns.Contains("NOM_VAR") ? (row.IsNull("NOM_VAR") ? "" : row["NOM_VAR"]?.ToString() ?? "") : "",

                Agronomo = row.Table.Columns.Contains("NOM_USUARIO_AGRONOMO")  ? (row.IsNull("NOM_USUARIO_AGRONOMO") ? "" : row["NOM_USUARIO_AGRONOMO"]?.ToString() ?? "") : "",

                DistribucionCalibre = row["DIST_CAL"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_CAL"]),

                DistribucionCategoria = row["DIST_CAT"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_CAT"]),

                PorcentajeExportacion = row["PCT_EXP_PORC"] == DBNull.Value ? 0 : Convert.ToInt32(row["PCT_EXP_PORC"]),

                // Envase
                EnvaseId = row.Table.Columns.Contains("ID_ENVASE_COSECHA") ? (row.IsNull("ID_ENVASE_COSECHA") ? "" : row["ID_ENVASE_COSECHA"]?.ToString() ?? "") : "",

                EnvaseNombre = row.Table.Columns.Contains("NOM_ENVASE_COSECHA") ? (row.IsNull("NOM_ENVASE_COSECHA") ? "" : row["NOM_ENVASE_COSECHA"]?.ToString() ?? "") : "",

                EnvaseKilo = row["KILOS_ENVASE_COSECHA"] == DBNull.Value ? 0 : Convert.ToInt32(row["KILOS_ENVASE_COSECHA"]),

                // Estimación
                Est_ID = row["ID_ESTIMACION"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ID_ESTIMACION"]),

                Est_Contratado = row["CAJAS_CONTRATADAS"] == DBNull.Value ? 0 : Convert.ToInt32(row["CAJAS_CONTRATADAS"]),

                Est_FCosecha = row.Table.Columns.Contains("FECHA_INICIO_COSECHA_YM") ? (row.IsNull("FECHA_INICIO_COSECHA_YM") ? "" : row["FECHA_INICIO_COSECHA_YM"]?.ToString() ?? "") : "",

                Ant_Estimado = cajasAnteriorEstimado,

                Ant_Producido = cajasAnteriorProducido,

                Sig_Estimado = cajasSiguienteEstimado,

                Sig_Producido = cajasSiguienteProducido,

                // Bisemanal
                Bis_AnioBase = row["ANIO"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ANIO"]),

                Bis_SemanaBase = row.Table.Columns.Contains("SEMANA_NRO") ? (row.IsNull("SEMANA_NRO") ? "" : row["SEMANA_NRO"]?.ToString() ?? "") : "",

                // Días
                Bis_ID = row["ID_ESTIMACION_BISEMANAL"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ID_ESTIMACION_BISEMANAL"]),

                Dia_Nombre = row.Table.Columns.Contains("NOMBRE_DIA") ? (row.IsNull("NOMBRE_DIA") ? "" : row["NOMBRE_DIA"]?.ToString() ?? "") : "",

                Dia_Fecha = row["DIA"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DIA"]),

                Dia_Estimado = cajasEstimadas,

                Dia_Producido = cajasProducidas,

                Dia_DistribucionFrio = row["DIST_FRI"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_FRI"]),

                Dia_DistribucionPacking = row["DIST_PACK"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["DIST_PACK"])
            };
        }

        public async Task Publicar(PublicacionDTO input, Guid usuario)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ID_EMPRESA", input.idEmpresa),
                    new SqlParameter("@ID_TEMPORADA", input.idTemporada),
                    new SqlParameter("@ID_ESPECIE", input.idEspecie),
                    new SqlParameter("@CodigoGrupoProductor", input.codigoGrupoProductor),
                    new SqlParameter("@ID_USUARIO_EJECUTOR", usuario),
                    new SqlParameter
                    {
                        ParameterName = "@CodigoSalida",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    },
                    new SqlParameter 
                    { 
                        ParameterName = "@MensajeSalida",
                        Size = 4000,
                        SqlDbType = SqlDbType.NVarChar,
                        Direction = ParameterDirection.Output
                    }
                    
                };
                var res = await repository.GetDataTable("[Estimaciones].[usp_ESTIMACION_OFICIAL]", parameters);
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
