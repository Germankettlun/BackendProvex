using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProvexApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSensiWatchTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.CreateTable(
                name: "Asoex_Consignatario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodConsignatario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomConsignatario = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Consignatario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Especie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodEspecie = table.Column<int>(type: "int", nullable: false),
                    CodTipoEpecie = table.Column<int>(type: "int", nullable: false),
                    NomEspecie = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Especie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Exportador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RutEmpresa = table.Column<long>(type: "bigint", nullable: false),
                    DvEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomEmpresa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Exportador", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Nave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodNave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomNave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoNave = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Nave", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_PaisesDestino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodPais = table.Column<int>(type: "int", nullable: false),
                    NomPais = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodRegionPais = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_PaisesDestino", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Procesos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProceso = table.Column<int>(type: "int", nullable: false),
                    FechaProceso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraProceso = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Procesos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_PuertoEmbarque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodPuerto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomPuerto = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_PuertoEmbarque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_PuertosDestino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodPuerto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomPuerto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodCosta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodPais = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_PuertosDestino", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_RegionesDestino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodRegionPais = table.Column<int>(type: "int", nullable: false),
                    NomRegionPais = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_RegionesDestino", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_RegionOrigen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodRegionOrigen = table.Column<int>(type: "int", nullable: false),
                    NomRegion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_RegionOrigen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Semana",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TempSeqNro = table.Column<int>(type: "int", nullable: false),
                    NroOtro = table.Column<int>(type: "int", nullable: false),
                    Fecha1 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha2 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NroSemana = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Semana", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Temporada",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TempSeqNro = table.Column<int>(type: "int", nullable: false),
                    TempFechaDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TempFechaHasta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TempEstado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomTemporada = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Temporada", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_TipoEspecie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodTipoEpecie = table.Column<int>(type: "int", nullable: false),
                    NombreTipo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_TipoEspecie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_Variedad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodVariedad = table.Column<int>(type: "int", nullable: false),
                    CodEspecie = table.Column<int>(type: "int", nullable: false),
                    CodTipoEpecie = table.Column<int>(type: "int", nullable: false),
                    NomVariedad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_Variedad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Component = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.ModuleId);
                    table.ForeignKey(
                        name: "FK_Module_Module_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Module",
                        principalColumn: "ModuleId");
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SensiWatch_Devices",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IMEI = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlatformId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrgUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensiWatch_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "SensiWatch_Trips",
                columns: table => new
                {
                    TripId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SwpTripId = table.Column<int>(type: "int", nullable: false),
                    TripGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternalTripId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrailerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PoNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensiWatch_Trips", x => x.TripId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceApiCallLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApiName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    AuthSuccess = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    ResponseSize = table.Column<int>(type: "int", nullable: false),
                    Snippet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    token = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceApiCallLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jti = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_ProcesoDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProceso = table.Column<int>(type: "int", nullable: false),
                    NroOtro = table.Column<int>(type: "int", nullable: false),
                    TempSeqNro = table.Column<int>(type: "int", nullable: false),
                    CodPuertoEmbarque = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelativoViaje = table.Column<int>(type: "int", nullable: false),
                    RutExportador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodEspecie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodVariedad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodRegionOrigen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PsdCantidad = table.Column<int>(type: "int", nullable: false),
                    PsdTotKgNeto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CodPuertoDestino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodConsignatario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodCondicion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pagina_api = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_ProcesoDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asoex_ProcesoDetalles_Asoex_Procesos_IdProceso",
                        column: x => x.IdProceso,
                        principalTable: "Asoex_Procesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asoex_ProcesoViajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProceso = table.Column<int>(type: "int", nullable: false),
                    NroOtro = table.Column<int>(type: "int", nullable: false),
                    TempSeqNro = table.Column<int>(type: "int", nullable: false),
                    CodPuertoEmbarque = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelativoViaje = table.Column<int>(type: "int", nullable: false),
                    CodPuertoArribo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaZarpe = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaArribo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CodNave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asoex_ProcesoViajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asoex_ProcesoViajes_Asoex_Procesos_IdProceso",
                        column: x => x.IdProceso,
                        principalTable: "Asoex_Procesos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleModule",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModule", x => new { x.RoleId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_RoleModule_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Module",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleModule_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensiWatch_Events",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeviceTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    LocationAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerShipmentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensiWatch_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_SensiWatch_Events_SensiWatch_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "SensiWatch_Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SensiWatch_Events_SensiWatch_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "SensiWatch_Trips",
                        principalColumn: "TripId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserModule",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModule", x => new { x.UserId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_UserModule_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Module",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModule_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensiWatch_SensorReadings",
                columns: table => new
                {
                    SensorReadingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    SensorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SensorValue = table.Column<double>(type: "float", nullable: false),
                    DeviceTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiveTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensiWatch_SensorReadings", x => x.SensorReadingId);
                    table.ForeignKey(
                        name: "FK_SensiWatch_SensorReadings_SensiWatch_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "SensiWatch_Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asoex_ProcesoDetalles_IdProceso",
                table: "Asoex_ProcesoDetalles",
                column: "IdProceso");

            migrationBuilder.CreateIndex(
                name: "IX_Asoex_ProcesoViajes_IdProceso",
                table: "Asoex_ProcesoViajes",
                column: "IdProceso");

            migrationBuilder.CreateIndex(
                name: "IX_Module_ParentId",
                table: "Module",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModule_ModuleId",
                table: "RoleModule",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_Devices_SerialNumber",
                table: "SensiWatch_Devices",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_Events_DeviceId_ReceivedAt",
                table: "SensiWatch_Events",
                columns: new[] { "DeviceId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_Events_TripId",
                table: "SensiWatch_Events",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_SensorReadings_EventId",
                table: "SensiWatch_SensorReadings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_SensorReadings_SensorType_DeviceTime",
                table: "SensiWatch_SensorReadings",
                columns: new[] { "SensorType", "DeviceTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_Trips_InternalTripId",
                table: "SensiWatch_Trips",
                column: "InternalTripId");

            migrationBuilder.CreateIndex(
                name: "IX_SensiWatch_Trips_SwpTripId",
                table: "SensiWatch_Trips",
                column: "SwpTripId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserModule_ModuleId",
                table: "UserModule",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asoex_Consignatario");

            migrationBuilder.DropTable(
                name: "Asoex_Especie");

            migrationBuilder.DropTable(
                name: "Asoex_Exportador");

            migrationBuilder.DropTable(
                name: "Asoex_Nave");

            migrationBuilder.DropTable(
                name: "Asoex_PaisesDestino");

            migrationBuilder.DropTable(
                name: "Asoex_ProcesoDetalles");

            migrationBuilder.DropTable(
                name: "Asoex_ProcesoViajes");

            migrationBuilder.DropTable(
                name: "Asoex_PuertoEmbarque");

            migrationBuilder.DropTable(
                name: "Asoex_PuertosDestino");

            migrationBuilder.DropTable(
                name: "Asoex_RegionesDestino");

            migrationBuilder.DropTable(
                name: "Asoex_RegionOrigen");

            migrationBuilder.DropTable(
                name: "Asoex_Semana");

            migrationBuilder.DropTable(
                name: "Asoex_Temporada");

            migrationBuilder.DropTable(
                name: "Asoex_TipoEspecie");

            migrationBuilder.DropTable(
                name: "Asoex_Variedad");

            migrationBuilder.DropTable(
                name: "RoleModule");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "SensiWatch_SensorReadings");

            migrationBuilder.DropTable(
                name: "ServiceApiCallLogs");

            migrationBuilder.DropTable(
                name: "SessionTokens");

            migrationBuilder.DropTable(
                name: "UserModule");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Asoex_Procesos");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "SensiWatch_Events");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "SensiWatch_Devices");

            migrationBuilder.DropTable(
                name: "SensiWatch_Trips");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }
    }
}
