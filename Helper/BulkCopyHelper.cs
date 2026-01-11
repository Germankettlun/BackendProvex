using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ProvexApi.Helper
{
    public static class BulkCopyHelper
    {
        /// <summary>
        /// Elimina registros de <paramref name="destinationTable"/> según <paramref name="deleteCondition"/>,
        /// luego inserta la lista <paramref name="items"/> en la tabla usando SqlBulkCopy.
        /// Usa transacción. Lote de inserción = <paramref name="batchSize"/>.
        /// Retorna la cantidad de filas eliminadas al inicio.
        /// </summary>
        /// <typeparam name="T">Tipo del modelo que se convertirá a DataTable</typeparam>
        /// <param name="items">La lista de datos a insertar</param>
        /// <param name="destinationTable">Nombre de la tabla en BD, ej: \"dbo.Despachos\"</param>
        /// <param name="connectionString">Cadena de conexión a SQL Server</param>
        /// <param name="deleteCondition">Condición SQL para eliminar previo a la inserción (ej: WHERE CodigoTemporada='10107')</param>
        /// <param name="excludedProps">Propiedades del modelo a no mapear en DataTable (ej: \"Id\")</param>
        /// <param name="batchSize">Tamaño del lote en inserts masivos</param>
        /// <returns>Cantidad de filas eliminadas antes de la inserción</returns>
        public static async Task<int> BulkInsertAsync<T>(
            IEnumerable<T> items,
            string destinationTable,
            string connectionString,
            string? deleteCondition = null,
            ISet<string>? excludedProps = null,
            int batchSize = 1000)
        {
            excludedProps ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1) Obtener columnas reales del destino
                var (schema, table) = ParseTable(destinationTable); // devuelve ("dbo","API_SDT_RecepcionFrutaEmbalada")
                var destCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = new SqlCommand(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=@s AND TABLE_NAME=@t",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@s", schema);
                    cmd.Parameters.AddWithValue("@t", table);
                    using var r = await cmd.ExecuteReaderAsync();
                    while (await r.ReadAsync()) destCols.Add(r.GetString(0));
                }

                // 2) Propiedades del modelo que existen en la tabla
                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => !excludedProps.Contains(p.Name)
                                && destCols.Contains(p.Name)
                                && p.CanRead)
                    .ToArray();

                if (props.Length == 0)
                    throw new InvalidOperationException("No hay columnas comunes entre el modelo y la tabla de destino.");

                // 3) DataTable con solo columnas válidas
                var dt = new DataTable();
                foreach (var p in props)
                {
                    var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    // todo como string salvo DateTime
                    dt.Columns.Add(p.Name, t == typeof(DateTime) ? typeof(DateTime) : typeof(string));
                }

                foreach (var item in items)
                {
                    var row = dt.NewRow();
                    foreach (var p in props)
                    {
                        var val = p.GetValue(item);
                        row[p.Name] = val ?? DBNull.Value;
                    }
                    dt.Rows.Add(row);
                }

                // 4) Borrar si aplica
                if (!string.IsNullOrWhiteSpace(deleteCondition))
                {
                    using var del = new SqlCommand($"DELETE FROM [{schema}].[{table}] {deleteCondition}", conn, tx);
                    await del.ExecuteNonQueryAsync();
                }

                // 5) BulkCopy solo con columnas existentes
                using var bc = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tx)
                {
                    DestinationTableName = $"[{schema}].[{table}]",
                    BatchSize = batchSize
                };
                foreach (DataColumn c in dt.Columns)
                    bc.ColumnMappings.Add(c.ColumnName, c.ColumnName);

                await bc.WriteToServerAsync(dt);
                tx.Commit();
                return dt.Rows.Count;
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            static (string schema, string table) ParseTable(string full)
            {
                var parts = full.Trim().Trim('[', ']').Split('.');
                return parts.Length == 2 ? (parts[0].Trim('[', ']'), parts[1].Trim('[', ']'))
                                         : ("dbo", parts[0].Trim('[', ']'));
            }
        }
    }
}
