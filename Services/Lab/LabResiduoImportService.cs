// Services/Lab/LabResiduoImportService.cs
using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;            // <-- provider correcto
using Microsoft.EntityFrameworkCore;
using ProvexApi.Data;                     // <-- tu DbContext
using ProvexApi.Models.Lab;

namespace ProvexApi.Services.Lab;

public sealed class LabResiduoImportService
{
    private readonly ProvexDbContext _db;
    private readonly ILogger<LabResiduoImportService> _logger;

    public LabResiduoImportService(ProvexDbContext db, ILogger<LabResiduoImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(int ins, int upd, int tot)> UpsertAsync(IEnumerable<LabResiduoRowV2> rows, CancellationToken ct)
    {
        using var dtMaster = Build(rows);
        using var dtSpecs = BuildSpecs(rows);

        var cn = _db.Database.GetDbConnection();
        if (cn.State != ConnectionState.Open) await cn.OpenAsync(ct);
        await using var cmd = cn.CreateCommand();
        cmd.CommandText = "dbo.sp_Lab_Residuo_Upsert";
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.Add(new SqlParameter("@Items", SqlDbType.Structured) { TypeName = "dbo.Lab_ResiduoType", Value = dtMaster });
        cmd.Parameters.Add(new SqlParameter("@Specs", SqlDbType.Structured) { TypeName = "dbo.Lab_ResiduoSpecType", Value = dtSpecs });

        using var rdr = await cmd.ExecuteReaderAsync(ct);
        int ins = 0, upd = 0, tot = 0; if (await rdr.ReadAsync(ct)) { ins = rdr.GetInt32(0); upd = rdr.GetInt32(1); tot = rdr.GetInt32(2); }
        return (ins, upd, tot);
    }


    private static DataTable Build(IEnumerable<LabResiduoRowV2> rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("Project", typeof(string));   // C_ANALISIS
        dt.Columns.Add("FechaEnvio", typeof(DateTime)); // F_ENVIO
        dt.Columns.Add("Product", typeof(string));   // PROD
        dt.Columns.Add("Description", typeof(string));   // DSC
        dt.Columns.Add("Especie", typeof(string));
        dt.Columns.Add("Variedad", typeof(string));
        dt.Columns.Add("Productor", typeof(string));
        dt.Columns.Add("Csg", typeof(string));
        dt.Columns.Add("CodigoMuestra", typeof(string));   // COD_MUESTRA
        dt.Columns.Add("Variation", typeof(string));   // VARIACION
        dt.Columns.Add("Name", typeof(string));   // PARAM
        dt.Columns.Add("EntryValue", typeof(decimal));  // RESULT
        dt.Columns.Add("Unit", typeof(string));

        foreach (var r in rows)
        {
            dt.Rows.Add(
                r.C_ANALISIS?.Trim(),
                (object?)ParseDate(r.F_ENVIO) ?? DBNull.Value,
                r.PROD,
                r.DSC,
                r.ESPECIE,
                r.VARIEDAD,
                r.PRODUCTOR,
                r.CSG,
                r.COD_MUESTRA,
                r.VARIACION,
                r.PARAM,
                (object?)ParseDecimalComma(r.RESULT) ?? DBNull.Value,
                r.UNIT
            );
        }
        return dt;
    }

    private static DataTable BuildSpecs(IEnumerable<LabResiduoRowV2> rows)
    {
        var dt = new DataTable();
        dt.Columns.Add("Project", typeof(string));
        dt.Columns.Add("CodigoMuestra", typeof(string));
        dt.Columns.Add("Variation", typeof(string));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Season", typeof(string));
        dt.Columns.Add("MaxVal", typeof(decimal));
        dt.Columns.Add("CMerc", typeof(string));
        foreach (var r in rows)
        {
            if (r.SPEC == null) continue;
            foreach (var s in r.SPEC)
            {
                dt.Rows.Add(
                    r.C_ANALISIS?.Trim(),
                    r.COD_MUESTRA,
                    r.VARIACION,
                    r.PARAM,
                    s.SEASON,
                    (object?)ParseDecimalComma(s.MAXVAL) ?? DBNull.Value,
                    (s.CMERC ?? "").Trim().ToUpperInvariant()
                );
            }
        }
        return dt;
    }
    private static DateTime? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var fmts = new[] { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };
        if (DateTime.TryParseExact(s.Trim(), fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d.Date;
        if (DateTime.TryParse(s, out d)) return d.Date;
        return null;
    }

    private static decimal? ParseDecimalComma(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : null;
    }
}
