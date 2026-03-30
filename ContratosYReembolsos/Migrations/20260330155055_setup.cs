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
                    id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    año_contrato = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    num_contrato = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    tipoSoli = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    idfaf = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Solicitante = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    domicilio_soli = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    dni_soli = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    tel_soli = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    cip_titular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    tipoDifun = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    codBenef = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    codAgen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Difunto = table.Column<string>(type: "nvarchar(360)", maxLength: 360, nullable: false),
                    dni_difu = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    velatorio = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    fallecimiento_lugar = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    hor_sepelio = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    codCemente = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    codfilial = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    fec_contrato = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fec_falleci = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fec_sepelio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoPor = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.id);
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

            migrationBuilder.CreateIndex(
                name: "IX_Nichos_PavilionId",
                table: "Nichos",
                column: "PavilionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agencias");

            migrationBuilder.DropTable(
                name: "Cementerios");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Nichos");

            migrationBuilder.DropTable(
                name: "Pabellones");
        }
    }
}
