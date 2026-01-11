using ProvexApi.Converters;
using System.Text.Json.Serialization;

namespace ProvexApi.Data.DTOs.ASOEX
{
    public class ProcesoViajeDto
    {
        public int idproceso { get; set; }
        public int nro_otro { get; set; }
        public int temp_seq_nro { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_puerto_embarqu { get; set; }
        public int correlativo_viaje { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_puerto_arribo { get; set; }
        public string fecha_zarpe { get; set; }
        public string fecha_arribo { get; set; } // Puede ser null

        [JsonConverter(typeof(NumericToStringConverter))]
        public string cod_nave { get; set; }
        public string status { get; set; }
    }

    public class ProcesoDetalleDtoPaged : IApiPagedResponse<ProcesoDetalleDto>
    {
        public int current_page { get; set; }
        public int last_page { get; set; }
        public List<ProcesoDetalleDto> data { get; set; } = new();
    }
}
