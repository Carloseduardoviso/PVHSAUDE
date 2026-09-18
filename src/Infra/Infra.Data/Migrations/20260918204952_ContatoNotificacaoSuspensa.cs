using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContatoNotificacaoSuspensa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificacaoSuspensa",
                table: "Contato",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificacaoSuspensa",
                table: "Contato");
        }
    }
}
