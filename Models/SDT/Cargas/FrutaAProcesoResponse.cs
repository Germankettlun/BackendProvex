using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class FrutaAProcesoResponse
    {
        public List<FrutaAProcesoResponseFrutaItem> Data { get; set; }
    }

    public class FrutaAProcesoResponseFrutaItem
    {
        public int Id { get; set; }  // PK autoincrement

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Planilla { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodFacilitie { get; set; }

        public string? Facilitie { get; set; }
        public string? TipoRecepcion { get; set; }
        public string? FechaRecepcion { get; set; }
        public string? HoraRecepcion { get; set; }
        public string? GuiaSII { get; set; }
        public string? Transportista { get; set; }
        public string? Patente { get; set; }
        public string? Glosa { get; set; }
        public string? HidroEnfriado { get; set; }
        public string? Cerrado { get; set; }
        public string? Productor { get; set; }
        public string? CodigoPredio { get; set; }
        public string? NomPredio { get; set; }
        public string? CodigoCuartel { get; set; }
        public string? NomCuartel { get; set; }
        public string? Lote { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }
        public string? Variedad { get; set; }
        public string? TipoTratamientoFruta { get; set; }
        public string? Envase { get; set; }
        public string? CalidadPacking { get; set; }
        public string? Calibre { get; set; }
        public string? Exportador { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CorrelativoBin { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CantidadEnvasesContenidosBin { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosBin { get; set; }

        public string? CodigoEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? NombreTemporada { get; set; }
        public string? CodigoProductor { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
        public string? FechaCosecha { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? AnoRecepcion { get; set; }

        public string? MesRecepcion { get; set; }
        public string? Techo { get; set; }
        public string? TipoCuartel { get; set; }
        public string? TipoFruta { get; set; }
        public string? CodigoGrupoVariedad { get; set; }
        public string? NomGrupoVariedad { get; set; }
        public string? CodigoGrupoPredio { get; set; }
        public string? NomGrupoPredio { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Semana { get; set; }
        public string? AsignadoCtaCteEnvase { get; set; }
        public string? FacilitieOrigen { get; set; }
        public string? TipoCertificadoProductor { get; set; }
        public string? NumeroCertificadoProductor { get; set; }
        public string? CSG { get; set; }
        public string? RutTransportista { get; set; }
        public string? RutProductor { get; set; }
        public string? ExportableEstimado { get; set; }
        public string? SegregacionDestino { get; set; }
        public string? VelocidadProceso { get; set; }
        public string? Agronomo { get; set; }
        public string? RutAgronomo { get; set; }
    }
}
