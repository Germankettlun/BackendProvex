using System.ComponentModel.DataAnnotations;

namespace ProvexBackendAPI.Dto
{
    public class ComboItemDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public sealed class ComboRequest
    {
        [Required]
        public required string NombreCombo { get; set; }          // PRODUCTOR | GRUPO_PRODUCTOR | ESPECIE | VARIEDAD | TEMPORADA | FRIGORIFICO | PACKING
        
        [Required]
        public required string CodigoEmpresa { get; set; }        // PRX, etc.

        // Filtros opcionales (aplican solo a SDT_View_Productores)
        public string? CodigoEspecie { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? CodigoProductor { get; set; }
        public string? CodigoVariedad { get; set; }
        public string? CodigoTemporada { get; set; }
    }
}
