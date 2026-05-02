using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class addedAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivosFijos_Productos_ProductId",
                table: "ActivosFijos");

            migrationBuilder.DropIndex(
                name: "IX_ActivosFijos_ProductId",
                table: "ActivosFijos");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ActivosFijos",
                newName: "Condition");

            migrationBuilder.AddColumn<int>(
                name: "AssetCatalogId",
                table: "ActivosFijos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Observation",
                table: "ActivosFijos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "ActivosFijos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RegisteredByUserId",
                table: "ActivosFijos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ActivosCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosCategorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivosHistorial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixedAssetId = table.Column<int>(type: "int", nullable: false),
                    FromBranchId = table.Column<int>(type: "int", nullable: true),
                    ToBranchId = table.Column<int>(type: "int", nullable: true),
                    ResponsibleUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivosHistorial_ActivosFijos_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "ActivosFijos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivosSubcategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosSubcategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivosSubcategorias_ActivosCategorias_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ActivosCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivosCatalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubcategoryId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosCatalogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivosCatalogo_ActivosSubcategorias_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "ActivosSubcategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_AssetCatalogId",
                table: "ActivosFijos",
                column: "AssetCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_BranchId",
                table: "ActivosFijos",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosCatalogo_SubcategoryId",
                table: "ActivosCatalogo",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosHistorial_FixedAssetId",
                table: "ActivosHistorial",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosSubcategorias_CategoryId",
                table: "ActivosSubcategorias",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivosFijos_ActivosCatalogo_AssetCatalogId",
                table: "ActivosFijos",
                column: "AssetCatalogId",
                principalTable: "ActivosCatalogo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivosFijos_Filiales_BranchId",
                table: "ActivosFijos",
                column: "BranchId",
                principalTable: "Filiales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivosFijos_ActivosCatalogo_AssetCatalogId",
                table: "ActivosFijos");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivosFijos_Filiales_BranchId",
                table: "ActivosFijos");

            migrationBuilder.DropTable(
                name: "ActivosCatalogo");

            migrationBuilder.DropTable(
                name: "ActivosHistorial");

            migrationBuilder.DropTable(
                name: "ActivosSubcategorias");

            migrationBuilder.DropTable(
                name: "ActivosCategorias");

            migrationBuilder.DropIndex(
                name: "IX_ActivosFijos_AssetCatalogId",
                table: "ActivosFijos");

            migrationBuilder.DropIndex(
                name: "IX_ActivosFijos_BranchId",
                table: "ActivosFijos");

            migrationBuilder.DropColumn(
                name: "AssetCatalogId",
                table: "ActivosFijos");

            migrationBuilder.DropColumn(
                name: "Observation",
                table: "ActivosFijos");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "ActivosFijos");

            migrationBuilder.DropColumn(
                name: "RegisteredByUserId",
                table: "ActivosFijos");

            migrationBuilder.RenameColumn(
                name: "Condition",
                table: "ActivosFijos",
                newName: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_ProductId",
                table: "ActivosFijos",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivosFijos_Productos_ProductId",
                table: "ActivosFijos",
                column: "ProductId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
