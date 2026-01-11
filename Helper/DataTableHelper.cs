using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace ProvexApi.Helper
{
    public static class DataTableHelper
    {
        /// <summary>
        /// Convierte una lista de T en DataTable usando reflexión.
        /// Excluye opcionalmente algunas propiedades.
        /// </summary>
        public static DataTable ToDataTable<T>(IEnumerable<T> items, HashSet<string> excludedProperties = null)
        {
            var dataTable = new DataTable();
            var props = typeof(T).GetProperties();

            // 1) Crear columnas
            foreach (var prop in props)
            {
                if (excludedProperties != null && excludedProperties.Contains(prop.Name))
                    continue;

                Type colType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, colType);
            }

            // 2) Llenar filas
            foreach (var item in items)
            {
                var row = dataTable.NewRow();

                foreach (var prop in props)
                {
                    if (excludedProperties != null && excludedProperties.Contains(prop.Name))
                        continue;

                    if (dataTable.Columns.Contains(prop.Name))
                    {
                        var value = prop.GetValue(item, null);
                        row[prop.Name] = value ?? DBNull.Value;
                    }
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
