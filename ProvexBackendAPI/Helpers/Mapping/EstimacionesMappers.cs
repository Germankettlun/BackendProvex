using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Helpers.Shared.Extensions;
using static ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones.EstimacionesDto;

namespace ProvexBackendAPI.Helpers.Mapping
{
    internal static class EstimacionesMappers
    {
        public static RowFlat MapRowFlat(this SqlDataReader rdr)
        {
            var estId = rdr.Get<int?>("ID_ESTIMACION");

            return new RowFlat
            {
                // Raíz
                PesoBaseEspecie = rdr.Get<double?>("ESPECIE_KILO_BASE") ?? 0.0,
                Especie = rdr.FirstExistingAsString("NOM_ESP"),

                // Item
                IdProductor = rdr.FirstExistingAsString("ID_PRODUCTOR"),
                Productor = rdr.FirstExistingAsString("NOM_PROD"),
                Variedad = rdr.FirstExistingAsString("NOM_VAR"),
                Agronomo = rdr.FirstExistingAsString("NOM_USUARIO_AGRONOMO") ?? "",
                DistribucionCalibre = rdr.Get<bool?>("DIST_CAL"),
                DistribucionCategoria = rdr.Get<bool?>("DIST_CAT"),
                PorcentajeExportacion = rdr.Get<int?>("PCT_EXP_PORC") ?? 0,

                // Envase
                EnvaseId = rdr.FirstExistingAsString("ENVASE_ID") ?? "",
                EnvaseNombre = rdr.FirstExistingAsString("NOM_ENVASE_COSECHA") ?? "",
                EnvaseKilo = rdr.Get<int?>("KG_DIA_ENVASE") ?? 0,

                // Estimación
                Est_ID = estId,
                Est_Contratado = rdr.Get<int?>("CAJAS_CONTRATADAS") ?? 0,
                Est_FCosecha = rdr.FirstExistingAsString("FECHA_INICIO_COSECHA_YM") ?? "",

                Ant_Estimado = rdr.Get<int?>("CAJAS_E_ANTERIOR_SIN_PORC"),
                Ant_Producido = rdr.Get<int?>("CAJAS_P_ANTERIOR"),
                Sig_Estimado = rdr.Get<int?>("CAJAS_E_SIGUIENTE_SIN_PORC"),
                Sig_Producido = rdr.Get<int?>("CAJAS_P_SIGUIENTE_SIN_PORC"),

                // Bisemanal
                Bis_AnioBase = rdr.Get<int?>("ANIO"),
                Bis_SemanaBase = rdr.FirstExistingAsString("SEMANA_NRO"),

                // Días
                Bis_ID = rdr.Get<int?>("ID_ESTIMACION_BISEMANAL"),
                Dia_Nombre = rdr.FirstExistingAsString("NOMBRE_DIA"),
                Dia_Fecha = rdr.Get<DateTime?>("DIA"),
                Dia_Estimado = rdr.Get<decimal?>("CAJAS_E_DISTRIB_SIN_PORC"),
                Dia_Producido = rdr.Get<decimal?>("CAJAS_P"),
                Dia_DistribucionFrio = rdr.Get<bool?>("DIST_FRI"),
                Dia_DistribucionPacking = rdr.Get<bool?>("DIST_PACK")
            };
        }
    }
}
