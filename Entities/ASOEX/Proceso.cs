using ProvexApi.Data.DTOs.ASOEX;

namespace ProvexApi.Entities.ASOEX
{
    public class Proceso
    {
        public int Id { get; set; }
        public int IdProceso { get; set; }
        public DateTime FechaProceso { get; set; }
        public TimeSpan HoraProceso { get; set; }

        public ICollection<ProcesoViaje> Viajes { get; set; }
        public ICollection<ProcesoDetalle> Detalles { get; set; }
    }
}
