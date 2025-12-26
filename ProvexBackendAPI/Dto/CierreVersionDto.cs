namespace ProvexBackendAPI.Dto
{
    public class CierreVersionDto
    {
        public int IdVersion { get; set; }  
        public string IdEspecie { get; set; } = default!;

        public string Especie { get; set; } = default!; 

        public string Version { get; set; } = default!; 
        public string Descripcion { get; set; } = default!; 

        public DateTime Fecha { get; set; }  

        public Guid IdUsuario { get; set; } 
        public string Usuario { get; set; } = default!;   
    }
}
