using System.Text.Json.Serialization;

namespace ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie
{
    public class DistribucionCategoriaEspecieDto
    {

        public class DistribucionCategoriaEspecieRequestDto
        {
            public string CodigoEmpresa { get; set; } = default!;   // @CODIGOEMPRESA
            public string CodigoEspecie { get; set; } = default!;   // @CODIGOESPECIE
            public string CodigoTemporada { get; set; } = default!; // @CODIGOTEMPORADA
            public string? IdCategoria { get; set; }                   // @ID_CATEGORIA (null = todas)
        }


    }

    public class DistribucionCalibreEspecieDto
    {
        public class DistribucionCalibreEspecieRequestDto
        {
            public string CodigoEmpresa { get; set; } = default!;   // @CODIGOEMPRESA
            public string CodigoEspecie { get; set; } = default!;   // @CODIGOESPECIE
            public string CodigoTemporada { get; set; } = default!; // @CODIGOTEMPORADA
            public string? IdCalibre { get; set; }                   // @ID_CALIBRE (null = todas)
        }

    }

    public class DistribucionCategoriaEspecieRow
    {
        public string IdEstimacion { get; set; } = string.Empty;                   // ID_ESTIMACION
        public string IdCategoria { get; set; } = string.Empty;                 // IDCATEGORIA
        public string CategoriaNombre { get; set; } = "";
        public int? PorcDefectoCategoria { get; set; }      // PORCENTAJEPORDEFECTOCATEGORIA
        public int SemanaAnio { get; set; }                     // SEMANAANO
        public string SemanaNumero { get; set; } = string.Empty;                // SEMANANUMERO
        public int? PorcentajeSemana { get; set; }          // PORCENTAJEPORSEMANA
        public bool EsSemanaActual { get; set; } //true or false SEMANAACTUAL
    }

    public class DistribucionCalibreEspecieRow
    {
        public string IdEstimacion { get; set; } = string.Empty;                   // ID_ESTIMACION
        public string IdCalibre { get; set; } = string.Empty;                 // IDCALIBRE
        public string CalibreNombre { get; set; } = "";
        public int? PorcDefectoCalibre { get; set; }      // PORCENTAJEPORDEFECTOCALIBRE
        public int SemanaAnio { get; set; }                     // SEMANAANO
        public string SemanaNumero { get; set; } = string.Empty;                // SEMANANUMERO
        public int? PorcentajeSemana { get; set; }          // PORCENTAJEPORSEMANA
        public bool EsSemanaActual { get; set; } //true or false SEMANAACTUAL
    }

    public  class DistribucionCategoriaEspecieResponseDto
    {
        [JsonPropertyName("idestimacion")]
        public string IdEstimacion { get; set; } = string.Empty;

        [JsonPropertyName("categoriaid")]
        public string CategoriaId { get; set; } = string.Empty;

        [JsonPropertyName("categorianombre")]
        public string CategoriaNombre { get; set; } = "";

        [JsonPropertyName("predeterminado")]
        public int? Predeterminado { get; set; }

        [JsonPropertyName("%semanas")]
        public List<SemanaPorcentajeDto> Semanas { get; set; } = new();
    }

    public class DistribucionCalibreEspecieResponseDto
    {
        [JsonPropertyName("idestimacion")]
        public string IdEstimacion { get; set; } = string.Empty;

        [JsonPropertyName("calibreid")]
        public string CalibreId { get; set; } = string.Empty;

        [JsonPropertyName("calibrenombre")]
        public string CalibreNombre { get; set; } = "";

        [JsonPropertyName("predeterminado")]
        public int? Predeterminado { get; set; }

        [JsonPropertyName("%semanas")]
        public List<SemanaPorcentajeDto> Semanas { get; set; } = new();
    }

    public class SemanaPorcentajeDto
    {
        [JsonPropertyName("anio")]
        public int Anio { get; set; }

        [JsonPropertyName("semana")]
        public string Semana { get; set; } = string.Empty;

        [JsonPropertyName("porcentaje")]
        public int? Porcentaje { get; set; }

        [JsonPropertyName("actual")]
        public bool EsSemanaActual { get; set; }
    }

}
