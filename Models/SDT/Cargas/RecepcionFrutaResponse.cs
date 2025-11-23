#nullable enable
using System.Text.Json.Serialization;
using ProvexApi.Converters;

namespace ProvexApi.Models.SDT.Cargas
{
    public sealed class RecepcionFrutaResponse
    {
        [JsonPropertyName("Data")]
        public List<RecepcionFrutaItem> Data { get; set; } = new();
    }

    public sealed class RecepcionFrutaItem
    {
        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("Planilla")] public string? Planilla { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("CodFacilitie")] public string? CodFacilitie { get; set; }
        [JsonPropertyName("NomFacilitie")] public string? NomFacilitie { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("CodFacilitiePallet")] public string? CodFacilitiePallet { get; set; }
        [JsonPropertyName("NomFacilitiePallet")] public string? NomFacilitiePallet { get; set; }

        [JsonPropertyName("CodTipoRecepcion")] public string? CodTipoRecepcion { get; set; }
        [JsonPropertyName("NomTipoRecepcion")] public string? NomTipoRecepcion { get; set; }

        [JsonPropertyName("FechaRecepcion")] public DateTime? FechaRecepcion { get; set; } // única excepción
        [JsonPropertyName("HoraRecepcion")] public string? HoraRecepcion { get; set; }

        [JsonPropertyName("CodCondicionFrio")] public string? CodCondicionFrio { get; set; }
        [JsonPropertyName("NomCondicionFrio")] public string? NomCondicionFrio { get; set; }
        [JsonPropertyName("CodTipoInspeccion")] public string? CodTipoInspeccion { get; set; }
        [JsonPropertyName("NomTipoInspeccion")] public string? NomTipoInspeccion { get; set; }

        [JsonPropertyName("Guia")] public string? Guia { get; set; }
        [JsonPropertyName("Conductor")] public string? Conductor { get; set; }
        [JsonPropertyName("Patente")] public string? Patente { get; set; }
        [JsonPropertyName("Observacion")] public string? Observacion { get; set; }

        [JsonPropertyName("Pallet")] public string? Pallet { get; set; }
        [JsonPropertyName("CodTipoPallet")] public string? CodTipoPallet { get; set; }
        [JsonPropertyName("NomTipoPallet")] public string? NomTipoPallet { get; set; }
        [JsonPropertyName("CodBasePallet")] public string? CodBasePallet { get; set; }
        [JsonPropertyName("NomBasePallet")] public string? NomBasePallet { get; set; }

        [JsonPropertyName("CodEspecie")] public string? CodEspecie { get; set; }
        [JsonPropertyName("NomEspecie")] public string? NomEspecie { get; set; }
        [JsonPropertyName("CodTipoTrat")] public string? CodTipoTrat { get; set; }
        [JsonPropertyName("NomTipoTrat")] public string? NomTipoTrat { get; set; }
        [JsonPropertyName("Temperatura")] public string? Temperatura { get; set; }
        [JsonPropertyName("Altura")] public string? Altura { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("Mixs")] public string? Mixs { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("Cajas")] public string? Cajas { get; set; }

        [JsonPropertyName("CodPLU")] public string? CodPLU { get; set; }
        [JsonPropertyName("NomPLU")] public string? NomPLU { get; set; }
        [JsonPropertyName("CodVariedad")] public string? CodVariedad { get; set; }
        [JsonPropertyName("NomVariedad")] public string? NomVariedad { get; set; }
        [JsonPropertyName("CodVariedadEti")] public string? CodVariedadEti { get; set; }
        [JsonPropertyName("NomVariedadEti")] public string? NomVariedadEti { get; set; }
        [JsonPropertyName("CodEtiqueta")] public string? CodEtiqueta { get; set; }
        [JsonPropertyName("NomEtiqueta")] public string? NomEtiqueta { get; set; }
        [JsonPropertyName("CodCategoria")] public string? CodCategoria { get; set; }
        [JsonPropertyName("NomCategoria")] public string? NomCategoria { get; set; }
        [JsonPropertyName("CodCalibre")] public string? CodCalibre { get; set; }
        [JsonPropertyName("NomCalibre")] public string? NomCalibre { get; set; }
        [JsonPropertyName("CodEnvop")] public string? CodEnvop { get; set; }
        [JsonPropertyName("NomEnvop")] public string? NomEnvop { get; set; }

        [JsonPropertyName("CodCuartel")] public string? CodCuartel { get; set; }
        [JsonPropertyName("NomCuartel")] public string? NomCuartel { get; set; }
        [JsonPropertyName("CodCuartelEti")] public string? CodCuartelEti { get; set; }
        [JsonPropertyName("NomCuartelEti")] public string? NomCuartelEti { get; set; }

        [JsonPropertyName("CodigoPredioReal")] public string? CodigoPredioReal { get; set; }
        [JsonPropertyName("NomPredioReal")] public string? NomPredioReal { get; set; }
        [JsonPropertyName("CodigoPredioEti")] public string? CodigoPredioEti { get; set; }
        [JsonPropertyName("NomPredioEti")] public string? NomPredioEti { get; set; }

        [JsonPropertyName("CodigoProductorReal")] public string? CodigoProductorReal { get; set; }
        [JsonPropertyName("NomProductorReal")] public string? NomProductorReal { get; set; }
        [JsonPropertyName("CodigoProductorEti")] public string? CodigoProductorEti { get; set; }
        [JsonPropertyName("NomProductorEti")] public string? NomProductorEti { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("CajasEquivalentes")] public string? CajasEquivalentes { get; set; }

        [JsonPropertyName("CodigoTemporada")] public string? CodigoTemporada { get; set; }
        [JsonPropertyName("CodigoEmpresa")] public string? CodigoEmpresa { get; set; }

        [JsonPropertyName("CSG")] public string? CSG { get; set; }
        [JsonPropertyName("CSG_Eti")] public string? CSG_Eti { get; set; }
        [JsonPropertyName("CSP")] public string? CSP { get; set; }

        [JsonPropertyName("CodigoGrupoProductorReal")] public string? CodigoGrupoProductorReal { get; set; }
        [JsonPropertyName("CodigoGrupoProductorEti")] public string? CodigoGrupoProductorEti { get; set; }
        [JsonPropertyName("NombreGrupoProductorReal")] public string? NombreGrupoProductorReal { get; set; }
        [JsonPropertyName("NombreGrupoProductorEti")] public string? NombreGrupoProductorEti { get; set; }

        [JsonPropertyName("FechaPacking")] public string? FechaPacking { get; set; }
        [JsonPropertyName("CodigoExportador")] public string? CodigoExportador { get; set; }
        [JsonPropertyName("RutExportador")] public string? RutExportador { get; set; }
        [JsonPropertyName("NombreExportador")] public string? NombreExportador { get; set; }

        [JsonPropertyName("PredioCuarentenado")] public string? PredioCuarentenado { get; set; }
        [JsonPropertyName("FacilitieCuarentenado")] public string? FacilitieCuarentenado { get; set; }
    }
}
