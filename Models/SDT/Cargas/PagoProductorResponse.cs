using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class PagoProductorResponse
    {
        public List<PagoProductorItem> Data { get; set; }
    }
#nullable enable
    public class PagoProductorItem
    {
        public int Id { get; set; } // PK autoincrement
     
        public string? CodigoTemporada { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? GrupoProductor { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }
        public string? CodigoVariedad { get; set; }
        public string? Variedad { get; set; }
        public string? CodigoMercado { get; set; }
        public string? Mercado { get; set; }
        public string? CodigoRecibidor { get; set; }
        public string? Recibidor { get; set; }
        public string? CodigoCalibre { get; set; }
        public string? CodVariedadEti { get; set; }
        public string? VariedadEti { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoNetoReal { get; set; }
        public string? CodigoNave { get; set; }
        public string? Nave { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? SemanaZarpe { get; set; }
        public string? Fecha_Zarpe { get; set; }
        public string? Fecha_Arribo { get; set; }
        public string? CodigoPuerto { get; set; }
        public string? CodigoTipoNave { get; set; }
        public string? TipoNave { get; set; }
        public string? CodigoEnvop { get; set; }
        public string? DescEnvop { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cajas { get; set; }
        public string? CondicionLlegada { get; set; }
        public string? DeterminanteCondicion { get; set; }
        public string? EstadoLiquidacion { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalRetornoFOB { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FOBUnitarioBulto { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Comision { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? OtrosGastos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalRetorno { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RetornoUnitCajasBulto { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RetornoUnitCajasBase { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalFOBPercibido { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FOBUnitPercibido { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ComisionRetorno { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? OtrosCostos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalRetornoProductorLaFecha { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RetornoProductorPercibidoCajaBulto { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RetornoProductorPercibidoCajaBase { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Dif_Retorno_Estimado_Retorno_Percibido { get; set; }
        public string? CodigoCategoria { get; set; }
        public string? CodigoEtiqueta { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ComisionXGrupo { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoAbonosProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoCargoManualProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoEstandarGastos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoFobProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoFobSinProvision { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ProvisionInternaCaja { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RetornoProductorSinVD { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ProvisionInternaPorcentual { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ValorCompraFruta { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalGastoPorEstandarProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_Retorno_FOB_EF { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalFOBChile { get; set; }
        public string? ClaveEmbarque { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoDevolucionRebate { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoCtaCteProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoPagoProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ComisionPorc { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalProvisionInterna_Item { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoEnvop { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasEquivalentes { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalOtrosGastos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosReales { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NombreProductorReal { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalKilosNetos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoGrupoCalibre { get; set; }
        public string? Puerto { get; set; }
        public string? CodigoDestino { get; set; }
        public string? Destino { get; set; }
    }
}
