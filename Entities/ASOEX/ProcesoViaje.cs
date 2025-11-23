using ProvexApi.Data.DTOs.ASOEX;

namespace ProvexApi.Entities.ASOEX
{
    public class ProcesoViaje
    {
        public int Id { get; set; }
        public int IdProceso { get; set; }
        public int NroOtro { get; set; }
        public int TempSeqNro { get; set; }
        public string CodPuertoEmbarque { get; set; }
        public int CorrelativoViaje { get; set; }
        public string CodPuertoArribo { get; set; }
        public DateTime FechaZarpe { get; set; }
        public DateTime? FechaArribo { get; set; }
        public string CodNave { get; set; }
        public string Status { get; set; }

        // Navegación
        public Proceso Proceso { get; set; }
    }
}
