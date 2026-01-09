using System.ComponentModel.DataAnnotations;

namespace ProvexBackendAPI.Dto
{
    public class CrearAgrupacionRequest : RequestContextDTO
    {
        public required string descripcion { get; set; }
        public required List<ComboItemDto> IdsCalibres { get; set; }
    }
}
