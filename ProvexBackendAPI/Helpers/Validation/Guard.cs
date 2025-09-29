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
    }
}
