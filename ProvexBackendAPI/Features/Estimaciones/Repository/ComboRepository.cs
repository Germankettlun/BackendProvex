using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Sql.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Helpers.Shared.Extensions;
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
