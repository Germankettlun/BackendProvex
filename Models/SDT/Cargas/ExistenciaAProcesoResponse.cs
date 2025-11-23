#nullable enable
using ProvexApi;
using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{
    public class ExistenciaAProcesoResponse
    {
        public List<ExistenciaAProcesoItem> Data { get; set; }
    }
    public class ExistenciaAProcesoItem
    {
        public int Id { get; set; }  // PK autoincrement

        public string? CodigoEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }
        public string? CodigoProductor { get; set; }
        public string? NomProductor { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
        public string? CodPredio { get; set; }
        public string? NomPredio { get; set; }
        public string? CodCuartel { get; set; }
        public string? NomCuartel { get; set; }
        public string? Lote { get; set; }
        public string? CodEspecie { get; set; }
        public string? NomEspecie { get; set; }
        public string? CodVariedad { get; set; }
        public string? NomVariedad { get; set; }
        public string? CodTipoTrat { get; set; }
        public string? NomTipoTrat { get; set; }
        public string? CodEnvase { get; set; }
        public string? NomEnvase { get; set; }
        public string? CodCalidadPack { get; set; }
        public string? NomCalidadPack { get; set; }
        public string? CodCalibre { get; set; }
        public string? NomExportador { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CorrelativoBin { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CantidadEnvasesContenidosBin { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosBin { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodFacilitie { get; set; }
        public string? NomFacilitie { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PlanillaRecepcion { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PlanillaProcPacking { get; set; }
        public string? TipoFruta { get; set; }
        public string? ConHidro { get; set; }
        public string? Fecha { get; set; }
        public string? FechaCosecha { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? GuiaSII { get; set; }
        public string? Glosa { get; set; }
        public string? OrigenLote { get; set; }
        public string? Techo { get; set; }
        public string? TipoCuartel { get; set; }
        public string? CodigoCamara { get; set; }
        public string? DescCamara { get; set; }
        public string? CodigoRack { get; set; }
        public string? DescRack { get; set; }
        public string? AlturaEstiba { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PosicionEstiba { get; set; }
        public string? TipoRecepcion { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? NombreTemporada { get; set; }
        public string? CSG { get; set; }
        public string? HoraRecepcion { get; set; }
        public string? GGN { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? RutTransportista { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? DvRutTransportista { get; set; }
        public string? NombreTransportista { get; set; }
        public string? ExportableEstimado { get; set; }
        public string? SegregacionDestino { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? VelocidadProceso { get; set; }
    }
}
