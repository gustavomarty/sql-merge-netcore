using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Data.Migrations
{
    /// <inheritdoc />
    public partial class MudaNomeColuna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "update_at",
                schema: "test",
                table: "test_Campeonato",
                newName: "updatE_at");

            migrationBuilder.RenameColumn(
                name: "test_nome",
                schema: "test",
                table: "test_Campeonato",
                newName: "teStNoMe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updatE_at",
                schema: "test",
                table: "test_Campeonato",
                newName: "update_at");

            migrationBuilder.RenameColumn(
                name: "teStNoMe",
                schema: "test",
                table: "test_Campeonato",
                newName: "test_nome");
        }
    }
}
