namespace ProvexBackendAPI.Dto
{
    public class SpResponse
    {
        public bool Ok { get; set; }
        public string? Mensaje { get; set; }
        public int Filas { get; set; } //Count filas afectadas
        public int? Id { get; set; } // Insert: scope_identity(); Update: null 
    }
}
