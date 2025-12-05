
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static ProvexBackendAPI.Dto.EstimacionesDto;

namespace ProvexBackendAPI.Dto
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

            [JsonPropertyName("codigoEspecie")]
            public string? CodigoEspecie { get; set; }

            [JsonPropertyName("especie")]
            public string? Especie { get; set; }

            [JsonPropertyName("unidadMedidaEspecie")]
            public int? UnidadMedidaEspecie { get; set; }

            [JsonPropertyName("items")]
            public List<ItemNode>? Items { get; set; }
        }

        public sealed class ItemNode
        {
            [JsonPropertyName("idProductor")]
            public string? Id_Productor { get; set; }
            [JsonPropertyName("productor")]
            public string? Productor { get; set; }

            [JsonPropertyName("codigoVariedad")]
            public string? CodigoVariedad { get; set; }

            [JsonPropertyName("variedad")]
            public string? Variedad { get; set; }

            [JsonPropertyName("agronomo")]
            public string? Agronomo { get; set; }

            [JsonPropertyName("distribucionCalibre")]
            public bool? DistribucionCalibre { get; set; }

            [JsonPropertyName("distribucionCategoria")]
            public bool? DistribucionCategoria { get; set; }

            [JsonPropertyName("porcentajeexp")]
            public int? PorcentajeExportacion { get; set; }

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

            //// (Opcional) Si esto queda a nivel semana, se mantiene:
            //public decimal? PorcentajeExportacion { get; set; }



            public List<DiaValorNode>? Dias { get; set; }
        }



        public class ResumenSemanalEstimacionDto
        {
            public string IdEstimacion { get; set; } = string.Empty;
            public int? Contratado { get; set; }

            public double? CajasPesoFijo { get; set; }
            public int? KilosBaseEspecie { get; set; }
            public EnvaseCosecheroNode? EnvaseCosechero { get; set; }

            public TotalesEstimacionDto Totales { get; set; } = new();
            public List<SemanaEstimacionDto> Semanas { get; set; } = new();
        }

        public class TotalesEstimacionDto
        {
            public int? EstimadoSinPorcentaje { get; set; }   // TOTAL_E_SIN_PORC
            public int? EstimadoConPorcentaje { get; set; }   // TOTAL_E_CON_PORC
            public int? Producido { get; set; }              // TOTAL_P
            public int? DiferenciaEstimadoConProducido { get; set; } // DIF_E_CON_P
        }

        public class SemanaEstimacionDto
        {
           // public int? Pos { get; set; }                 

            public int Anio { get; set; }
            public string? SemanaNumero { get; set; }      
            public int? EstimadoSinPorcentaje { get; set; } 
            public int? EstimadoConPorcentaje { get; set; } 
            public int? Producido { get; set; }     

          //  public List<DistribucionCategoriaPorSemanaNode> DistribucionCategoria { get; set; }
          //  public List<DistribucionCalibrePorSemanaNode> DistribucionCalibre { get; set; }
          //  public List<Semana_DistribucionPackingPorDia> PackingPorDia { get; set; }
          //  public List<Semana_DistribucionFrigorificoPorDia> FrigorificoPorDia { get; set; }

        }

        public class DetalleDistribucionesSemanalDto
        {
            public int? Anio { get; set; }        
            public string? Semana { get; set; }       

            public List<DistribucionCategoriaPorSemanaNode> DistribucionCategoria { get; set; }
                = new();

            public List<DistribucionCalibrePorSemanaNode> DistribucionCalibre { get; set; }
                = new();

            public List<Semana_DistribucionPackingPorDia> PackingPorDia { get; set; }
                = new();

            public List<Semana_DistribucionFrigorificoPorDia> FrigorificoPorDia { get; set; }
                = new();
        }

        public class DetalleDistribucionesEstimacionDto
        {
            public int IdEstimacion { get; set; }
            public List<DetalleDistribucionesSemanalDto> Semanas { get; set; } = new();
        }

        //Helper Repository

        public sealed class RowFlat
        {
            // Raíz
            public double? PesoBaseEspecie { get; set; }

            public string? CodigoEspecie { get; set; }
            public string? Especie { get; set; }

            public int? UnidadMedidaEspecie { get; set; }


            // Item
            public string? IdProductor { get; set; }
            public string? Productor { get; set; }

            public string? CodigoVariedad { get; set; }
            public string? Variedad { get; set; }
            public string? Agronomo { get; set; }
            public bool? DistribucionCalibre { get; set; }
            public bool? DistribucionCategoria { get; set; }

            public int? PorcentajeExportacion { get; set; }

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

        public sealed class DistribucionCategoriaPorSemanaNode
        {

            public string? nombreCategoria { get; set; }


            public string? Porcentaje { get; set; }

            public int? Cajas { get; set; }

            public bool? EsPorcentajeDefault { get; set; }


        }

        public sealed class DistribucionCalibrePorSemanaNode
        {

            public string? nombreCalibre { get; set; }


            public string? Porcentaje { get; set; }

            public int? Cajas { get; set; }

            public bool? EsPorcentajeDefault { get; set; }


        }

        public sealed class Semana_DistribucionPackingPorDia
        {
            public string? nombreDia { get; set; }

            public DateTime fechaDia { get; set; }
            public List<NombrePorcentajeDto> Packings { get; set; } = new();
        }

        public sealed class Semana_DistribucionFrigorificoPorDia
        {
            public string? nombreDia { get; set; }

            public DateTime fechaDia { get; set; }
            public List<NombrePorcentajeDto> Frigorificos { get; set; } = new();
        }

        public sealed class NombrePorcentajeDto
        {
            public string? Nombre { get; set; }
            public string? Porcentaje { get; set; }
            public int? Cajas { get; set; }
            public Boolean? EsPorcentajeDefault { get; set; }

        }

        //DTO PARA POST

        public sealed class UpdateEstimacionBisemanalRequest
        {
            public required int IdEstimacion { get; set; }
            public required decimal ValorNuevo { get; set; } 
            public required DiaRequest Dia { get; set; }
        }

        public sealed class DiaRequest
        {
            public required string NombreDia { get; set; }    // informativo
            public required DateTime FechaDia { get; set; }   // ← mapea a @FECHA (insert) o @FECHA_ACTUAL (update)
            public decimal? Estimado { get; set; }
            public decimal? Producido { get; set; }
            public bool? DistribucionFrio { get; set; }
            public bool? DistribucionPacking { get; set; }
        }

        public sealed class SpResultEstimacionBisemanalDto
        {
          
            public int? IdEstimacion { get; set; }
            public string? Message { get; set; }

        }

        public class ResumenSemanalRowDto
        {
            public int? Contratado { get; set; }

            public double? CajasPesoFijo { get; set; }
            public int? KilosBaseEspecie { get; set; }

            public string? IdEnvaseCosecha { get; set; }
            public string? NomEnvaseCosecha { get; set; }
            public double? KilosEnvase { get; set; }

            public int? Total_E_Sin_Porc { get; set; }
            public int? Total_E_Con_Porc { get; set; }
            public int? Total_P { get; set; }
            public int? Dif_E_Con_P { get; set; }
            public int? Anio { get; set; }
            public string? Semana_Nro { get; set; }

            public int? E_Sin_Porc { get; set; }
            public int? E_Con_Porc { get; set; }
            public int? P_Semana { get; set; }

        }

    }  
}

