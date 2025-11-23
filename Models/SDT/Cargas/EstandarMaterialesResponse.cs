#nullable enable
using ProvexApi;
using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class EstandarResponseMaterialesResponse
    {
        public List<EstandarItem> Data { get; set; }
    }

    public class EstandarItem
    {
        public int Id { get; set; }  // PK autoincrement
        public string? CodigoEmpresa { get; set; }
        public string? Anio { get; set; }
        public string? CodigoEstandar { get; set; }
        public string? Estandar { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }
        public string? CodigoEnvop { get; set; }
        public string? Envop { get; set; }
        public string? TipoEstandar { get; set; }
        public string? CodigoVariedad { get; set; }
        public string? Variedad { get; set; }
        public string? CodigoEtiqueta { get; set; }
        public string? Etiqueta { get; set; }
        public string? CodigoCalibre { get; set; }
        public string? Calibre { get; set; }
        public string? CodigoMercado { get; set; }
        public string? Mercado { get; set; }
        public string? CodigoMaterial { get; set; }
        public string? Material { get; set; }
        public string? AplicadoPor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cantidad { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Factor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PrecioCLP { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PrecioFUN { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalCLP { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalFUN { get; set; }
    }
}