using ProvexBackendAPI.Helpers.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProvexBackendAPI.Features.Estimaciones.Dto.DistribucionCategoriaEspecie
{
    public class DistribucionesDto
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
        public string IdEstimacion { get; set; } = string.Empty;
        public string CodEspecie { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string IdCategoria { get; set; } = string.Empty;                
        public string CategoriaNombre { get; set; } = "";

        public int SemanaAnio { get; set; }
        public string SemanaNumero { get; set; } = string.Empty;

        public int? IdDistribucionDefecto { get; set; }
        public int? PorcDefectoCategoria { get; set; }
        public int? IdDistribucionPorSemana { get; set; }
        public int? PorcentajeSemana { get; set; }          
        public bool EsSemanaActual { get; set; } 
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

        [JsonPropertyName("codEspecie")]
        public string CodigoEspecie { get; set; } = string.Empty;

        [JsonPropertyName("Especie")]
        public string Especie { get; set; } = string.Empty;

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

        [JsonPropertyName("porcentajePorSemana")]
        public int? PorcentajePorSemana { get; set; }

        [JsonPropertyName("actual")]
        public bool EsSemanaActual { get; set; }
    }

    public class DistribucionPackingQueryDto
    {
        [Required]
        public string CodigoEmpresa { get; set; } = default!;     // @CODIGOEMPRESA

        [Required]
        public string CodigoEspecie { get; set; } = default!;     // @CODIGOESPECIE

        [Required]
        public string CodigoTemporada { get; set; } = default!;   // @CODIGOTEMPORADA

        [Range(2000, 2100)]
        public int? Anio { get; set; }                              // @ANIO

        
        [WeekIsoString(ErrorMessage = "Semana inválida. Usa '01' a '53'.")]
        public string? Semana { get; set; } = default!;          // @SEMANA (varchar en BD)

        public DateTime? FechaDia { get; set; }

    }

    //FRIGORIFICO AGRUPADO
    public class FrigorificoItemDto
    {
        public string? IdDistribucionFrigorifico { get; set; } // puede venir vacío => null
        public string IdFrigorifico { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;     // from FrigorificoNombre
        public int Porcentaje { get; set; }                    // 0..100 (numérico)
    }

    public class DistribucionFrigorificoDiaDto
    {
        public string IdEstimacion { get; set; } = string.Empty;
        public string IdEspecie { get; set; } = string.Empty;
        public string IdEstimacionBisemanal { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Semana { get; set; } = string.Empty;
        public DateTime? FechaDia { get; set; }
        public string? DiaNombre { get; set; }
        public int TotalCajasBisemanal { get; set; }
        public bool SumaPorcentajeEs100 { get; set; }

        public List<FrigorificoItemDto> FrigorificoPorDia { get; set; } = new();
    }

    //PACKING AGRUPADO
    public class PackingItemDto
    {
        public string? IdDistribucionPacking { get; set; } // puede venir vacío => null
        public string IdPacking { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;     // from FrigorificoNombre
        public int Porcentaje { get; set; }                    // 0..100 (numérico)
    }

    public class DistribucionPackingDiaDto
    {
        public string IdEstimacion { get; set; } = string.Empty;
        public string IdEspecie { get; set; } = string.Empty;
        public string IdEstimacionBisemanal { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string Semana { get; set; } = string.Empty;
        public DateTime? FechaDia { get; set; }
        public string? DiaNombre { get; set; }
        public int TotalCajasBisemanal { get; set; }
        public bool SumaPorcentajeEs100 { get; set; }

        public List<PackingItemDto> PackingPorDia { get; set; } = new();
    }

    //ANTIGUO

    public class DistribucionFrigorificoDto
    {
        public string IdEstimacion { get; set; } = string.Empty; 
        public string IdEspecie { get; set; } = string.Empty; 
       public string IdEstimacionBisemanal { get; set; } = string.Empty; 
        public int Anio { get; set; } 
        public string Semana { get; set; } = string.Empty; 
        public DateTime? FechaDia { get; set; } 
        public String? DiaNombre { get; set; } 
        public int TotalCajasBisemanal { get; set; } 
        public string IdDistribucionFrigorifico { get; set; } = string.Empty; 
        public string IdFrigorifico { get; set; } = string.Empty; 
        public int Porcentaje { get; set; } 
        public string FrigorificoNombre { get; set; } = string.Empty;
        public bool SumaPorcentajeEs100 { get; set; } 
    }

    public class DistribucionPackingDto
    {
        public string IdEstimacion { get; set; } = string.Empty;            // IDESTIMACION
        public string IdEspecie { get; set; } = string.Empty;               // IDESPECIE
        public string IdEstimacionBisemanal { get; set; } = string.Empty;   // IDESTIMACIONBISEMANAL
        public int Anio { get; set; }                                       // BISEMANALANIO
        public string Semana { get; set; } = string.Empty;                  // BISEMANALSEMANA
        public DateTime? FechaDia { get; set; }
        public String? DiaNombre { get; set; }
        public int TotalCajasBisemanal { get; set; }                        // TOTALCAJASBISEMANAL
        public string IdDistribucionPacking { get; set; } = string.Empty;   // IDDISTRIBUCIONPACKING
        public string IdPacking { get; set; } = string.Empty;               // IDPACKING
        public int Porcentaje { get; set; }                             // PORCENTAJE
        public string PackingNombre { get; set; } = string.Empty;           // PACKING
        public bool SumaPorcentajeEs100 { get; set; }                        // SumaPorcentajeEs100
    }

}
