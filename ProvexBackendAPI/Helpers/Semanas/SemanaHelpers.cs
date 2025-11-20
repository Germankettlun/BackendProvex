namespace ProvexBackendAPI.Helpers.Semanas
{
    internal static class SemanaHelpers
    {
        private static readonly string[] _diasEs = { "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SABADO", "DOMINGO" };
        public static IReadOnlyList<string> DiasEs => _diasEs;

        public static int MapDayOfWeekToIndex(DayOfWeek dow) => ((int)dow + 6) % 7;

        public static string ToWeek2(string? s)
        {
            s = (s ?? "").Trim();
            return s.Length == 1 ? "0" + s : s;
        }

        public static int MapNombreADiaIndex(string? nombreDia)
        {
            if (string.IsNullOrWhiteSpace(nombreDia)) return -1;

            var raw = StripDiacritics(nombreDia.Trim().ToUpperInvariant());
            string canon = raw switch
            {
                var x when x.StartsWith("LUN") => "LUNES",
                var x when x.StartsWith("MAR") => "MARTES",
                var x when x.StartsWith("MIE") => "MIERCOLES",
                var x when x.StartsWith("JUE") => "JUEVES",
                var x when x.StartsWith("VIE") => "VIERNES",
                var x when x.StartsWith("SAB") => "SABADO",
                var x when x.StartsWith("DOM") => "DOMINGO",
                _ => raw
            };
            for (int i = 0; i < _diasEs.Length; i++) if (_diasEs[i] == canon) return i;
            return -1;
        }

        public static string StripDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";
            var norm = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(norm.Length);
            foreach (var ch in norm)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
