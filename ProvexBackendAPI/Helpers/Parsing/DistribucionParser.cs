using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using static ProvexBackendAPI.Dto.EstimacionesDto;

namespace ProvexBackendAPI.Helpers.Parsing
{
    internal static class DistribucionParser
    {
        public static List<Semana_DistribucionPackingPorDia> BuildPackingPorDia(string? raw)
       => ParseDayNamePercentList(raw)
          .GroupBy(x => x.Day, StringComparer.OrdinalIgnoreCase)
          .Select(g => new Semana_DistribucionPackingPorDia
          {
              nombreDia = g.Key,
              Packings = g.Select(p => new NombrePorcentajeDto
              {
                  Nombre = p.Name,
                  Porcentaje = p.PercentText
              }).ToList()
          }).ToList();

        public static List<Semana_DistribucionFrigorificoPorDia> BuildFrigorificoPorDia(string? raw)
            => ParseDayNamePercentList(raw)
               .GroupBy(x => x.Day, StringComparer.OrdinalIgnoreCase)
               .Select(g => new Semana_DistribucionFrigorificoPorDia
               {
                   nombreDia = g.Key,
                   Frigorificos = g.Select(p => new NombrePorcentajeDto
                   {
                       Nombre = p.Name,
                       Porcentaje = p.PercentText
                   }).ToList()
               }).ToList();

        public static List<TOut> MapPairs<TOut>(string? raw, Func<string, string, TOut> factory, char[]? pairSeps = null, char[]? kvSeps = null)
            => ParseNamePercentPairs(raw, pairSeps, kvSeps).Select(p => factory(p.Name, p.PercentText)).ToList();

        public static List<(string Day, string Name, string PercentText, double? PercentValue)>
            ParseDayNamePercentList(string? raw, char[]? daySeps = null, char[]? innerPairSeps = null, char[]? innerKvSeps = null)
        {
            var result = new List<(string, string, string, double?)>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            daySeps ??= new[] { '|', ';', ',' };
            innerPairSeps ??= new[] { ',', ';' };
            innerKvSeps ??= new[] { ':', '=' };

            var dayChunks = SplitOutsideParentheses(raw!, daySeps);
            var rx = new Regex(@"^\s*([^(]+?)\s*\(\s*(.*?)\s*\)\s*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            foreach (var chunk in dayChunks)
            {
                var piece = chunk.Trim();
                if (piece.Length == 0) continue;

                var m = rx.Match(piece);
                if (!m.Success) { result.Add((piece, "", "", null)); continue; }

                var day = m.Groups[1].Value.Trim();
                var inner = m.Groups[2].Value.Trim();

                var pairs = ParseNamePercentPairs(inner, innerPairSeps, innerKvSeps);
                if (pairs.Count == 0) { result.Add((day, "", "", null)); continue; }

                result.AddRange(pairs.Select(pp => (day, pp.Name, pp.PercentText, pp.PercentValue)));
            }
            return result;
        }

        private static List<string> SplitOutsideParentheses(string text, char[] seps)
        {
            var list = new List<string>();
            int depth = 0, start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '(') depth++;
                else if (c == ')') depth = Math.Max(0, depth - 1);
                else if (depth == 0 && Array.IndexOf(seps, c) >= 0)
                {
                    var seg = text.Substring(start, i - start).Trim();
                    if (seg.Length > 0) list.Add(seg);
                    start = i + 1;
                }
            }

            var last = text.Substring(start).Trim();
            if (last.Length > 0) list.Add(last);
            return list;
        }

        private static List<(string Name, string PercentText, double? PercentValue)>
            ParseNamePercentPairs(string? inner, char[]? pairSeps, char[]? kvSeps)
        {
            var pairs = new List<(string, string, double?)>();
            if (string.IsNullOrWhiteSpace(inner)) return pairs;

            pairSeps ??= new[] { ';', ',' };
            kvSeps ??= new[] { ':', '=' };

            var chunks = inner.Split(pairSeps, StringSplitOptions.RemoveEmptyEntries);
            foreach (var chunk in chunks)
            {
                var part = chunk.Trim();
                int idx = -1;
                foreach (var sep in kvSeps) { idx = part.IndexOf(sep); if (idx >= 0) break; }

                if (idx < 0) { pairs.Add((part, "", null)); continue; }

                var name = part[..idx].Trim();
                var rawVal = part[(idx + 1)..].Trim();
                var percentText = rawVal;

                var cleaned = rawVal.Replace("%", "").Trim().Replace(',', '.');
                if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                    pairs.Add((name, percentText, val));
                else
                    pairs.Add((name, percentText, null));
            }
            return pairs;
        }
    }
}
