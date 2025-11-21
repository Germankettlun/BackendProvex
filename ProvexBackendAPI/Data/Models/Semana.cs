using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexBackendAPI.Data.Models
{
    [Table("Semanas")]
    public class Semana
    {
        [Column("COD_TEM")]
        public string codTem { get; set; } = default!;
        [Column("COD_EMP")]
        public string codEmp { get; set; } = default!;
        [Column("SEMANA")]
        public string semana { get; set; } = default!;

        [Column("ANO")]
        public int anio { get; set; }
        [Column("INICIO")]
        public DateTime inicio { get; set; }
        [Column("TERMINO")]
        public DateTime termino { get; set; }           

        public Temporada temporada { get; set; } = default!;
    }
}
