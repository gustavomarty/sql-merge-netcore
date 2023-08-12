using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Data.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clube",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    apelido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    abreviacao = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clube", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fornecedor",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    cep = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    numero = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contrato",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_fornecedor = table.Column<int>(type: "int", nullable: false),
                    id_material = table.Column<int>(type: "int", nullable: false),
                    id_clube = table.Column<int>(type: "int", nullable: false),
                    numero = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato", x => x.id);
                    table.ForeignKey(
                        name: "fk_contrato_clube_clube_id",
                        column: x => x.id_clube,
                        principalTable: "clube",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contrato_fornecedor_fornecedor_id",
                        column: x => x.id_fornecedor,
                        principalTable: "fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contrato_material_material_id",
                        column: x => x.id_material,
                        principalTable: "material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clube_nome",
                table: "clube",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contrato_id_clube",
                table: "contrato",
                column: "id_clube");

            migrationBuilder.CreateIndex(
                name: "ix_contrato_id_fornecedor",
                table: "contrato",
                column: "id_fornecedor");

            migrationBuilder.CreateIndex(
                name: "ix_contrato_id_material",
                table: "contrato",
                column: "id_material");

            migrationBuilder.CreateIndex(
                name: "ix_contrato_numero",
                table: "contrato",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_documento",
                table: "fornecedor",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_material_numero",
                table: "material",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contrato");

            migrationBuilder.DropTable(
                name: "clube");

            migrationBuilder.DropTable(
                name: "fornecedor");

            migrationBuilder.DropTable(
                name: "material");
        }
    }
}
