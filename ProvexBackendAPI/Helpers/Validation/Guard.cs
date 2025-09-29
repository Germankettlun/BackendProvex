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

        public static string Require(string paramName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"El {paramName} es requerido.", paramName);
            return value.Trim();
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
