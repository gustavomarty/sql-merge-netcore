using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldDataAlteracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "data_alteracao",
                table: "material",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "data_alteracao",
                table: "fornecedor",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "data_alteracao",
                table: "contrato",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "data_alteracao",
                table: "clube",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data_alteracao",
                table: "material");

            migrationBuilder.DropColumn(
                name: "data_alteracao",
                table: "fornecedor");

            migrationBuilder.DropColumn(
                name: "data_alteracao",
                table: "contrato");

            migrationBuilder.DropColumn(
                name: "data_alteracao",
                table: "clube");
        }
    }
}
