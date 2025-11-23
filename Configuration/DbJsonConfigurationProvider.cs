using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ProvexApi.Helper;


namespace ProvexApi.Configuration
{
    public class DbJsonConfigurationProvider : ConfigurationProvider
    {
        private readonly DbJsonConfigurationSource _source;

        public DbJsonConfigurationProvider(DbJsonConfigurationSource source)
        {
            _source = source;
        }
        public override void Load()
        {
            try
            {
                // Validar que la cadena de conexión esté inicializada antes de usarla
                if (string.IsNullOrWhiteSpace(_source?.ConnectionString))
                {
                    var msg = $"ConnectionString no inicializada para DbJsonConfigurationSource (Environment='{_source?.Environment}').";
                    LogHelper.Log(msg, "ConfigJson");
                    throw new InvalidOperationException(msg);
                }

                string json;
                string sql = "SELECT Settings FROM Provex.dbo.ConfigBackend WHERE Environment = @env";
               // LogHelper.Log($"Carga de configuracion de ambiente : {_source.Environment}", "ConfigJson");
                using (var conn = new SqlConnection(_source.ConnectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@env", _source.Environment);
                    conn.Open();

                    json = cmd.ExecuteScalar() as string
                           ?? throw new InvalidOperationException($"No se encontró configuración JSON para el entorno '{_source.Environment}' en la tabla ConfigBackend.");
                }

                using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                Recurse("", doc.RootElement);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error configuracion de ambiente : {ex.Message}", "ConfigJson");
                // Preservar la excepción original como InnerException para diagnóstico
                throw new InvalidOperationException($"Error configuracion de ambiente : {ex.Message}", ex);
            }
           
        }

        private void Recurse(string prefix, JsonElement node)
        {
            try
            {
                switch (node.ValueKind)
                {
                    case JsonValueKind.Object:
                        foreach (var prop in node.EnumerateObject())
                            Recurse(prefix + prop.Name + ":", prop.Value);
                        break;

                    case JsonValueKind.Array:
                        int i = 0;
                        foreach (var item in node.EnumerateArray())
                        {
                            Recurse($"{prefix}{i}:", item);
                            i++;
                        }
                        break;

                    default:
                        // JsonValueKind.String, Number, True, False, Null
                        Data[prefix.TrimEnd(':')] = node.ToString();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error configuracion de ambiente : {ex.Message}", "ConfigJson");
                throw new InvalidOperationException($"Error configuracion de ambiente : {ex.Message}", ex);
            }           
        }
    }
}
