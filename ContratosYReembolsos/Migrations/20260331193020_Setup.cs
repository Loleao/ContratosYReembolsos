using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class Setup : Migration
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
                name: "Cementerios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RUC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UbigeoId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cementerios", x => x.Id);
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
                name: "IX_Nichos_PavilionId",
                table: "Nichos",
                column: "PavilionId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_ServiceCategoryId",
                table: "Servicios",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_ServiceId",
                table: "StockItems",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agencias");

            migrationBuilder.DropTable(
                name: "Cementerios");

            migrationBuilder.DropTable(
                name: "DetallesContrato");

            migrationBuilder.DropTable(
                name: "Nichos");

            migrationBuilder.DropTable(
                name: "UnidadesFisicas");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "StockItems");

            migrationBuilder.DropTable(
                name: "Pabellones");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "CategoriasServicios");
        }
    }
}
