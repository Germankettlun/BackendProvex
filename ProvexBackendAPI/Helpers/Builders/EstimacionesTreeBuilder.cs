using ProvexBackendAPI.Helpers.Semanas;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Semanas.SemanasDto;

namespace ProvexBackendAPI.Helpers.Builders
{
    internal static class EstimacionesTreeBuilder
    {
        public static EstructuraDistribucionDto BuildTree(
        List<RowFlat> rows,
        EstimacionBisemanalQueryDto req,
        IReadOnlyList<SemanaVigenteRow> semanasProvider)
        {
            var root = new EstructuraDistribucionDto
            {
                PesoBaseEspecie = rows.FirstOrDefault()?.PesoBaseEspecie,
                Especie = rows.FirstOrDefault()?.Especie,
                Items = new List<ItemNode>()
            };

            var itemGroups = rows.GroupBy(r => new
            {
                HasEst = r.Est_ID.HasValue,
                EstId = r.Est_ID ?? -1,
                IdProductor = r.Est_ID.HasValue ? null : r.IdProductor,
                Especie = r.Est_ID.HasValue ? null : r.Especie,
                Variedad = r.Est_ID.HasValue ? null : r.Variedad
            });

            foreach (var g in itemGroups)
            {
                var any = g.First();

                var item = new ItemNode
                {
                    Id_Productor = any.IdProductor,
                    Productor = any.Productor,
                    Variedad = any.Variedad,
                    Agronomo = any.Agronomo,
                    DistribucionCalibre = any.DistribucionCalibre,
                    DistribucionCategoria = any.DistribucionCategoria,
                    PorcentajeExportacion = any.PorcentajeExportacion,
                    EnvaseCosechero = new EnvaseCosecheroNode
                    {
                        Id = any.EnvaseId,
                        Nombre = any.EnvaseNombre,
                        Kilo = any.EnvaseKilo
                    }
                };

                var est = new EstimacionNode
                {
                    ID = any.Est_ID,
                    Contratado = any.Est_Contratado,
                    FCosecha = any.Est_FCosecha,
                    Semanas = new SemanasNode
                    {
                        Anterior = new SemanaValorNode { Estimado = any.Ant_Estimado, Producido = any.Ant_Producido },
                        Siguiente = new SemanaValorNode { Estimado = any.Sig_Estimado, Producido = any.Sig_Producido },
                        Bisemanal = new List<BisemanalNode>()
                    }
                };

                int n = req.WeeksPerPage <= 0 ? 2 : req.WeeksPerPage;
                var expectedRows = PickWeeks(semanasProvider, req.AnioBase, req.SemanaBase, g, n);

                var byKey = expectedRows.ToDictionary(
                    s => $"{s.AnioBase:D4}-{SemanaHelpers.ToWeek2(s.SemanaBase)}",
                    BuildEmptyFromSemanaRow
                );

                var bisGroups = g.Where(r => r.Bis_ID.HasValue
                                          || (r.Bis_AnioBase.HasValue && !string.IsNullOrWhiteSpace(r.Bis_SemanaBase)))
                                 .GroupBy(r => new
                                 {
                                     r.Bis_AnioBase,
                                     Semana = SemanaHelpers.ToWeek2(r.Bis_SemanaBase!)
                                 });

                foreach (var bg in bisGroups)
                {
                    if (!bg.Key.Bis_AnioBase.HasValue) continue;
                    var key = $"{bg.Key.Bis_AnioBase.Value:D4}-{bg.Key.Semana}";
                    if (!byKey.TryGetValue(key, out var bis)) continue;

                    bis.AnioBase = bg.Key.Bis_AnioBase;
                    bis.SemanaBase = bg.Key.Semana;

                    foreach (var d in bg)
                    {
                        if (bis.Dias is null || bis.Dias.Count != 7) continue;

                        int idx = -1;
                        if (d.Dia_Fecha.HasValue)
                        {
                            idx = bis.Dias.FindIndex(x => x.FechaDia.HasValue &&
                                                          x.FechaDia.Value.Date == d.Dia_Fecha.Value.Date);
                            if (idx < 0) idx = SemanaHelpers.MapDayOfWeekToIndex(d.Dia_Fecha.Value.DayOfWeek);
                        }

                        if (idx < 0 && !string.IsNullOrWhiteSpace(d.Dia_Nombre))
                            idx = SemanaHelpers.MapNombreADiaIndex(d.Dia_Nombre);

                        if (idx < 0 && d.Dia_Fecha.HasValue)
                            idx = bis.Dias.FindIndex(x => x.FechaDia.HasValue &&
                                                          x.FechaDia.Value.Date == d.Dia_Fecha.Value.Date);

                        if (idx < 0 || idx >= bis.Dias.Count) continue;

                        var dia = bis.Dias[idx];
                        dia.IdBisemanal = d.Bis_ID ?? dia.IdBisemanal;
                        dia.Estimado = (dia.Estimado ?? 0) + (d.Dia_Estimado ?? 0);
                        dia.Producido = (dia.Producido ?? 0) + (d.Dia_Producido ?? 0);
                        if (d.Dia_Fecha.HasValue) dia.FechaDia = d.Dia_Fecha;
                        if (!string.IsNullOrWhiteSpace(d.Dia_Nombre)) dia.NombreDia = d.Dia_Nombre;
                        dia.DistribucionFrio = d.Dia_DistribucionFrio ?? dia.DistribucionFrio;
                        dia.DistribucionPacking = d.Dia_DistribucionPacking ?? dia.DistribucionPacking;
                    }
                }

                est.Semanas!.Bisemanal = expectedRows
                    .Select(s => byKey[$"{s.AnioBase:D4}-{SemanaHelpers.ToWeek2(s.SemanaBase)}"])
                    .ToList();

                item.Estimacion = est;
                root.Items!.Add(item);
            }

            return root;
        }

        private static BisemanalNode BuildEmptyFromSemanaRow(SemanaVigenteRow m)
        {
            var monday = m.Inicio.Date;

            var dias = new List<DiaValorNode>(7);
            for (int i = 0; i < 7; i++)
            {
                dias.Add(new DiaValorNode
                {
                    IdBisemanal = null,
                    NombreDia = SemanaHelpers.DiasEs[i],
                    FechaDia = monday.AddDays(i),
                    Estimado = null,
                    Producido = null,
                    DistribucionFrio = null,
                    DistribucionPacking = null
                });
            }

            return new BisemanalNode
            {
                AnioBase = m.AnioBase,
                SemanaBase = SemanaHelpers.ToWeek2(m.SemanaBase),
                Dias = dias
            };
        }

        public static List<SemanaVigenteRow> PickWeeks(
            IEnumerable<SemanaVigenteRow> all,
            int? reqAnioBase, string? reqSemanaBase,
            IEnumerable<RowFlat> grupoFilas, int weeksPerPage)
        {
            var ordered = all.OrderBy(x => x.AnioBase)
                             .ThenBy(x => int.Parse(SemanaHelpers.ToWeek2(x.SemanaBase)))
                             .ToList();

            if (reqAnioBase.HasValue && !string.IsNullOrWhiteSpace(reqSemanaBase))
            {
                var ww = SemanaHelpers.ToWeek2(reqSemanaBase);
                var idx = ordered.FindIndex(s => s.AnioBase == reqAnioBase.Value && SemanaHelpers.ToWeek2(s.SemanaBase) == ww);
                if (idx >= 0) return ordered.Skip(idx).Take(weeksPerPage).ToList();
            }

            var cand = grupoFilas
                .Where(r => r.Bis_AnioBase.HasValue && !string.IsNullOrWhiteSpace(r.Bis_SemanaBase))
                .Select(r => new { Anio = r.Bis_AnioBase!.Value, Semana = SemanaHelpers.ToWeek2(r.Bis_SemanaBase!) })
                .OrderBy(x => x.Anio).ThenBy(x => x.Semana)
                .FirstOrDefault();

            if (cand is not null)
            {
                var idx2 = ordered.FindIndex(s => s.AnioBase == cand.Anio && SemanaHelpers.ToWeek2(s.SemanaBase) == cand.Semana);
                if (idx2 >= 0) return ordered.Skip(idx2).Take(weeksPerPage).ToList();
            }

            var today = DateTime.Today;
            var vigente = ordered.FirstOrDefault(s => s.Inicio.Date <= today && today <= s.Termino.Date);
            if (vigente is not null)
            {
                var idx3 = ordered.IndexOf(vigente);
                return ordered.Skip(idx3).Take(weeksPerPage).ToList();
            }

            return ordered.Take(weeksPerPage).ToList();
        }
    }
}
