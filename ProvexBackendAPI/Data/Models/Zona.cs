using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexBackendAPI.Data.Models
{
    [Table("Zona", Schema = "Estimaciones")]
    public class Zona
    {
        [Key]
        public int idZona { get; set; }
        public string idEmpresa { get; set; }
        public string nombre { get; set; }
    }
}
