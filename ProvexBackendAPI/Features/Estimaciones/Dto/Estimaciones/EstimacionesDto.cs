using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using System.Text.Json.Serialization;

namespace ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones
{
    public class EstimacionesDto
    {

        public sealed class EstimacionDistribucionDto
        {
            public string IdEstimacion { get; set; } = "";
            public Dictionary<string, SemanaDto> Semanas { get; set; } = new();
        }

        public sealed class SemanaDto
        {
            public IndiceDto Indice { get; set; } = new();
            public TotalesSemanaDto TotalesSemana { get; set; } = new();
            public HistorialDto? Historial { get; set; }
            public Dictionary<string, ProductorDto> Productores { get; set; } = new();
        }

        public sealed class IndiceDto
        {
            public int IdEstimacionBisemanal { get; set; }
            public int Anio { get; set; }
            public int Semana { get; set; }
        }

        public sealed class TotalesSemanaDto
        {
            public int CajasEstimadasSinPorc { get; set; }
            public int CajasEstimadasConPorc { get; set; }
            public int CajasDistribSinPorc { get; set; }
            public decimal CajasDistribConPorc { get; set; } // 127.5
            public int CajasP { get; set; }
        }

        public sealed class HistorialDto
        {
            public int? CajasPAnterior { get; set; }
            public int? CajasEAnteriorSinPorc { get; set; }
            public int? CajasEAnteriorConPorc { get; set; }
            public int? CajasPSiguienteSinPorc { get; set; }
            public int? CajasESiguienteSinPorc { get; set; }
            public int? CajasESiguienteConPorc { get; set; }
        }

        public sealed class ProductorDto
        {
            public string Nombre { get; set; } = "";
            public List<ItemDto> Items { get; set; } = new();
        }

        public sealed class ItemDto
        {
            public string Especie { get; set; } = "";
            public string Variedad { get; set; } = "";
            public CajasDto Cajas { get; set; } = new();
            public DistDto Dist { get; set; } = new();
        }

        public sealed class CajasDto
        {
            public int CajasEstimadasSinPorc { get; set; }
            public int CajasEstimadasConPorc { get; set; }
            public int CajasDistribSinPorc { get; set; }
            public decimal CajasDistribConPorc { get; set; }
            [JsonPropertyName("p")]
            public int P { get; set; }
        }

        public sealed class DistDto
        {
            public bool Categoria { get; set; }
            public bool Calibre { get; set; }
            public bool Packing { get; set; }
            public bool Frigorifico { get; set; }
        }

        public sealed class EstimacionBisemanalQueryDto
        {
            public string CodEmpresa { get; set; } = null!;

         
            public string IdTemporada { get; set; } = null!;         // @ID_TEMPORADA
            public string CodGrupoProductor { get; set; } = null!;    // @COD_GRUPO_PRODUCTOR
            public string IdEspecie { get; set; } = null!;           // @ID_ESPECIE

            // Opcionales
            public string? IdProductor { get; set; }           // @ID_PRODUCTOR
            public string? IdVariedad { get; set; }            // @ID_VARIEDAD
            public int? AnioBase { get; set; }                 // @ANIO_BASE
            public string? SemanaBase { get; set; }               // @SEMANA_BASE

            // Paginación por semanas
            public int Page { get; set; } = 1;                 // @PAGE
            public int WeeksPerPage { get; set; } = 2;         // @WEEKS_PER_PAGE
        }

        public static int? FirstNN(IEnumerable<int?> seq) => seq.FirstOrDefault(v => v.HasValue);

        public sealed class RowFlat
        {
            public string IdEstimacion { get; set; } = "";
            public int IdEstimacionBisemanal { get; set; }
            public int Anio { get; set; }
            public int SemanaNro { get; set; }

            public string IdProductor { get; set; } = "";
            public string NomProd { get; set; } = "";
            public string NomEsp { get; set; } = "";
            public string NomVar { get; set; } = "";

            public int CajasEstimadasSinPorc { get; set; }
            public int CajasEstimadasConPorc { get; set; }
            public int CajasDistribSinPorc { get; set; }
            public decimal CajasDistribConPorc { get; set; }
            public int CajasP { get; set; }

            public bool DistCat { get; set; }
            public bool DistCal { get; set; }
            public bool DistPack { get; set; }
            public bool DistFri { get; set; }

            public int? CajasPAnterior { get; set; }
            public int? CajasEAnteriorSinPorc { get; set; }
            public int? CajasEAnteriorConPorc { get; set; }
            public int? CajasPSiguienteSinPorc { get; set; }
            public int? CajasESiguienteSinPorc { get; set; }
            public int? CajasESiguienteConPorc { get; set; }
        }
    }
    }

