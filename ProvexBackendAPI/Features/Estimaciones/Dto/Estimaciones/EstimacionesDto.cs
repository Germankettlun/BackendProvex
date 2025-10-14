using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using System.Text.Json.Serialization;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones
{
    public class EstimacionesDto
    {

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

        public sealed class EstructuraDistribucionDto
        {
            [JsonPropertyName("pesoBaseEspecie")]
            public double? PesoBaseEspecie { get; set; }



            [JsonPropertyName("especie")]
            public string? Especie { get; set; }

            [JsonPropertyName("items")]
            public List<ItemNode>? Items { get; set; }
        }

        public sealed class ItemNode
        {
            [JsonPropertyName("idProductor")]
            public string? Id_Productor { get; set; }
            [JsonPropertyName("productor")]
            public string? Productor { get; set; }

            [JsonPropertyName("variedad")]
            public string? Variedad { get; set; }

            [JsonPropertyName("agronomo")]
            public string? Agronomo { get; set; }

            [JsonPropertyName("distribucionCalibre")]
            public bool? DistribucionCalibre { get; set; }

            [JsonPropertyName("distribucionCategoria")]
            public bool? DistribucionCategoria { get; set; }

            [JsonPropertyName("envaseCosechero")]
            public EnvaseCosecheroNode? EnvaseCosechero { get; set; }

            [JsonPropertyName("estimacion")]
            public object? Estimacion { get; set; }
        }

        public sealed class EnvaseCosecheroNode
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("nombre")]
            public string? Nombre { get; set; }

            [JsonPropertyName("kilo")]
            public double? Kilo { get; set; }
        }

        public sealed class EstimacionNode
        {
            [JsonPropertyName("ID")]
            public int? ID { get; set; }

            [JsonPropertyName("Contratado")]
            public int? Contratado { get; set; }

            [JsonPropertyName("FCosecha")]
            public string? FCosecha { get; set; }

            [JsonPropertyName("semanas")]
            public SemanasNode? Semanas { get; set; }
        }

        public sealed class SemanasNode
        {
            [JsonPropertyName("Anterior")]
            public SemanaValorNode? Anterior { get; set; }

            [JsonPropertyName("Siguiente")]
            public SemanaValorNode? Siguiente { get; set; }

            [JsonPropertyName("Bisemanal")]
            public List<BisemanalNode>? Bisemanal { get; set; }
        }

        public sealed class SemanaValorNode
        {
            [JsonPropertyName("Estimado")]
            public decimal? Estimado { get; set; }

            [JsonPropertyName("Producido")]
            public decimal? Producido { get; set; }
        }

        public class BisemanalNode
        {
           
            public int? AnioBase { get; set; }
            public string? SemanaBase { get; set; }

            // (Opcional) Si esto queda a nivel semana, se mantiene:
            public decimal? PorcentajeExportacion { get; set; }

           

            public List<DiaValorNode>? Dias { get; set; }
        }

       

        public class EstimacionSemanalDto
        {
            public string IdEstimacion { get; set; } = string.Empty;
            public int? Contratado { get; set; }
            public string? IdEnvaseCosecha { get; set; }

            public TotalesEstimacionDto Totales { get; set; } = new();
            public List<SemanaEstimacionDto> Semanas { get; set; } = new();
        }

        public class TotalesEstimacionDto
        {
            public int? EstimadoSinPorcentaje { get; set; }   // TOTAL_E_SIN_PORC
            public int? EstimadoConPorcentaje { get; set; }   // TOTAL_E_CON_PORC
            public int? Proyectado { get; set; }              // TOTAL_P
            public int? DiferenciaEstimadoConProyectado { get; set; } // DIF_E_CON_P
        }

        public class SemanaEstimacionDto
        {
            public int? Pos { get; set; }                    // POS

            public int Anio { get; set; }
            public string? SemanaNumero { get; set; }        // SEMANA_NRO
            public int? EstimadoSinPorcentaje { get; set; } // E_SIN_PORC
            public int? EstimadoConPorcentaje { get; set; } // E_CON_PORC
            public int? PorcentajeSemana { get; set; }      // P_SEMANA
        }

        //Helper Repository

        public sealed class RowFlat
        {
            // Raíz
            public double? PesoBaseEspecie { get; set; }
            public string? Especie { get; set; }

            // Item
            public string? IdProductor { get; set; }
            public string? Productor { get; set; }
            public string? Variedad { get; set; }
            public string? Agronomo { get; set; }
            public bool? DistribucionCalibre { get; set; }
            public bool? DistribucionCategoria { get; set; }

            // Envase
            public string? EnvaseId { get; set; }
            public string? EnvaseNombre { get; set; }
            public int? EnvaseKilo { get; set; }

            // Estimación + semanas
            public int? Est_ID { get; set; }
            public int? Est_Contratado { get; set; }
            public string? Est_FCosecha { get; set; }

            public decimal? Ant_Estimado { get; set; }
            public decimal? Ant_Producido { get; set; }
            public decimal? Sig_Estimado { get; set; }
            public decimal? Sig_Producido { get; set; }

            // Bisemanal
            
            public int? Bis_AnioBase { get; set; }
            public string? Bis_SemanaBase { get; set; }
           
            public int? Bis_PorcExport { get; set; }

            // Días
            public int? Bis_ID { get; set; }
            public string? Dia_Nombre { get; set; }
            public DateTime? Dia_Fecha { get; set; } // yyyy-MM-dd
            public decimal? Dia_Estimado { get; set; }
            public decimal? Dia_Producido { get; set; }

            public bool? Dia_DistribucionFrio { get; set; }
            public bool? Dia_DistribucionPacking { get; set; }
        }

        public class DiaValorNode
        {
            public int? IdBisemanal { get; set; } 
            public DateTimeOffset? FechaDia { get; set; }
            public string? NombreDia { get; set; }
            public decimal? Estimado { get; set; }
            public decimal? Producido { get; set; }
            public bool? DistribucionFrio { get; set; }
            public bool? DistribucionPacking { get; set; }
        }


    }
}

