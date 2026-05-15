using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class setup5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutionDate",
                table: "DetallesServiciosContrato",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "DetallesServiciosContrato",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DetallesServiciosContrato",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DetallesProductosContrato",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionDate",
                table: "DetallesServiciosContrato");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "DetallesServiciosContrato");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DetallesServiciosContrato");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DetallesProductosContrato");
        }
    }
}
