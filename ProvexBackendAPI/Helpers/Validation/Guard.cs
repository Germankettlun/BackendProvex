using System.ComponentModel.DataAnnotations;

namespace ProvexBackendAPI.Helpers.Validation
{
    public class Guard
    {
        public static string RequireAndUpper(string? value, string fieldName)
        {
            var v = value?.Trim();
            if (string.IsNullOrEmpty(v))
                throw new ValidationException($"{fieldName} es requerido.");
            return v.ToUpperInvariant();
        }

        public static T Require<T>(string paramName, T? value)
        {
            if (value is null)
                throw new ArgumentNullException(paramName, $"{paramName} es requerido.");

            if (value is string s)
            {
                var trimmed = s.Trim();
                if (trimmed.Length == 0)
                    throw new ArgumentException($"{paramName} es requerido.", paramName);
                // devolvemos el string ya normalizado
                return (T)(object)trimmed;
            }

            return value;
        }

        public static string? TrimToNull(string? value)
       => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public static string EnsureAllowedCombo(string nombreComboUpper)
        {
            // Lista blanca de combos válidos (ajústala si agregas más)
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PRODUCTOR", "GRUPO_PRODUCTOR", "ESPECIE", "VARIEDAD",
            "TEMPORADA", "FRIGORIFICO", "PACKING"
        };
            if (!allowed.Contains(nombreComboUpper))
                throw new ArgumentException($"NombreCombo no soportado: {nombreComboUpper}", nameof(nombreComboUpper));
            return nombreComboUpper; // ya viene upper
        }
    }
}
