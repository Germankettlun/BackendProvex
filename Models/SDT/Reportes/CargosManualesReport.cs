namespace ProvexApi.Models.SDT.Reportes
{
    public class CargosManualesReport
    {
        public int Id { get; set; }         // int NOT NULL
        public string? CodigoTemporada { get; set; }         // nvarchar(20) NULL
        public string? CodigoGrupoProductor { get; set; }         // nvarchar(100) NULL
        public string? GrupoProductor { get; set; }         // nvarchar(100) NULL
        public string? CodigoProductor { get; set; }         // nvarchar(100) NULL
        public string? Productor { get; set; }         // nvarchar(100) NULL
        public string? RutProductor { get; set; }         // nvarchar(100) NULL
        public string? CodigoItem { get; set; }         // nvarchar(100) NULL
        public string? DescItem { get; set; }         // nvarchar(100) NULL
        public string? TipoMovItem { get; set; }         // nvarchar(100) NULL
        public string? TipoPagoItem { get; set; }         // nvarchar(100) NULL
        public string? CodigoSubItem { get; set; }         // nvarchar(100) NULL
        public string? DescSubItem { get; set; }         // nvarchar(100) NULL
        public string? CodigoGrpSubItem { get; set; }         // nvarchar(100) NULL
        public string? DescGrpSubItem { get; set; }         // nvarchar(100) NULL
        public string? CodMoneda { get; set; }         // nvarchar(100) NULL
        public string? DescMoneda { get; set; }         // nvarchar(100) NULL
        public string? CodigEspecie { get; set; }         // nvarchar(100) NULL
        public string? DescEspecie { get; set; }         // nvarchar(100) NULL
        public string? CodigoTipoNave { get; set; }         // nvarchar(100) NULL
        public string? DescTipoNave { get; set; }         // nvarchar(100) NULL
        public string? CodigoNave { get; set; }         // nvarchar(100) NULL
        public string? DescNave { get; set; }         // nvarchar(100) NULL

        // Estas columnas admiten NULL, por lo que usamos los tipos anulables:
        public int? NumeroDoc { get; set; }         // int NULL
        public string? GlosaDocumento { get; set; }         // nvarchar(100) NULL
        public DateTime? Fecha { get; set; }         // date NULL
        public decimal? TiCaCursoLegal { get; set; }         // numeric(18,8) NULL
        public decimal? TiCaFuncional { get; set; }         // numeric(18,8) NULL
        public decimal? Monto_TR { get; set; }         // numeric(18,8) NULL
        public decimal? Monto_FU { get; set; }         // numeric(18,8) NULL
        public decimal? Monto_CL { get; set; }         // numeric(18,8) NULL
        public int? AnoCont { get; set; }         // int NULL
        public int? NumeroFolio { get; set; }         // int NULL

        public string? SwPagoProductores { get; set; }         // nvarchar(100) NULL
        public DateTime? Fecha_registro { get; set; }         // date NULL
    }
}
