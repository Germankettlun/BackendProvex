using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class KardexResponse
    {
        public List<KardexItem> Data { get; set; }
    }

#nullable enable
    public class KardexItem
    {
        public int Id { get; set; } // PK autoincrement
        public int Anio { get; set; } 

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoMaterial { get; set; }
        public string? DesMaterial { get; set; }
        public string? CodigoAgrupacion { get; set; }
        public string? CodigoUnidadMedida { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroPlanilla { get; set; }
        public string? FechaMovimiento { get; set; }
        public string? MesMovimiento { get; set; }
        public string? CodigoMovimiento { get; set; }
        public string? TipoMovimiento { get; set; }
        public string? Proveedor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? BodegaStock { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? BodegaOrigen { get; set; }
        public string? BodegaDestino { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEmpresa { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoFacilitie { get; set; }
        public string? NomFacilitie { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroGuia { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoTipoGuia { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? TipoKardex { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Stock { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TiCaCursoLegal { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PrecioCursoLegal { get; set; }
       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TiCaFuncional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PrecioFuncional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalCursoLegal { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalFuncional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PMPCursoLegal { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PMPFuncional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FechaVenc { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TipoGuía { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroOrden { get; set; }
    }
}
