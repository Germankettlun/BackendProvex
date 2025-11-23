namespace ProvexApi.Models.Reports;

public sealed class EmbarquesRow
{
    public string CodigoTemporada { get; init; } = "";
    public string GrupoProductor { get; init; } = "";        // -> Grupo Productor
    public string NombreProductorReal { get; init; } = "";   // -> Productor
    public string Especie { get; init; } = "";
    public string Variedad { get; init; } = "";              // -> Nombre Variedad
    public string Mercado { get; init; } = "";
    public string Recibidor { get; init; } = "";
    public string CodigoGrupoCalibre { get; init; } = "";    // -> Codigo Calibre
    public string CodigoCalibre { get; init; } = "";         // -> Calibre
    public string VariedadEti { get; init; } = "";
    public decimal? PesoEnvop { get; init; }                 // -> Peso Neto
    public decimal? PesoNetoReal { get; init; }              // -> Peso Neto Real
    public string CodigoNave { get; init; } = "";
    public string Nave { get; init; } = "";
    public int? SemanaZarpe { get; init; }
    public DateTime? Fecha_Zarpe { get; init; }
    public DateTime? Fecha_Arribo { get; init; }
    public string Destino { get; init; } = "";
    public string TipoNave { get; init; } = "";
    public string CodigoEnvop { get; init; } = "";
    public string DescEnvop { get; init; } = "";
    public decimal? CajasEquivalentes { get; init; }         // -> CajasBases
    public decimal? Cajas { get; init; }
    public string? CondicionLlegada { get; init; }           // -> Condicion de Llegada
    public string? DeterminanteCondicion { get; init; }      // -> Determinante de Condicion
    public string? EstadoLiquidacion { get; init; }          // -> Estado Liquidacion
    public decimal? TotalRetornoFOB { get; init; }           // -> Total Retorno FOB
    public decimal? FOBUnitarioBulto { get; init; }          // -> FOB Unitario Bulto
    public decimal? Comision { get; init; }
    public decimal? OtrosGastos { get; init; }
    public decimal? TotalRetorno { get; init; }
    public decimal? RetornoUnitCajasBulto { get; init; }
    public decimal? RetornoUnitCajasBase { get; init; }
    public decimal? TotalFOBPercibido { get; init; }
    public decimal? FOBUnitPercibido { get; init; }
    public decimal? ComisionRetorno { get; init; }
    public decimal? OtrosCostos { get; init; }
    public decimal? TotalRetornoProductorLaFecha { get; init; }
    public decimal? RetornoProductorPercibidoCajaBulto { get; init; }
    public decimal? RetornoProductorPercibidoCajaBase { get; init; }
    public decimal? Dif_Retorno_Estimado_Retorno_Percibido { get; init; }
    public DateTime? Fecha_registro { get; init; }
    public decimal? FOBCajaBase { get; init; }               // promedio B10
}

public sealed class CtaCteRow
{
    public string? Productor { get; set; }
    public string? Especie { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Glosa { get; set; }
    public string? Item { get; set; }
    public string? SubItem { get; set; }
    public decimal? TipoCambio { get; set; }
    public decimal? Dolar { get; set; }
    public decimal? Pesos { get; set; }
    //public decimal? SaldoDolar { get; set; }
    //public decimal? SaldoPesos { get; set; }
}
