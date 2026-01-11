using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Data.DTOs.ASOEX
{
    public class ProcesoDetalleDto
    {
        public int idproceso { get; set; }
        public int nro_otro { get; set; }
        public int temp_seq_nro { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_puerto_embarqu { get; set; }
        public int correlativo_viaje { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string rut_exportador { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_especie { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_variedad { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_region_origen { get; set; }
        public int psd_cantidad { get; set; }
        public decimal psd_tot_kg_neto { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_puerto_destino { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_consignatario { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_condicion { get; set; }
        public int? pagina_api { get; set; }
    }

    public class ProcesoViajeDtoPaged : IApiPagedResponse<ProcesoViajeDto>
    {
        public int current_page { get; set; }
        public int last_page { get; set; }
        public List<ProcesoViajeDto> data { get; set; } = new();
    }
}
