using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Helpers.Validation;
using ProvexBackendAPI.Repository;
using ProvexBackendAPI.Repository.IRepository;
using System.Data;

namespace ProvexBackendAPI.Features.Estimaciones.Services
{
    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;
        private readonly IGenericRepository repository;

        public ComboService(IComboRepository comboRepository, IGenericRepository repository)
        {
            _comboRepository = comboRepository;
            this.repository = repository;
        }
        public async Task<List<ComboItemDto>> GetComboGenericoAsync(ComboRequest req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (req is null) throw new ArgumentNullException(nameof(req));

            if (string.IsNullOrWhiteSpace(req.CodigoEmpresa))
                throw new ArgumentException("CodigoEmpresa es obligatorio.", nameof(req.CodigoEmpresa));

            if (string.IsNullOrWhiteSpace(req.NombreCombo))
                throw new ArgumentException("NombreCombo es obligatorio.", nameof(req.NombreCombo));


            var parameters = new[]
            {
            new SqlParameter("@NombreCombo",       SqlDbType.NVarChar, 50) { Value = req.NombreCombo.Trim().ToUpperInvariant() },
            new SqlParameter("@CodigoEmpresa",     SqlDbType.VarChar,  50) { Value = req.CodigoEmpresa.Trim().ToUpperInvariant() },
            new SqlParameter("@CodigoEspecie",     SqlDbType.NVarChar, 50) { Value = (object?)req.CodigoEspecie.Trim().ToUpperInvariant()      ?? DBNull.Value },
            new SqlParameter("@CodigoGrupoProductor", SqlDbType.VarChar, 50) { Value = (object?)req.CodigoGrupoProductor.Trim().ToUpperInvariant() ?? DBNull.Value },
            new SqlParameter("@CodigoProductor",   SqlDbType.NVarChar, 50) { Value = (object?)req.CodigoProductor.Trim().ToUpperInvariant()    ?? DBNull.Value },
            new SqlParameter("@CodigoVariedad",    SqlDbType.VarChar,  50) { Value = (object?)req.CodigoVariedad.Trim().ToUpperInvariant()     ?? DBNull.Value },
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].usp_UI_LlenaComboGenerico",parameters);

            var list = new List<ComboItemDto>(dataTable.Rows.Count);

            foreach (DataRow row in dataTable.Rows)
            {
                // VALUE (Valor / Value / VALOR / ID)
                string value =
                    (row.Table.Columns.Contains("Valor") && row["Valor"] != DBNull.Value ? row["Valor"].ToString() : null) ??
                    (row.Table.Columns.Contains("Value") && row["Value"] != DBNull.Value ? row["Value"].ToString() : null) ??
                    (row.Table.Columns.Contains("VALOR") && row["VALOR"] != DBNull.Value ? row["VALOR"].ToString() : null) ??
                    (row.Table.Columns.Contains("ID") && row["ID"] != DBNull.Value ? row["ID"].ToString() : null) ??
                    string.Empty;

                // LABEL (Texto / Label / NOMBRE / DESCRIPCION)
                string label =
                    (row.Table.Columns.Contains("Texto") && row["Texto"] != DBNull.Value ? row["Texto"].ToString() : null) ??
                    (row.Table.Columns.Contains("Label") && row["Label"] != DBNull.Value ? row["Label"].ToString() : null) ??
                    (row.Table.Columns.Contains("NOMBRE") && row["NOMBRE"] != DBNull.Value ? row["NOMBRE"].ToString() : null) ??
                    (row.Table.Columns.Contains("DESCRIPCION") && row["DESCRIPCION"] != DBNull.Value ? row["DESCRIPCION"].ToString() : null) ??
                    string.Empty;

                list.Add(new ComboItemDto
                {
                    Value = value,
                    Label = label
                });
            }
            return list;
        }

        public async Task<List<ComboItemDto>> GetComboEnvaseProductorEspecieVariedadAsync(string idProductor, string idEspecie, string idVariedad)
        {
            if (string.IsNullOrWhiteSpace(idProductor))
                throw new ArgumentException("El id de productor es requerido.", nameof(idProductor));
            if (string.IsNullOrWhiteSpace(idEspecie))
                throw new ArgumentException("El id de especie es requerido.", nameof(idEspecie));
            if (string.IsNullOrWhiteSpace(idEspecie))
                throw new ArgumentException("El id de variedad es requerido.", nameof(idVariedad));

            var rows = await _comboRepository.LlenaComboEnvaseProductorEspecieVariedad(idProductor, idEspecie, idVariedad);

            var list = new List<ComboItemDto>(rows.Count);
            foreach (var r in rows)
            {
                list.Add(new ComboItemDto
                {
                    Value = r.Value,
                    Label = r.Label
                });
            }
            return list;
        }
    }
}
