using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProvexApi.Migrations.RossiDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    despacho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    booking = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dusfecha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dusnro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dusdv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consignatario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    puertoemb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    puertodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pais = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    programa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    motonave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    viaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tiponave = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechainicio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horainicio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechaestzarpe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horaestzarpe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechazarpe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horazarpe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechaenvioinforme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horaenvioinforme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    terminal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sitio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_tipoflete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_despacho_extranjero = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_despacho_nacional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_etadestino = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_errorplanilla = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_certificado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_factura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_origen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_correccionbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_bl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_bltipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_blnumero = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_fullset = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_vistobueno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    c_duslegalizada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aduananombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aduanacodigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valorfob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valorliquidoretorno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valorflete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valorseguro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ws_bls = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchivoUrls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    TipoArchivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivoUrls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivoUrls_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Embarques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    envio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    patente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    transportista = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    condicion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nrocontenedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechaipto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horaipto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechatemb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horatemb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechacump = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horacump = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechaicon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horaicon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fechatcon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    horatcon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallemb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tcajas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkilosn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkilosb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tpallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_latearrival = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_ingresomultipuerto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_aforoaduana = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_consolidado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_fzextraportuarias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_inspeccionsag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_instalacioncortina = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costo_guiaingresadazeal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tcontenedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sello1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sello2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sello3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sello4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tara = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ventvalor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    venttipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tempsigno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tempvalor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    temptipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    atmosfera = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    atmco2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    atmo2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    etiqueta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallet1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    termografo1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallet2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    termografo2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallet3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    termografo3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallet4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    termografo4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallet5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    termografo5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    guia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    packing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    especie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    variedad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    atributo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    embalaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    embalajetipo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    embalajekn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    embalajekb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cajas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pltxcjs = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Embarques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Embarques_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facturaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    f_factura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_empresa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_empresarut = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_empresarutdv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_servicionombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_fechaemision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_fechavencimiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_moneda = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_valorobservado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_monto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_cantidad = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturaciones_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<int>(type: "int", nullable: false),
                    p_pallet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_camara = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_nivel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_contenedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_termografo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_temperatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pallets_Documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PalletId = table.Column<int>(type: "int", nullable: false),
                    p_especie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_variedad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_atributo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_cajas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_etiqueta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_calidad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_calibre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_fecha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_productor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_codembalaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_embalaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_kn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_kb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_embalajet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_guia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    p_packing = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PDetalles_Pallets_PalletId",
                        column: x => x.PalletId,
                        principalTable: "Pallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivoUrls_DocumentoId",
                table: "ArchivoUrls",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Embarques_DocumentoId",
                table: "Embarques",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturaciones_DocumentoId",
                table: "Facturaciones",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pallets_DocumentoId",
                table: "Pallets",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_PDetalles_PalletId",
                table: "PDetalles",
                column: "PalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivoUrls");

            migrationBuilder.DropTable(
                name: "Embarques");

            migrationBuilder.DropTable(
                name: "Facturaciones");

            migrationBuilder.DropTable(
                name: "PDetalles");

            migrationBuilder.DropTable(
                name: "Pallets");

            migrationBuilder.DropTable(
                name: "Documentos");
        }
    }
}
