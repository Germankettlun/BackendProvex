using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{

    public class CuentasCorrientesResponse
    {
        public List<CuentasCorrientesItems> Data { get; set; }
    }

#nullable enable
        public class CuentasCorrientesItems
    {
        public int Id { get; set; } // PK autoincrement

        public string? CodigoEmpresa { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? CodigoGrupoRecibidor { get; set; }
        public string? GrupoRecibidor { get; set; }
        public string? RutRecibidor { get; set; }
        public string? Recibidor { get; set; }
        public string? CodigoTipoNave { get; set; }
        public string? TipoNave { get; set; }
        public string? CodigoNaviera { get; set; }
        public string? Naviera { get; set; }
        public string? CodigoPuerto { get; set; }
        public string? Puerto { get; set; }
        public string? CodigoDestino { get; set; }
        public string? Destino { get; set; }
        public string? CodigoNave { get; set; }
        public string? Nave { get; set; }
        public string? CodigoMercado { get; set; }
        public string? Mercado { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? GrupoProductor { get; set; }
        public string? RutProductor { get; set; }
        public string? NomProductorReal { get; set; }
        public string? CodigoGrupoPredio { get; set; }
        public string? GrupoPredio { get; set; }
        public string? CodigoGrupoCuartel { get; set; }
        public string? GrupoCuartel { get; set; }
        public string? CodigoPredio { get; set; }
        public string? Predio { get; set; }
        public string? CodigoCuartel { get; set; }
        public string? Cuartel { get; set; }
        public string? Pallet { get; set; }
        public string? TipoPallet { get; set; }
        public string? CodigoBasePallet { get; set; }
        public string? BasePallet { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasPallet { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NroMix { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }
        public string? CodigoVariedad { get; set; }
        public string? Variedad { get; set; }
        public string? CodigoEnvop { get; set; }
        public string? Envop { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoEnvop { get; set; }
        public string? CodigoEtiqueta { get; set; }
        public string? Etiqueta { get; set; }
        public string? CodigoCategoria { get; set; }
        public string? Categoria { get; set; }
        public string? CodigoCalibre { get; set; }
        public string? Calibre { get; set; }
        public string? CodigoPlu { get; set; }
        public string? Plu { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cajas { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ValorUnitarioSII { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ValorUnitarioProf { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalVentaSII { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? GastoUnitarioRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalGastoRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ComisionRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalFOBChile { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FOBChileUnitario { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? ComisionProductor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalGastoPorEstandarProductor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FOBNetoProductor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Contenedor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroDespacho { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? VentaUnitariaDestino { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalVentaDestino { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodVariedadEti { get; set; }
        public string? VariedadEti { get; set; }
        public string? CodigoProductor { get; set; }
        public string? CodigoProductorEti { get; set; }
        public string? NomProductorEti { get; set; }
        public string? CodigoExportador { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Precio_unitario_Venta_Destino_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Precio_unitario_Venta_destino_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_Venta_destino_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_Venta_Destino_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Gasto_unitario_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Gasto_unitario_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_Gasto_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_Gasto_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Precio_unitario_Fob_Chile_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Precio_unitario_Fob_Chile_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Comisión_recibidor_unitario_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Comisión_recibidor_unitario_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_ComisiónRec_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_ComisiónRec_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Consignatario { get; set; }
        public string? Moneda { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_FOB_Chile_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_FOB_Chile_FU { get; set; }
        public string? FechaVenta { get; set; }
        public string? FechaDespacho { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SemanaDespacho { get; set; }
        public string? CodigoGrupoCalibre { get; set; }
        public string? NombreGrupoCalibre { get; set; }
        public string? ClaveEmbarque { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? DUS { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? BL { get; set; }
        public string? FechaEta { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasEquivalentes { get; set; }
        public string? ModalidadVenta { get; set; }
        public string? ClausulaVenta { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SemanaEta { get; set; }
        public string? TipoPrecioLiquidacion { get; set; }
        public string? EstadoLiquidacion { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PrecioPool { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TipoCambioFuncional { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_ComisiónProd_TR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Total_ComisiónProd_FU { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Fecha_Zarpe { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SemanaZarpe { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SemanaPacking { get; set; }
        public string? FechaPacking { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodFacilitie { get; set; }
        public string? NomFacilitie { get; set; }
        public string? Milimetraje { get; set; }
        public string? CodigoRecibidor { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PrecioVenta { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? FrutaFueraNorma { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202022_TRADE_FOUNDS_PROV { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202026_LUMPER { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202012_OTHERS { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202004_CUSTOMS_BROKER { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202001_OCEAN_FREIGHT { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202028_WIRE_FEE { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202013_COLD_STORAGE { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202010_INLAND_FREIGHT { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202024_CHEP { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202009_CLEARENCE { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R30303001_COMISION_RECIBIDOR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202038_TRUCKLOADING_PIER { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202018_QUALITY_CONTROL { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202036_TEMP_RECORDER { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202016_BANKING_FEE { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? R202049_MARKET_COST { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? P8301_SEGURO_TAPERING_ { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? P6001_SEGURO_TRANSPORTE_ { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? P4001_MATERIALES_ESTANDAR { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? P9102_EXTRA_COSTO_ZONA_NORTE_ { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? P7001_CONTROL_DE_CALIDAD_ORIGEN_ { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? P9101_GASTO_ESTIMADO_WEB_ { get; set; }
    }

}
