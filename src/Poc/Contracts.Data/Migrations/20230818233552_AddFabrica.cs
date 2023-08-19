using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFabrica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test");

            migrationBuilder.CreateTable(
                name: "Fabrica",
                schema: "test",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    apelido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    quantidade_funcionarios = table.Column<int>(type: "int", nullable: false),
                    bulk_merge_status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    update_by = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fabrica", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fabrica_id",
                schema: "test",
                table: "Fabrica",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fabrica",
                schema: "test");
        }
    }
}
