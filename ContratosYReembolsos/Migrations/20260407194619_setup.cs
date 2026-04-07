using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class setup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RUC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ataudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ataudes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInventoryTracked = table.Column<bool>(type: "bit", nullable: false),
                    RequiresScheduleValidation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasServicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conductores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubsidiaryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conductores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitorDni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitorCip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitorType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeceasedDni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeceasedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BurialDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BurialTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IneiCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UbigeoFull = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WakeId = table.Column<int>(type: "int", nullable: true),
                    WakeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CemeteryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CemeteryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BurialType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BurialDetail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgencyAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pabellones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CemeteryId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pabellones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplatesSepulturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalFloors = table.Column<int>(type: "int", nullable: false),
                    RowsCount = table.Column<int>(type: "int", nullable: false),
                    ColsPerFace = table.Column<int>(type: "int", nullable: false),
                    IsDoubleFace = table.Column<bool>(type: "bit", nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatesSepulturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ubigeos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubigeos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesFisicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Plate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesFisicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    SubsidiaryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AtaudVariantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoffinModelId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtaudVariantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtaudVariantes_Ataudes_CoffinModelId",
                        column: x => x.CoffinModelId,
                        principalTable: "Ataudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    LogicType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servicios_CategoriasServicios_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "CategoriasServicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesMovilidadContrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDispatched = table.Column<bool>(type: "bit", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesMovilidadContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesMovilidadContrato_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nichos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PavilionId = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    CemeteryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBeingReserved = table.Column<bool>(type: "bit", nullable: false),
                    ReservationExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReservedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nichos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nichos_Pabellones_PavilionId",
                        column: x => x.PavilionId,
                        principalTable: "Pabellones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Filiales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UbigeoId = table.Column<string>(type: "nvarchar(6)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filiales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filiales_Ubigeos_UbigeoId",
                        column: x => x.UbigeoId,
                        principalTable: "Ubigeos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehiculosServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TripStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiculosServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_Conductores_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Conductores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_Vehiculos_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosAtaudes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoffinVariantId = table.Column<int>(type: "int", nullable: false),
                    SubsidiaryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegisteredBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BalanceAfter = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosAtaudes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosAtaudes_AtaudVariantes_CoffinVariantId",
                        column: x => x.CoffinVariantId,
                        principalTable: "AtaudVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockFilial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoffinVariantId = table.Column<int>(type: "int", nullable: false),
                    SubsidiaryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MinimumStock = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockFilial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockFilial_AtaudVariantes_CoffinVariantId",
                        column: x => x.CoffinVariantId,
                        principalTable: "AtaudVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockItems_Servicios_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cementerios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RUC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cementerios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cementerios_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtaudTransferencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoffinVariantId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OriginSubsidiaryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetSubsidiaryId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureMovementId = table.Column<int>(type: "int", nullable: true),
                    ArrivalMovementId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuiaRemision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceptionObservations = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtaudTransferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtaudTransferencias_AtaudVariantes_CoffinVariantId",
                        column: x => x.CoffinVariantId,
                        principalTable: "AtaudVariantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtaudTransferencias_MovimientosAtaudes_ArrivalMovementId",
                        column: x => x.ArrivalMovementId,
                        principalTable: "MovimientosAtaudes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AtaudTransferencias_MovimientosAtaudes_DepartureMovementId",
                        column: x => x.DepartureMovementId,
                        principalTable: "MovimientosAtaudes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetallesContrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    StockItemId = table.Column<int>(type: "int", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesContrato_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesContrato_Servicios_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesContrato_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SepulturasEstructura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CemeteryId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepulturasEstructura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SepulturasEstructura_Cementerios_CemeteryId",
                        column: x => x.CemeteryId,
                        principalTable: "Cementerios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SepulturasEstructura_TemplatesSepulturas_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TemplatesSepulturas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SepulturasNichos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false),
                    RowLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    InhumationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepulturasNichos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SepulturasNichos_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SepulturasNichos_SepulturasEstructura_StructureId",
                        column: x => x.StructureId,
                        principalTable: "SepulturasEstructura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtaudTransferencias_ArrivalMovementId",
                table: "AtaudTransferencias",
                column: "ArrivalMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_AtaudTransferencias_CoffinVariantId",
                table: "AtaudTransferencias",
                column: "CoffinVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AtaudTransferencias_DepartureMovementId",
                table: "AtaudTransferencias",
                column: "DepartureMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_AtaudVariantes_CoffinModelId",
                table: "AtaudVariantes",
                column: "CoffinModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Cementerios_BranchId",
                table: "Cementerios",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesContrato_ContractId",
                table: "DetallesContrato",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesContrato_ServiceId",
                table: "DetallesContrato",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesContrato_StockItemId",
                table: "DetallesContrato",
                column: "StockItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesMovilidadContrato_ContractId",
                table: "DetallesMovilidadContrato",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Filiales_UbigeoId",
                table: "Filiales",
                column: "UbigeoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosAtaudes_CoffinVariantId",
                table: "MovimientosAtaudes",
                column: "CoffinVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Nichos_PavilionId",
                table: "Nichos",
                column: "PavilionId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasEstructura_CemeteryId",
                table: "SepulturasEstructura",
                column: "CemeteryId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasEstructura_TemplateId",
                table: "SepulturasEstructura",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasNichos_ContractId",
                table: "SepulturasNichos",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasNichos_StructureId",
                table: "SepulturasNichos",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_ServiceCategoryId",
                table: "Servicios",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockFilial_CoffinVariantId",
                table: "StockFilial",
                column: "CoffinVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ServiceId",
                table: "StockItems",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_ContractId",
                table: "VehiculosServicios",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_DriverId",
                table: "VehiculosServicios",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_VehicleId",
                table: "VehiculosServicios",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agencias");

            migrationBuilder.DropTable(
                name: "AtaudTransferencias");

            migrationBuilder.DropTable(
                name: "DetallesContrato");

            migrationBuilder.DropTable(
                name: "DetallesMovilidadContrato");

            migrationBuilder.DropTable(
                name: "Nichos");

            migrationBuilder.DropTable(
                name: "SepulturasNichos");

            migrationBuilder.DropTable(
                name: "StockFilial");

            migrationBuilder.DropTable(
                name: "UnidadesFisicas");

            migrationBuilder.DropTable(
                name: "VehiculosServicios");

            migrationBuilder.DropTable(
                name: "MovimientosAtaudes");

            migrationBuilder.DropTable(
                name: "StockItems");

            migrationBuilder.DropTable(
                name: "Pabellones");

            migrationBuilder.DropTable(
                name: "SepulturasEstructura");

            migrationBuilder.DropTable(
                name: "Conductores");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "AtaudVariantes");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Cementerios");

            migrationBuilder.DropTable(
                name: "TemplatesSepulturas");

            migrationBuilder.DropTable(
                name: "Ataudes");

            migrationBuilder.DropTable(
                name: "CategoriasServicios");

            migrationBuilder.DropTable(
                name: "Filiales");

            migrationBuilder.DropTable(
                name: "Ubigeos");
        }
    }
}
