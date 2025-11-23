using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace ProvexApi.Services
{
    using ProvexApi.Data;
    using ProvexApi.Data.DTOs.ASOEX;
    using ProvexApi.Entities.ASOEX;
    using ProvexApi.Helpers;
    using System.Xml;

    public interface IAsoexService
    {
        Task<string> SincronizarHechosAsoexAsync(DateTime fechaini, DateTime fechafin, bool guardarEnBD, bool usarLogCsv = false);
        Task DescargarYGuardarProcesoCompletoAsync(int idProceso);
    }

    public class AsoexService : IAsoexService
    {
        private readonly IConfiguration _config;
        private readonly ProvexDbContext _db;
        private readonly ILogger<AsoexService> _logger;
        private const int MAX_CONCURRENT_TASKS = 8;
        private const string DEFAULT_NAVE = "SIN";

        public AsoexService(IConfiguration config, ProvexDbContext db, ILogger<AsoexService> logger)
        {
            _config = config;
            _db = db;
            _logger = logger;
        }

        public async Task<string> SincronizarHechosAsoexAsync(DateTime fechaini, DateTime fechafin, bool guardarEnBD, bool usarLogCsv = false)
        {
            var logBuilder = new StringBuilder();
            LogAndAppend(logBuilder, LogLevel.Information, $"--- INICIO SINCRONIZACIÓN ASOEX POR RANGO (Procesando Día a Día) ---");
            LogAndAppend(logBuilder, LogLevel.Information, $"Rango de Fechas de Proceso solicitado: {fechaini:yyyy-MM-dd} a {fechafin:yyyy-MM-dd}");

            StreamWriter? logWriter = null;
            if (usarLogCsv)
            {
                string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "asoex", "logs");
                if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
                string logCsvPath = Path.Combine(logsDir, $"asoex_sync_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                logWriter = new StreamWriter(logCsvPath, false, Encoding.UTF8);
                logWriter.WriteLine("Timestamp,Accion,Entidad,IdProceso,TempSeqNro,NroOtro,CodPuertoEmbarque,CorrelativoViaje,Status,PaginaApi,Resultado,Mensaje,Payload");
            }

            for (var fechaProceso = fechaini.Date; fechaProceso <= fechafin.Date; fechaProceso = fechaProceso.AddDays(1))
            {
                LogAndAppend(logBuilder, LogLevel.Information, $"================== PROCESANDO DÍA: {fechaProceso:yyyy-MM-dd} ==================");
                try
                {
                    await SincronizarUnDiaAsync(fechaProceso, guardarEnBD, logBuilder, logWriter);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception ex)
                {
                    LogAndAppend(logBuilder, LogLevel.Error, $"[ERROR-CRITICO] Falló la sincronización para el día {fechaProceso:yyyy-MM-dd}. Mensaje: {ex.Message}", ex);
                    if (logWriter != null)
                        LogCsvOperacion(logWriter, "Error", "Global", null, null, null, null, null, null, null, "Error", ex.Message, "");
                }
            }

            if (logWriter != null)
            {
                logWriter.Flush();
                logWriter.Close();
            }

            LogAndAppend(logBuilder, LogLevel.Information, "--- FIN DE LA SINCRONIZACIÓN POR RANGO ---");
            return logBuilder.ToString();
        }

        private async Task SincronizarUnDiaAsync(DateTime fechaProceso, bool guardarEnBD, StringBuilder logBuilder, StreamWriter? logWriter = null)
        {
            var apiName = "MoviminetosProceso";
            var urlProcesos = $"{GetUrl(apiName)}?fechaini={fechaProceso:yyyy-MM-dd}&fechafin={fechaProceso:yyyy-MM-dd}";
            var procesosDelDia = await AsoexApiHelper.ObtenerDatosPaginadosConNumeroPaginaAsync<ProcesoDto, ProcesoDtoPaged>(_config, apiName, urlProcesos, _logger);

            if (!procesosDelDia.Any())
            {
                LogAndAppend(logBuilder, LogLevel.Warning, "[INFO] No se encontraron procesos para esta fecha.");
                if (logWriter != null)
                    LogCsvOperacion(logWriter, "Skip", "Proceso", null, null, null, null, null, null, null, "OK", "No se encontraron procesos", "");
                return;
            }
            LogAndAppend(logBuilder, LogLevel.Information, $"[API] Se encontraron {procesosDelDia.Count} procesos.");

            var viajesDescargados = new ConcurrentBag<ProcesoViajeDto>();
            var detallesDescargados = new ConcurrentBag<ProcesoDetalleDto>();
            var semaphore = new SemaphoreSlim(MAX_CONCURRENT_TASKS);

            var fetchTasks = procesosDelDia.Select(proc => Task.Run(async () => {
                await semaphore.WaitAsync();
                try
                {
                    var urlViajes = $"{GetUrl("MoviminetosProcesoviajes")}?idproceso={proc.idproceso}";
                    var viajes = await AsoexApiHelper.ObtenerDatosPaginadosConNumeroPaginaAsync<ProcesoViajeDto, ProcesoViajeDtoPaged>(_config, "MoviminetosProcesoviajes", urlViajes, _logger);
                    if (viajes != null) foreach (var v in viajes) viajesDescargados.Add(v);

                    var urlDetalles = $"{GetUrl("MoviminetosProcesodetalle")}?idproceso={proc.idproceso}";
                    var detalles = await AsoexApiHelper.ObtenerDatosPaginadosConNumeroPaginaAsync<ProcesoDetalleDto, ProcesoDetalleDtoPaged>(_config, "MoviminetosProcesodetalle", urlDetalles, _logger);
                    if (detalles != null) foreach (var d in detalles) detallesDescargados.Add(d);
                }
                finally { semaphore.Release(); }
            })).ToList();
            await Task.WhenAll(fetchTasks);

            LogAndAppend(logBuilder, LogLevel.Information, $"[API] Descarga completada. Total brutos: {viajesDescargados.Count} viajes, {detallesDescargados.Count} detalles.");

            var procesosBulk = procesosDelDia.Select(p => new Proceso
            {
                IdProceso = p.idproceso,
                FechaProceso = ParseDate(p.fecha_proceso).GetValueOrDefault(),
                HoraProceso = TimeSpan.TryParse(p.hora_poceso, out var time) ? time : TimeSpan.Zero
            }).ToList();

            var viajesBulk = viajesDescargados.Select(v => new ProcesoViaje
            {
                IdProceso = v.idproceso,
                NroOtro = v.nro_otro,
                TempSeqNro = v.temp_seq_nro,
                CodPuertoEmbarque = v.cod_puerto_embarqu ?? "SIN",
                CorrelativoViaje = v.correlativo_viaje,
                CodPuertoArribo = v.cod_puerto_arribo ?? "SIN",
                FechaZarpe = ParseDate(v.fecha_zarpe).GetValueOrDefault(),
                FechaArribo = ParseDate(v.fecha_arribo),
                CodNave = v.cod_nave ?? DEFAULT_NAVE,
                Status = v.status ?? ""
            }).ToList();

            var detallesBulk = detallesDescargados.Select(d => new ProcesoDetalle
            {
                IdProceso = d.idproceso,
                NroOtro = d.nro_otro,
                TempSeqNro = d.temp_seq_nro,
                CodPuertoEmbarque = d.cod_puerto_embarqu ?? "SIN",
                CorrelativoViaje = d.correlativo_viaje,
                RutExportador = d.rut_exportador.ToString(),
                CodEspecie = d.cod_especie,
                CodVariedad = d.cod_variedad,
                CodRegionOrigen = d.cod_region_origen,
                PsdCantidad = d.psd_cantidad,
                PsdTotKgNeto = d.psd_tot_kg_neto,
                CodPuertoDestino = d.cod_puerto_destino ?? "SIN",
                CodConsignatario = d.cod_consignatario ?? "SIN",
                CodCondicion = d.cod_condicion ?? "C",
                pagina_api = d.pagina_api // <-- aquí
            }).ToList();

            // Liberar memoria de los bags
            viajesDescargados.Clear();
            detallesDescargados.Clear();

            var invalidNaves = viajesBulk.Where(v => string.IsNullOrWhiteSpace(v.CodNave)).ToList();

            if (invalidNaves.Any())
            {
                LogAndAppend(logBuilder, LogLevel.Warning, $"[NAVES-INVALIDAS] Se encontraron {invalidNaves.Count} viajes sin código de nave:");
                foreach (var nav in invalidNaves)
                {
                    LogAndAppend(logBuilder, LogLevel.Warning, $"[NAVE-NULL] TempSeqNro: {nav.TempSeqNro}, Correlativo: {nav.CorrelativoViaje}, proceso: {nav.IdProceso}, Puerto: {nav.CodPuertoEmbarque}, Nave: {nav.CodNave}");
                    if (logWriter != null)
                        LogCsvOperacion(logWriter, "Warning", "Viaje", nav.IdProceso, nav.TempSeqNro, nav.NroOtro, nav.CodPuertoEmbarque, nav.CorrelativoViaje, nav.Status, null, "Warning", "Viaje sin nave", JsonConvert.SerializeObject(nav));
                }
            }

            if (!guardarEnBD) return;

            using var trx = await _db.Database.BeginTransactionAsync();
            try
            {
                // PASO 1: Manejo de status 'R'
                var viajesREntidades = viajesBulk
                    .Where(v => v.Status == "R")
                    .Select(v => new ProcesoViaje
                    {
                        TempSeqNro = v.TempSeqNro,
                        CodPuertoEmbarque = v.CodPuertoEmbarque ?? "SIN",
                        CorrelativoViaje = v.CorrelativoViaje
                    }).ToList();

                if (viajesREntidades.Any())
                {
                    // Eliminar detalles primero
                    foreach (var viaje in viajesREntidades)
                    {
                        var detallesEliminar = await _db.Asoex_ProcesoDetalles
                            .Where(d => d.TempSeqNro == viaje.TempSeqNro
                                     && d.CodPuertoEmbarque == viaje.CodPuertoEmbarque
                                     && d.CorrelativoViaje == viaje.CorrelativoViaje)
                            .ToListAsync();

                        if (detallesEliminar.Any())
                        {
                            _db.Asoex_ProcesoDetalles.RemoveRange(detallesEliminar);
                            await _db.SaveChangesAsync();
                            if (logWriter != null)
                                foreach (var detalle in detallesEliminar)
                                    LogCsvOperacion(logWriter, "Delete", "Detalle", detalle.IdProceso, detalle.TempSeqNro, detalle.NroOtro, detalle.CodPuertoEmbarque, detalle.CorrelativoViaje, null, detalle.pagina_api, "OK", "Eliminado por status R", JsonConvert.SerializeObject(detalle));
                        }
                    }

                    // Eliminar viajes
                    foreach (var viaje in viajesREntidades)
                    {
                        var viajeEliminar = await _db.Asoex_ProcesoViajes
                            .FirstOrDefaultAsync(v => v.TempSeqNro == viaje.TempSeqNro
                                                  && v.CodPuertoEmbarque == viaje.CodPuertoEmbarque
                                                  && v.CorrelativoViaje == viaje.CorrelativoViaje);

                        if (viajeEliminar != null)
                        {
                            _db.Asoex_ProcesoViajes.Remove(viajeEliminar);
                            await _db.SaveChangesAsync();
                            if (logWriter != null)
                                LogCsvOperacion(logWriter, "Delete", "Viaje", viajeEliminar.IdProceso, viajeEliminar.TempSeqNro, viajeEliminar.NroOtro, viajeEliminar.CodPuertoEmbarque, viajeEliminar.CorrelativoViaje, viajeEliminar.Status, null, "OK", "Viaje eliminado por status R", JsonConvert.SerializeObject(viajeEliminar));
                        }
                    }

                    LogAndAppend(logBuilder, LogLevel.Information, $"[DB] Eliminados {viajesREntidades.Count} viajes 'R' y sus detalles");

                    // Insertar los reemplazos
                    var viajesR = viajesBulk
                        .Where(v => v.Status == "R")
                        .ToList();

                    if (viajesR.Any())
                    {
                        await _db.BulkInsertAsync(viajesR);
                        if (logWriter != null)
                            foreach (var v in viajesR)
                                LogCsvOperacion(logWriter, "Insert", "Viaje", v.IdProceso, v.TempSeqNro, v.NroOtro, v.CodPuertoEmbarque, v.CorrelativoViaje, v.Status, null, "OK", "Reemplazo insertado (R)", JsonConvert.SerializeObject(v));
                        LogAndAppend(logBuilder, LogLevel.Information, $"[DB] Insertados {viajesR.Count} reemplazos (status R)");
                    }
                }

                // PASO 2: Insertar/Actualizar Procesos
                if (procesosBulk.Any())
                {
                    await _db.BulkInsertOrUpdateAsync(procesosBulk, new BulkConfig
                    {
                        UpdateByProperties = new List<string> { nameof(Proceso.IdProceso) }
                    });
                    if (logWriter != null)
                        foreach (var p in procesosBulk)
                            LogCsvOperacion(logWriter, "Upsert", "Proceso", p.IdProceso, null, null, null, null, null, null, "OK", "Upsert Proceso", JsonConvert.SerializeObject(p));
                }

                // PASO 3: Insertar viajes NUEVOS (no 'R')
                var viajesNuevos = new List<ProcesoViaje>();
                foreach (var viaje in viajesBulk.Where(v => v.Status != "R"))
                {
                    bool existe = await _db.Asoex_ProcesoViajes
                        .AnyAsync(v => v.TempSeqNro == viaje.TempSeqNro
                                    && v.CodPuertoEmbarque == viaje.CodPuertoEmbarque
                                    && v.CorrelativoViaje == viaje.CorrelativoViaje);

                    if (!existe) viajesNuevos.Add(viaje);
                }

                if (viajesNuevos.Any())
                {
                    await _db.BulkInsertAsync(viajesNuevos);
                    if (logWriter != null)
                        foreach (var v in viajesNuevos)
                            LogCsvOperacion(logWriter, "Insert", "Viaje", v.IdProceso, v.TempSeqNro, v.NroOtro, v.CodPuertoEmbarque, v.CorrelativoViaje, v.Status, null, "OK", "Viaje nuevo insertado", JsonConvert.SerializeObject(v));
                    LogAndAppend(logBuilder, LogLevel.Information, $"[DB] Insertados {viajesNuevos.Count} viajes nuevos");
                }

                // PASO 4: Insertar detalles
                if (detallesBulk.Any())
                {
                    await _db.BulkInsertAsync(detallesBulk);
                    if (logWriter != null)
                        foreach (var d in detallesBulk)
                            LogCsvOperacion(logWriter, "Insert", "Detalle", d.IdProceso, d.TempSeqNro, d.NroOtro, d.CodPuertoEmbarque, d.CorrelativoViaje, null, d.pagina_api, "OK", "Detalle insertado", JsonConvert.SerializeObject(d));
                    LogAndAppend(logBuilder, LogLevel.Information, $"[DB] Insertados {detallesBulk.Count} detalles");
                }

                await trx.CommitAsync();
                LogAndAppend(logBuilder, LogLevel.Information, "[SUCCESS] Transacción completada");

                // Limpieza de memoria post transacción
                procesosBulk.Clear();
                viajesBulk.Clear();
                detallesBulk.Clear();
            }
            catch (Exception dbEx)
            {
                await trx.RollbackAsync();
                LogAndAppend(logBuilder, LogLevel.Error, $"[ROLLBACK] Transacción fallida para {fechaProceso:yyyy-MM-dd}: {dbEx.Message}");
                if (logWriter != null)
                    LogCsvOperacion(logWriter, "Error", "Global", null, null, null, null, null, null, null, "Error", $"Rollback: {dbEx.Message}", dbEx.ToString());
                throw new Exception($"Falló la transacción para {fechaProceso:yyyy-MM-dd}", dbEx);
            }
            finally
            {
                // Forzar limpieza
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public async Task DescargarYGuardarProcesoCompletoAsync(int idProceso)
        {
            var viajes = await AsoexApiHelper.ObtenerDatosPaginadosConNumeroPaginaAsync<ProcesoViajeDto, ProcesoViajeDtoPaged>(
                _config, "MoviminetosProcesoviajes", $"{GetUrl("MoviminetosProcesoviajes")}?idproceso={idProceso}", _logger);

            var detalles = await AsoexApiHelper.ObtenerDatosPaginadosConNumeroPaginaAsync<ProcesoDetalleDto, ProcesoDetalleDtoPaged>(
                _config, "MoviminetosProcesodetalle", $"{GetUrl("MoviminetosProcesodetalle")}?idproceso={idProceso}", _logger);

            var procesoJson = new
            {
                proceso = idProceso,
                viajes,
                detalles
            };

            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "asoex", "jsonProceso");
            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            var jsonFilePath = Path.Combine(rootPath, $"{idProceso}.json");
            File.WriteAllText(jsonFilePath, Newtonsoft.Json.JsonConvert.SerializeObject(procesoJson, Newtonsoft.Json.Formatting.Indented));
        }

        // LOGGING CSV
        private void LogCsvOperacion(StreamWriter writer, string accion, string entidad, int? idProceso, int? tempSeqNro, int? nroOtro, string codPuertoEmbarque, int? correlativoViaje, string status, int? paginaApi, string resultado, string mensaje, string payload)
        {
            writer.WriteLine($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss},{accion},{entidad},{idProceso},{tempSeqNro},{nroOtro},{codPuertoEmbarque},{correlativoViaje},{status},{paginaApi},{resultado},{mensaje},{payload}");
            writer.Flush();
        }

        #region Métodos Privados Auxiliares

        private void LogAndAppend(StringBuilder logBuilder, LogLevel logLevel, string message, Exception? ex = null)
        {
            _logger.Log(logLevel, ex, message);
            logBuilder.AppendLine($"[{DateTime.UtcNow:HH:mm:ss}] [{logLevel.ToString().ToUpper()}] {message}");

            if (ex != null)
            {
                logBuilder.AppendLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private string GetUrl(string key) => _config[$"Asoex:Apis:{key}:url"];

        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;
            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            _logger.LogWarning("Formato de fecha inválido encontrado en la API: {DateString}", dateString);
            return null;
        }

        #endregion
    }
}
