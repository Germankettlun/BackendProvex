using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProvexApi.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProvexApi.Controllers.BackEnd.Packinglist
{
    [Authorize(Policy = "LocalOnly")]
    [ApiController]
    [Route("api/packinglist")]
    public class PackingListController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ProvexDbContext _context;

        public PackingListController(IWebHostEnvironment env, ProvexDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpGet("download")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadPackingList(
            [FromQuery] string empresa,
            [FromQuery] string temporada,
            [FromQuery] string embarque)
        {
            // 1. Ruta de la plantilla
            string plantillaPath = Path.Combine(_env.WebRootPath, "templates", "PACKINGLIST.xlsx");
            if (!System.IO.File.Exists(plantillaPath))
                return NotFound("No se encontró la plantilla Excel");

            // 2. Consulta SQL
            var datos = await _context.ViewPLPangea
                .FromSqlRaw(@"SELECT    [Receiver], [Lot #], [Product Code], [Producto], [Variety], [Label],
                                        [Dep PORT], [Arri PORT], [Container Nbr.], [Vessel Name], [ETD], [ETA],
                                        [Vessel  Number], [Country of Origin], [USDA Seal/ SAG Seal], [Size],
                                        [Commodity], [Pallet number], [Thermometer No], [Packing Date],
                                        [(21).Carton Weight], [NBoxes], [Pack Style], [Pack], [Category], [PLU],
                                        [Grower Name], [Grower Code], [Invoice], [ShippingCompany]
                              FROM [PROVEX].[dbo].[View_PL_Pangea] 
                              WHERE CodigoEmpresa = {0} AND CodigoTemporada = {1} AND CodNave = {2}",
                    empresa, temporada, embarque)
                .ToListAsync();

            if (datos == null || datos.Count == 0)
                return NotFound("No se encontraron datos para esos parámetros. Revise los filtros o el modelo.");

            string receiverSafe = (datos[0].Receiver ?? "SIN_RECEIVER").Replace('/', ' ').Replace('\\', ' ').Trim();
            string fileName = $"PACKING LIST {embarque} {receiverSafe}.xlsx".ToUpper();

            // Abrir la plantilla en un MemoryStream para lectura
            using (var ms = new MemoryStream(System.IO.File.ReadAllBytes(plantillaPath)))
            using (var package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets["PGG PL"];
                if (ws == null)
                    return BadRequest("No se encontró la hoja PGG PL en la plantilla.");

                int headerRow = 8;
                int dataStartRow = 9;
                int colCount = 28; // A la AB, ajusta según tus columnas

                // 3. Limpiar filas de datos previas en el detalle
                int lastRow = ws.Dimension.End.Row;
                if (lastRow >= dataStartRow)
                    ws.Cells[$"A{dataStartRow}:AB{lastRow}"].Clear();

                // 4. Insertar filas si corresponde
                if (datos.Count > 1)
                    ws.InsertRow(dataStartRow + 1, datos.Count - 1, dataStartRow);

                // 5. Escribir los datos y dar formato a B y C
                var fillColor = ColorTranslator.FromHtml("#FFFF00");
                for (int i = 0; i < datos.Count; i++)
                {
                    var row = datos[i];
                    int excelRow = dataStartRow + i;
                    ws.Cells[excelRow, 1].Value = row.Receiver;
                    ws.Cells[excelRow, 2].Value = null; // Lot #
                    ws.Cells[excelRow, 3].Value = null; // Product Code
                    ws.Cells[excelRow, 4].Value = row.Producto;
                    ws.Cells[excelRow, 5].Value = row.Variety;
                    ws.Cells[excelRow, 6].Value = row.Label;
                    ws.Cells[excelRow, 7].Value = row.DepPORT;
                    ws.Cells[excelRow, 8].Value = row.ArriPORT;
                    ws.Cells[excelRow, 9].Value = row.ContainerNbr;
                    ws.Cells[excelRow, 10].Value = row.VesselName;
                    ws.Cells[excelRow, 11].Value = row.ETD?.ToString("yyyy-MM-dd");
                    ws.Cells[excelRow, 12].Value = row.ETA?.ToString("yyyy-MM-dd");
                    ws.Cells[excelRow, 13].Value = row.VesselNumber;
                    ws.Cells[excelRow, 14].Value = row.CountryOfOrigin;
                    ws.Cells[excelRow, 15].Value = row.UsdaSeal;
                    ws.Cells[excelRow, 16].Value = row.Size;
                    ws.Cells[excelRow, 17].Value = row.Commodity;
                    ws.Cells[excelRow, 18].Value = row.PalletNumber;
                    ws.Cells[excelRow, 19].Value = row.ThermometerNo;
                    ws.Cells[excelRow, 20].Value = row.PackingDate?.ToString("yyyy-MM-dd");
                    ws.Cells[excelRow, 21].Value = row.CartonWeight;
                    ws.Cells[excelRow, 22].Value = row.NBoxes;
                    ws.Cells[excelRow, 23].Value = row.PackStyle;
                    ws.Cells[excelRow, 24].Value = row.Pack;
                    ws.Cells[excelRow, 25].Value = row.Category;
                    ws.Cells[excelRow, 26].Value = row.PLU;
                    ws.Cells[excelRow, 27].Value = row.GrowerName;
                    ws.Cells[excelRow, 28].Value = row.GrowerCode;

                    // Fondo amarillo B y C
                    ws.Cells[excelRow, 2, excelRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[excelRow, 2, excelRow, 3].Style.Fill.BackgroundColor.SetColor(fillColor);
                }

                // 6. Shipping Company (B4) y Nº Invoice (B3)
                ws.Cells[4, 2].Value = datos[0].ShippingCompany ?? "MAERSK";
                ws.Cells[3, 2].Value = datos[0].Invoice ?? "";

                // 7. Mensaje final
                int lastDataRow = dataStartRow + datos.Count - 1;
                int messageRow = lastDataRow + 2;
                ws.InsertRow(messageRow, 1);
                ws.Cells[messageRow, 1].Value = "Please leave columns B and C in blank";

                // --- 8. Crear/Actualizar tabla Packinglist sobre el rango de datos ---
                var table = ws.Tables.FirstOrDefault(t => t.Name == "Packinglist");
                string tableRange = $"A{headerRow}:AB{lastDataRow}";
                if (table != null)
                    ws.Tables.Delete(table.Name);

                // Crear la tabla en el rango de datos actualizado
                table = ws.Tables.Add(ws.Cells[tableRange], "Packinglist");


                // 1. Fondo blanco en todo el rango de datos de la tabla
                var dataRange = table.Range;
                dataRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                dataRange.Style.Fill.BackgroundColor.SetColor(Color.White);

                // 2. Bordes delgados en todas las celdas de la tabla
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // 3. Mantener fondo amarillo SOLO en columnas B y C (del detalle, filas de datos)
                ws.Cells[$"B{dataStartRow}:C{lastDataRow}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[$"B{dataStartRow}:C{lastDataRow}"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                // --- 9. Actualizar origen de PivotTable1 ---
                var pivotSheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name == "Summary");
                if (pivotSheet != null && pivotSheet.PivotTables.Any())
                {
                    var pivot = pivotSheet.PivotTables.FirstOrDefault(p => p.Name == "PivotTable1");
                    if (pivot != null)
                    {
                        pivot.CacheDefinition.SourceRange = table.Range;
                        // Esto no elimina el formato, solo actualiza el rango fuente.
                    }
                }

                // --- 10. Guardar SIEMPRE en un NUEVO MemoryStream ---
                using (var outStream = new MemoryStream())
                {
                    package.SaveAs(outStream);
                    outStream.Position = 0;
                    return File(
                        outStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
        }
    }
}
