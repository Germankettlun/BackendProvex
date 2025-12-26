namespace ProvexBackendAPI.Dto
{
    public class CierreVersionDto
    {
        public int IdVersion { get; set; }  
        public string IdEspecie { get; set; } = default!;

        public string Especie { get; set; } = default!; 

        public int? Version { get; set; } 
        public string Descripcion { get; set; } = default!; 

        public String Fecha { get; set; }  

        public Guid IdUsuario { get; set; } 
        public string Usuario { get; set; } = default!;   
    }

    public class IngresarCierreRequest
    {
        public string idEmpresa { get; set; } = default!;

        public string idTemporada { get; set; } = default!;

        public string idEspecie { get; set; } = default!;
        public string descripcion { get; set; } = default!;
    }
}
