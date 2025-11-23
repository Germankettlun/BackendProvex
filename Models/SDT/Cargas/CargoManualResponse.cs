using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class CargoManualResponse
    {
        public List<CargoManualItem> Data { get; set; }
    }
#nullable enable
    public class CargoManualItem
    {
        public int Id { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? GrupoProductor { get; set; }
        public string? CodigoProductor { get; set; }
        public string? Productor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? RutProductor { get; set; }
        public string? CodigoItem { get; set; }
        public string? DescItem { get; set; }
        public string? TipoMovItem { get; set; }
        public string? TipoPagoItem { get; set; }
        public string? CodigoSubItem { get; set; }
        public string? DescSubItem { get; set; }
        public string? CodigoGrpSubItem { get; set; }
        public string? DescGrpSubItem { get; set; }
        public string? CodMoneda { get; set; }
        public string? DescMoneda { get; set; }
        public string? CodigEspecie { get; set; }
        public string? DescEspecie { get; set; }
        public string? CodigoTipoNave { get; set; }
        public string? DescTipoNave { get; set; }
        public string? CodigoNave { get; set; }
        public string? DescNave { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroDoc { get; set; }
        public string? GlosaDocumento { get; set; }
        public string? Fecha { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TiCaCursoLegal { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TiCaFuncional { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Monto_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Monto_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Monto_CL { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AnoCont { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroFolio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SwPagoProductores { get; set; }
    }

}
