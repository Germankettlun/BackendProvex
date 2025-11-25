using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexBackendAPI.Data.Models
{

    [Table("Temporadas")]
    public class Temporada
    {
        [Key]
        [Column("COD_TEM")]
        public string codTem { get; set; } = default!;

        [Column("COD_EMP")]
        public string codEmp { get; set; } = default!;
        [Column("DESCRIPCION")]
        public string descripcion { get; set; } = default!;
        [Column("FECHA_INI")]
        public DateTime fechaIni { get; set; }
        [Column("ZON")]
        public string zon { get; set; } = default!;
        [Column("ORDEN")]
        public int orden { get; set; }
        [Column("VIGENTE")]
        public string? vigente { get; set; }

        public ICollection<Semana> semanas { get; set; } = new List<Semana>();
    }
}
