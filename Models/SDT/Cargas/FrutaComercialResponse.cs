using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class FrutaComercialResponse
    {
        public List<FrutaComercialResponseItem> Data { get; set; }
    }
    public class FrutaComercialResponseItem
    {
        public int Id { get; set; } // PK autoincrement

        public string? Origen { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Planilla { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? Temporada { get; set; }
        public string? Lote { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Correlativo { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CantEnvasesContenidos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cantidad { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Kilos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoVariedad { get; set; }
        public string? Variedad { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }
        public string? CodigoEnvase { get; set; }
        public string? Envase { get; set; }
        public string? CodigoCalidad { get; set; }
        public string? Calidad { get; set; }
        public string? CodigoCalibre { get; set; }
        public string? CodigoTipoTratamiento { get; set; }
        public string? TipoTratamiento { get; set; }
        public string? CodigoCuartel { get; set; }
        public string? Cuartel { get; set; }
        public string? CodigoPredio { get; set; }
        public string? Predio { get; set; }
        public string? CSE { get; set; }
        public string? Exportador { get; set; }
        public string? FechaCosecha { get; set; }
        public string? TipoFruta { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoFacility { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Facility { get; set; }
        public string? CodigoProductor { get; set; }
        public string? NombreProductor { get; set; }
        public string? FechaProceso { get; set; }
    }
}
