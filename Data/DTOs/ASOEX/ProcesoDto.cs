namespace ProvexApi.Data.DTOs.ASOEX
{
    public class ProcesoDto
    {
        public int idproceso { get; set; }
        public string fecha_proceso { get; set; }
        public string hora_poceso { get; set; }
    }
    public class ProcesoDtoPaged : IApiPagedResponse<ProcesoDto>
    {
        public int current_page { get; set; }
        public int last_page { get; set; }
        public List<ProcesoDto> data { get; set; } = new();
    }
}
