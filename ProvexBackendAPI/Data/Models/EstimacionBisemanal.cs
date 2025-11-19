using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexBackendAPI.Data.Models
{
    [Table("ESTIMACION_BISEMANAL", Schema = "Estimaciones")]
    public class EstimacionBisemanal
    {
        [Key]
        [Column("ID_ESTIMACION_BISEMANAL")]
        public int idEstimacionBisemanal { get; set; }

        [Column("ID_ESTIMACION")]
        public int idEstimacion { get; set; }

        [Column("ANIO")]
        public int anio { get; set; }

        [Column("SEMANA")]
        public string semana { get; set; }

        [Column("FECHA")]
        public DateTime fecha { get; set; }

        [Column("CAJAS")]
        public int cajas { get; set; }
    }
}
