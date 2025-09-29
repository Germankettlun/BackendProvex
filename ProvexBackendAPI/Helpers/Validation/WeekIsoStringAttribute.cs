using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ProvexBackendAPI.Helpers.Validation
{
    /// <summary>
    /// Valida una semana ISO en formato "01"–"53" (dos dígitos).
    /// No valida null/empty (eso lo hace [Required]).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class WeekIsoStringAttribute : ValidationAttribute, IClientModelValidator
    {
        // Regex: 01–09 | 10–49 | 50–53
        private static readonly Regex _regex = new Regex(@"^(0[1-9]|[1-4]\d|5[0-3])$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Si true, hace Trim() antes de validar.
        /// </summary>
        public bool TrimWhitespace { get; set; } = true;

        public WeekIsoStringAttribute()
        {
            // Mensaje por defecto (puedes sobreescribir via ErrorMessage)
            ErrorMessage = "Semana debe estar entre '01' y '53' (dos dígitos).";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success; // [Required] se encarga
            if (value is not string s) return new ValidationResult(ErrorMessage);

            if (TrimWhitespace) s = s.Trim();

            if (s.Length == 0) return ValidationResult.Success; // [Required] se encarga

            return _regex.IsMatch(s)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }

        /// <summary>
        /// Client-side (jQuery unobtrusive): emite data-val-regex con el mismo patrón.
        /// </summary>
        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-regex", ErrorMessage ?? "Formato de semana inválido.");
            MergeAttribute(context.Attributes, "data-val-regex-pattern", _regex.ToString());
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return false;
            attributes.Add(key, value);
            return true;
        }
    }
}
