using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class setup3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesProductosContrato_ActivosFijos_FixedAssetId",
                table: "DetallesProductosContrato");

            migrationBuilder.DropIndex(
                name: "IX_DetallesProductosContrato_FixedAssetId",
                table: "DetallesProductosContrato");

            migrationBuilder.DropColumn(
                name: "FixedAssetId",
                table: "DetallesProductosContrato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FixedAssetId",
                table: "DetallesProductosContrato",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesProductosContrato_FixedAssetId",
                table: "DetallesProductosContrato",
                column: "FixedAssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesProductosContrato_ActivosFijos_FixedAssetId",
                table: "DetallesProductosContrato",
                column: "FixedAssetId",
                principalTable: "ActivosFijos",
                principalColumn: "Id");
        }
    }
}
