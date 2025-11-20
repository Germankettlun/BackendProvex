namespace ProvexBackendAPI.Dto
{
    public class IngresarEstimacionRequest
    {
        public int? idEstimacion { get; set; }

        public string idEmpresa { get; set; }
        public string idTemporada { get; set; }
        public string idEspecie { get; set; }
        public string idVariedad { get; set; }
        public string idProductor { get; set; }
        public string semanaInicio { get; set; }
        public int anioInicio { get; set; }
        public float porcExportacion { get; set; }
        public string frigorifico { get; set; }
        public string packing { get; set; }
        public int envase { get; set; }
	    public int contratado { get; set; }
    }
}
