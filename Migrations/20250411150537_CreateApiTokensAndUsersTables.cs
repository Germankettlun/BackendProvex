using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProvexApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateApiTokensAndUsersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudFruitItems");

            migrationBuilder.CreateTable(
                name: "ApiTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiTokens");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.CreateTable(
                name: "CloudFruitItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Altura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AñoDespacho = table.Column<int>(type: "int", nullable: true),
                    BL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingAwb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CSG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cajas = table.Column<int>(type: "int", nullable: true),
                    CajasEquivalentes = table.Column<double>(type: "float", nullable: true),
                    Celular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificadoFumigacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaveEmbarque = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodBasePallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodCalibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodCuartelEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodCuartelReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodEmbalaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodEnvase = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodEnvop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodEspecie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodEtiqueta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodFacilitie = table.Column<double>(type: "float", nullable: true),
                    CodFacilitieDestino = table.Column<double>(type: "float", nullable: true),
                    CodFacilitiePallet = table.Column<double>(type: "float", nullable: true),
                    CodGrpoVariedadEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodGrpoVariedadReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodPLU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodPredioEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodPredioReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodPuerto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodPuertoDestino = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodRecibidor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodTipoInspeccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodTipoInspeccionEnc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodTipoNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodTipoTrat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodVariedadEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodVariedadReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoComunaProdEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoComunaProdReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoExportador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoGrupoCalibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoGrupoCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoGrupoProductor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoGrupoRecibidor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoMercadoNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoNaviera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoPaisNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoProductorEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoProductorReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoProvinciaProdEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoProvinciaProdReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoTemporada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComunaProdEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComunaProdReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Consignatario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contenedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Despachador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstibaEnCamara = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaArribo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCosecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaDespacho = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEtaReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaInspeccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPack = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaZarpe = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FolioSAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuiaDespacho = table.Column<double>(type: "float", nullable: false),
                    HoraDespacho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdCategoria = table.Column<double>(type: "float", nullable: true),
                    IdPallet = table.Column<double>(type: "float", nullable: true),
                    MesDespacho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mixs = table.Column<int>(type: "int", nullable: true),
                    NomAgenteAduana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomBasePallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomCalibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomChofer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomCuartelEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomCuartelReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomEmbalaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomEnvase = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomEnvop = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomEspecie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomEtiqueta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomExportador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomFacilitie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomFacilitieDestino = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomFacilitiePallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomGrpoVariedadEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomGrpoVariedadReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomPLU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomPais = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomPredioEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomPredioReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomProductorEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomProductorReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomPuerto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomPuertoDestino = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomRecibidor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomTipoInspeccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomTipoInspeccionEnc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomTipoNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomTipoTrat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomTransportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomVariedadEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomVariedadReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreEmbarcador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreGrupoCalibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreGrupoCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreGrupoProductor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreGrupoRecibidor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreMercadoNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreNaviera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotaVenta = table.Column<int>(type: "int", nullable: true),
                    NroProceso = table.Column<int>(type: "int", nullable: true),
                    NumeroCertificadoProductorEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroCertificadoProductorReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObsGuia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ObsSAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaisNave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Patente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PesoBrutoAduana = table.Column<double>(type: "float", nullable: true),
                    PesoNeto = table.Column<double>(type: "float", nullable: false),
                    PlanillaDespacho = table.Column<int>(type: "int", nullable: false),
                    PlanillaOrdenEmbarque = table.Column<int>(type: "int", nullable: false),
                    ProvinciaProdEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinciaProdReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutAgenteAduana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutChofer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutExportador = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutProductorEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutProductorReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutRecibidor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutTransportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sellos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SemanaZarpe = table.Column<int>(type: "int", nullable: true),
                    Temperatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Termografo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoCamion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoCertificadoProductorEti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoCertificadoProductorReal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoContenedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoPallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalCajas = table.Column<int>(type: "int", nullable: true),
                    TotalDet = table.Column<int>(type: "int", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudFruitItems", x => x.Id);
                });
        }
    }
}
