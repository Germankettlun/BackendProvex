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
        public async Task<EstructuraDistribucionDto> GetEstimacionBisemanalAsync(EstimacionBisemanalQueryDto req)
        {

            var rows = new List<RowFlat>();

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

            return BuildTree(rows);
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

        private static EstructuraDistribucionDto BuildTree(List<RowFlat> rows)
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

               
                EstimacionNode? estObj = null;

                if (any.Est_ID.HasValue && any.Est_ID.Value >= 0)
                {
                    estObj = new EstimacionNode
                    {
                        ID = any.Est_ID,
                        Contratado = any.Est_Contratado,
                        FCosecha = any.Est_FCosecha,
                        Semanas = new SemanasNode
                        {
                            Anterior = new SemanaValorNode
                            {
                                Estimado = any.Ant_Estimado,
                                Producido = any.Ant_Producido
                            },
                            Siguiente = new SemanaValorNode
                            {
                                Estimado = any.Sig_Estimado,
                                Producido = any.Sig_Producido
                            },
                            Bisemanal = new List<BisemanalNode>()
                        }
                    };

                   
                    var bisGroups = g.Where(r => r.Bis_ID.HasValue)
                                     .GroupBy(r => new
                                     {
                                         r.Bis_ID,
                                         r.Bis_AnioBase,
                                         r.Bis_SemanaBase,
                                         r.Bis_DistFrio,
                                         r.Bis_DistPacking,
                                         r.Bis_PorcExport
                                     });

                    foreach (var bg in bisGroups)
                    {
                        var bis = new BisemanalNode
                        {
                            ID = bg.Key.Bis_ID,
                            AnioBase = bg.Key.Bis_AnioBase,
                            SemanaBase = bg.Key.Bis_SemanaBase,
                            DistribucionFrio = bg.Key.Bis_DistFrio,
                            DistribucionPacking = bg.Key.Bis_DistPacking,
                            PorcentajeExportacion = bg.Key.Bis_PorcExport,
                            Dias = new List<DiaNode>()
                        };

                        foreach (var d in bg)
                        {
                            var tieneDia = d.Dia_Nombre is not null
                                        || d.Dia_Fecha is not null
                                        || d.Dia_Estimado.HasValue
                                        || d.Dia_Producido.HasValue;

                            if (!tieneDia) continue;

                            bis.Dias!.Add(new DiaNode
                            {
                                NombreDia = d.Dia_Nombre,
                                FechaDia = d.Dia_Fecha,
                                Estimado = d.Dia_Estimado,
                                Producido = d.Dia_Producido
                            });
                        }

                        estObj!.Semanas!.Bisemanal!.Add(bis);
                    }

                    // Asignamos como objeto
                    item.Estimacion = estObj;
                }
                else
                {
                   
                    item.Estimacion = new List<EstimacionNode>();
                }

                root.Items!.Add(item);
            }

            return root;
        }
    }

}
