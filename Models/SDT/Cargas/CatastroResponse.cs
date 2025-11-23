using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT
{
    public class CatastroResponse
    {
        public List<CatastroItem> Data { get; set; }
    }

#nullable enable
    public class CatastroItem
    {
        public int Id { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodEmpresa { get; set; }
        public string? Empresa { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoProductor { get; set; }
        public string? NombreProductor { get; set; }
        public string? ProdPropio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? RutProductor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ProductorActivo { get; set; }
        public string? CodTipoEmail { get; set; }
        public string? TipoEmail { get; set; }
        public string? Email { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoPredio { get; set; }
        public string? NombrePredio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoCuartel { get; set; }
        public string? NombreCuartel { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CSG { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PredioActivo { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CuartelActivo { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AsociadoSuelo { get; set; }
        public string? ComunaPredio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Plantas { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Hectareas { get; set; }
        public string? DireccionProductor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoExportador { get; set; }
        public string? NombreExportador { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoGrupoPredio { get; set; }
        public string? NombreGrupoPredio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? DireccionPredio { get; set; }
        public string? ProvinciaPredio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AnoPlantacion { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoCeCto { get; set; }
        public string? NombreCeCto { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoSubCto { get; set; }
        public string? NombreSubCto { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? NombreEspecie { get; set; }
        public string? CodVariedadPrincipal { get; set; }
        public string? NomVariedadPrincipal { get; set; }
        public string? CodigoMedidor { get; set; }
        public string? NombreMedidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FechaProductiva { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FechaCosecha { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? DistanciaX { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? DistanciaY { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Hileras { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoSistemaRiego { get; set; }
        public string? NombreSistemaRiego { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoPortaInjerto { get; set; }
        public string? NombrePortaInjerto { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? UsaTecho { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ControlHeladas { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
    }
}
