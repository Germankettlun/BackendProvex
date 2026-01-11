using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using ProvexApi.Converters;

namespace ProvexApi.Models.SDT.Cargas
{
    public class DespachoResponse
    {
        public List<DespachoItem> Data { get; set; }
    }

    public class DespachoItem
    {
        // Aquí incluyo todas las propiedades que aparecen en el JSON,
        // usando tipos sugeridos (int, double, DateTime?, string, etc.)
        // Ajusta según tu lógica y la nulabilidad que necesites.

        // Identificador de base de datos (recomendado para EF Core)
        public int Id { get; set; } // Primary Key (PK) en la tabla (puedes cambiarlo si deseas)

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PlanillaDespacho { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? PlanillaOrdenEmbarque { get; set; }
        public string? CodTipoNave { get; set; }
        public string? NomTipoNave { get; set; }
        public string? CodNave { get; set; }
        public string? NomNave { get; set; }
        public string? NomPais { get; set; }
        public string? CodPuerto { get; set; }
        public string? NomPuerto { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? GuiaDespacho { get; set; }

        // Las fechas u horas pueden ser DateTime, pero dependerá del formato exacto.
        // Si pueden fallar o venir vacías, usa DateTime? y comprueba su parseo.
        public string? FechaDespacho { get; set; }
        public string? HoraDespacho { get; set; }
        public string? Contenedor { get; set; }
        public string? RutTransportista { get; set; }
        public string? NomTransportista { get; set; }
        public string? RutChofer { get; set; }
        public string? NomChofer { get; set; }
        public string? Patente { get; set; }
        public string? FolioSAG { get; set; }
        public string? Sellos { get; set; }
        public double? CodFacilitie { get; set; }
        public string? NomFacilitie { get; set; }

        // "Booking/AWB": en JSON, la diagonal es un caracter especial,
        // lo representaremos como una propiedad normal (renombrada a BookingAwb).
        // Usando PropertyNameCaseInsensitive = true en JsonSerializer, no habrá problema.
        public string? BookingAwb { get; set; }

        public string? Ubicacion { get; set; }
        public string? CodTipoInspeccionEnc { get; set; }
        public string? NomTipoInspeccionEnc { get; set; }
        public string? RutRecibidor { get; set; }
        public string? CodRecibidor { get; set; }
        public string? NomRecibidor { get; set; }
        public string? RutExportador { get; set; }
        public string? NomExportador { get; set; }
        public string? TipoCamion { get; set; }
        public string? RutAgenteAduana { get; set; }
        public string? NomAgenteAduana { get; set; }
        public string? Consignatario { get; set; }
        public string? Celular { get; set; }
        public string? ObsSAG { get; set; }
        public string? ObsGuia { get; set; }

        public string? Despachador { get; set; }
        public string? TipoContenedor { get; set; }
        public string? CodTipoInspeccion { get; set; }
        public string? NomTipoInspeccion { get; set; }
        public string? Pallet { get; set; }
        public string? TipoPallet { get; set; }
        public string? CodBasePallet { get; set; }
        public string? NomBasePallet { get; set; }
        public string? CodEspecie { get; set; }
        public string? NomEspecie { get; set; }
        public string? CodTipoTrat { get; set; }
        public string? NomTipoTrat { get; set; }
        public string? Temperatura { get; set; }
        public string? Altura { get; set; }
        public string? Termografo { get; set; }
        public double? IdPallet { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalCajas { get; set; }
        [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalDet { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Mixs { get; set; }
        public string? RutProductorReal { get; set; }
        public string? NomProductorReal { get; set; }
        public string? NumeroCertificadoProductorReal { get; set; }
        public string? TipoCertificadoProductorReal { get; set; }
        public string? RutProductorEti { get; set; }
        public string? NomProductorEti { get; set; }
        public string? NumeroCertificadoProductorEti { get; set; }
        public string? TipoCertificadoProductorEti { get; set; }
        public string? CodPredioReal { get; set; }
        public string? CodPredioEti { get; set; }
        public string? NomPredioReal { get; set; }
        public string? NomPredioEti { get; set; }
        public string? CodCuartelReal { get; set; }
        public string? CodCuartelEti { get; set; }
        public string? NomCuartelReal { get; set; }
        public string? NomCuartelEti { get; set; }
        public string? CodGrpoVariedadReal { get; set; }
        public string? CodVariedadReal { get; set; }
        public string? CodGrpoVariedadEti { get; set; }
        public string? CodVariedadEti { get; set; }
        public string? NomGrpoVariedadReal { get; set; }
        public string? NomVariedadReal { get; set; }
        public string? NomGrpoVariedadEti { get; set; }
        public string? NomVariedadEti { get; set; }
        public string? CodEnvop { get; set; }
        public string? NomEnvop { get; set; }
        public double PesoNeto { get; set; }
        public string? CodEnvase { get; set; }
        public string? NomEnvase { get; set; }
        public string? CodEmbalaje { get; set; }
        public string? NomEmbalaje { get; set; }
        public string? CodEtiqueta { get; set; }
        public string? NomEtiqueta { get; set; }
        public string? CodCategoria { get; set; }
        public string? NomCategoria { get; set; }
        public string? CodCalibre { get; set; }
        public string? NomCalibre { get; set; }
        public string? CodPLU { get; set; }
        public string? NomPLU { get; set; }
        public string? FechaPack { get; set; }
        public string? FechaCosecha { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cajas { get; set; }
        public string? CodigoProductorReal { get; set; }
        public string? CodigoProductorEti { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? AñoDespacho { get; set; }
        public string? MesDespacho { get; set; }
        public string? CSG { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
        public double? PesoBrutoAduana { get; set; }
        public double? CodFacilitieDestino { get; set; }
        public string? NomFacilitieDestino { get; set; }
        public double? CajasEquivalentes { get; set; }
        public string? FechaEtaReal { get; set; }
        public string? CodPuertoDestino { get; set; }
        public string? NomPuertoDestino { get; set; }
        public string? FechaZarpe { get; set; }
        public string? NombreMercadoNave { get; set; }
        public string? FechaArribo { get; set; }
        public string? ClaveEmbarque { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? SemanaZarpe { get; set; }
        public string? NombreEmbarcador { get; set; }
        public string? CodigoTemporada { get; set; }
        public string? CodigoEmpresa { get; set; }
        public string? NombreEmpresa { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NroProceso { get; set; }
        public string? CodigoGrupoRecibidor { get; set; }
        public string? NombreGrupoRecibidor { get; set; }
        public string? CodigoGrupoCalibre { get; set; }
        public string? NombreGrupoCalibre { get; set; }
        public string? CodigoGrupoCategoria { get; set; }
        public string? NombreGrupoCategoria { get; set; }
        public double? IdCategoria { get; set; }
        public double? CodFacilitiePallet { get; set; }
        public string? NomFacilitiePallet { get; set; }
        public string? TipoMovimiento { get; set; }
        public string? FechaInspeccion { get; set; }
        public string? CodigoMercadoNave { get; set; }
        public string? CodigoExportador { get; set; }
        public string? CodigoNaviera { get; set; }
        public string? NombreNaviera { get; set; }
        public string? CodigoProvinciaProdReal { get; set; }
        public string? ProvinciaProdReal { get; set; }
        public string? CodigoComunaProdReal { get; set; }
        public string? ComunaProdReal { get; set; }
        public string? CodigoProvinciaProdEti { get; set; }
        public string? ProvinciaProdEti { get; set; }
        public string? CodigoComunaProdEti { get; set; }
        public string? ComunaProdEti { get; set; }

        [JsonConverter(typeof(NumericToStringConverter))]
        public string? NotaVenta { get; set; }
        public string? CertificadoFumigacion { get; set; }
        public string? CodigoPaisNave { get; set; }
        public string? PaisNave { get; set; }
        public string? BL { get; set; }
        public string? EstibaEnCamara { get; set; }
    }

}
