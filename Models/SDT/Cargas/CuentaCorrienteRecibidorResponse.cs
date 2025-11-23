#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ProvexApi.Converters;


namespace ProvexApi.Models.SDT.Cargas
{
    public class CuentaCorrienteRecibidorResponse
    {
        public List<CuentaCorrienteRecibidorResponseItem> Data { get; set; }
    }

    public class CuentaCorrienteRecibidorResponseItem
    {
        [Key]
        public int Id { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? CodigoGrupoRecibidor { get; set; }
        public string? GrupoRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? RutRecibidor { get; set; }
        public string? Recibidor { get; set; }
        public string? CodigoTipoNave { get; set; }
        public string? TipoNave { get; set; }
        public string? CodigoNaviera { get; set; }
        public string? Naviera { get; set; }
        public string? CodigoPuerto { get; set; }
        public string? Puerto { get; set; }
        public string? CodigoDestino { get; set; }
        public string? Destino { get; set; }
        public string? CodigoNave { get; set; }
        public string? Nave { get; set; }
        public string? CodigoMercado { get; set; }
        public string? Mercado { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cajas { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroSII { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? MonedaFact { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? MonedaAnt { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? MonedaAbono { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalGastoRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ComisionRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_FOB_Chile_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_FOB_Chile_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FOBChileAjustado { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Contenedor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroDespacho { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalVentaDestino { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalAbonosTrans { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalAbonosFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AbonosPorSaldoLiquidacionTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AbonosPorAnticiposTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AbonosPorSaldoLiquidacionFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AbonosPorAnticiposFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SaldoFinalPorCobrar { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalAbonoManualFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalAbonoManualTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroProforma { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Consignatario { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Booking_AWD { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TipoPrecioLiquidacion { get; set; }
        public string? EstadoLiquidacion { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CargoManualRecibidorFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CargoManualRecibidorTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ClausulaVenta { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FletePorCobrarFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FletePorCobrarTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SaldoPorCobrarFleteFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SaldoPorCobrarFleteTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AbonosFleteTran { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AbonosFleteFun { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ClaveEmbarque { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalVentaProforma { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? BL { get; set; }
        public string? EstibaEnCamara { get; set; }
    }
}
