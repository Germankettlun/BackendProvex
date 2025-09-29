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
        public async Task<List<ComboItem>> LlenaComboGenericoAsync(ComboRequest req)
        {
            var list = new List<ComboItem>();

            await using var conn = new SqlConnection(_connString);
            await conn.OpenAsync();

            
            await using var cmd = new SqlCommand("[Estimaciones].usp_UI_LlenaComboGenerico", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            
            cmd.Parameters.Add(new SqlParameter("@NombreCombo", SqlDbType.NVarChar, 50) { Value = req.NombreCombo });
            cmd.Parameters.Add(new SqlParameter("@CodigoEmpresa ", SqlDbType.VarChar, 50) { Value = req.CodigoEmpresa });
            cmd.Parameters.Add(new SqlParameter("@CodigoEspecie", SqlDbType.NVarChar, 50) { Value = (object?)req.CodigoEspecie ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@CodigoGrupoProductor ", SqlDbType.VarChar, 50) { Value = (object?)req.CodigoGrupoProductor ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@CodigoProductor", SqlDbType.NVarChar, 50) { Value = (object?)req.CodigoProductor ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@CodigoVariedad ", SqlDbType.VarChar, 50) { Value = (object?)req.CodigoVariedad ?? DBNull.Value });


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
