using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Models.SDT.Cargas
{


    public class CuadraturaProcesosDiariosResponse
    {
        public List<CuadraturaProcesosDiariosItem> Data { get; set; }
    }

#nullable enable
    public class CuadraturaProcesosDiariosItem
    {
        public int Id { get; set; }  // PK autoincrement

        public string? TipoProceso { get; set; }
        public string? FechaProceso { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NumeroProceso { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CodigoFacilitie { get; set; }
        public string? Facilitie { get; set; }
        public string? CSG { get; set; }
        public string? Productor { get; set; }
        public string? CodigoEspecie { get; set; }
        public string? Especie { get; set; }
        public string? CodigoVariedad { get; set; }
        public string? Variedad { get; set; }
        public string? CodigoJornadaPacking { get; set; }
        public string? JornadaPacking { get; set; }
        public string? CodigoLineaProceso { get; set; }
        public string? LineaProceso { get; set; }
        public string? CodigoPredio { get; set; }
        public string? Predio { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Semana { get; set; }
        public string? CSE { get; set; }
        public string? Exportador { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosProcesados { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? EnvasesProcesados { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? BinsComercial { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? BinsPreCalibre { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        [JsonPropertyName("BinsBajas/Desechos")]
        public string? BinsBajasDesechos { get; set; }
        // Ajustado, sin '/' en el nombre
        [JsonConverter(typeof(NumericToStringConverter))]
        public string? BinsSobreCalibre { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasProducidas { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosExportacion { get; set; }
        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PorcentajeExportacion { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosComercial { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PorcentajeComercial { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosBajasDesecho { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosMerma { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PorcentajeMerma { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasPorEnvase { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosPreSobreCalibre { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasEquivalentes { get; set; }
        public string? CodigoCuartel { get; set; }
        public string? Estado { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraCierre { get; set; }
        public string? HoraTermino { get; set; }
        public string? NombreEmpresa { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? CodigoProductor { get; set; }
        public string? CodigoGrupoVariedad { get; set; }
        public string? NomGrupoVariedad { get; set; }
        public string? NroPersonas { get; set; }
        public string? Observacion { get; set; }
        public string? MotivoDetencion { get; set; }
        public string? TipoDetencion { get; set; }
        public string? MinDetencion { get; set; }
        public string? Techo { get; set; }
        public string? CodigoTemporada { get; set; } // Campo requerido
        public string? NomTemporada { get; set; }
        public string? FechaCosecha { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosPreProcesado { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PorcentajeKilosPreProcesado { get; set; }
        public string? ObservacionDetencion { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? BinesExportacion { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
    }
}
