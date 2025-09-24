using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Sql.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using System.Data;

namespace ProvexBackendAPI.Features.Estimaciones.Repository
{
    public class ComboRepository : IComboRepository
    {
        private readonly string _connString;

        public ComboRepository(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")!;
        }
        public async Task<List<ComboItem>> LlenaComboGenericoAsync(string nombreCombo, string codigoEmpresa)
        {
            var list = new List<ComboItem>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            
            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_LlenaComboGenerico", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            
            cmd.Parameters.Add(new SqlParameter("@NombreCombo", SqlDbType.NVarChar, 50) { Value = nombreCombo });
            cmd.Parameters.Add(new SqlParameter("@CodigoEmpresa ", SqlDbType.VarChar, 50) { Value = codigoEmpresa });
            

            await using var rdr = await cmd.ExecuteReaderAsync();

            
            while (await rdr.ReadAsync())
            {
                var value = ReadFirstExistingAsString(rdr, "Valor");
                var label = ReadFirstExistingAsString(rdr, "Texto");

                list.Add(new ComboItem
                {
                    Value = value,
                    Label = label
                });
            }

            return list;
        }

        private static string ReadFirstExistingAsString(SqlDataReader rdr, params string[] candidates)
        {
            foreach (var name in candidates)
            {
                var ordinal = SafeOrdinal(rdr, name);
                if (ordinal >= 0 && !rdr.IsDBNull(ordinal))
                {
                    // Devuelve como string independiente del tipo subyacente
                    return Convert.ToString(rdr.GetValue(ordinal)) ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private static int SafeOrdinal(SqlDataReader rdr, string column)
        {
            try { return rdr.GetOrdinal(column); }
            catch (IndexOutOfRangeException) { return -1; }
        }
    }
}
