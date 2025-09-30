using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using System.Data;
using System.Globalization;
using System.Xml.Linq;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Repository
{
    public class EstimacionesRepository : IEstimacionesRepository
    {
        private readonly string _connString;
        public EstimacionesRepository(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")!;
        }
        public async Task<EstimacionesDto.EstimacionDistribucionDto> GetEstimacionBisemanalAsync(EstimacionesDto.EstimacionBisemanalQueryDto req)
        {
            
            var flat = new List<RowFlat>();

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
                // Fallback por si el SP a veces devuelve coma decimal como string
                decimal SafeDec(string col)
                {
                    var d = rdr.Get<decimal?>(col);
                    if (d.HasValue) return d.Value;
                    var s = rdr.Get<string?>(col);
                    if (!string.IsNullOrWhiteSpace(s) &&
                        decimal.TryParse(s.Replace(',', '.'),
                            NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                        return parsed;
                    return 0m;
                }

                flat.Add(new RowFlat
                {
                    IdEstimacion = rdr.Get<string?>("ID_ESTIMACION") ?? "",
                    IdEstimacionBisemanal = rdr.Get<int?>("ID_ESTIMACION_BISEMANAL") ?? 0,
                    Anio = rdr.Get<int?>("ANIO") ?? 0,
                    SemanaNro = rdr.Get<int?>("SEMANA_NRO") ?? 0,

                    IdProductor = rdr.Get<string?>("ID_PRODUCTOR") ?? "",
                    NomProd = rdr.Get<string?>("NOM_PROD") ?? "",
                    NomEsp = rdr.Get<string?>("NOM_ESP") ?? "",
                    NomVar = rdr.Get<string?>("NOM_VAR") ?? "",

                    CajasEstimadasSinPorc = rdr.Get<int?>("CAJAS_ESTIMADAS_SIN_PORC") ?? 0,
                    CajasEstimadasConPorc = rdr.Get<int?>("CAJAS_ESTIMADAS_CON_PORC") ?? 0,
                    CajasDistribSinPorc = rdr.Get<int?>("CAJAS_E_DISTRIB_SIN_PORC") ?? 0,
                    CajasDistribConPorc = SafeDec("CAJAS_E_DISTRIB_CON_PORC"),
                    CajasP = rdr.Get<int?>("CAJAS_P") ?? 0,

                    DistCat = (rdr.Get<int?>("DIST_CAT") ?? 0) == 1,
                    DistCal = (rdr.Get<int?>("DIST_CAL") ?? 0) == 1,
                    DistPack = (rdr.Get<int?>("DIST_PACK") ?? 0) == 1,
                    DistFri = (rdr.Get<int?>("DIST_FRI") ?? 0) == 1,

                    CajasPAnterior = rdr.Get<int?>("CAJAS_P_ANTERIOR"),
                    CajasEAnteriorSinPorc = rdr.Get<int?>("CAJAS_E_ANTERIOR_SIN_PORC"),
                    CajasEAnteriorConPorc = rdr.Get<int?>("CAJAS_E_ANTERIOR_CON_PORC"),
                    CajasPSiguienteSinPorc = rdr.Get<int?>("CAJAS_P_SIGUIENTE_SIN_PORC"),
                    CajasESiguienteSinPorc = rdr.Get<int?>("CAJAS_E_SIGUIENTE_SIN_PORC"),
                    CajasESiguienteConPorc = rdr.Get<int?>("CAJAS_E_SIGUIENTE_CON_PORC")
                });
            }

            // Sin filas → objeto vacío (como sueles hacer)
            if (flat.Count == 0)
                return new EstimacionDistribucionDto { IdEstimacion = "", Semanas = new() };

            var idEstimacion = flat.FirstOrDefault(x => !string.IsNullOrEmpty(x.IdEstimacion))?.IdEstimacion ?? "";

            // Construir el objeto final (agrupado: semana → productor → (especie,variedad))
            var result = new EstimacionDistribucionDto
            {
                IdEstimacion = idEstimacion,
                Semanas = new()
            };

            var semanasGroup = flat
                .GroupBy(x => new { x.IdEstimacionBisemanal, x.Anio, x.SemanaNro })
                .OrderBy(g => g.Key.Anio).ThenBy(g => g.Key.SemanaNro);

            foreach (var g in semanasGroup)
            {
                var semanaKey = $"{g.Key.IdEstimacionBisemanal}-{g.Key.Anio}-{g.Key.SemanaNro}";

                var semanaDto = new SemanaDto
                {
                    Indice = new IndiceDto
                    {
                        IdEstimacionBisemanal = g.Key.IdEstimacionBisemanal,
                        Anio = g.Key.Anio,
                        Semana = g.Key.SemanaNro
                    },
                    TotalesSemana = new TotalesSemanaDto
                    {
                        CajasEstimadasSinPorc = g.Sum(i => i.CajasEstimadasSinPorc),
                        CajasEstimadasConPorc = g.Sum(i => i.CajasEstimadasConPorc),
                        CajasDistribSinPorc = g.Sum(i => i.CajasDistribSinPorc),
                        CajasDistribConPorc = g.Sum(i => i.CajasDistribConPorc),
                        CajasP = g.Sum(i => i.CajasP)
                    },
                    // Historial repetido → a nivel semana (primer no nulo por campo)
                    Historial = new HistorialDto
                    {
                        CajasPAnterior = FirstNN(g.Select(x => x.CajasPAnterior)),
                        CajasEAnteriorSinPorc = FirstNN(g.Select(x => x.CajasEAnteriorSinPorc)),
                        CajasEAnteriorConPorc = FirstNN(g.Select(x => x.CajasEAnteriorConPorc)),
                        CajasPSiguienteSinPorc = FirstNN(g.Select(x => x.CajasPSiguienteSinPorc)),
                        CajasESiguienteSinPorc = FirstNN(g.Select(x => x.CajasESiguienteSinPorc)),
                        CajasESiguienteConPorc = FirstNN(g.Select(x => x.CajasESiguienteConPorc))
                    },
                    Productores = new()
                };

                foreach (var pg in g.GroupBy(x => x.IdProductor))
                {
                    var productorDto = new ProductorDto
                    {
                        Nombre = pg.First().NomProd,
                        Items = new List<ItemDto>()
                    };

                    foreach (var ig in pg.GroupBy(v => new { v.NomEsp, v.NomVar }))
                    {
                        productorDto.Items.Add(new ItemDto
                        {
                            Especie = ig.Key.NomEsp,
                            Variedad = ig.Key.NomVar,
                            Cajas = new CajasDto
                            {
                                CajasEstimadasSinPorc = ig.Sum(x => x.CajasEstimadasSinPorc),
                                CajasEstimadasConPorc = ig.Sum(x => x.CajasEstimadasConPorc),
                                CajasDistribSinPorc = ig.Sum(x => x.CajasDistribSinPorc),
                                CajasDistribConPorc = ig.Sum(x => x.CajasDistribConPorc),
                                P = ig.Sum(x => x.CajasP)
                            },
                            Dist = new DistDto
                            {
                                Categoria = ig.Any(x => x.DistCat),
                                Calibre = ig.Any(x => x.DistCal),
                                Packing = ig.Any(x => x.DistPack),
                                Frigorifico = ig.Any(x => x.DistFri)
                            }
                        });
                    }

                    semanaDto.Productores[pg.Key] = productorDto;
                }

                result.Semanas[semanaKey] = semanaDto;
            }

            return result;
        }

        //Helpers
        private static int? FirstNN(IEnumerable<int?> seq) => seq.FirstOrDefault(v => v.HasValue);

        private sealed class RowFlat
        {
            public string IdEstimacion { get; set; } = "";
            public int IdEstimacionBisemanal { get; set; }
            public int Anio { get; set; }
            public int SemanaNro { get; set; }

            public string IdProductor { get; set; } = "";
            public string NomProd { get; set; } = "";
            public string NomEsp { get; set; } = "";
            public string NomVar { get; set; } = "";

            public int CajasEstimadasSinPorc { get; set; }
            public int CajasEstimadasConPorc { get; set; }
            public int CajasDistribSinPorc { get; set; }
            public decimal CajasDistribConPorc { get; set; }
            public int CajasP { get; set; }

            public bool DistCat { get; set; }
            public bool DistCal { get; set; }
            public bool DistPack { get; set; }
            public bool DistFri { get; set; }

            public int? CajasPAnterior { get; set; }
            public int? CajasEAnteriorSinPorc { get; set; }
            public int? CajasEAnteriorConPorc { get; set; }
            public int? CajasPSiguienteSinPorc { get; set; }
            public int? CajasESiguienteSinPorc { get; set; }
            public int? CajasESiguienteConPorc { get; set; }
        }
    }
}
