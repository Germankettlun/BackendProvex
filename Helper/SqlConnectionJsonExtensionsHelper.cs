// SqlConnectionJsonExtensionsHelper.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ProvexApi.Helper
{
    public static class SqlConnectionJsonExtensionsHelper
    {
        /// <summary>
        /// Ejecuta un SELECT (o stored procedure) parametrizado y devuelve el resultado como JSON.
        /// </summary>
        /// <param name="connection">La conexión abierta o por abrir.</param>
        /// <param name="sql">
        /// Si <paramref name="commandType"/> = Text: la consulta SQL (por ejemplo "SELECT ...").
        /// Si <paramref name="commandType"/> = StoredProcedure: el nombre del SP (sin EXEC).
        /// </param>
        /// <param name="parameters">Parámetros SqlParameter (opcional).</param>
        /// <param name="options">Opciones de serialización JSON (opcional).</param>
        /// <param name="commandTimeoutSeconds">Timeout en segundos (por defecto 120s).</param>
        /// <param name="commandType">Text o StoredProcedure (por defecto Text).</param>
        /// <param name="cancellationToken"></param>
        public static async Task<string> QueryJsonAsync(
            this SqlConnection connection,
            string sql,
            IEnumerable<SqlParameter> parameters = null,
            JsonSerializerOptions options = null,
            int commandTimeoutSeconds = 120,
            CommandType commandType = CommandType.Text,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                await using var cmd = new SqlCommand(sql, connection)
                {
                    CommandType = commandType,
                    CommandTimeout = commandTimeoutSeconds
                };

                if (parameters != null)
                {
                    // Add parameters safely even if IEnumerable is not an array
                    foreach (var p in parameters)
                    {
                        cmd.Parameters.Add(p);
                    }
                }

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

                options ??= new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var rows = new List<Dictionary<string, object>>(capacity: 100);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object>(reader.FieldCount);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var value = await reader.IsDBNullAsync(i, cancellationToken)
                            ? null
                            : reader.GetValue(i);
                        row[name] = value;
                    }
                    rows.Add(row);
                }

                return JsonSerializer.Serialize(rows, options);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error de SQL Server al ejecutar {(commandType == CommandType.StoredProcedure ? "StoredProcedure" : "consulta")} '{sql}'.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al ejecutar {(commandType == CommandType.StoredProcedure ? "StoredProcedure" : "consulta")} '{sql}'.", ex);
            }
        }

        /// <summary>
        /// Sobrecarga para facilitar llamadas con un solo parámetro y defaults en timeout/commandType.
        /// </summary>
        public static Task<string> QueryJsonAsync(
            this SqlConnection connection,
            string sql,
            SqlParameter parameter,
            int commandTimeoutSeconds = 30,
            CommandType commandType = CommandType.Text
        )
        {
            // Se delega en la sobrecarga principal que ya implementa manejo de excepciones
            return connection.QueryJsonAsync(
                sql: sql,
                parameters: new[] { parameter },
                options: null,
                commandTimeoutSeconds: commandTimeoutSeconds,
                commandType: commandType,
                cancellationToken: default
            );
        }
    }
}
