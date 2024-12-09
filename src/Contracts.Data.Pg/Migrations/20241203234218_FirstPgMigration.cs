using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Contracts.Data.Pg.Migrations
{
    /// <inheritdoc />
    public partial class FirstPgMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test");

            migrationBuilder.CreateTable(
                name: "clube",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    apelido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    abreviacao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clube", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Fabrica",
                schema: "test",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    apelido = table.Column<string>(type: "text", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    quantidade_funcionarios = table.Column<int>(type: "integer", nullable: false),
                    bulk_merge_status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fabrica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fornecedor",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "material",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    numero = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_material", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "test_Campeonato",
                schema: "test",
                columns: table => new
                {
                    idf_campeonato = table.Column<Guid>(type: "uuid", nullable: false),
                    updatE_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    teStNoMe = table.Column<string>(type: "text", nullable: false),
                    test_pais = table.Column<string>(type: "text", nullable: false),
                    test_ano = table.Column<int>(type: "integer", nullable: false),
                    sts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_campeonato", x => x.idf_campeonato);
                });

            migrationBuilder.CreateTable(
                name: "contrato",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_fornecedor = table.Column<int>(type: "integer", nullable: false),
                    id_material = table.Column<int>(type: "integer", nullable: false),
                    id_clube = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    preco = table.Column<decimal>(type: "numeric", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    data_alteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato", x => x.id);
                    table.ForeignKey(
                        name: "fk_contrato_clube_id_clube",
                        column: x => x.id_clube,
                        principalTable: "clube",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contrato_fornecedor_id_fornecedor",
                        column: x => x.id_fornecedor,
                        principalTable: "fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_contrato_material_id_material",
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
                name: "ix_fabrica_id",
                schema: "test",
                table: "Fabrica",
                column: "id");

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
                name: "contrato");

            migrationBuilder.DropTable(
                name: "Fabrica",
                schema: "test");

            migrationBuilder.DropTable(
                name: "test_Campeonato",
                schema: "test");

            migrationBuilder.DropTable(
                name: "clube");

            migrationBuilder.DropTable(
                name: "fornecedor");

            migrationBuilder.DropTable(
                name: "material");
        }
    }
}
