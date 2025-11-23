using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using ProvexApi.Converters;


namespace ProvexApi.Models.SDT.Cargas
{
    public class ProduccionResponse
    {
        public List<ProduccionItem>? Data { get; set; }
        public string? Columns { get; set; } // A veces viene, a veces no
                                             // etc.
    }

#nullable enable
    public class ProduccionItem
    {
        public int Id { get; set; } // Primary Key (PK) en la tabla (puedes cambiarlo si deseas)
        public string? Origen { get; set; }
        public string? CodEmpresa { get; set; }
        public string? NomEmpresa { get; set; }
        public string? CodTemporada { get; set; }
        public string? NomTemporada { get; set; }
        public string? AlturaPallet { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Planilla { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? FechaProceso { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Semana { get; set; }
        public string? CodEnvase { get; set; }
        public string? NomEnvase { get; set; }
        public string? CodEmbalaje { get; set; }
        public string? NomEmbalaje { get; set; }
        public string? CodCuartelReal { get; set; }
        public string? NomCuartelReal { get; set; }
        public string? CodCuartelEti { get; set; }
        public string? NomCuartelEti { get; set; }
        public string? CodPredioReal { get; set; }
        public string? NomPredioReal { get; set; }
        public string? CodPredioEti { get; set; }
        public string? NomPredioEti { get; set; }
        public string? CsgReal { get; set; }
        public string? CsgEtiquetado { get; set; }
        public string? CodEspecieReal { get; set; }
        public string? NomEspecieReal { get; set; }
        public string? CodEspecieEti { get; set; }
        public string? NomEspecieEti { get; set; }
        public string? CodVariedadReal { get; set; }
        public string? NomVariedadReal { get; set; }
        public string? CodVariedadEti { get; set; }
        public string? NomVariedadEti { get; set; }
        public string? CodCalibre { get; set; }
        public string? CodEtiqueta { get; set; }
        public string? NomEtiqueta { get; set; }
        public string? CodFacilitie { get; set; }
        public string? NomFacilitie { get; set; }
        public string? CodPLU { get; set; }
        public string? NomPLU { get; set; }
        public string? NumeroPallet { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Cajas { get; set; }
        public string? NomExportador { get; set; }
        public string? CodigoProductorReal { get; set; }
        public string? NombreProductorReal { get; set; }
        public string? RutProductorReal { get; set; }
        public string? DVProductorReal { get; set; }
        public string? RazonSocialProductorReal { get; set; }
        public string? CodigoProductorEti { get; set; }
        public string? NombreProductorEti { get; set; }
        public string? CodigoGrupoProductor { get; set; }
        public string? NombreGrupoProductor { get; set; }
        public string? FechaCosecha { get; set; }
        public string? FechaPacking { get; set; }
        public string? CodigoEnvop { get; set; }
        public string? NombreEnvop { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? PesoNetoEnvop { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? TotalKilos { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? Guia { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? CajasEquivalentes { get; set; }
        public string? CodigoCategoria { get; set; }
        public string? NomCategoria { get; set; }
        public string? TipoPallet { get; set; }
        public string? CodigoJornada { get; set; }
        public string? NomJornada { get; set; }
        public string? CodigoLinea { get; set; }
        public string? DescripcionLinea { get; set; }
        public string? TipoProceso { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosAjustados { get; set; }

       [JsonConverter(typeof(NumericToStringConverter))]
        public string? KilosMaquina { get; set; }
        public string? CodigoGrupoCalibre { get; set; }
        public string? NombreGrupoCalibre { get; set; }
        public string? CodigoGrupoCategoria { get; set; }
        public string? NombreGrupoCategoria { get; set; }
    }

}
