// Models/SDT/RecibidoresResponse.cs
using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class RecibidoresResponse
    {
        public List<RecibidoresItem> Data { get; set; }
    }

#nullable enable
    public class RecibidoresItem
    {
        public int Id { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoRecibidor { get; set; }
        public string? NombreRecibidor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RutRecibidor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RecibidorActivo { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoGrupoRecibidor { get; set; }
        public string? NombreGrupoRecibidor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CoberturaSeguro { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? LineaCredito { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NomAseguradoCredito { get; set; }
    }
}
