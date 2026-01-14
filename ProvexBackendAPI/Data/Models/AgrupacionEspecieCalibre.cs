using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexBackendAPI.Data.Models
{
    public class AgrupacionEspecieCalibre
    {
        public int idAgrupacionEspcieCalibre { get; set; }
        public string idTemporada { get; set; }
        public string idEmpresa { get; set; }
        public string idEspecie { get; set; }
        public string descripcion { get; set; }
        public DateTime fecha { get; set; }
    }
}
