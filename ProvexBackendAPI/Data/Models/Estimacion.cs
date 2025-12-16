using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProvexBackendAPI.Data.Models
{
    [Table("ESTIMACION", Schema = "Estimaciones")]
    public class Estimacion
    {
        [Key]
        [Column("ID_ESTIMACION")]
        public int idEstimacion { get; set; }

        [Column("ID_EMPRESA")]
        public string idEmpresa { get; set; }

        [Column("ID_TEMPORADA")]
        public string idTemporada { get; set; }

        [Column("ID_ESPECIE")]
        public string idEspecie { get; set; }

        [Column("ID_VARIEDAD")]
        public string idVariedad { get; set; }

        [Column("ID_PRODUCTOR")]
        public string idProductor { get; set; }

        [Column("ID_ENVASE_COSECHA")]
        public int? idEnvaseCosecha { get; set; }

        [Column("ID_FRIGORIFICO")]
        public string? idFrigorifico { get; set; }

        [Column("ID_PACKING")]
        public string? idPacking { get; set; }

        [Column("ANIO_INICIO")]
        public int anioInicio { get; set; }

        [Column("SEMANA_INICIO")]
        public string semanaInicio { get; set; }

        [Column("CAJAS")]
        public int? cajasContratadas { get; set; }

        [Column("PORC_EXPORTACION")]
        public double? porcentajeExportacion { get; set; }

        [Column("KILO_ENVASE")]
        public decimal? kiloEnvase { get; set; }

        [Column("PESO_FIJO")]
        public decimal? pesoFijo { get; set; }


    }
}
