// Models/Lab/LabResiduoDtos.cs
using System.Text.Json.Serialization;

namespace ProvexApi.Models.Lab;

public sealed class LabResiduoPayloadV2
{
    [JsonPropertyName("DATA")]
    public List<LabResiduoRowV2> Data { get; set; } = new();
}

public sealed class SpecItem
{
    public string? SEASON { get; set; }
    public string? MAXVAL { get; set; }   // "0,5"
    public string? CMERC { get; set; }   // V/A/R
}
public sealed class LabResiduoRowV2
{
    public string? C_ANALISIS { get; set; }
    public string? F_ENVIO { get; set; }
    public string? PROD { get; set; }
    public string? DSC { get; set; }
    public string? ESPECIE { get; set; }
    public string? VARIEDAD { get; set; }
    public string? PRODUCTOR { get; set; }
    public string? CSG { get; set; }
    public string? COD_MUESTRA { get; set; }
    public string? VARIACION { get; set; }
    public string? PARAM { get; set; }
    public string? RESULT { get; set; }
    public string? UNIT { get; set; }
    public List<SpecItem>? SPEC { get; set; }  // <---
}