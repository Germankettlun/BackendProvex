namespace ProvexApi.Entities.ASOEX
{
    public class Temporada
    {
        public int Id { get; set; }
        public int TempSeqNro { get; set; }
        public DateTime TempFechaDesde { get; set; }
        public DateTime TempFechaHasta { get; set; }
        public string TempEstado { get; set; }
        public string NomTemporada { get; set; }
    }
}
