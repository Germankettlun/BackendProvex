// Services/Reports/EmbarquesReportService.cs
using ClosedXML.Excel;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ProvexApi.Models.Reports;
using ProvexApi.Services.Integrations;   // ISharePointService
using CtaCteModel = ProvexApi.Models.Reports.CtaCteRow;

namespace ProvexApi.Services.Reports
{
    public sealed class EmbarquesReportService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;
        private readonly ISharePointService _sp;
        private readonly ILogger<EmbarquesReportService> _log;

        public EmbarquesReportService(
            IConfiguration configuration,
            IHostEnvironment env,
            ISharePointService sp,
            ILogger<EmbarquesReportService> log)
        {
            _configuration = configuration;
            _env = env;
            _sp = sp;
            _log = log;
        }

        public async Task<(MemoryStream stream, string savedPath, string fileName)>
            BuildAsync(string empresa, string temporada, CancellationToken ct)
        {
            var cs =
                _configuration["ConnectionProvexStrings:DatabaseConnection"]
                ?? _configuration.GetConnectionString("DatabaseConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionProvexStrings__DatabaseConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DatabaseConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Falta ConnectionProvexStrings:DatabaseConnection.");

            const string sqlEmbarques = @"
                SELECT CodigoTemporada, GrupoProductor, NombreProductorReal, Especie, Variedad, Mercado, Recibidor,
                       CodigoGrupoCalibre, CodigoCalibre, VariedadEti, PesoEnvop, PesoNetoReal, CodigoNave, Nave,
                       SemanaZarpe, Fecha_Zarpe, Fecha_Arribo, Destino, TipoNave, CodigoEnvop, DescEnvop,
                       CajasEquivalentes, Cajas, CondicionLlegada, DeterminanteCondicion, EstadoLiquidacion,
                       TotalRetornoFOB, FOBUnitarioBulto, Comision, OtrosGastos, TotalRetorno,
                       RetornoUnitCajasBulto, RetornoUnitCajasBase, TotalFOBPercibido, FOBUnitPercibido,
                       ComisionRetorno, OtrosCostos, TotalRetornoProductorLaFecha,
                       RetornoProductorPercibidoCajaBulto, RetornoProductorPercibidoCajaBase,
                       Dif_Retorno_Estimado_Retorno_Percibido, Fecha_registro, FOBCajaBase
                FROM dbo.SDT_View_PagoProductor
                WHERE CodigoEmpresa=@empresa AND CodigoTemporada=@temporada";

            const string sqlCtaCte = @"
                SELECT 
                   Productor     = Productor,
                   Especie       = DescEspecie,
                   Fecha         = Fecha,
                   Glosa         = GlosaDocumento,
                   Item          = DescItem,
                   SubItem       = DescSubItem,
                   TipoCambio    = TiCaCursoLegal,
                   Dolar         = SUM(Monto_FU),
                   Pesos         = SUM(Monto_CL)
                FROM dbo.SDT_View_CargosManuales
                WHERE CodigoEmpresa=@empresa AND CodigoTemporada=@temporada AND SwPagoProductores='SI'
                GROUP BY Productor, DescEspecie, Fecha, GlosaDocumento, DescItem, DescSubItem, TiCaCursoLegal";

            List<EmbarquesRow> rows;
            List<CtaCteModel> cta;
            await using (var con = new SqlConnection(cs))
            {
                await con.OpenAsync(ct);
                rows = (await con.QueryAsync<EmbarquesRow>(
                    new CommandDefinition(sqlEmbarques, new { empresa, temporada }, cancellationToken: ct))).ToList();

                cta = (await con.QueryAsync<CtaCteModel>(
                    new CommandDefinition(sqlCtaCte, new { empresa, temporada }, cancellationToken: ct))).ToList();
            }
            if (rows.Count == 0 && cta.Count == 0)
                throw new InvalidOperationException("Sin datos para generar el reporte.");

            var candidates = new[]
            {
                Path.Combine(_env.ContentRootPath, "wwwroot", "templates", "PlanillaWeb.xlsx"),
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "templates", "PlanillaWeb.xlsx")
            };
            var tplPath = candidates.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException("Plantilla no encontrada. Buscados: " + string.Join(" | ", candidates));

            using var wb = new XLWorkbook(tplPath);

            // ---------- Hoja: Embarques ----------
            var ws = wb.Worksheet("Embarques");
            const int headerRow = 13;
            const int dataStart = 14;

            var headers = new List<(int Col, string Raw)>();
            for (int c = 1; ; c++)
            {
                var raw = ws.Cell(headerRow, c).GetString();
                if (string.IsNullOrWhiteSpace(raw)) break;
                headers.Add((c, raw));
            }

            static string NormLbl(string s)
            {
                var t = new string(s.Where(ch => char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch)).ToArray());
                return string.Join(" ", t.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }
            static IXLCell FindMetricCellRightOf(IXLWorksheet sheet, string label)
            {
                var goal = NormLbl(label);
                for (int r = 5; r <= 12; r++)
                {
                    var left = sheet.Cell(r, 1).GetString();
                    if (NormLbl(left) == goal) return sheet.Cell(r, 2);
                }
                return sheet.Cell(8, 2);
            }

            var datosAl = rows.Select(r => r.Fecha_registro ?? DateTime.Today).DefaultIfEmpty(DateTime.Today).Max();
            var promFob = rows.Where(r => r.FOBCajaBase.HasValue).Select(r => r.FOBCajaBase!.Value)
                              .DefaultIfEmpty(0m).Average();

            var datosAlCell = FindMetricCellRightOf(ws, "Datos Al");
            datosAlCell.Clear(); datosAlCell.SetValue(datosAl);
            datosAlCell.Style.DateFormat.Format = EmbarquesColumnMaps.DateFormat;

            //var promCell = FindMetricCellRightOf(ws, "Promedio Neto Caja Base");
            //promCell.Clear(); promCell.SetValue(promFob);
            //promCell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat;

            var hadFilter = ws.AutoFilter.IsEnabled;
            var filterRange = hadFilter ? ws.AutoFilter.Range.RangeAddress.ToString() : null;

            var lastUsed = ws.LastRowUsed()?.RowNumber() ?? dataStart - 1;
            if (lastUsed >= dataStart)
                ws.Range(dataStart, 1, lastUsed, headers.Count).Clear(XLClearOptions.Contents);

            if (rows.Count > 1) ws.Row(dataStart).InsertRowsBelow(rows.Count - 1);

            for (int c = 1; c <= headers.Count; c++)
            {
                var styleSrc = ws.Cell(dataStart, c).Style;
                for (int i = 0; i < rows.Count; i++)
                    ws.Cell(dataStart + i, c).Style = styleSrc;
            }

            static void SetCell(IXLCell cell, object? val)
            {
                if (val is null)
                {
                    // Deja el estilo (fondo/formato) y borra solo el contenido
                    cell.Clear(XLClearOptions.Contents); // <- antes usabas Clear() y perdías el formato
                                                         // Alternativa: cell.SetValue(string.Empty);
                    return;
                }

                switch (val)
                {
                    case DateTime dt:
                        cell.SetValue(dt);
                        cell.Style.DateFormat.Format = EmbarquesColumnMaps.DateFormat;
                        break;
                    case int i: cell.SetValue(i); cell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat; break;
                    case long l: cell.SetValue(l); cell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat; break;
                    case short s: cell.SetValue(s); cell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat; break;
                    case float f: cell.SetValue(f); cell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat; break;
                    case double d: cell.SetValue(d); cell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat; break;
                    case decimal m: cell.SetValue(m); cell.Style.NumberFormat.Format = EmbarquesColumnMaps.NumFormat; break;
                    default: cell.SetValue(val.ToString()); break;
                }
            }


            for (int i = 0; i < rows.Count; i++)
            {
                int rIdx = dataStart + i;
                var src = rows[i];
                foreach (var (col, raw) in headers)
                    if (EmbarquesColumnMaps.TryGetter(raw, out var getter))
                        SetCell(ws.Cell(rIdx, col), getter(src));
            }
            if (hadFilter && filterRange is not null) ws.Range(filterRange).SetAutoFilter();

            // ---------- Hoja: Cta Cte ----------
            var wsCta = wb.Worksheet("CtaCte");
            if (wsCta != null)
            {
                const int headerRowCta = 13;
                const int dataStartCta = 14;

                var headersCta = new List<(int Col, string Raw)>();
                for (int c = 1; ; c++)
                {
                    var raw = wsCta.Cell(headerRowCta, c).GetString();
                    if (string.IsNullOrWhiteSpace(raw)) break;
                    headersCta.Add((c, raw));
                }

                var datosAlCellCta = FindMetricCellRightOf(wsCta, "Datos Al");
                datosAlCellCta.Clear(); datosAlCellCta.SetValue(datosAl);
                datosAlCellCta.Style.DateFormat.Format = EmbarquesColumnMaps.DateFormat;

                var lastCta = wsCta.LastRowUsed()?.RowNumber() ?? dataStartCta - 1;
                if (lastCta >= dataStartCta)
                    wsCta.Range(dataStartCta, 1, lastCta, headersCta.Count).Clear(XLClearOptions.Contents);

                if (cta.Count > 1) wsCta.Row(dataStartCta).InsertRowsBelow(cta.Count - 1);

                for (int c = 1; c <= headersCta.Count; c++)
                {
                    var styleSrc = wsCta.Cell(dataStartCta, c).Style;
                    for (int i = 0; i < cta.Count; i++)
                        wsCta.Cell(dataStartCta + i, c).Style = styleSrc;
                }

                for (int i = 0; i < cta.Count; i++)
                {
                    int rIdx = dataStartCta + i;
                    var src = cta[i];
                    foreach (var (col, raw) in headersCta)
                        if (CtaCteColumnMaps.TryGetter(raw, out Func<CtaCteModel, object?> getter))
                            SetCell(wsCta.Cell(rIdx, col), getter(src));
                }
            }

            // ---------- Guardar local ----------
            var outDir = Path.Combine(_env.ContentRootPath, "wwwroot", "output");
            Directory.CreateDirectory(outDir);
            var fileName = $"ConsultaEmbarque_{empresa}_{temporada}_{DateTime.Now:yyyyMMdd}.xlsx";
            var savedPath = Path.Combine(outDir, fileName);
            wb.SaveAs(savedPath);

            // -> stream para descarga HTTP
            var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            // ---------- Subir a SharePoint (no rompe si falla) ----------
            try
            {
                await using var fs = File.OpenRead(savedPath);
                var result = await _sp.UploadAsync(fs, fileName, empresa, temporada, ct);
                if (result.Success)
                    _log.LogInformation("SharePoint OK. Url={Url} Empresa={Emp} Temporada={Temp}", result.WebUrl, empresa, temporada);
                else
                    _log.LogError("SharePoint FAIL. Msg={Msg} Empresa={Emp} Temporada={Temp}", result.Message, empresa, temporada);
            }
            catch (OperationCanceledException)
            {
                _log.LogWarning("SharePoint cancelado. Empresa={Emp} Temporada={Temp}", empresa, temporada);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SharePoint excepción. Empresa={Emp} Temporada={Temp}", empresa, temporada);
            }

            return (ms, savedPath, fileName);
        }
    }
}
