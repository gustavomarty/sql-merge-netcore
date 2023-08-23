using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrigeTabCampeonato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "test",
                table: "test_Campeonato");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "test",
                table: "test_Campeonato");

            migrationBuilder.DropColumn(
                name: "update_by",
                schema: "test",
                table: "test_Campeonato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                schema: "test",
                table: "test_Campeonato",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "test",
                table: "test_Campeonato",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "update_by",
                schema: "test",
                table: "test_Campeonato",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
