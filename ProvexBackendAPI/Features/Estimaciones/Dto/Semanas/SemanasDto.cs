namespace ProvexBackendAPI.Features.Estimaciones.Dto.Semanas
{
    public class SemanasDto
    {

        public sealed class SemanaVigenteRow
        {
            public string? CodigoEmpresa { get; set; }
            public string? CodigoTemporada { get; set; }
            public int AnioBase { get; set; }
            public string? SemanaBase { get; set; }
            public DateTime Inicio { get; set; }
            public DateTime Termino { get; set; }
        }
    }
}
