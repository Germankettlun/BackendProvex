using System;
using System.Data;
using System.Globalization;

namespace ProvexBackendAPI.Helpers.Shared.Extensions
{
    public static class DataReaderExtensions
    {
        // Devuelve el índice de columna o -1 si no existe
        public static int TryGetOrdinal(this IDataRecord r, string column)
        {
            try { return r.GetOrdinal(column); }
            catch (IndexOutOfRangeException) { return -1; }
        }

        // Devuelve el primer valor no nulo encontrado entre varios nombres de columna, como string
        public static string FirstExistingAsString(this IDataRecord r, params string[] candidates)
        {
            foreach (var name in candidates)
            {
                var idx = r.TryGetOrdinal(name);
                if (idx >= 0 && !r.IsDBNull(idx))
                {
                    var val = r.GetValue(idx);
                    return val?.ToString() ?? string.Empty; // o Convert.ToString(val, CultureInfo.InvariantCulture)
                }
            }
            return string.Empty;
        }

        // Lectura tipada por nombre de columna (null-safe)
        public static T? Get<T>(this IDataRecord r, string column)
        {
            var idx = r.TryGetOrdinal(column);
            if (idx < 0 || r.IsDBNull(idx)) return default;

            var target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            var raw = r.GetValue(idx);

            // Nota: usa InvariantCulture si te preocupa el formateo numérico/fecha
            return (T)Convert.ChangeType(raw, target, CultureInfo.InvariantCulture);
        }
    }
}
