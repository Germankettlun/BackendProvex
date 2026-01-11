using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using ProvexApi.Converters;

namespace ProvexApi.Models.SDT.Cargas
{
    public class ExistenciaPalletResponse
    {
        public List<ExistenciaResponseItem> Data { get; set; }
    }

    //[JsonConverter(typeof(StringOrNumberConverter))]

    public class ExistenciaResponseItem
    {

        //[JsonConverter(typeof(StringOrNumberConverter))]
        public int Id { get; set; } // PK autoincrement

        public string? CodigoTipoInspeccion { get; set; }
        public string? DescTipoInspeccion { get; set; }
        public string? Pallet { get; set; }
        public string? TipoPallet { get; set; }
        public string? CodigoBasePallet { get; set; }
        public string? DescBasePallet { get; set; }
        public string? CodigEspecie { get; set; }
        public string? DescEspecie { get; set; }
        public string? CodigoTipoTratamiento { get; set; }
        public string? DescTipoTratamiento { get; set; }
        public string? Temperatura { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Altura { get; set; }
        public string? CodigoPredioReal { get; set; }
        public string? CodigoPredioEtiquetado { get; set; }
        public string? DescPredioReal { get; set; }
        public string? DescPredioEtiquetado { get; set; }
        public string? CodigoCuartelReal { get; set; }
        public string? CodigoCuartelEtiquetado { get; set; }
        public string? DescCuartelReal { get; set; }
        public string? DescCuartelEtiquetado { get; set; }
        public string? CodigoGrupoVariedadReal { get; set; }
        public string? CodigoVariedadReal { get; set; }
        public string? CodigoGrupoVariedadEtiquetado { get; set; }
        public string? CodigoVariedadEtiquetado { get; set; }
        public string? NombreGrupoVariedadReal { get; set; }
        public string? NombreVariedadReal { get; set; }
        public string? NombreGrupoVariedadEtiquetado { get; set; }
        public string? NombreVariedadEtiquetado { get; set; }
        public string? CodigoEnvop { get; set; }
        public string? DescEnvop { get; set; }
        public string? CodigoEnvase { get; set; }
        public string? DescEnvase { get; set; }
        public string? CodigoEmbalaje { get; set; }
        public string? DescEmbalaje { get; set; }
        public string? CodigoEtiqueta { get; set; }
        public string? DescEtiqueta { get; set; }
        public string? CodigoCategoria { get; set; }
        public string? DescCategoria { get; set; }
        public string? CodigoCalibre { get; set; }
        public string? DescCalibre { get; set; }
        public string? CodigoPlu { get; set; }
        public string? DescPlu { get; set; }
        public string? FechaPack { get; set; }
        public string? FechaCosecha { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cajas { get; set; }
        public string? CodigoCamara { get; set; }
        public string? DescCamara { get; set; }
        public string? CodigoRack { get; set; }
        public string? DescRack { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? AlturaEstiba { get; set; }
        public string? PosicionEstiba { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoFacilitie { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NombreFacilitie { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Mixs { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoNetoEnvop { get; set; }
        public string? Origen { get; set; }
        public string? ReservaOrdenEmbarque { get; set; }
        public string? GlosaOrdenEmbarque { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroProceso { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FechaProceso { get; set; }
        public string? GlosaReservaOrdenEmbarque { get; set; }
        public string? GlosaReservaNotaProduccion { get; set; }
        public string? GlosaReservaSAG { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasEquivalentes { get; set; }
        public string? CodigoGrupoPredio { get; set; }
        public string? NomGrupoPredio { get; set; }
        public string? CSG { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? NomTemporada { get; set; }
        public string? CSE { get; set; }
        public string? NombreComuna { get; set; }
        public string? NombreComunaEti { get; set; }
        public string? NombreProvincia { get; set; }
        public string? NombreProvinciaEti { get; set; }
        public string? MercadoInspeccionado { get; set; }
        public string? CodigoProductorReal { get; set; }
        public string? NombreProductorReal { get; set; }
        public string? CodigoProductorEti { get; set; }
        public string? NombreProductorEti { get; set; }
        public string? Turno { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? CodigoGrupoCalibre { get; set; }
        public string? NombreGrupoCalibre { get; set; }
        public string? CodigoGrupoCategoria { get; set; }
        public string? NombreGrupoCategoria { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodFacilitiePallet { get; set; }
        public string? NomFacilitiePallet { get; set; }
        public string? CSP { get; set; }
        public string? CodigoExportador { get; set; }
        public string? NombreExportador { get; set; }
        public string? NumeroCertificadoProductorReal { get; set; }
        public string? TipoCertificadoProductorReal { get; set; }
        public string? NumeroCertificadoProductorEti { get; set; }
        public string? TipoCertificadoProductorEti { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
        public string? CodigoGrupoExportador { get; set; }
        public string? NombreGrupoExportador { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? GuiaSII { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? GuiaInspeccionSAG { get; set; }
        public string? Ubicacion { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PlanillaRecepcion { get; set; }
        public string? FechaRecepcion { get; set; }
        public string? EstadoInspeccion { get; set; }
    }
}
