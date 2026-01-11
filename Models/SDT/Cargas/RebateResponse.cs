using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class RebateResponse
    {
        public List<RebateItem> Data { get; set; }
    }

#nullable enable
    public class RebateItem
    {
        public int Id { get; set; } // PK autoincrement
        public string? CodigoTemporada { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? CodigoNave { get; set; }
        public string? Nave { get; set; }
        public string? CodigoTipoNave { get; set; }
        public string? TipoNave { get; set; }
        public string? CodigoNaviera { get; set; }
        public string? Naviera { get; set; }
        public string? NumeroReserva { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? IdRebate { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroProforma { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroSII { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? IdFactEmbarque { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? BL { get; set; }
        public string? CodigoRecibidor { get; set; }
        public string? NombreRecibidor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoFlete { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoDevolucion { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalCajas { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalPallet { get; set; }
    }

}
