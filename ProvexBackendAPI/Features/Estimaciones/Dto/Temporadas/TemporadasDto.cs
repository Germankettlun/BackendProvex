namespace ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas
{
    public class TemporadaDto
    {
        public string CodTem { get; set; } = default!;
        public string Descripcion { get; set; } = default!;
        public DateTime FechaIni { get; set; }
        public int Orden { get; set; }
        public int Vigente { get; set; } // 1 = vigente, 0 = no vigente (ajusta si usas BIT)
    }

    public class TemporadaDetalleDto
    {
        public string CodTem { get; set; } = default!;
        public string Descripcion { get; set; } = default!;
        public DateTime FechaIni { get; set; }
        public int Orden { get; set; }
        public int Vigente { get; set; }

        public List<SemanaDto> Semanas { get; set; } = new();
    }


    public class SemanaDto
    {
        public string CodTem { get; set; } = default!;
        public string TemporadaDesc { get; set; } = default!;
        public DateTime TempInicio { get; set; }
        public int TempOrden { get; set; }
        public int TempVigente { get; set; }

        public int Semana { get; set; }
        public int Ano { get; set; }
        public DateTime SemanaInicio { get; set; }
        public DateTime SemanaTermino { get; set; }
    }

    public class SemanasQuery
    {
        public string CodTem { get; set; } = default!;
        public string CodEmp { get; set; } = default!;
        public int? Vigente { get; set; } // null = todas, 1 = solo temporadas vigentes
    }


}
