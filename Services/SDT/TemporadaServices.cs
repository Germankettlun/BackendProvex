using Microsoft.Data.SqlClient;
using ProvexApi.Helper;

namespace ProvexApi.Services.SDT
{
    public class TemporadaService
    {
        private readonly IConfiguration _configuration;

        public TemporadaService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retorna (codigoSDT, codigoInterno) o (null, null) si no existe.
        /// </summary>
        public async Task<(string? codigoSDT, string? codigoInterno, string? FechaInicio)> GetTemporadaMappingAsync(string temporada, string? empresa)
        {
            try
            {
                if (string.IsNullOrEmpty(temporada))
                {
                    // Podrías retornar (null, null) o lanzar excepción
                    throw new ArgumentException("Falta el parámetro 'temporada'.", nameof(temporada));
                }
                if (string.IsNullOrEmpty(empresa))
                {
                    // Podrías retornar (null, null) o lanzar excepción
                    throw new ArgumentException("Falta el parámetro 'empresa'.", nameof(empresa));
                }

                //var connectionString = _configuration.GetConnectionString("DatabaseConnection");
                var connectionString = _configuration["ConnectionProvexStrings:DatabaseConnection"];
                var sql = @" SELECT TOP 1 CodigoSDT, CodigoInterno, Fecha_Inicio FROM [dbo].[API_SDT_Conversion_Temporada] WHERE CodigoSDT = @temp and codigoEmpresa = @emp ";
          

                string? codigoSDT = null;
                string? codigoInterno = null;
                string? FechaInicio = null;

                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@temp", temporada);
                        cmd.Parameters.AddWithValue("@emp", empresa);

                        //LogHelper.Log($"Query : {sql} -  emp {empresa} | temp {temporada}", "ServiceTem");

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                return (null, null, null);
                            }

                            // Recuperamos los valores
                            codigoSDT = reader["CodigoSDT"].ToString();
                            codigoInterno = reader["CodigoInterno"].ToString();
                            FechaInicio = reader["Fecha_Inicio"].ToString();
                        }
                    }
                }

                return (codigoSDT, codigoInterno, FechaInicio);
               
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error : {ex.Message}", "ServiceTem");
                throw new Exception("Error al insertar log : " + ex.Message);
            }         
        }

        /// <summary>
        /// Retorna el IdFacilitie (int) correspondiente al codigoFacilitie y al idEmpresa dados.
        /// Devuelve null si no se encuentra ningún registro.
        /// </summary>
        public async Task<string?> GetFacilitieIdAsync(string idEmpresa, string codigoFacilitie)
        {
            try
            {
                var connectionString = _configuration["ConnectionProvexStrings:DatabaseConnection"];
                var sql = @"
                    SELECT TOP 1 
                        IdFacilitie 
                    FROM [dbo].[Facilitie] 
                    WHERE IdEmpresa = @idEmp AND CodigoFacility = @codFac";

                string? idFacilitie = null;
                using (var conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@idEmp", idEmpresa);
                        cmd.Parameters.AddWithValue("@codFac", codigoFacilitie);

                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            idFacilitie = result.ToString();
                        }
                    }
                }

                return idFacilitie;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error al obtener IdFacilitie: {ex.Message}", "ServiceFacilitie");
                throw new Exception("Error al obtener IdFacilitie: " + ex.Message);
            }
        }
    }
}
