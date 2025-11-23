using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT
{
    public class EnvopsResponse
    {
        public List<EnvasesItem> Data { get; set; }
    }

#nullable enable
    public class EnvasesItem
    {
        public int Id { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEnvase { get; set; }
        public string? NombreEnvase { get; set; }
        public string? TipoEnvase { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEmbalaje { get; set; }
        public string? NombreEmbalaje { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEnvop { get; set; }
        public string? NombreEnvaseOp { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEspecie { get; set; }
        public string? NombreEspecie { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoBruto { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoBrutoAduana { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoMaquina { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoNeto { get; set; }
        [JsonConverter(typeof(NumericToStringConverter))]
        public string? EnvaseOpActivo { get; set; }
        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CantidadCajas { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PieCubico { get; set; }
    }
}
