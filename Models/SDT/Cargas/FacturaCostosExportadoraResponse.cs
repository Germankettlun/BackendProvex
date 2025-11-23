using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{

    public class FacturaCostosExportadoraResponse
    {
        public List<FacturaCostosExportadoraItem>? Data { get; set; }
        public string? Columns { get; set; } // A veces viene, a veces no
                                             // etc.
    }

    #nullable enable
    public class FacturaCostosExportadoraItem
    {
        public int Id { get; set; }  // PK autoincrement


        public string? Empresa { get; set; }
        public string? Temporada { get; set; }
        public string? CodEspecie { get; set; }
        public string? NomEspecie { get; set; }
        public string? CodProductor { get; set; }
        public string? NomProductor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodFacilitie { get; set; }
        public string? NomFacilitie { get; set; }
        public string? TipoCosto { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? GuiaDespacho { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Rut { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Ano { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroDoc { get; set; }
        public string? CodItem { get; set; }
        public string? NomItem { get; set; }
        public string? CodSubItem { get; set; }
        public string? NomSubItem { get; set; }
        public string? TipoFormaAplicacion { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cantidad { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Monto { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TipoSII { get; set; }
        public string? DV { get; set; }
        public string? NomProveedor { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FolioContable { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? MontoFUN { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TipoCambio { get; set; }

        // Campo adicional para tu homologación interna
        public string? CodigoTemporada { get; set; }
        public string? CodigoEmpresa { get; set; }
    }

}
