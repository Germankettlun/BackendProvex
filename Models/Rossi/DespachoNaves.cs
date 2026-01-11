namespace ProvexApi.Models.Rossi
{
    // Sin clave porque es una vista
    [Microsoft.EntityFrameworkCore.Keyless]
    public class DespachoNaves
    {
        public string? CodigoEmpresa { get; set; }
        public string? CodigoTemporada { get; set; }
        public string CodTipoNave { get; set; } = null!;
        public string CodNave { get; set; } = null!;
        public string NomNave { get; set; } = null!;
        public DateTime FechaZarpe { get; set; }

    }
}
