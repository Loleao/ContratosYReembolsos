using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class setup2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomWakeAddress",
                table: "Contratos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_WakeId",
                table: "Contratos",
                column: "WakeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Velatorios_WakeId",
                table: "Contratos",
                column: "WakeId",
                principalTable: "Velatorios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Velatorios_WakeId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_WakeId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "CustomWakeAddress",
                table: "Contratos");
        }
    }
}
