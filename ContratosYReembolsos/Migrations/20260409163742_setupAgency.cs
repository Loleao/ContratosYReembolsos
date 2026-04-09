using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class setupAgency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RUC",
                table: "Agencias",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Agencias",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Agencias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Agencias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Agencias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Agencias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Agencias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Agencias_BranchId",
                table: "Agencias",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agencias_Filiales_BranchId",
                table: "Agencias",
                column: "BranchId",
                principalTable: "Filiales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agencias_Filiales_BranchId",
                table: "Agencias");

            migrationBuilder.DropIndex(
                name: "IX_Agencias_BranchId",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Agencias");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Agencias");

            migrationBuilder.AlterColumn<string>(
                name: "RUC",
                table: "Agencias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Agencias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
