using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTabCampeonato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "test_Campeonato",
                schema: "test",
                columns: table => new
                {
                    idf_campeonato = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    test_nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    test_pais = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    test_ano = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    update_by = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_campeonato", x => x.idf_campeonato);
                });

            migrationBuilder.CreateIndex(
                name: "ix_test_campeonato_idf_campeonato",
                schema: "test",
                table: "test_Campeonato",
                column: "idf_campeonato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_Campeonato",
                schema: "test");
        }
    }
}
