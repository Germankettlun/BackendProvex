using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Sql.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using System.Data;
using ProvexBackendAPI.Helpers.Shared.Extensions;

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
                var value = rdr.FirstExistingAsString("Valor", "Value", "VALOR", "ID");
                var label = rdr.FirstExistingAsString("Texto", "Label", "NOMBRE", "DESCRIPCION");

                list.Add(new ComboItem
                {
                    Value = value,
                    Label = label
                });
            }

            return list;
        }

        public async Task<List<ComboItem>> LlenaComboEnvaseProductorEspecieVariedad(string idProductor, string idEspecie, string idVariedad)
        {
            var list = new List<ComboItem>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();


            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_ENVASE_COSECHA_ESTIMACION", conn)
            {
                CommandType = CommandType.StoredProcedure
            };


            cmd.Parameters.Add(new SqlParameter("@ID_PRODUCTOR", SqlDbType.NVarChar, 50) { Value = idProductor });
            cmd.Parameters.Add(new SqlParameter("@ID_ESPECIE", SqlDbType.VarChar, 50) { Value = idEspecie });
            cmd.Parameters.Add(new SqlParameter("@ID_VARIEDAD", SqlDbType.VarChar, 50) { Value = idVariedad });

            await using var rdr = await cmd.ExecuteReaderAsync();


            while (await rdr.ReadAsync())
            {
                var value = rdr.FirstExistingAsString("Valor", "Value", "VALOR", "ID");
                var label = rdr.FirstExistingAsString("Texto", "Label", "NOMBRE", "DESCRIPCION");

                list.Add(new ComboItem
                {
                    Value = value,
                    Label = label
                });
            }

            return list;
        }
        
    }
}
