using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{

    public class OrdenCompraResponse
    {
        public List<OrdenCompraItem> Data { get; set; }
    }

#nullable enable
    public class OrdenCompraItem
    {
        public int Id { get; set; }  // PK autoincrement

        public string? CodigoEmpresa { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Ano { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RutContr { get; set; }
        public string? Nombre { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroOrden { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoMaterial { get; set; }
        public string? NombreMaterial { get; set; }
        public string? CodigoUM { get; set; }
        public string? CodMoneda { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cantidad { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ValorTransaccional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalTransaccional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ValorCursoLegal { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalCursoLegal { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? ValorFuncional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalFuncional { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? IdOrdenCompraDet { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FechaOrden { get; set; }
        public string? FormaDePago { get; set; }
        public string? TipoOrden { get; set; }
        public string? CuentaContable { get; set; }
        public string? CentroCosto { get; set; }
        public string? SubCentroCosto { get; set; }
        public string? ObservacionGeneral { get; set; }
        public string? Estado { get; set; }
        public string? CodigoFamiliaMaterial { get; set; }
        public string? NomFamiliaMaterial { get; set; }
        public string? NombreAgrupacionMat { get; set; }
        public string? CodItem { get; set; }
        public string? NomItem { get; set; }
        public string? CodSubItem { get; set; }
        public string? NomSubItem { get; set; }
        public string? ResponsableCompra { get; set; }
    }

}
