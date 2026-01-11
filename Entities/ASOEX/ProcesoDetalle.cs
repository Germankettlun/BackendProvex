using ProvexApi.Data.DTOs.ASOEX;

namespace ProvexApi.Entities.ASOEX
{
    public class ProcesoDetalle
    {
        public int Id { get; set; }
        public int IdProceso { get; set; }
        public int NroOtro { get; set; }
        public int TempSeqNro { get; set; }
        public string CodPuertoEmbarque { get; set; }
        public int CorrelativoViaje { get; set; }
        public string RutExportador { get; set; }
        public string CodEspecie { get; set; }
        public string CodVariedad { get; set; }
        public string CodRegionOrigen { get; set; }
        public int PsdCantidad { get; set; }
        public decimal PsdTotKgNeto { get; set; }
        public string CodPuertoDestino { get; set; }
        public string CodConsignatario { get; set; }
        public string CodCondicion { get; set; }
        public int? pagina_api { get; set; }

        // Navegación
        public Proceso Proceso { get; set; }
    }
}
